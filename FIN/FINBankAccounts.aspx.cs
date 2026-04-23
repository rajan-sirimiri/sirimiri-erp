using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    /// <summary>
    /// Bank Accounts master CRUD + XLSX layout editor.
    /// Finance role required. Links to Zoho Books Chart of Accounts so postings
    /// can use the correct ledger as Dr/Cr.
    /// </summary>
    public partial class FINBankAccounts : System.Web.UI.Page
    {
        // Nav + alerts
        protected Label lblNavUser, lblFormTitle, lblAlert, lblCount;
        protected HtmlGenericControl alertBox;
        protected Panel pnlAlert, pnlEmpty, pnlLayout;

        // Form state
        protected HiddenField hfBankID;
        protected TextBox txtCode, txtName, txtAcctNo, txtBranch;
        protected DropDownList ddlZohoAccount;

        // Layout fields
        protected TextBox txtHeaderRow, txtFirstData, txtDateCol, txtDateFormat;
        protected TextBox txtDescCol, txtRefCol;
        protected RadioButton rbModeTwoCol, rbModeFlag, rbModeSigned;
        protected TextBox txtDebitCol, txtCreditCol, txtAmountCol, txtFlagCol, txtBalanceCol;
        protected TextBox txtSignatureText, txtSignatureRows;

        // PDF layout fields (parallel to XLSX)
        protected TextBox txtPdfHeaderRow, txtPdfFirstData, txtPdfDateCol, txtPdfDateFormat;
        protected TextBox txtPdfDescCol, txtPdfRefCol;
        protected RadioButton rbPdfModeTwoCol, rbPdfModeFlag, rbPdfModeSigned;
        protected TextBox txtPdfDebitCol, txtPdfCreditCol, txtPdfAmountCol, txtPdfFlagCol, txtPdfBalanceCol;
        protected Button btnSaveLayoutXlsx, btnSaveLayoutPdf, btnSaveSignature;
        protected FileUpload fuSamplePdf;
        protected Button btnAnalyzePdf;
        protected Panel pnlPdfPreview;
        protected Literal litPdfPreview;

        // Buttons
        protected Button btnSave, btnClear, btnToggleActive;

        // List
        protected Repeater rptBanks;

        private string UserRole  => Session["FIN_Role"]?.ToString() ?? "";
        private bool   IsFinance => FINConsignments.IsFinanceRole(UserRole);
        private int    CurrentUserID => Convert.ToInt32(Session["FIN_UserID"] ?? 0);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }
            if (!IsFinance)
            {
                Response.Redirect("FINHome.aspx");
                return;
            }

            if (lblNavUser != null)
                lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                LoadZohoAccounts();
                LoadBanks();
                SetFormMode(false);
            }
        }

        // ── Zoho Chart of Accounts dropdown ───────────────────────────

        private void LoadZohoAccounts()
        {
            DataTable dt = FINDatabaseHelper.GetChartOfAccounts(activeOnly: true);
            ddlZohoAccount.Items.Clear();
            ddlZohoAccount.Items.Add(new ListItem("-- Select Zoho account --", ""));
            foreach (DataRow r in dt.Rows)
            {
                string zid   = r["ZohoAccountID"].ToString();
                string zname = r["AccountName"].ToString();
                string atype = r["AccountType"] == DBNull.Value ? "" : r["AccountType"].ToString();
                // Only surface bank / cash / credit-card type accounts (accounts suitable as a bank posting target)
                string lowerType = atype.ToLowerInvariant();
                if (!(lowerType.Contains("bank") || lowerType.Contains("cash") || lowerType.Contains("credit")))
                    continue;
                ddlZohoAccount.Items.Add(new ListItem(zname + "  (" + atype + ")", zid + "|" + zname));
            }
        }

        // ── List ──────────────────────────────────────────────────────

        private void LoadBanks()
        {
            DataTable dt = FINDatabaseHelper.GetAllBankAccounts();
            if (dt.Rows.Count > 0)
            {
                rptBanks.DataSource = dt;
                rptBanks.DataBind();
                ((Panel)FindControl("pnlEmpty")).Visible = false;
                lblCount.Text = dt.Rows.Count + " bank" + (dt.Rows.Count == 1 ? "" : "s");
            }
            else
            {
                rptBanks.DataSource = null;
                rptBanks.DataBind();
                ((Panel)FindControl("pnlEmpty")).Visible = true;
                lblCount.Text = "0 banks";
            }
        }

        /// <summary>Used by the repeater to show per-format layout status:
        /// two small badges, one for XLSX, one for PDF.</summary>
        protected string RenderLayoutBadge(object bankIdObj)
        {
            if (bankIdObj == null || bankIdObj == DBNull.Value) return "";
            int bankId = Convert.ToInt32(bankIdObj);
            DataRow layout = FINDatabaseHelper.GetBankLayout(bankId);
            bool xlsxSet = layout != null
                && layout["IsConfigured"] != DBNull.Value
                && Convert.ToInt32(layout["IsConfigured"]) == 1;
            bool pdfSet = layout != null
                && layout.Table.Columns.Contains("PdfIsConfigured")
                && layout["PdfIsConfigured"] != DBNull.Value
                && Convert.ToInt32(layout["PdfIsConfigured"]) == 1;

            string xlsxBadge = xlsxSet
                ? "<span class='badge-layout yes' title='XLSX configured'>XLSX</span>"
                : "<span class='badge-layout no'  title='XLSX not set'>XLSX</span>";
            string pdfBadge = pdfSet
                ? "<span class='badge-layout yes' title='PDF configured'>PDF</span>"
                : "<span class='badge-layout no'  title='PDF not set'>PDF</span>";

            return xlsxBadge + " " + pdfBadge;
        }

        // ── Form helpers ──────────────────────────────────────────────

        private void SetFormMode(bool isEdit)
        {
            lblFormTitle.Text = isEdit ? "Edit Bank Account" : "New Bank Account";
            btnToggleActive.Visible = isEdit;
            pnlLayout.Visible = isEdit;
        }

        private void ShowAlert(string message, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = message;
            alertBox.Attributes["class"] = "alert " + (success ? "alert-success" : "alert-danger");
        }

        private void ClearForm()
        {
            hfBankID.Value = "0";
            txtCode.Text = "";
            txtName.Text = "";
            txtAcctNo.Text = "";
            txtBranch.Text = "";
            ddlZohoAccount.SelectedValue = "";
            ClearLayoutForm();
            SetFormMode(false);
        }

        private void ClearLayoutForm()
        {
            txtHeaderRow.Text = "1";
            txtFirstData.Text = "2";
            txtDateCol.Text = "";
            txtDateFormat.Text = "dd/MM/yyyy";
            txtDescCol.Text = "";
            txtRefCol.Text = "";
            rbModeTwoCol.Checked = true;
            rbModeFlag.Checked = false;
            rbModeSigned.Checked = false;
            txtDebitCol.Text = "";
            txtCreditCol.Text = "";
            txtAmountCol.Text = "";
            txtFlagCol.Text = "";
            txtBalanceCol.Text = "";
            txtSignatureText.Text = "";
            txtSignatureRows.Text = "15";

            // PDF side
            txtPdfHeaderRow.Text = "1";
            txtPdfFirstData.Text = "2";
            txtPdfDateCol.Text = "";
            txtPdfDateFormat.Text = "dd/MM/yyyy";
            txtPdfDescCol.Text = "";
            txtPdfRefCol.Text = "";
            rbPdfModeTwoCol.Checked = true;
            rbPdfModeFlag.Checked = false;
            rbPdfModeSigned.Checked = false;
            txtPdfDebitCol.Text = "";
            txtPdfCreditCol.Text = "";
            txtPdfAmountCol.Text = "";
            txtPdfFlagCol.Text = "";
            txtPdfBalanceCol.Text = "";
        }

        private void LoadLayoutForm(int bankId)
        {
            DataRow row = FINDatabaseHelper.GetBankLayout(bankId);
            if (row == null)
            {
                ClearLayoutForm();
                return;
            }
            txtHeaderRow.Text = row["HeaderRow"] == DBNull.Value ? "1" : row["HeaderRow"].ToString();
            txtFirstData.Text = row["FirstDataRow"] == DBNull.Value ? "2" : row["FirstDataRow"].ToString();
            txtDateCol.Text   = row["DateCol"]  == DBNull.Value ? "" : row["DateCol"].ToString();
            txtDateFormat.Text= row["DateFormat"] == DBNull.Value ? "dd/MM/yyyy" : row["DateFormat"].ToString();
            txtDescCol.Text   = row["DescCol"]  == DBNull.Value ? "" : row["DescCol"].ToString();
            txtRefCol.Text    = row["RefCol"]   == DBNull.Value ? "" : row["RefCol"].ToString();

            string mode = row["AmountMode"] == DBNull.Value ? "TWO_COL" : row["AmountMode"].ToString();
            rbModeTwoCol.Checked = (mode == "TWO_COL");
            rbModeFlag.Checked   = (mode == "FLAG");
            rbModeSigned.Checked = (mode == "SIGNED");

            txtDebitCol.Text   = row["DebitCol"]   == DBNull.Value ? "" : row["DebitCol"].ToString();
            txtCreditCol.Text  = row["CreditCol"]  == DBNull.Value ? "" : row["CreditCol"].ToString();
            txtAmountCol.Text  = row["AmountCol"]  == DBNull.Value ? "" : row["AmountCol"].ToString();
            txtFlagCol.Text    = row["FlagCol"]    == DBNull.Value ? "" : row["FlagCol"].ToString();
            txtBalanceCol.Text = row["BalanceCol"] == DBNull.Value ? "" : row["BalanceCol"].ToString();

            // Signature fields (added in migration 31)
            if (row.Table.Columns.Contains("SignatureText"))
                txtSignatureText.Text = row["SignatureText"] == DBNull.Value ? "" : row["SignatureText"].ToString();
            if (row.Table.Columns.Contains("SignatureScanRows"))
                txtSignatureRows.Text = row["SignatureScanRows"] == DBNull.Value ? "15" : row["SignatureScanRows"].ToString();

            // PDF-side fields (added in migration 32). Guard on column existence
            // so pre-migration rows degrade to defaults.
            if (row.Table.Columns.Contains("PdfHeaderRow"))
            {
                txtPdfHeaderRow.Text = row["PdfHeaderRow"] == DBNull.Value ? "1" : row["PdfHeaderRow"].ToString();
                txtPdfFirstData.Text = row["PdfFirstDataRow"] == DBNull.Value ? "2" : row["PdfFirstDataRow"].ToString();
                txtPdfDateCol.Text   = row["PdfDateCol"]   == DBNull.Value ? "" : row["PdfDateCol"].ToString();
                txtPdfDateFormat.Text= row["PdfDateFormat"]== DBNull.Value ? "dd/MM/yyyy" : row["PdfDateFormat"].ToString();
                txtPdfDescCol.Text   = row["PdfDescCol"]   == DBNull.Value ? "" : row["PdfDescCol"].ToString();
                txtPdfRefCol.Text    = row["PdfRefCol"]    == DBNull.Value ? "" : row["PdfRefCol"].ToString();
                string pdfMode = row["PdfAmountMode"] == DBNull.Value ? "TWO_COL" : row["PdfAmountMode"].ToString();
                rbPdfModeTwoCol.Checked = (pdfMode == "TWO_COL");
                rbPdfModeFlag.Checked   = (pdfMode == "FLAG");
                rbPdfModeSigned.Checked = (pdfMode == "SIGNED");
                txtPdfDebitCol.Text   = row["PdfDebitCol"]   == DBNull.Value ? "" : row["PdfDebitCol"].ToString();
                txtPdfCreditCol.Text  = row["PdfCreditCol"]  == DBNull.Value ? "" : row["PdfCreditCol"].ToString();
                txtPdfAmountCol.Text  = row["PdfAmountCol"]  == DBNull.Value ? "" : row["PdfAmountCol"].ToString();
                txtPdfFlagCol.Text    = row["PdfFlagCol"]    == DBNull.Value ? "" : row["PdfFlagCol"].ToString();
                txtPdfBalanceCol.Text = row["PdfBalanceCol"] == DBNull.Value ? "" : row["PdfBalanceCol"].ToString();
            }
        }

        // ── Save / clear / toggle ─────────────────────────────────────

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ShowAlert("Bank Name is required.", false);
                return;
            }

            // Decompose the composite Zoho selection (stored as "id|name" in the dropdown)
            string zohoId = "", zohoName = "";
            string sel = ddlZohoAccount.SelectedValue ?? "";
            if (!string.IsNullOrEmpty(sel))
            {
                int pipe = sel.IndexOf('|');
                if (pipe > 0)
                {
                    zohoId   = sel.Substring(0, pipe);
                    zohoName = sel.Substring(pipe + 1);
                }
            }

            int bankId = Convert.ToInt32(hfBankID.Value);

            try
            {
                if (bankId == 0)
                {
                    FINDatabaseHelper.AddBankAccount(name, txtAcctNo.Text.Trim(), txtBranch.Text.Trim(),
                                                     zohoId, zohoName, CurrentUserID);
                    ShowAlert("Bank '" + name + "' added. Now configure the XLSX column layout below.", true);
                }
                else
                {
                    FINDatabaseHelper.UpdateBankAccount(bankId, name, txtAcctNo.Text.Trim(), txtBranch.Text.Trim(),
                                                        zohoId, zohoName);
                    ShowAlert("Bank '" + name + "' updated.", true);
                }
                // Reload list. If edit-mode, keep the form populated so user can continue with layout.
                LoadBanks();
                if (bankId == 0)
                {
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlAlert.Visible = false;
        }

        protected void btnToggleActive_Click(object sender, EventArgs e)
        {
            int bankId = Convert.ToInt32(hfBankID.Value);
            if (bankId == 0) return;

            DataRow dr = FINDatabaseHelper.GetBankAccountById(bankId);
            if (dr == null) return;

            bool currentlyActive = Convert.ToBoolean(dr["IsActive"]);
            FINDatabaseHelper.ToggleBankAccountActive(bankId, !currentlyActive);
            ShowAlert("Bank " + (currentlyActive ? "deactivated" : "activated") + ".", true);
            ClearForm();
            LoadBanks();
        }

        // ── XLSX layout save ──────────────────────────────────────────

        protected void btnSaveLayoutXlsx_Click(object sender, EventArgs e)
        {
            int bankId = Convert.ToInt32(hfBankID.Value);
            if (bankId == 0)
            {
                ShowAlert("Save the bank first, then configure the layout.", false);
                return;
            }

            int headerRow, firstData;
            if (!int.TryParse(txtHeaderRow.Text.Trim(), out headerRow) || headerRow < 1) headerRow = 1;
            if (!int.TryParse(txtFirstData.Text.Trim(), out firstData)  || firstData  < 1) firstData  = headerRow + 1;

            string mode = rbModeFlag.Checked ? "FLAG"
                        : rbModeSigned.Checked ? "SIGNED"
                        : "TWO_COL";

            string validationErr = ValidateLayout(mode,
                txtDateCol.Text, txtDebitCol.Text, txtCreditCol.Text,
                txtAmountCol.Text, txtFlagCol.Text, "XLSX");
            if (validationErr != null) { ShowAlert(validationErr, false); return; }

            try
            {
                int scanRows;
                if (!int.TryParse(txtSignatureRows.Text.Trim(), out scanRows) || scanRows <= 0) scanRows = 15;
                FINDatabaseHelper.SaveBankLayoutXlsx(bankId, headerRow, firstData,
                    txtDateCol.Text, txtDescCol.Text, txtRefCol.Text,
                    mode,
                    txtDebitCol.Text, txtCreditCol.Text,
                    txtAmountCol.Text, txtFlagCol.Text,
                    txtBalanceCol.Text,
                    string.IsNullOrEmpty(txtDateFormat.Text) ? "dd/MM/yyyy" : txtDateFormat.Text.Trim(),
                    txtSignatureText.Text.Trim(),
                    scanRows);
                ShowAlert("XLSX layout saved.", true);
                LoadBanks();
            }
            catch (Exception ex)
            {
                ShowAlert("Error saving XLSX layout: " + ex.Message, false);
            }
        }

        // ── PDF layout save ───────────────────────────────────────────

        protected void btnSaveLayoutPdf_Click(object sender, EventArgs e)
        {
            int bankId = Convert.ToInt32(hfBankID.Value);
            if (bankId == 0)
            {
                ShowAlert("Save the bank first, then configure the layout.", false);
                return;
            }

            int headerRow, firstData;
            if (!int.TryParse(txtPdfHeaderRow.Text.Trim(), out headerRow) || headerRow < 1) headerRow = 1;
            if (!int.TryParse(txtPdfFirstData.Text.Trim(), out firstData)  || firstData  < 1) firstData  = headerRow + 1;

            string mode = rbPdfModeFlag.Checked ? "FLAG"
                        : rbPdfModeSigned.Checked ? "SIGNED"
                        : "TWO_COL";

            string validationErr = ValidateLayout(mode,
                txtPdfDateCol.Text, txtPdfDebitCol.Text, txtPdfCreditCol.Text,
                txtPdfAmountCol.Text, txtPdfFlagCol.Text, "PDF");
            if (validationErr != null) { ShowAlert(validationErr, false); return; }

            try
            {
                FINDatabaseHelper.SaveBankLayoutPdf(bankId, headerRow, firstData,
                    txtPdfDateCol.Text, txtPdfDescCol.Text, txtPdfRefCol.Text,
                    mode,
                    txtPdfDebitCol.Text, txtPdfCreditCol.Text,
                    txtPdfAmountCol.Text, txtPdfFlagCol.Text,
                    txtPdfBalanceCol.Text,
                    string.IsNullOrEmpty(txtPdfDateFormat.Text) ? "dd/MM/yyyy" : txtPdfDateFormat.Text.Trim());
                ShowAlert("PDF layout saved.", true);
                LoadBanks();
            }
            catch (Exception ex)
            {
                ShowAlert("Error saving PDF layout: " + ex.Message, false);
            }
        }

        // ── Signature save (shared between XLSX and PDF) ──────────────

        protected void btnSaveSignature_Click(object sender, EventArgs e)
        {
            int bankId = Convert.ToInt32(hfBankID.Value);
            if (bankId == 0)
            {
                ShowAlert("Save the bank first, then configure the signature.", false);
                return;
            }
            int scanRows;
            if (!int.TryParse(txtSignatureRows.Text.Trim(), out scanRows) || scanRows <= 0) scanRows = 15;
            try
            {
                FINDatabaseHelper.SaveBankSignature(bankId, txtSignatureText.Text.Trim(), scanRows);
                ShowAlert("Signature saved. Auto-detect will use this when a bank is not specified at upload.", true);
                LoadBanks();
            }
            catch (Exception ex)
            {
                ShowAlert("Error saving signature: " + ex.Message, false);
            }
        }

        /// <summary>Shared layout validation. Returns error string, or null on success.</summary>
        private static string ValidateLayout(string mode,
                                              string dateCol, string debCol, string crdCol,
                                              string amtCol, string flagCol, string fmtLabel)
        {
            if (string.IsNullOrEmpty(dateCol))
                return fmtLabel + ": Date column is required.";
            if (mode == "TWO_COL" && (string.IsNullOrEmpty(debCol) || string.IsNullOrEmpty(crdCol)))
                return fmtLabel + ": TWO_COL mode requires both Debit and Credit column letters.";
            if (mode == "FLAG" && (string.IsNullOrEmpty(amtCol) || string.IsNullOrEmpty(flagCol)))
                return fmtLabel + ": FLAG mode requires Amount and Flag column letters.";
            if (mode == "SIGNED" && string.IsNullOrEmpty(amtCol))
                return fmtLabel + ": SIGNED mode requires the Amount column letter.";
            return null;
        }

        // ── Edit from list ────────────────────────────────────────────

        protected void rptBanks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int bankId = Convert.ToInt32(e.CommandArgument);
                DataRow dr = FINDatabaseHelper.GetBankAccountById(bankId);
                if (dr == null) return;

                hfBankID.Value  = bankId.ToString();
                txtCode.Text    = dr["BankCode"].ToString();
                txtName.Text    = dr["BankName"].ToString();
                txtAcctNo.Text  = dr["AccountNumber"] == DBNull.Value ? "" : dr["AccountNumber"].ToString();
                txtBranch.Text  = dr["Branch"]        == DBNull.Value ? "" : dr["Branch"].ToString();

                string zid   = dr["ZohoAccountID"]   == DBNull.Value ? "" : dr["ZohoAccountID"].ToString();
                string zname = dr["ZohoAccountName"] == DBNull.Value ? "" : dr["ZohoAccountName"].ToString();
                string want  = string.IsNullOrEmpty(zid) ? "" : zid + "|" + zname;
                ListItem sel = ddlZohoAccount.Items.FindByValue(want);
                if (sel != null) ddlZohoAccount.SelectedValue = want;
                else ddlZohoAccount.SelectedValue = "";

                bool isActive = Convert.ToBoolean(dr["IsActive"]);
                btnToggleActive.Text = isActive ? "Deactivate" : "Activate";

                LoadLayoutForm(bankId);
                SetFormMode(true);
                pnlAlert.Visible = false;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  PDF SAMPLE PREVIEW — extract columns from an uploaded sample
        //  PDF so the user can see how to set the PDF layout fields.
        //  No DB writes, no parsing of amounts; pure raw extraction view.
        // ══════════════════════════════════════════════════════════════

        protected void btnAnalyzePdf_Click(object sender, EventArgs e)
        {
            if (!fuSamplePdf.HasFile)
            {
                ShowAlert("Choose a sample PDF to preview.", false);
                return;
            }
            if (!fuSamplePdf.FileName.ToLowerInvariant().EndsWith(".pdf"))
            {
                ShowAlert("Only .pdf files work for the sample preview.", false);
                return;
            }

            try
            {
                byte[] fileBytes = fuSamplePdf.FileBytes;
                List<List<string>> grid = PdfStatementExtractor.Extract(fileBytes);

                if (grid == null || grid.Count == 0)
                {
                    ShowAlert("Couldn't extract any rows from this PDF. It may be a scanned image rather than native-text PDF.", false);
                    pnlPdfPreview.Visible = false;
                    return;
                }

                litPdfPreview.Text = RenderPreviewTable(grid);
                pnlPdfPreview.Visible = true;
                ShowAlert("Preview generated: " + grid.Count + " rows, "
                    + (grid[0] == null ? 0 : grid[0].Count) + " columns. Use the letters below to fill the PDF layout fields.", true);
            }
            catch (Exception ex)
            {
                ShowAlert("Preview failed: " + ex.Message, false);
                pnlPdfPreview.Visible = false;
            }
        }

        /// <summary>Render a PDF-extracted grid as an HTML table with column-letter
        /// headers (A, B, C…). Only shows the first 20 rows to keep the page manageable.</summary>
        private static string RenderPreviewTable(List<List<string>> grid)
        {
            int maxCols = 0;
            foreach (var row in grid) if (row != null && row.Count > maxCols) maxCols = row.Count;
            int previewCount = Math.Min(grid.Count, 20);

            var sb = new System.Text.StringBuilder();
            sb.Append("<table style=\"width:100%;border-collapse:collapse;font-size:11px;font-family:\'Roboto Mono\',monospace;\">");

            // Header with column letters
            sb.Append("<thead><tr style=\"background:#f0f0f0;\">");
            sb.Append("<th style=\"padding:6px 8px;text-align:right;width:30px;color:var(--text-dim);border-bottom:1px solid var(--border);\">#</th>");
            for (int c = 0; c < maxCols; c++)
            {
                sb.Append("<th style=\"padding:6px 8px;text-align:center;color:var(--accent);font-weight:700;border-bottom:1px solid var(--border);min-width:80px;\">");
                sb.Append(ColIndexToLetter(c + 1));
                sb.Append("</th>");
            }
            sb.Append("</tr></thead>");

            // Body
            sb.Append("<tbody>");
            for (int r = 0; r < previewCount; r++)
            {
                var row = grid[r];
                sb.Append("<tr>");
                sb.Append("<td style=\"padding:5px 8px;text-align:right;color:var(--text-dim);border-bottom:1px solid #f5f5f5;\">")
                  .Append(r + 1)
                  .Append("</td>");
                for (int c = 0; c < maxCols; c++)
                {
                    string val = (row != null && c < row.Count) ? row[c] : "";
                    sb.Append("<td style=\"padding:5px 8px;border-bottom:1px solid #f5f5f5;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;max-width:200px;\">")
                      .Append(System.Web.HttpUtility.HtmlEncode(val ?? ""))
                      .Append("</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");

            if (grid.Count > previewCount)
            {
                sb.Append("<tfoot><tr><td colspan=\"")
                  .Append(maxCols + 1)
                  .Append("\" style=\"padding:8px;text-align:center;color:var(--text-dim);font-style:italic;\">")
                  .Append("&hellip; " + (grid.Count - previewCount) + " more rows not shown")
                  .Append("</td></tr></tfoot>");
            }

            sb.Append("</table>");
            return sb.ToString();
        }

        /// <summary>Convert 1-based column index to Excel-style letter (1→A, 2→B, 27→AA).</summary>
        private static string ColIndexToLetter(int idx)
        {
            if (idx < 1) return "";
            var sb = new System.Text.StringBuilder();
            while (idx > 0)
            {
                int rem = (idx - 1) % 26;
                sb.Insert(0, (char)('A' + rem));
                idx = (idx - 1) / 26;
            }
            return sb.ToString();
        }

    }
}
