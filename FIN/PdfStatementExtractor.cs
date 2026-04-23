using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace FINApp
{
    /// <summary>
    /// Converts a native-text (non-scanned) PDF bank statement into a row-grid
    /// &mdash; <c>List&lt;List&lt;string&gt;&gt;</c> where each inner list is one row's
    /// columns from left to right.
    ///
    /// Strategy:
    ///   1. Walk every page with a custom event listener that records every text
    ///      span along with its bottom-left X/Y coordinates.
    ///   2. Group spans by Y-coordinate (tolerance ~2pt) to form rows.
    ///   3. Cluster X-coordinates across ALL spans to discover column boundaries.
    ///   4. For each row, assign spans to their column bucket and emit a string
    ///      per column (empty if no span).
    ///   5. Post-process: filter out header/footer rows (no date AND no numeric
    ///      amount), merge orphan continuation rows (desc but no amount) into
    ///      the previous row.
    ///
    /// Output slots into existing XLSX layout config &mdash; "Date in column A" etc.
    /// works identically because we've produced columns A, B, C, D... left to
    /// right.
    /// </summary>
    public static class PdfStatementExtractor
    {
        // Span picked up from a PDF page.
        private class Span
        {
            public int    PageNum;
            public double X;        // left edge
            public double Y;        // bottom edge (PDF origin is bottom-left)
            public double Width;
            public string Text;
        }

        // ── Public API ────────────────────────────────────────────────

        /// <summary>Extract a row-grid from a PDF. Returns empty list if nothing parseable.
        ///
        /// Strategy (simplified, after over-clever two-pass attempt broke things):
        ///   1. Cluster column boundaries using ALL spans. This picks up the
        ///      real data table's columns reliably.
        ///   2. Build grid for ALL rows found on all pages.
        ///   3. Merge continuation lines (description wraps) into their parent row.
        ///   4. Filter: only keep rows that contain a date-like cell. This drops
        ///      the meta-info box at the top (Customer ID, Branch, Address, etc.)
        ///      AND the page-footer lines (e.g., "Closing Balance") that have no date.
        ///
        /// This prioritises NOT losing transaction data over perfectly clean
        /// column clustering. Meta-box may cause clustering to find an extra
        /// column or two; transaction data will still land in correct columns.</summary>
        public static List<List<string>> Extract(byte[] pdfBytes)
        {
            var allSpans = CollectAllSpans(pdfBytes);
            if (allSpans.Count == 0) return new List<List<string>>();

            // Column boundaries from all spans — includes meta-box columns but
            // the data table's own columns dominate if there are enough rows.
            var columnBoundaries = DiscoverColumnBoundaries(allSpans);
            if (columnBoundaries.Count == 0) return new List<List<string>>();

            var rowsOfSpans = GroupSpansIntoRows(allSpans);
            var grid = new List<List<string>>();
            foreach (var rowSpans in rowsOfSpans)
            {
                var row = BuildRow(rowSpans, columnBoundaries);
                if (row != null) grid.Add(row);
            }

            // Merge continuation lines BEFORE filtering — so a date-row with its
            // continuation absorbs the orphan text.
            MergeOrphanContinuations(grid);

            // Final filter: only rows that have a date survive. This cleanly
            // drops the meta-info box (has no dates), column headers ("Date",
            // "Particulars" etc. — no dates), and footer lines ("Closing Balance"
            // — no date).
            var filtered = new List<List<string>>();
            foreach (var row in grid)
            {
                if (row.Any(cell => LooksLikeDate(cell))) filtered.Add(row);
            }

            // If the date filter removed EVERYTHING (e.g., PDF uses a date
            // format my regex doesn't catch), return the unfiltered grid so
            // user has something to diagnose with.
            return filtered.Count > 0 ? filtered : grid;
        }

        /// <summary>Flatten the row-grid into a single block of text &mdash;
        /// used by the bank-signature auto-detect so it can search for
        /// "HDFC BANK" etc. in the PDF.</summary>
        public static string ExtractPlainText(byte[] pdfBytes)
        {
            var spans = CollectAllSpans(pdfBytes);
            return string.Join(" ", spans.Select(s => s.Text));
        }

        // ── Page walking ──────────────────────────────────────────────

        /// <summary>Open the PDF and gather every text span with its position.</summary>
        private static List<Span> CollectAllSpans(byte[] pdfBytes)
        {
            var spans = new List<Span>();
            using (var stream = new MemoryStream(pdfBytes))
            using (var reader = new PdfReader(stream))
            using (var doc = new PdfDocument(reader))
            {
                int pageCount = doc.GetNumberOfPages();
                for (int p = 1; p <= pageCount; p++)
                {
                    var listener = new SpanListener(p);
                    var processor = new PdfCanvasProcessor(listener);
                    processor.ProcessPageContent(doc.GetPage(p));
                    spans.AddRange(listener.Spans);
                }
            }
            return spans;
        }

        // iText event listener &mdash; records every text chunk encountered.
        private class SpanListener : IEventListener
        {
            public List<Span> Spans = new List<Span>();
            private readonly int _page;

            public SpanListener(int page) { _page = page; }

            public void EventOccurred(IEventData data, EventType type)
            {
                if (type != EventType.RENDER_TEXT) return;
                var info = (TextRenderInfo)data;
                string t = info.GetText();
                if (string.IsNullOrWhiteSpace(t)) return;

                Vector bl = info.GetBaseline().GetStartPoint();   // bottom-left origin
                Vector br = info.GetBaseline().GetEndPoint();

                Spans.Add(new Span
                {
                    PageNum = _page,
                    X       = bl.Get(Vector.I1),
                    Y       = bl.Get(Vector.I2),
                    Width   = br.Get(Vector.I1) - bl.Get(Vector.I1),
                    Text    = t.Trim()
                });
            }

            public ICollection<EventType> GetSupportedEvents()
            {
                return new List<EventType> { EventType.RENDER_TEXT };
            }
        }

        // ── Column discovery ──────────────────────────────────────────

        /// <summary>Cluster all spans' left-edge X values to discover column starts.
        /// Uses a simple 1D threshold-gap clustering: sort X values, walk them,
        /// start a new cluster whenever the gap exceeds a threshold.</summary>
        private static List<double> DiscoverColumnBoundaries(List<Span> spans)
        {
            // Group X values by rounding to nearest 2pt, then count occurrences.
            // Columns are bins with many hits (= many rows share this left edge).
            var xHits = new Dictionary<int, int>();
            foreach (var s in spans)
            {
                int key = (int)Math.Round(s.X / 2.0) * 2;   // 2pt bucket
                if (!xHits.ContainsKey(key)) xHits[key] = 0;
                xHits[key]++;
            }

            // Keep only buckets with enough hits to suggest a real column
            // (at least 3 spans aligned here). Small PDFs might need lower,
            // but 3 is a reasonable floor.
            const int minHitsForColumn = 5;
            var popularXs = xHits
                .Where(kv => kv.Value >= minHitsForColumn)
                .Select(kv => (double)kv.Key)
                .OrderBy(x => x)
                .ToList();

            if (popularXs.Count == 0)
            {
                // Very small PDF? Fall back to every distinct X.
                popularXs = xHits.Keys.Select(k => (double)k).OrderBy(x => x).ToList();
            }

            // Merge near-duplicates. Description columns in particular have
            // sub-pixel X-drift across rows that creates many phantom "columns".
            // Using a wider merge window (30pt) coalesces these into one real column.
            const double mergeWithin = 30.0;
            var merged = new List<double>();
            foreach (var x in popularXs)
            {
                if (merged.Count == 0 || x - merged[merged.Count - 1] > mergeWithin)
                    merged.Add(x);
            }
            return merged;
        }

        // ── Row grouping ──────────────────────────────────────────────

        /// <summary>Group spans with very close Y-coordinates into rows.
        /// Uses a tolerance (default 2pt) on a per-page basis so rows near a
        /// page break aren't accidentally merged.</summary>
        private static List<List<Span>> GroupSpansIntoRows(List<Span> spans)
        {
            // Sort: page ascending, then Y descending (PDF Y grows upward, so
            // top of page = high Y, but we want reading order = top first).
            var sorted = spans
                .OrderBy(s => s.PageNum)
                .ThenByDescending(s => s.Y)
                .ThenBy(s => s.X)
                .ToList();

            var rows = new List<List<Span>>();
            List<Span> current = null;
            int currentPage = -1;
            double currentY = double.NaN;
            const double yTolerance = 2.0;

            foreach (var s in sorted)
            {
                bool newRow =
                    current == null ||
                    s.PageNum != currentPage ||
                    Math.Abs(s.Y - currentY) > yTolerance;

                if (newRow)
                {
                    current = new List<Span>();
                    rows.Add(current);
                    currentPage = s.PageNum;
                    currentY = s.Y;
                }
                current.Add(s);
            }

            // Within each row, sort by X so columns land left-to-right.
            foreach (var row in rows) row.Sort((a, b) => a.X.CompareTo(b.X));
            return rows;
        }

        // ── Row → column string array ────────────────────────────────

        /// <summary>Assign each span in a row to its column bucket based on X
        /// position, concatenate spans that land in the same bucket, and
        /// produce one string per column. Returns null if the row looks like
        /// noise (empty everywhere).</summary>
        private static List<string> BuildRow(List<Span> rowSpans, List<double> columnBoundaries)
        {
            var cols = new List<string>();
            for (int i = 0; i < columnBoundaries.Count; i++) cols.Add("");

            foreach (var s in rowSpans)
            {
                int colIdx = FindColumnIndex(s.X, columnBoundaries);
                if (colIdx < 0) continue;
                if (cols[colIdx].Length > 0) cols[colIdx] += " " + s.Text;
                else                          cols[colIdx]  = s.Text;
            }

            // Trim everything and drop if row is entirely empty.
            for (int i = 0; i < cols.Count; i++) cols[i] = cols[i].Trim();
            if (cols.All(c => c.Length == 0)) return null;
            return cols;
        }

        /// <summary>Find which column bucket an X coordinate belongs to.
        /// Returns the index of the largest boundary &le; x.</summary>
        private static int FindColumnIndex(double x, List<double> boundaries)
        {
            if (boundaries.Count == 0) return -1;
            int idx = -1;
            for (int i = 0; i < boundaries.Count; i++)
            {
                if (boundaries[i] <= x + 0.5) idx = i;
                else break;
            }
            return idx;
        }

        // ── Post-processing: merge multi-line descriptions ────────────

        /// <summary>Banks often split a long description across 2-3 lines.
        /// A continuation row has text but no date and no amount-like value.
        /// Merge it into the previous row, concatenating all non-empty cells
        /// into whichever cell in the previous row looks most like "description"
        /// (first non-date, non-numeric cell).</summary>
        private static void MergeOrphanContinuations(List<List<string>> grid)
        {
            int i = 0;
            while (i < grid.Count)
            {
                var row = grid[i];
                bool hasDate   = row.Any(c => LooksLikeDate(c));
                bool hasAmount = row.Any(c => LooksLikeAmount(c));
                bool hasAnyText = row.Any(c => !string.IsNullOrEmpty(c));

                if (!hasDate && !hasAmount && hasAnyText && i > 0)
                {
                    var prev = grid[i - 1];
                    var extras = row.Where(s => !string.IsNullOrEmpty(s)).ToList();

                    if (extras.Count > 0)
                    {
                        string addendum = string.Join(" ", extras);

                        // Find the prev row's "description column" — first column
                        // that is neither date-like nor amount-like. Usually col 1 (B).
                        int descColIdx = -1;
                        for (int c = 0; c < prev.Count; c++)
                        {
                            var txt = prev[c];
                            if (string.IsNullOrEmpty(txt)) continue;
                            if (LooksLikeDate(txt) || LooksLikeAmount(txt)) continue;
                            descColIdx = c;
                            break;
                        }

                        // If the prev row has no description column yet, create
                        // one in the column immediately after the date cell.
                        if (descColIdx < 0)
                        {
                            for (int c = 0; c < prev.Count; c++)
                            {
                                if (!string.IsNullOrEmpty(prev[c]) && LooksLikeDate(prev[c]))
                                {
                                    if (c + 1 < prev.Count) { descColIdx = c + 1; break; }
                                }
                            }
                        }

                        // Last-resort fallback: use column 1 (second column), or 0 if row has only one column.
                        if (descColIdx < 0) descColIdx = prev.Count > 1 ? 1 : 0;

                        if (!string.IsNullOrEmpty(prev[descColIdx]))
                            prev[descColIdx] = prev[descColIdx] + " " + addendum;
                        else
                            prev[descColIdx] = addendum;
                    }

                    grid.RemoveAt(i);
                    continue;
                }
                i++;
            }
        }

        // ── Tiny heuristic helpers (also exposed for the parser) ──────

        public static bool LooksLikeDate(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            // Very loose: dd/mm/yyyy, dd-mm-yyyy, dd MMM yyyy, dd.mm.yyyy
            return System.Text.RegularExpressions.Regex.IsMatch(
                text,
                @"^\s*\d{1,2}[\/\-\.\s][A-Za-z0-9]{1,4}[\/\-\.\s]\d{2,4}\s*$");
        }

        public static bool LooksLikeAmount(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            // Numeric with optional comma separators, optional decimals, optional CR/DR suffix
            return System.Text.RegularExpressions.Regex.IsMatch(
                text,
                @"^\s*\d[\d,]*\.?\d*\s*(CR|DR|Cr|Dr)?\s*$");
        }
    }
}
