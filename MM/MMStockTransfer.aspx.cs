using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    /// <summary>
    /// Stock Transfer (Store -> Floor) — unified page covering PM/CN/ST.
    /// Save flow:
    ///   1. JS validates header + lines, packs lines into hfLineItems JSON
    ///   2. JS calls __doPostBack('SAVE_TRANSFER', '')
    ///   3. Page_Load detects the event target and routes to ProcessSave()
    ///   4. ProcessSave parses JSON, calls MMDatabaseHelper.AddStockTransfer
    /// </summary>
    public partial class MMStockTransfer : Page
    {
        // Designer fields
        protected Label        lblUser;
        protected TextBox      txtTransferDate;
        protected DropDownList ddlFromLocation;
        protected DropDownList ddlToLocation;
        protected TextBox      txtRequestedBy;
        protected TextBox      txtRemarks;
        protected HiddenField  hfLineItems;
        protected Button       btnSavePostback;
        protected TextBox      txtFilterFrom;
        protected TextBox      txtFilterTo;
        protected Repeater     rptTransfers;
        protected Panel        pnlEmpty;

        // Material catalog JSON properties used in markup
        public string PMOptionsJson { get; private set; }
        public string CNOptionsJson { get; private set; }
        public string STOptionsJson { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Auth
            if (Session["MM_UserID"] == null)
            {
                Response.Redirect("../StockApp/Login.aspx");
                return;
            }

            // Build material catalog JSON (always — needed every render)
            PMOptionsJson = BuildOptionsJson(MMDatabaseHelper.GetActivePackingMaterials(),
                                              "PMID", "PMName", "Abbreviation");
            CNOptionsJson = BuildOptionsJson(MMDatabaseHelper.GetActiveConsumables(),
                                              "ConsumableID", "ConsumableName", "Abbreviation");
            STOptionsJson = BuildOptionsJson(MMDatabaseHelper.GetActiveStationaryForTransfer(),
                                              "StationaryID", "StationaryName", "Abbreviation");

            if (!IsPostBack)
            {
                lblUser.Text = (Session["MM_UserName"] ?? "").ToString();

                // Default dates: today
                txtTransferDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

                // Default filter range: last 14 days through today
                txtFilterFrom.Text = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd");
                txtFilterTo.Text   = DateTime.Now.ToString("yyyy-MM-dd");

                // Populate Location dropdowns
                LoadLocations();

                // Default From=STORES, To=FLOOR (the common case)
                int storesId = MMDatabaseHelper.GetLocationIdByCode("STORES");
                int floorId  = MMDatabaseHelper.GetLocationIdByCode("FLOOR");
                if (storesId > 0) ddlFromLocation.SelectedValue = storesId.ToString();
                if (floorId  > 0) ddlToLocation.SelectedValue   = floorId.ToString();

                LoadTransferList();
            }

            // Detect the JS-triggered SAVE_TRANSFER postback
            string target = Request["__EVENTTARGET"] ?? "";
            if (target == "SAVE_TRANSFER")
            {
                ProcessSave();
            }
        }

        private void LoadLocations()
        {
            DataTable dt = MMDatabaseHelper.GetActiveLocations();
            ddlFromLocation.DataSource = dt;
            ddlFromLocation.DataValueField = "LocationID";
            ddlFromLocation.DataTextField  = "LocationName";
            ddlFromLocation.DataBind();

            ddlToLocation.DataSource = dt;
            ddlToLocation.DataValueField = "LocationID";
            ddlToLocation.DataTextField  = "LocationName";
            ddlToLocation.DataBind();
        }

        private void LoadTransferList()
        {
            DateTime from, to;
            if (!DateTime.TryParse(txtFilterFrom.Text, out from)) from = DateTime.Now.AddDays(-14);
            if (!DateTime.TryParse(txtFilterTo.Text,   out to))   to   = DateTime.Now;

            DataTable dt = MMDatabaseHelper.GetStockTransferList(from, to);
            if (dt != null && dt.Rows.Count > 0)
            {
                rptTransfers.DataSource = dt;
                rptTransfers.DataBind();
                pnlEmpty.Visible = false;
            }
            else
            {
                rptTransfers.DataSource = null;
                rptTransfers.DataBind();
                pnlEmpty.Visible = true;
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadTransferList();
        }

        protected void btnSavePostback_Click(object sender, EventArgs e)
        {
            // Reserved for any non-JS-driven save path. Not currently used.
        }

        // ── Save handler invoked from JS via __doPostBack('SAVE_TRANSFER',…) ──
        private void ProcessSave()
        {
            try
            {
                // Parse header
                DateTime transferDate;
                if (!DateTime.TryParse(txtTransferDate.Text, out transferDate))
                {
                    ShowAlert("Invalid transfer date.", false);
                    return;
                }
                int fromId = Convert.ToInt32(ddlFromLocation.SelectedValue);
                int toId   = Convert.ToInt32(ddlToLocation.SelectedValue);
                if (fromId == 0 || toId == 0 || fromId == toId)
                {
                    ShowAlert("From and To locations are invalid.", false);
                    return;
                }

                // Determine transfer type from location types (Store->Floor or Floor->Store)
                string transferType = "STORE_TO_FLOOR";
                // (Phase A: only one Store and one Floor; type derived from the name pair)
                if (fromId == MMDatabaseHelper.GetLocationIdByCode("FLOOR")
                    && toId == MMDatabaseHelper.GetLocationIdByCode("STORES"))
                    transferType = "FLOOR_TO_STORE";

                string requestedBy = (txtRequestedBy.Text ?? "").Trim();
                string remarks     = (txtRemarks.Text ?? "").Trim();
                int issuedBy       = Convert.ToInt32(Session["MM_UserID"]);

                // Parse line items JSON
                string json = (hfLineItems.Value ?? "[]").Trim();
                var lines = ParseLineItemsJson(json);
                if (lines.Count == 0)
                {
                    ShowAlert("No line items to save.", false);
                    return;
                }

                // Generate transfer number
                string transferNo = MMDatabaseHelper.GenerateTransferNumber();

                // Save
                int newId = MMDatabaseHelper.AddStockTransfer(
                    transferNo, transferDate, transferType,
                    fromId, toId, requestedBy, issuedBy, remarks,
                    lines);

                ShowAlert("Transfer " + transferNo + " saved with " + lines.Count + " line item(s).", true);

                // Reset form
                hfLineItems.Value = "[]";
                txtRequestedBy.Text = "";
                txtRemarks.Text = "";
                LoadTransferList();
            }
            catch (Exception ex)
            {
                ShowAlert("Save failed: " + ex.Message, false);
            }
        }

        // ── Helpers ──

        private void ShowAlert(string msg, bool isSuccess)
        {
            string css = isSuccess ? "alert show success" : "alert show error";
            string esc = (msg ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
            string js = "var el=document.getElementById('alertBox');if(el){el.textContent=\"" + esc + "\";el.className=\"" + css + "\";window.scrollTo(0,0);}";
            ScriptManager.RegisterStartupScript(this, GetType(), "alert" + Guid.NewGuid().ToString("N"), js, true);
        }

        private string BuildOptionsJson(DataTable dt, string idCol, string nameCol, string uomCol)
        {
            if (dt == null || dt.Rows.Count == 0) return "[]";
            var sb = new StringBuilder("[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(",");
                DataRow r = dt.Rows[i];
                sb.Append("{");
                sb.Append("\"id\":").Append(Convert.ToInt32(r[idCol]));
                sb.Append(",\"name\":\"").Append(EscJson(r[nameCol].ToString())).Append("\"");
                if (dt.Columns.Contains(uomCol) && r[uomCol] != DBNull.Value)
                    sb.Append(",\"uom\":\"").Append(EscJson(r[uomCol].ToString())).Append("\"");
                sb.Append("}");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private static string EscJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length + 8);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\b': sb.Append("\\b");  break;
                    case '\f': sb.Append("\\f");  break;
                    case '\n': sb.Append("\\n");  break;
                    case '\r': sb.Append("\\r");  break;
                    case '\t': sb.Append("\\t");  break;
                    default:
                        if (c < 0x20) sb.AppendFormat("\\u{0:x4}", (int)c);
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        // Lightweight JSON parser for the lines array — avoids depending on JavaScriptSerializer.
        // Expected shape: [{"type":"PM","matId":12,"qty":3.5}, ...]
        // (ConsumedAtIssue is now resolved server-side from each material's ConsumptionMode)
        private List<object[]> ParseLineItemsJson(string json)
        {
            var result = new List<object[]>();
            if (string.IsNullOrWhiteSpace(json) || json == "[]") return result;

            // Use System.Web.Script.Serialization.JavaScriptSerializer (built into .NET 4.8)
            var ser = new System.Web.Script.Serialization.JavaScriptSerializer();
            var arr = ser.Deserialize<List<Dictionary<string, object>>>(json);
            foreach (var d in arr)
            {
                string type = d.ContainsKey("type") ? d["type"].ToString() : "";
                int matId = d.ContainsKey("matId") ? Convert.ToInt32(d["matId"]) : 0;
                decimal qty = d.ContainsKey("qty") ? Convert.ToDecimal(d["qty"], CultureInfo.InvariantCulture) : 0m;
                if (matId == 0 || qty <= 0) continue;
                // ConsumedAtIssue is resolved by the DAL from each material's ConsumptionMode
                result.Add(new object[] { type, matId, qty, /*UOMID*/ (object)null, /*consumed (ignored)*/ false, /*Remarks*/ null });
            }
            return result;
        }
    }
}
