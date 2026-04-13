using System;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMRawInward : Page
    {
        protected Label lblNavUser;
        protected Label lblGRNNo;
        protected Label lblAlert;
        protected Label lblCount;
        protected Label lblRecSupplier;
        protected Label lblRecTotal;
        protected HiddenField hfInwardID;
        protected HiddenField hfTaxable;
        protected HiddenField hfGSTAmount;
        protected HiddenField hfTotal;
        protected HiddenField hfLoading;
        protected HiddenField hfUnloading;
        protected HiddenField hfQtyVerified;
        protected Panel pnlAlert;
        protected Panel pnlEmpty;
        protected Panel pnlRecEmpty;
        protected Panel pnlRecList;
        protected DropDownList ddlRM;
        protected DropDownList ddlInvoiceUOM;
        protected DropDownList ddlReceivedUOM;
        protected DropDownList ddlStdUOM;
        protected DropDownList ddlSupplier;
        protected TextBox txtGRNDate;
        protected TextBox txtInvoiceNo;
        protected TextBox txtInvoiceDate;
        protected TextBox txtPONo;
        protected TextBox txtQtyInvoice;
        protected TextBox txtQtyReceived;
        protected TextBox txtQtyUOM;
        protected TextBox txtSupplierQty;
        protected DropDownList ddlStdInvoiceUOM;
        protected TextBox txtRate;
        protected TextBox txtHSN;
        protected TextBox txtGSTRate;
        protected TextBox txtTransport;
        protected TextBox txtRemarks;
        protected TextBox txtFromDate;
        protected TextBox txtToDate;
        protected CheckBox chkTransportInInvoice;
        protected CheckBox chkTransportInGST;
        protected CheckBox chkQC;
        protected Button btnReceive;
        protected Button btnReject;
        protected Button btnClear;
        protected Button btnFilter;
        protected Repeater rptGRN;
        protected Repeater rptRecoverables;
        protected Repeater rptPending;
        protected Panel pnlPendingEmpty, pnlPendingList, pnlInvoiceUpdate;
        protected Label lblPendingCount, lblEditGRN;
        protected HiddenField hfEditInwardId;
        protected TextBox txtEditInvoiceNo, txtEditInvoiceDate;
        protected Button btnUpdateInvoice, btnCancelInvoice;

        protected string RMDataJson = "{}"; // JSON for inline script
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            // Module access check
            string __role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(__role, "MM", "MM_RM_GRN"))
            { Response.Redirect("MMHome.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                LoadDropdowns();
                GenerateGRN();
                txtGRNDate.Text  = DateTime.Today.ToString("yyyy-MM-dd");
                txtFromDate.Text = DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd");
                txtToDate.Text   = DateTime.Today.ToString("yyyy-MM-dd");
                LoadGRNList();
                LoadRecoverables(0); // blank state
                LoadPendingInvoices();
            }
            else
            {
                BuildRMJson();
                // Save selected values before rebind
                string selectedSupplier = ddlSupplier.SelectedValue;
                string selectedRM = ddlRM.SelectedValue;
                // Re-bind supplier dropdown so FindByValue works in LoadRecoverables
                DataTable supDt = MMDatabaseHelper.GetActiveSuppliers();
                ddlSupplier.DataSource     = supDt;
                ddlSupplier.DataTextField  = "SupplierName";
                ddlSupplier.DataValueField = "SupplierID";
                ddlSupplier.DataBind();
                ddlSupplier.Items.Insert(0, new ListItem("-- Select Supplier --", "0"));
                // Restore selection
                if (!string.IsNullOrEmpty(selectedSupplier) && selectedSupplier != "0")
                {
                    ListItem item = ddlSupplier.Items.FindByValue(selectedSupplier);
                    if (item != null) ddlSupplier.SelectedValue = selectedSupplier;
                }

                // Handle supplier change postback from JS __doPostBack
                string arg = Request["__EVENTARGUMENT"] ?? "";
                if (arg.StartsWith("supplier_change:"))
                {
                    int supId;
                    if (int.TryParse(arg.Replace("supplier_change:", ""), out supId))
                        LoadRecoverables(supId);
                }
            }
        }

        private void LoadDropdowns()
        {
            DataTable rmDt = MMDatabaseHelper.GetActiveRawMaterials();
            ddlRM.DataSource     = rmDt;
            ddlRM.DataTextField  = "RMName";
            ddlRM.DataValueField = "RMID";
            ddlRM.DataBind();
            ddlRM.Items.Insert(0, new ListItem("-- Select Material --", "0"));

            DataTable supDt = MMDatabaseHelper.GetActiveSuppliers();
            ddlSupplier.DataSource     = supDt;
            ddlSupplier.DataTextField  = "SupplierName";
            ddlSupplier.DataValueField = "SupplierID";
            ddlSupplier.DataBind();
            ddlSupplier.Items.Insert(0, new ListItem("-- Select Supplier --", "0"));

            // UOM dropdowns — fetch separately to avoid exhausted DataTable reader
            ddlInvoiceUOM.DataSource     = MMDatabaseHelper.GetActiveUOM();
            ddlInvoiceUOM.DataTextField  = "Abbreviation";
            ddlInvoiceUOM.DataValueField = "UOMID";
            ddlInvoiceUOM.DataBind();
            ddlInvoiceUOM.Items.Insert(0, new ListItem("UOM", "0"));

            ddlReceivedUOM.DataSource     = MMDatabaseHelper.GetActiveUOM();
            ddlReceivedUOM.DataTextField  = "Abbreviation";
            ddlReceivedUOM.DataValueField = "UOMID";
            ddlReceivedUOM.DataBind();
            ddlReceivedUOM.Items.Insert(0, new ListItem("UOM", "0"));

            ddlStdUOM.DataSource     = MMDatabaseHelper.GetActiveUOM();
            ddlStdUOM.DataTextField  = "Abbreviation";
            ddlStdUOM.DataValueField = "UOMID";
            ddlStdUOM.DataBind();
            ddlStdUOM.Items.Insert(0, new ListItem("UOM", "0"));

            // Standard Invoice UOM — pre-select kg
            ddlStdInvoiceUOM.DataSource     = MMDatabaseHelper.GetActiveUOM();
            ddlStdInvoiceUOM.DataTextField  = "Abbreviation";
            ddlStdInvoiceUOM.DataValueField = "UOMID";
            ddlStdInvoiceUOM.DataBind();
            ddlStdInvoiceUOM.Items.Insert(0, new ListItem("UOM", "0"));
            // Pre-select kg on all 3 standard UOM dropdowns
            foreach (ListItem li in ddlStdInvoiceUOM.Items)
            {
                if (li.Text.ToLower() == "kg" || li.Text.ToLower() == "kgs")
                {
                    ddlStdInvoiceUOM.SelectedValue = li.Value;
                    ddlReceivedUOM.SelectedValue = li.Value;
                    ddlStdUOM.SelectedValue = li.Value;
                    break;
                }
            }

            BuildRMJson();
        }

        private void BuildRMJson()
        {
            DataTable dt = MMDatabaseHelper.GetAllRawMaterials();
            var sb = new StringBuilder("{");
            foreach (DataRow r in dt.Rows)
            {
                string hsn = r["HSNCode"] == DBNull.Value ? "" : EscapeJson(r["HSNCode"].ToString());
                string gst = r["GSTRate"]  == DBNull.Value ? "" : EscapeJson(r["GSTRate"].ToString());
                string uom = EscapeJson(r["Abbreviation"].ToString());
                sb.AppendFormat("\"{0}\":{{\"hsn\":\"{1}\",\"gst\":\"{2}\",\"uom\":\"{3}\"}},",
                    r["RMID"], hsn, gst, uom);
            }
            if (sb.Length > 1) sb.Length--;
            sb.Append("}");
            RMDataJson = sb.ToString();
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        private void GenerateGRN()
        {
            lblGRNNo.Text = MMDatabaseHelper.GenerateGRNNumber("RM");
        }

        private void LoadRecoverables(int supplierId)
        {
            if (supplierId <= 0)
            {
                lblRecSupplier.Text = "— Select a supplier —";
                pnlRecList.Visible  = false;
                pnlRecEmpty.Visible = true;
                return;
            }

            // Set supplier name label
            if (ddlSupplier.Items.FindByValue(supplierId.ToString()) != null)
                lblRecSupplier.Text = ddlSupplier.Items.FindByValue(supplierId.ToString()).Text;

            DataTable dt = MMDatabaseHelper.GetSupplierRecoverables(supplierId);
            if (dt.Rows.Count > 0)
            {
                rptRecoverables.DataSource = dt;
                rptRecoverables.DataBind();
                pnlRecList.Visible  = true;
                pnlRecEmpty.Visible = false;

                decimal total = 0;
                foreach (DataRow r in dt.Rows)
                    total += r["ShortageValue"] == DBNull.Value ? 0 : Convert.ToDecimal(r["ShortageValue"]);
                lblRecTotal.Text = total.ToString("N2");
            }
            else
            {
                pnlRecList.Visible  = false;
                pnlRecEmpty.Visible = true;
            }
        }

        private void LoadPendingInvoices()
        {
            DataTable dt = MMDatabaseHelper.GetPendingInvoiceRM();
            if (dt.Rows.Count > 0)
            {
                rptPending.DataSource = dt;
                rptPending.DataBind();
                pnlPendingList.Visible  = true;
                pnlPendingEmpty.Visible = false;
                lblPendingCount.Text = dt.Rows.Count.ToString();
            }
            else
            {
                pnlPendingList.Visible  = false;
                pnlPendingEmpty.Visible = true;
            }
        }

        protected void rptPending_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "EditInvoice")
            {
                int inwardId = Convert.ToInt32(e.CommandArgument);
                var row = MMDatabaseHelper.GetRawInwardById(inwardId);
                if (row != null)
                {
                    hfEditInwardId.Value = inwardId.ToString();
                    lblEditGRN.Text = row["GRNNo"].ToString();
                    txtEditInvoiceNo.Text = "";
                    txtEditInvoiceDate.Text = "";
                    pnlInvoiceUpdate.Visible = true;
                }
                LoadPendingInvoices();
            }
        }

        protected void btnUpdateInvoice_Click(object sender, EventArgs e)
        {
            int inwardId = Convert.ToInt32(hfEditInwardId.Value);
            string invoiceNo = txtEditInvoiceNo.Text.Trim();
            if (inwardId == 0 || string.IsNullOrEmpty(invoiceNo))
            {
                ShowAlert("Please enter an invoice number.", false);
                return;
            }
            if (invoiceNo.Equals("PENDING", StringComparison.OrdinalIgnoreCase))
            {
                ShowAlert("Please enter an actual invoice number, not 'PENDING'.", false);
                return;
            }

            DateTime? invoiceDate = null;
            DateTime dt2;
            if (DateTime.TryParse(txtEditInvoiceDate.Text, out dt2)) invoiceDate = dt2;

            MMDatabaseHelper.UpdateInvoiceNumber("MM_RawInward", inwardId, invoiceNo, invoiceDate);
            ShowAlert("Invoice number updated to: " + invoiceNo, true);
            pnlInvoiceUpdate.Visible = false;
            hfEditInwardId.Value = "0";
            LoadPendingInvoices();
            LoadGRNList();
        }

        protected void btnCancelInvoice_Click(object sender, EventArgs e)
        {
            pnlInvoiceUpdate.Visible = false;
            hfEditInwardId.Value = "0";
        }

        private void LoadGRNList()
        {
            DateTime from = DateTime.Today.AddMonths(-1);
            DateTime to   = DateTime.Today;
            if (!string.IsNullOrEmpty(txtFromDate.Text)) DateTime.TryParse(txtFromDate.Text, out from);
            if (!string.IsNullOrEmpty(txtToDate.Text))   DateTime.TryParse(txtToDate.Text,   out to);

            DataTable dt = MMDatabaseHelper.GetRawInwardList(from, to);
            if (dt.Rows.Count > 0)
            {
                rptGRN.DataSource = dt;
                rptGRN.DataBind();
                pnlEmpty.Visible = false;
                lblCount.Text = dt.Rows.Count + " record" + (dt.Rows.Count == 1 ? "" : "s");
            }
            else
            {
                rptGRN.DataSource = null;
                rptGRN.DataBind();
                pnlEmpty.Visible = true;
                lblCount.Text = "0 records";
            }
        }

        private void ShowAlert(string msg, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text    = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }

        protected void btnReceive_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;
            SaveGRN("Received");
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            ClearForm();
            ShowAlert("GRN entry discarded — goods rejected.", false);
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            // Also used for supplier_change postback — just reload recoverables
            string arg = Request["__EVENTARGUMENT"] ?? "";
            if (arg.StartsWith("supplier_change:"))
            {
                int supId;
                if (int.TryParse(arg.Replace("supplier_change:", ""), out supId))
                    LoadRecoverables(supId);
                return;
            }
            ClearForm();
            pnlAlert.Visible = false;
        }

        protected void btnFilter_Click(object sender, EventArgs e) { LoadGRNList(); }

        protected void ddlSupplier_Changed(object sender, EventArgs e)
        {
            int supId = 0;
            int.TryParse(ddlSupplier.SelectedValue, out supId);
            LoadRecoverables(supId);
        }

        protected void ddlRM_Changed(object sender, EventArgs e)
        {
            int rmId = 0;
            int.TryParse(ddlRM.SelectedValue, out rmId);
            LoadSuppliersByMaterial("RM", rmId);
        }

        private void LoadSuppliersByMaterial(string materialType, int materialId)
        {
            string selectedSupplier = ddlSupplier.SelectedValue;
            DataTable supDt;
            if (materialId > 0)
                supDt = MMDatabaseHelper.GetSuppliersSortedByMaterial(materialType, materialId);
            else
                supDt = MMDatabaseHelper.GetActiveSuppliers();

            ddlSupplier.Items.Clear();
            ddlSupplier.Items.Add(new ListItem("-- Select Supplier --", "0"));
            bool hasPrevious = false;
            foreach (DataRow r in supDt.Rows)
            {
                int purchaseCount = supDt.Columns.Contains("PurchaseCount") ? Convert.ToInt32(r["PurchaseCount"]) : 0;
                string label = r["SupplierName"].ToString();
                if (purchaseCount > 0)
                {
                    label += " ★ (" + purchaseCount + " previous)";
                    hasPrevious = true;
                }
                ddlSupplier.Items.Add(new ListItem(label, r["SupplierID"].ToString()));
            }
            // Restore selection if still valid
            if (!string.IsNullOrEmpty(selectedSupplier) && selectedSupplier != "0")
            {
                ListItem item = ddlSupplier.Items.FindByValue(selectedSupplier);
                if (item != null) ddlSupplier.SelectedValue = selectedSupplier;
            }
        }

        private bool ValidateForm()
        {
            if (ddlRM.SelectedValue == "0")           { ShowAlert("Please select a Raw Material.", false); return false; }
            if (ddlSupplier.SelectedValue == "0")     { ShowAlert("Please select a Supplier.", false); return false; }
            if (string.IsNullOrEmpty(txtGRNDate.Text)){ ShowAlert("GRN Date is required.", false); return false; }
            if (string.IsNullOrEmpty(txtQtyInvoice.Text)) { ShowAlert("Invoice Qty is required.", false); return false; }
            if (string.IsNullOrEmpty(txtQtyReceived.Text)){ ShowAlert("Actual Received Qty is required.", false); return false; }
            if (string.IsNullOrEmpty(txtQtyUOM.Text)) { ShowAlert("Qty in UOM is required.", false); return false; }
            if (string.IsNullOrEmpty(txtRate.Text))   { ShowAlert("Unit Price is required.", false); return false; }
            return true;
        }

        private void SaveGRN(string status)
        {
            try
            {
                string   grnNo    = lblGRNNo.Text;
                DateTime grnDate  = DateTime.Parse(txtGRNDate.Text);
                DateTime? invDate = null;
                if (!string.IsNullOrEmpty(txtInvoiceDate.Text))
                    invDate = DateTime.Parse(txtInvoiceDate.Text);

                int     rmId          = Convert.ToInt32(ddlRM.SelectedValue);
                int     supId         = Convert.ToInt32(ddlSupplier.SelectedValue);
                int     invoiceUOMId  = Convert.ToInt32(ddlInvoiceUOM.SelectedValue);
                decimal qtyInvoice    = Convert.ToDecimal(txtQtyInvoice.Text);
                decimal qtyReceived = Convert.ToDecimal(txtQtyReceived.Text);
                decimal qtyUOM      = Convert.ToDecimal(txtQtyUOM.Text);
                decimal rate        = Convert.ToDecimal(txtRate.Text);
                decimal transport   = string.IsNullOrEmpty(txtTransport.Text) ? 0 : Convert.ToDecimal(txtTransport.Text);
                decimal loading     = 0; decimal.TryParse(hfLoading.Value, out loading);
                decimal unloading   = 0; decimal.TryParse(hfUnloading.Value, out unloading);
                bool    qtyVerified = hfQtyVerified.Value == "1";
                decimal? gstRate    = null;
                decimal gstParsed;
                if (decimal.TryParse(txtGSTRate.Text, out gstParsed)) gstRate = gstParsed;

                decimal gstAmt = Convert.ToDecimal(hfGSTAmount.Value);
                decimal total  = Convert.ToDecimal(hfTotal.Value);
                int userId     = Convert.ToInt32(Session["MM_UserID"]);

                // Build remarks with supplier invoice qty if provided
                string remarks = txtRemarks.Text.Trim();
                string supQty = txtSupplierQty != null ? txtSupplierQty.Text.Trim() : "";
                string supUom = ddlInvoiceUOM != null && ddlInvoiceUOM.SelectedValue != "0"
                    ? ddlInvoiceUOM.SelectedItem.Text : "";
                if (!string.IsNullOrEmpty(supQty))
                {
                    string invNote = "[Supplier Invoice: " + supQty + (!string.IsNullOrEmpty(supUom) ? " " + supUom : "") + "]";
                    remarks = string.IsNullOrEmpty(remarks) ? invNote : invNote + " " + remarks;
                }

                MMDatabaseHelper.AddRawInward(
                    grnNo, grnDate, invDate,
                    txtInvoiceNo.Text.Trim(), supId, rmId,
                    qtyInvoice, qtyReceived, qtyUOM, rate,
                    txtHSN.Text.Trim(), gstRate, gstAmt,
                    transport, chkTransportInInvoice.Checked, chkTransportInGST.Checked,
                    loading, unloading, qtyVerified,
                    total, txtPONo.Text.Trim(), remarks,
                    chkQC.Checked, status, userId);

                ShowAlert("GRN " + grnNo + " saved — goods received.", true);
                ClearForm();
                LoadGRNList();
                LoadRecoverables(0);
                LoadPendingInvoices();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        private void ClearForm()
        {
            hfInwardID.Value = "0";
            ddlRM.SelectedIndex = 0;
            ddlSupplier.SelectedIndex = 0;
            ddlInvoiceUOM.SelectedIndex = 0;
            ddlReceivedUOM.SelectedIndex = 0;
            ddlStdUOM.SelectedIndex = 0;
            txtGRNDate.Text   = DateTime.Today.ToString("yyyy-MM-dd");
            txtInvoiceNo.Text = txtInvoiceDate.Text = txtPONo.Text = "";
            txtQtyInvoice.Text = txtQtyReceived.Text = txtQtyUOM.Text = "";
            if (txtSupplierQty != null) txtSupplierQty.Text = "";
            if (ddlStdInvoiceUOM != null)
            {
                foreach (ListItem li in ddlStdInvoiceUOM.Items)
                { if (li.Text.ToLower() == "kg" || li.Text.ToLower() == "kgs") { ddlStdInvoiceUOM.SelectedValue = li.Value; break; } }
            }
            txtRate.Text = txtHSN.Text = txtGSTRate.Text = txtTransport.Text = "";
            txtRemarks.Text = "";
            chkTransportInInvoice.Checked = false;
            chkTransportInGST.Checked = false;
            chkQC.Checked = false;
            hfTaxable.Value = hfGSTAmount.Value = hfTotal.Value = "0";
            hfLoading.Value = hfUnloading.Value = hfQtyVerified.Value = "0";
            txtInvoiceNo.ReadOnly = false;
            GenerateGRN();
            // Reset Manual Invoice checkbox via client script
            ClientScript.RegisterStartupScript(this.GetType(), "resetManualInv",
                "var cb=document.getElementById('chkManualInvoice');if(cb)cb.checked=false;toggleManualInvoice(false);", true);
        }
    }
}
