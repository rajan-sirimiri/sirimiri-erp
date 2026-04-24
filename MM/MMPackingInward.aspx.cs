using System;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMPackingInward : Page
    {
        protected Label           lblNavUser;
        protected Label           lblGRNNo;
        protected Label           lblAlert;
        protected Label           lblCount;
        protected Label           lblRecSupplier;
        protected Label           lblRecTotal;
        protected HiddenField     hfInwardID;
        protected HiddenField     hfTaxable;
        protected HiddenField     hfGSTAmount;
        protected HiddenField     hfTotal;
        protected HiddenField     hfLoading;
        protected HiddenField     hfUnloading;
        protected HiddenField     hfQtyVerified;
        protected HiddenField     hfInvoiceMode;
        protected HiddenField     hfSupplierID;
        protected Button          btnSupplierTrigger;
        protected Panel           pnlAlert;
        protected Panel           pnlEmpty;
        protected Panel           pnlRecEmpty;
        protected Panel           pnlRecList;
        protected DropDownList    ddlPM;
        protected DropDownList    ddlSupplier;
        protected DropDownList    ddlInvoiceUOM;
        protected DropDownList    ddlReceivedUOM;
        protected DropDownList    ddlStdUOM;
        protected TextBox         txtGRNDate;
        protected TextBox         txtInvoiceNo;
        protected TextBox         txtInvoiceDate;
        protected TextBox         txtPONo;
        protected TextBox         txtQtyInvoice;
        protected TextBox         txtQtyReceived;
        protected TextBox         txtQtyUOM;
        protected TextBox         txtRate;
        protected TextBox         txtHSN;
        protected TextBox         txtGSTRate;
        protected TextBox         txtTransport;
        protected TextBox         txtRemarks;
        protected TextBox         txtFromDate;
        protected TextBox         txtToDate;
        protected CheckBox        chkTransportInInvoice;
        protected CheckBox        chkTransportInGST;
        protected CheckBox        chkQC;
        protected Button          btnReceive;
        protected Button          btnReject;
        protected Button          btnClear;
        protected Button          btnFilter;
        protected Repeater        rptGRN;
        protected Repeater        rptRecoverables;

        protected string PMDataJson = "{}"; // JSON for inline script
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            // Module access check
            string __role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(__role, "MM", "MM_PM_GRN"))
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
                LoadRecoverables(0);
            }
            else
            {
                BuildPMJson();
                // Save selected values before rebind
                string selectedSupplier = ddlSupplier.SelectedValue;
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
            }
        }

        private void LoadDropdowns()
        {
            DataTable pmDt = MMDatabaseHelper.GetActivePackingMaterials();
            ddlPM.DataSource     = pmDt;
            ddlPM.DataTextField  = "PMName";
            ddlPM.DataValueField = "PMID";
            ddlPM.DataBind();
            ddlPM.Items.Insert(0, new ListItem("-- Select Material --", "0"));

            DataTable supDt = MMDatabaseHelper.GetActiveSuppliers();
            ddlSupplier.DataSource     = supDt;
            ddlSupplier.DataTextField  = "SupplierName";
            ddlSupplier.DataValueField = "SupplierID";
            ddlSupplier.DataBind();
            ddlSupplier.Items.Insert(0, new ListItem("-- Select Supplier --", "0"));

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

            BuildPMJson();
        }

        private void BuildPMJson()
        {
            DataTable dt = MMDatabaseHelper.GetAllPackingMaterials();
            var sb = new StringBuilder("{");
            foreach (DataRow r in dt.Rows)
            {
                string hsn = r["HSNCode"]     == DBNull.Value ? "" : EscapeJson(r["HSNCode"].ToString());
                string gst = r["GSTRate"]     == DBNull.Value ? "" : EscapeJson(r["GSTRate"].ToString());
                string uom = EscapeJson(r["Abbreviation"].ToString());
                sb.AppendFormat("\"{0}\":{{\"hsn\":\"{1}\",\"gst\":\"{2}\",\"uom\":\"{3}\"}},",
                    r["PMID"], hsn, gst, uom);
            }
            if (sb.Length > 1) sb.Length--;
            sb.Append("}");
            PMDataJson = sb.ToString();
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        private void GenerateGRN()
        {
            lblGRNNo.Text = MMDatabaseHelper.GenerateGRNNumber("PM");
        }

        private void LoadRecoverables(int supplierId)
        {
            if (supplierId <= 0)
            {
                lblRecSupplier.Text = "-- Select a supplier --";
                pnlRecList.Visible  = false;
                pnlRecEmpty.Visible = true;
                return;
            }

            if (ddlSupplier.Items.FindByValue(supplierId.ToString()) != null)
                lblRecSupplier.Text = ddlSupplier.Items.FindByValue(supplierId.ToString()).Text;

            DataTable dt = MMDatabaseHelper.GetSupplierPackingRecoverables(supplierId);
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

        private void LoadGRNList()
        {
            DateTime from = DateTime.Today.AddMonths(-1);
            DateTime to   = DateTime.Today;
            if (!string.IsNullOrEmpty(txtFromDate.Text)) DateTime.TryParse(txtFromDate.Text, out from);
            if (!string.IsNullOrEmpty(txtToDate.Text))   DateTime.TryParse(txtToDate.Text,   out to);

            DataTable dt = MMDatabaseHelper.GetPackingInwardList(from, to);
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
            ShowAlert("GRN entry discarded -- goods rejected.", false);
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            string arg = Request["__EVENTARGUMENT"] ?? "";
            if (arg.StartsWith("supplier_change:"))
            {
                int supId;
                if (int.TryParse(arg.Replace("supplier_change:", ""), out supId))
                    LoadRecoverables(supId);
                // Also reload GRN list so page stays consistent
                LoadGRNList();
                return;
            }
            ClearForm();
            pnlAlert.Visible = false;
        }

        protected void btnSupplierTrigger_Click(object sender, EventArgs e)
        {
            int supId;
            if (int.TryParse(hfSupplierID.Value, out supId) && supId > 0)
                LoadRecoverables(supId);
            else
                LoadRecoverables(0);
            LoadGRNList();
        }

        protected void btnFilter_Click(object sender, EventArgs e) { LoadGRNList(); }

        protected void ddlSupplier_Changed(object sender, EventArgs e)
        {
            int supId = 0;
            int.TryParse(ddlSupplier.SelectedValue, out supId);
            LoadRecoverables(supId);
        }

        protected void ddlPM_Changed(object sender, EventArgs e)
        {
            int pmId = 0;
            int.TryParse(ddlPM.SelectedValue, out pmId);
            LoadSuppliersByMaterial("PM", pmId);
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
            foreach (DataRow r in supDt.Rows)
            {
                int purchaseCount = supDt.Columns.Contains("PurchaseCount") ? Convert.ToInt32(r["PurchaseCount"]) : 0;
                string label = r["SupplierName"].ToString();
                if (purchaseCount > 0)
                    label += " \u2605 (" + purchaseCount + " previous)";
                ddlSupplier.Items.Add(new ListItem(label, r["SupplierID"].ToString()));
            }
            if (!string.IsNullOrEmpty(selectedSupplier) && selectedSupplier != "0")
            {
                ListItem item = ddlSupplier.Items.FindByValue(selectedSupplier);
                if (item != null) ddlSupplier.SelectedValue = selectedSupplier;
            }
        }

        private bool ValidateForm()
        {
            if (ddlPM.SelectedValue == "0")               { ShowAlert("Please select a Packing Material.", false); return false; }
            if (ddlSupplier.SelectedValue == "0")         { ShowAlert("Please select a Supplier.", false); return false; }
            if (string.IsNullOrEmpty(txtGRNDate.Text))    { ShowAlert("GRN Date is required.", false); return false; }
            if (string.IsNullOrEmpty(txtQtyInvoice.Text)) { ShowAlert("Invoice Qty is required.", false); return false; }
            if (string.IsNullOrEmpty(txtQtyReceived.Text)){ ShowAlert("Actual Received Qty is required.", false); return false; }
            if (string.IsNullOrEmpty(txtQtyUOM.Text))     { ShowAlert("Qty in Standard UOM is required.", false); return false; }
            if (string.IsNullOrEmpty(txtRate.Text))       { ShowAlert("Unit Price is required.", false); return false; }
            return true;
        }

        private void SaveGRN(string status)
        {
            try
            {
                string    grnNo   = lblGRNNo.Text;
                DateTime  grnDate = DateTime.Parse(txtGRNDate.Text);
                DateTime? invDate = null;
                if (!string.IsNullOrEmpty(txtInvoiceDate.Text))
                    invDate = DateTime.Parse(txtInvoiceDate.Text);

                int     pmId        = Convert.ToInt32(ddlPM.SelectedValue);
                int     supId       = Convert.ToInt32(ddlSupplier.SelectedValue);
                decimal qtyInvoice  = Convert.ToDecimal(txtQtyInvoice.Text);
                decimal qtyReceived = Convert.ToDecimal(txtQtyReceived.Text);
                decimal qtyUOM      = Convert.ToDecimal(txtQtyUOM.Text);
                decimal rate        = Convert.ToDecimal(txtRate.Text);
                decimal transport   = string.IsNullOrEmpty(txtTransport.Text) ? 0 : Convert.ToDecimal(txtTransport.Text);
                decimal loading     = 0; decimal.TryParse(hfLoading.Value, out loading);
                decimal unloading   = 0; decimal.TryParse(hfUnloading.Value, out unloading);
                bool    qtyVerified = hfQtyVerified.Value == "1";

                decimal? gstRate  = null;
                decimal  gstParsed;
                if (decimal.TryParse(txtGSTRate.Text, out gstParsed)) gstRate = gstParsed;

                decimal gstAmt = Convert.ToDecimal(hfGSTAmount.Value);
                decimal total  = Convert.ToDecimal(hfTotal.Value);
                int     userId = Convert.ToInt32(Session["MM_UserID"]);

                // ── Invoice mode (radio) ──
                string invoiceMode = hfInvoiceMode != null ? (hfInvoiceMode.Value ?? "normal") : "normal";
                string invoiceNoVal = txtInvoiceNo.Text.Trim();
                if (invoiceMode == "none")
                {
                    invoiceNoVal = "NO-INVOICE";
                    invDate = null;
                    gstRate = 0;
                    gstAmt  = 0;
                }
                else if (invoiceMode == "manual")
                {
                    if (!invoiceNoVal.StartsWith("MN-", StringComparison.OrdinalIgnoreCase))
                        invoiceNoVal = "MN-" + invoiceNoVal;
                    gstRate = 0;
                    gstAmt  = 0;
                }

                MMDatabaseHelper.AddPackingInward(
                    grnNo, grnDate, invDate,
                    invoiceNoVal, supId, pmId,
                    qtyInvoice, qtyReceived, qtyUOM, rate,
                    txtHSN.Text.Trim(), gstRate, gstAmt,
                    transport, chkTransportInInvoice.Checked, chkTransportInGST.Checked,
                    loading, unloading, qtyVerified,
                    total, txtPONo.Text.Trim(), txtRemarks.Text.Trim(),
                    chkQC.Checked, status, userId);

                ShowAlert("GRN " + grnNo + " saved -- goods received.", true);
                ClearForm();
                LoadGRNList();
                LoadRecoverables(0);
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        private void ClearForm()
        {
            hfInwardID.Value              = "0";
            ddlPM.SelectedIndex           = 0;
            ddlSupplier.SelectedIndex     = 0;
            ddlInvoiceUOM.SelectedIndex   = 0;
            ddlReceivedUOM.SelectedIndex  = 0;
            ddlStdUOM.SelectedIndex       = 0;
            txtGRNDate.Text               = DateTime.Today.ToString("yyyy-MM-dd");
            txtInvoiceNo.Text             = "";
            txtInvoiceDate.Text           = "";
            txtPONo.Text                  = "";
            txtQtyInvoice.Text            = "";
            txtQtyReceived.Text           = "";
            txtQtyUOM.Text                = "";
            txtRate.Text                  = "";
            txtHSN.Text                   = "";
            txtGSTRate.Text               = "";
            txtTransport.Text             = "";
            txtRemarks.Text               = "";
            chkTransportInInvoice.Checked = false;
            chkTransportInGST.Checked     = false;
            chkQC.Checked                 = false;
            hfTaxable.Value               = "0";
            hfGSTAmount.Value             = "0";
            hfTotal.Value                 = "0";
            txtInvoiceNo.ReadOnly = false;
            if (hfInvoiceMode != null) hfInvoiceMode.Value = "normal";
            GenerateGRN();
            ClientScript.RegisterStartupScript(this.GetType(), "resetInvMode",
                "var rb=document.getElementById('rbInvNormal');if(rb){rb.checked=true;}" +
                "if(typeof setInvoiceMode==='function')setInvoiceMode('normal');", true);
        }
    }
}
