using System;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMMultiGRNPM : Page
    {
        protected Label lblNavUser;
        protected Panel pnlAlert, pnlPendingPanel, pnlPendingEmpty, pnlPendingList;
        protected Literal litAlert;
        protected DropDownList ddlSupplier;
        protected HiddenField hfLineItems, hfSupplierID;
        protected Button btnSave, btnSupplierTrigger;
        protected Repeater rptPending;

        // GRN History (multi-item only)
        protected TextBox txtFromDate, txtToDate;
        protected Button btnFilter;
        protected Repeater rptGRN;
        protected Panel pnlEmpty;
        protected Label lblCount;

        public string ItemDataJson  = "{}";
        public string ItemOptionsJson = "[]";
        public string UOMOptionsJson = "[]";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }
            string role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_PM_GRN"))
            { Response.Redirect("MMHome.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                LoadSuppliers();
                LoadPendingInvoices();
                txtFromDate.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
                txtToDate.Text   = DateTime.Today.ToString("yyyy-MM-dd");
                LoadGRNList();
            }
            else
            {
                // Rebind supplier dropdown on postback
                LoadSuppliers();
                string selSup = hfSupplierID.Value;
                if (!string.IsNullOrEmpty(selSup) && selSup != "0")
                {
                    var item = ddlSupplier.Items.FindByValue(selSup);
                    if (item != null) ddlSupplier.SelectedValue = selSup;
                }
                LoadPendingInvoices();
            }
            BuildItemJson();
            BuildItemOptionsJson();
            BuildUOMOptionsJson();
        }

        void LoadSuppliers()
        {
            ddlSupplier.Items.Clear();
            DataTable dt = MMDatabaseHelper.GetActiveSuppliers();
            ddlSupplier.DataSource = dt;
            ddlSupplier.DataTextField = "SupplierName";
            ddlSupplier.DataValueField = "SupplierID";
            ddlSupplier.DataBind();
            ddlSupplier.Items.Insert(0, new ListItem("-- Select Supplier --", "0"));
        }

        void BuildItemJson()
        {
            DataTable dt = MMDatabaseHelper.GetAllPackingMaterials();
            var sb = new StringBuilder("{");
            foreach (DataRow r in dt.Rows)
            {
                string hsn = r["HSNCode"] == DBNull.Value ? "" : EscapeJson(r["HSNCode"].ToString());
                string gst = r["GSTRate"] == DBNull.Value ? "" : EscapeJson(r["GSTRate"].ToString());
                string uom = EscapeJson(r["Abbreviation"].ToString());
                string uomId = r["UOMID"].ToString();
                sb.AppendFormat("\"{0}\":{{\"hsn\":\"{1}\",\"gst\":\"{2}\",\"uom\":\"{3}\",\"uomId\":\"{4}\"}},",
                    r["PMID"], hsn, gst, uom, uomId);
            }
            if (sb.Length > 1) sb.Length--;
            sb.Append("}");
            ItemDataJson = sb.ToString();
        }

        void BuildItemOptionsJson()
        {
            DataTable dt = MMDatabaseHelper.GetActivePackingMaterials();
            var sb = new StringBuilder("[");
            foreach (DataRow r in dt.Rows)
            {
                string name = EscapeJson(r["PMName"].ToString());
                sb.AppendFormat("{{\"id\":\"{0}\",\"name\":\"{1}\"}},", r["PMID"], name);
            }
            if (sb.Length > 1) sb.Length--;
            sb.Append("]");
            ItemOptionsJson = sb.ToString();
        }

        void BuildUOMOptionsJson()
        {
            DataTable dt = MMDatabaseHelper.GetActiveUOM();
            var sb = new StringBuilder("[");
            foreach (DataRow r in dt.Rows)
            {
                string abbr = EscapeJson(r["Abbreviation"].ToString());
                sb.AppendFormat("{{\"id\":\"{0}\",\"name\":\"{1}\"}},", r["UOMID"], abbr);
            }
            if (sb.Length > 1) sb.Length--;
            sb.Append("]");
            UOMOptionsJson = sb.ToString();
        }

        static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                    .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        void LoadPendingInvoices()
        {
            DataTable dt = MMDatabaseHelper.GetPendingInvoicePM();
            if (dt.Rows.Count > 0)
            {
                rptPending.DataSource = dt;
                rptPending.DataBind();
                pnlPendingList.Visible = true;
                pnlPendingEmpty.Visible = false;
            }
            else
            {
                pnlPendingList.Visible = false;
                pnlPendingEmpty.Visible = true;
            }
        }

        // ── GRN History (multi-item only) ────────────────────────
        void LoadGRNList()
        {
            DateTime from, to;
            if (!DateTime.TryParse(txtFromDate.Text, out from)) from = DateTime.Today.AddDays(-30);
            if (!DateTime.TryParse(txtToDate.Text,   out to))   to   = DateTime.Today;

            DataTable dt = MMDatabaseHelper.GetMultiItemPackingInwardList(from, to);
            if (dt != null && dt.Rows.Count > 0)
            {
                rptGRN.DataSource = dt;
                rptGRN.DataBind();
                pnlEmpty.Visible = false;
                lblCount.Text = dt.Rows.Count.ToString();
            }
            else
            {
                rptGRN.DataSource = null;
                rptGRN.DataBind();
                pnlEmpty.Visible = true;
                lblCount.Text = "0";
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadGRNList();
        }

        protected void btnSupplierTrigger_Click(object sender, EventArgs e)
        {
            // Kept for form compatibility — recoverables now loaded via AJAX
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string json = hfLineItems.Value;
            if (string.IsNullOrEmpty(json) || json == "[]")
            {
                ShowAlert("No items to save.", false);
                return;
            }

            try
            {
                // Parse JSON manually (no Newtonsoft dependency)
                var payload = ParsePayload(json);
                if (payload == null || payload.Items.Count == 0)
                {
                    ShowAlert("No valid line items found.", false);
                    return;
                }

                int supplierId = Convert.ToInt32(ddlSupplier.SelectedValue);
                if (supplierId == 0) { ShowAlert("Please select a supplier.", false); return; }

                DateTime grnDate;
                if (!DateTime.TryParse(payload.GrnDate, out grnDate))
                { ShowAlert("Invalid GRN Date.", false); return; }

                DateTime? invoiceDate = null;
                DateTime invDt;
                if (DateTime.TryParse(payload.InvoiceDate, out invDt)) invoiceDate = invDt;

                decimal totalTransport;
                decimal.TryParse(payload.Transport, out totalTransport);
                decimal totalLoading;
                decimal.TryParse(payload.Loading, out totalLoading);
                decimal totalUnloading;
                decimal.TryParse(payload.Unloading, out totalUnloading);
                bool transInInvoice = payload.TransInInvoice == "1";
                bool transGST = payload.TransGST == "1";

                // Calculate total line value for proportional allocation
                decimal totalLineValue = 0;
                foreach (var item in payload.Items)
                {
                    decimal qtyInv, rate;
                    decimal.TryParse(item.QtyInv, out qtyInv);
                    decimal.TryParse(item.Rate, out rate);
                    totalLineValue += qtyInv * rate;
                }

                int userId = Convert.ToInt32(Session["MM_UserID"]);
                int savedCount = 0;

                foreach (var item in payload.Items)
                {
                    int pmId = Convert.ToInt32(item.RmId);
                    decimal qtyInv, qtyAct, rate;
                    decimal.TryParse(item.QtyInv, out qtyInv);
                    decimal.TryParse(item.QtyAct, out qtyAct);
                    decimal.TryParse(item.Rate, out rate);

                    decimal? gstRate = null;
                    decimal gstParsed;
                    if (decimal.TryParse(item.Gst, out gstParsed)) gstRate = gstParsed;

                    // Proportional allocation of transport, loading, unloading
                    decimal lineValue = qtyInv * rate;
                    decimal proportion = (totalLineValue > 0) ? (lineValue / totalLineValue) : 0;
                    decimal lineTransport = Math.Round(totalTransport * proportion, 2);
                    decimal lineLoading   = Math.Round(totalLoading * proportion, 2);
                    decimal lineUnloading = Math.Round(totalUnloading * proportion, 2);

                    // Calculate GST and total for this line
                    decimal taxable = lineValue + (transInInvoice ? lineTransport : 0);
                    decimal gstBase = transGST ? taxable : lineValue;
                    decimal gstAmt = gstRate.HasValue ? Math.Round(gstBase * (gstRate.Value / 100), 2) : 0;
                    decimal total = taxable + gstAmt + (transInInvoice ? 0 : lineTransport) + lineLoading + lineUnloading;

                    string grnNo = MMDatabaseHelper.GenerateGRNNumber("PM");
                    bool qc = item.Qc == "1";

                    // Build remarks with supplier invoice qty if provided
                    string remarks = "Multi-GRN";
                    if (!string.IsNullOrEmpty(item.SupQty) && item.SupQty != "0")
                    {
                        string supUom = !string.IsNullOrEmpty(item.SupUom) ? " " + item.SupUom : "";
                        remarks = "[Supplier Invoice: " + item.SupQty + supUom + "] " + remarks;
                    }

                    // qtyInv = Standard Qty (stored as Quantity and QtyInUOM)
                    MMDatabaseHelper.AddPackingInward(
                        grnNo, grnDate, invoiceDate,
                        payload.InvoiceNo, supplierId, pmId,
                        qtyInv, qtyAct, qtyInv, rate,
                        item.Hsn, gstRate, gstAmt,
                        lineTransport, transInInvoice, transGST,
                        lineLoading, lineUnloading, true,
                        total, "", remarks,
                        qc, "Approved", userId);

                    savedCount++;
                }

                ShowAlert(savedCount + " GRN(s) saved successfully for invoice " +
                    (string.IsNullOrEmpty(payload.InvoiceNo) ? "(no invoice)" : payload.InvoiceNo) + ".", true);

                hfLineItems.Value = "[]";
                LoadPendingInvoices();

                // Reset line items but keep supplier, then reload recoverables
                string savedSupId = supplierId.ToString();
                ClientScript.RegisterStartupScript(this.GetType(), "afterSave",
                    "document.getElementById('tbodyItems').innerHTML='';" +
                    "document.getElementById('txtInvoiceNo').value='';" +
                    "document.getElementById('txtInvoiceDate').value='';" +
                    "document.getElementById('txtTransport').value='';" +
                    "document.getElementById('txtLoading').value='';" +
                    "document.getElementById('txtUnloading').value='';" +
                    "document.getElementById('chkTransInInvoice').checked=false;" +
                    "document.getElementById('chkTransGST').checked=false;" +
                    "var cb=document.getElementById('chkManualInvoice');if(cb)cb.checked=false;" +
                    "rowIdx=0;addRow();recalcAll();" +
                    "loadRecoverablesAjax('" + savedSupId + "');", true);
            }
            catch (Exception ex)
            {
                ShowAlert("Error saving: " + ex.Message, false);
            }
        }

        void ShowAlert(string msg, bool success)
        {
            string cls = success ? "alert-ok" : "alert-err";
            string icon = success ? "&#10003;" : "&#9888;";
            litAlert.Text = "<div class='" + cls + "'><strong>" + icon + "</strong> " +
                Server.HtmlEncode(msg) + "</div>";
            pnlAlert.Visible = true;
        }

        // ── Simple JSON parsing without Newtonsoft ──
        PayloadData ParsePayload(string json)
        {
            var result = new PayloadData();
            try
            {
                // Remove outer braces
                json = json.Trim();
                if (!json.StartsWith("{")) return null;

                result.InvoiceNo = ExtractString(json, "invoiceNo");
                result.InvoiceDate = ExtractString(json, "invoiceDate");
                result.GrnDate = ExtractString(json, "grnDate");
                result.Transport = ExtractString(json, "transport");
                result.Loading = ExtractString(json, "loading");
                result.Unloading = ExtractString(json, "unloading");
                result.TransInInvoice = ExtractString(json, "transInInvoice");
                result.TransGST = ExtractString(json, "transGST");

                // Extract items array
                int itemsStart = json.IndexOf("\"items\":[");
                if (itemsStart < 0) return result;
                itemsStart = json.IndexOf("[", itemsStart);
                int itemsEnd = json.LastIndexOf("]");
                string itemsJson = json.Substring(itemsStart + 1, itemsEnd - itemsStart - 1);

                // Split by },{
                string[] itemBlocks = itemsJson.Split(new[] { "},{" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string block in itemBlocks)
                {
                    string b = block.Trim().TrimStart('{').TrimEnd('}');
                    var li = new LineItemData
                    {
                        RmId = ExtractString("{" + b + "}", "rmId"),
                        QtyInv = ExtractString("{" + b + "}", "qtyInv"),
                        QtyAct = ExtractString("{" + b + "}", "qtyAct"),
                        SupQty = ExtractString("{" + b + "}", "supQty"),
                        SupUom = ExtractString("{" + b + "}", "supUom"),
                        Rate = ExtractString("{" + b + "}", "rate"),
                        Hsn = ExtractString("{" + b + "}", "hsn"),
                        Gst = ExtractString("{" + b + "}", "gst"),
                        Qc = ExtractString("{" + b + "}", "qc")
                    };
                    if (!string.IsNullOrEmpty(li.RmId) && li.RmId != "0")
                        result.Items.Add(li);
                }
            }
            catch { }
            return result;
        }

        string ExtractString(string json, string key)
        {
            string search = "\"" + key + "\":\"";
            int idx = json.IndexOf(search);
            if (idx < 0)
            {
                // Try without quotes (for numbers/booleans)
                search = "\"" + key + "\":";
                idx = json.IndexOf(search);
                if (idx < 0) return "";
                int start = idx + search.Length;
                int end = json.IndexOfAny(new[] { ',', '}' }, start);
                if (end < 0) end = json.Length;
                return json.Substring(start, end - start).Trim().Trim('"');
            }
            int valStart = idx + search.Length;
            int valEnd = json.IndexOf("\"", valStart);
            if (valEnd < 0) return "";
            return json.Substring(valStart, valEnd - valStart);
        }

        class PayloadData
        {
            public string InvoiceNo = "", InvoiceDate = "", GrnDate = "";
            public string Transport = "0", Loading = "0", Unloading = "0", TransInInvoice = "0", TransGST = "0";
            public System.Collections.Generic.List<LineItemData> Items = new System.Collections.Generic.List<LineItemData>();
        }

        class LineItemData
        {
            public string RmId, QtyInv, QtyAct, SupQty, SupUom, Rate, Hsn, Gst, Qc;
        }
    }
}
