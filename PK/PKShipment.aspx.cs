using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKShipment : Page
    {
        protected Label lblUser, lblAlert, lblFormTitle;
        protected Label lblLockedTitle, lblViewDCNum, lblViewDate, lblViewCustomer, lblViewRemarks;
        protected Label lblSAOrderId, lblSACustomer, lblSADate, lblSAArea, lblSAStatus, lblSAChannel, lblSATransport;
        protected Panel pnlAlert, pnlForm, pnlLocked, pnlViewRemarks;
        protected Panel pnlSAEmpty, pnlSAList, pnlSADetail, pnlSAEditLines, pnlCreateRetailBar;
        protected HiddenField hfDCID, hfLines, hfProductData, hfCustomerData, hfOrgState, hfSAShipId, hfSAProductOptions;
        protected HiddenField hfActiveConsig, hfActiveTab, hfSubTab;
        protected HiddenField hfNewCustName, hfNewCustPhone, hfNewCustEmail, hfNewCustAddress, hfNewCustCity, hfNewCustState, hfNewCustPinCode, hfNewCustGSTIN;
        protected TextBox txtDCNumber, txtDCDate, txtRemarks, txtConsigDate, txtConsigText, txtVehicleNo, txtCourierName, txtTrackingNo;
        protected DropDownList ddlCustomer, ddlChannel, ddlTransport, ddlDispatched, ddlArchived, ddlProjMonth, ddlProjYear;
        protected Button btnDraftSave, btnFinalise, btnNew, btnNewFromLocked, btnPrintDC, btnDownloadFromView, btnDeleteDC, btnUnconvertDCFromForm;
        protected Button btnCreateInvoice, btnCreateInvoiceHidden, btnCreateInvoiceDraft, btnCreateInvoiceDraftHidden, btnDownloadInvoicePDF;
        protected Button btnCreateConsignment, btnBulkInvoice, btnDispatchConsig, btnArchiveConsig, btnConfirmDispatch, btnTabRetail, btnSyncFromZoho;
        protected Button btnCreateRetailOrder, btnCreateRetailCustomerHidden;
        protected Panel pnlCreateInvoice, pnlInvoiceStatus, pnlInvoiceError, pnlConsigDCs, pnlBulkResult, pnlConsigContent, pnlConsigEmpty, pnlDispatchForm;
        protected Label lblInvoiceNo, lblInvoiceZohoStatus, lblInvoiceAmount, lblInvoiceError, lblConsigDCTitle, lblConsigStatus;
        protected Literal litBulkResult;
        protected HyperLink lnkViewInZoho;
        protected Button btnConvertDC, btnDispatch, btnUnconvertDC, btnCloseSADetail, btnSaveSAEdit;
        protected Repeater rptViewLines, rptSAOrders, rptSALines, rptSAEditLines, rptProjections, rptConsigDCs, rptConsigTabs;
        protected Panel pnlProjEmpty;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null)
            { Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery)); return; }

            // Module access check
            string __role = Session["PK_Role"]?.ToString() ?? "";
            if (!PKDatabaseHelper.RoleHasModuleAccess(__role, "PK", "PK_SHIPMENT"))
            { Response.Redirect("PKHome.aspx"); return; }
            if (lblUser != null) lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack)
            {
                BindCustomers("RT");
                BuildProductData();
                BuildCustomerData("RT");
                BindConsigTabs();
                BindDispatchedDropdown();
                BindArchivedDropdown();
                txtDCDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                if (txtConsigDate != null) txtConsigDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                if (btnDeleteDC != null) btnDeleteDC.Visible = false;
                LoadRetailDCs();
                LoadProjDropdowns();
                BindProjections();
                // Retail is the default landing tab: hide the DC form and show the Create Retail Order bar
                pnlForm.Visible = false;
                if (pnlCreateRetailBar != null) pnlCreateRetailBar.Visible = true;
                ConfigureTransportModesForTab();
            }
            BindSAOrders();
        }

        // Fix 4: Re-apply the persisted sub-tab selection after every postback.
        // `hfSubTab` is written client-side by switchShipTab() and read here — the JS reads the
        // value from the hidden field and invokes switchShipTab() to restore .active classes on
        // the correct sub-tab button and panel. Without this, every postback reset the view to
        // "Delivery Challans" because the DC sub-tab has `class="ship-tab active"` hardcoded.
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            string tab = hfSubTab != null && !string.IsNullOrEmpty(hfSubTab.Value) ? hfSubTab.Value : "dc";
            if (tab != "dc" && tab != "sa") tab = "dc";
            string js = "if(typeof switchShipTab==='function'){switchShipTab('" + tab + "');}";
            Page.ClientScript.RegisterStartupScript(GetType(), "restoreSubTab", js, true);
        }

        void BindCustomers(string typeFilter = null)
        {
            DataTable dt;
            if (!string.IsNullOrEmpty(typeFilter))
                dt = PKDatabaseHelper.GetActiveCustomersByType(typeFilter);
            else
                dt = PKDatabaseHelper.GetActiveCustomers();
            ddlCustomer.Items.Clear();
            ddlCustomer.Items.Add(new ListItem("-- Select Customer --", "0"));
            foreach (DataRow r in dt.Rows)
            {
                string typeName = r.Table.Columns.Contains("TypeName") && r["TypeName"] != DBNull.Value
                    ? r["TypeName"].ToString() : "";
                string label = r["CustomerName"] + " (" + r["CustomerCode"] + ")";
                if (!string.IsNullOrEmpty(typeName)) label += " — " + typeName;
                ddlCustomer.Items.Add(new ListItem(label, r["CustomerID"].ToString()));
            }
        }

        void BindConsigTabs()
        {
            var dt = PKDatabaseHelper.GetActiveConsignments();
            if (rptConsigTabs != null) { rptConsigTabs.DataSource = dt; rptConsigTabs.DataBind(); }
        }

        void BindDispatchedDropdown()
        {
            if (ddlDispatched == null) return;
            var dt = PKDatabaseHelper.GetDispatchedConsignments();
            ddlDispatched.Items.Clear();
            ddlDispatched.Items.Add(new ListItem("Dispatched (" + dt.Rows.Count + ")", "0"));
            foreach (DataRow r in dt.Rows)
                ddlDispatched.Items.Add(new ListItem(r["ConsignmentCode"].ToString(), r["ConsignmentID"].ToString()));
        }

        void BindArchivedDropdown()
        {
            if (ddlArchived == null) return;
            var dt = PKDatabaseHelper.GetArchivedConsignments();
            ddlArchived.Items.Clear();
            ddlArchived.Items.Add(new ListItem("Archived (" + dt.Rows.Count + ")", "0"));
            foreach (DataRow r in dt.Rows)
                ddlArchived.Items.Add(new ListItem(r["ConsignmentCode"].ToString(), r["ConsignmentID"].ToString()));
        }

        protected void btnTabRetail_Click(object s, EventArgs e)
        {
            hfActiveConsig.Value = "0";
            hfActiveTab.Value = "retail";
            if (pnlConsigContent != null) pnlConsigContent.Visible = false;
            // Retail tab: form hidden by default — "+ Create Retail Order" button opens it
            pnlForm.Visible = false;
            if (pnlLocked != null) pnlLocked.Visible = false;
            if (pnlCreateRetailBar != null) pnlCreateRetailBar.Visible = true;
            ResetDCForm();
            BindCustomers("RT");
            BuildCustomerData("RT");
            BindConsigTabs();
            ConfigureTransportModesForTab();
            // Show retail DCs list
            LoadRetailDCs();
        }

        /// <summary>Retail orders ship only via courier (no full-load option).
        /// Consignment/DI/ST orders can use either.</summary>
        void ConfigureTransportModesForTab()
        {
            if (ddlTransport == null) return;
            bool isRetail = hfActiveTab != null && hfActiveTab.Value == "retail";
            // Remove any existing Full Load entry, then add it back for non-retail
            var full = ddlTransport.Items.FindByValue("FULL_LOAD");
            if (isRetail)
            {
                if (full != null) ddlTransport.Items.Remove(full);
                // Default to Courier so the user doesn't have to pick
                var courier = ddlTransport.Items.FindByValue("COURIER");
                if (courier != null) ddlTransport.SelectedValue = "COURIER";
            }
            else
            {
                if (full == null) ddlTransport.Items.Insert(1, new ListItem("Full Load - Own Vehicle", "FULL_LOAD"));
            }
        }

        /// <summary>Click handler for the prominent "+ Create Retail Order" button.
        /// Shows a fresh DC form ready for a new retail order.</summary>
        protected void btnCreateRetailOrder_Click(object s, EventArgs e)
        {
            hfActiveTab.Value = "retail";
            ResetDCForm();
            txtDCDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            if (ddlChannel != null) ddlChannel.SelectedValue = "GT";
            pnlForm.Visible = true;
            if (pnlLocked != null) pnlLocked.Visible = false;
            if (pnlCreateRetailBar != null) pnlCreateRetailBar.Visible = true;
            BindCustomers("RT");
            BuildCustomerData("RT");
            BuildProductData();
            ConfigureTransportModesForTab();
            if (btnDeleteDC != null) btnDeleteDC.Visible = false;
            btnDraftSave.Visible = true;
            btnFinalise.Visible = true;
            lblFormTitle.Text = "New Retail Order";
            if (pnlAlert != null) pnlAlert.Visible = false;
        }

        /// <summary>Invoked by the customer modal's Save button in retail quick-add flow.
        /// Reads hidden fields populated by modal JS, dedupes by phone/email/name, creates
        /// the customer as RT type, rebinds dropdowns, and auto-selects the new customer.</summary>
        protected void btnCreateRetailCustomerHidden_Click(object s, EventArgs e)
        {
            string name    = hfNewCustName  != null ? hfNewCustName.Value.Trim()    : "";
            string phone   = hfNewCustPhone != null ? hfNewCustPhone.Value.Trim()   : "";
            string email   = hfNewCustEmail != null ? hfNewCustEmail.Value.Trim()   : "";
            string address = hfNewCustAddress != null ? hfNewCustAddress.Value.Trim(): "";
            string city    = hfNewCustCity  != null ? hfNewCustCity.Value.Trim()    : "";
            string state   = hfNewCustState != null ? hfNewCustState.Value.Trim()   : "";
            string pincode = hfNewCustPinCode != null ? hfNewCustPinCode.Value.Trim(): "";
            string gstin   = hfNewCustGSTIN != null ? hfNewCustGSTIN.Value.Trim().ToUpper() : "";

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            { ShowAlert("Name and phone are required to create a customer.", false); return; }

            int newCustId;
            try
            {
                // Dedupe: phone / email / name match
                var existing = PKDatabaseHelper.FindCustomerByContactInfo(phone, email, name);
                if (existing != null)
                {
                    newCustId = Convert.ToInt32(existing["CustomerID"]);
                    string existingName = existing["CustomerName"].ToString();
                    string existingCode = existing["CustomerCode"].ToString();
                    ShowAlert("Customer already exists: " + existingName + " (" + existingCode + "). Selected for you.", true);
                }
                else
                {
                    newCustId = PKDatabaseHelper.AddCustomer("RT", name, null, phone, email,
                        address, city, state, pincode, gstin);
                    ShowAlert("New retail customer added: " + name, true);
                }
            }
            catch (Exception ex) { ShowAlert("Could not create customer: " + ex.Message, false); return; }

            // Clear the hidden fields so they don't replay on a later postback
            if (hfNewCustName    != null) hfNewCustName.Value    = "";
            if (hfNewCustPhone   != null) hfNewCustPhone.Value   = "";
            if (hfNewCustEmail   != null) hfNewCustEmail.Value   = "";
            if (hfNewCustAddress != null) hfNewCustAddress.Value = "";
            if (hfNewCustCity    != null) hfNewCustCity.Value    = "";
            if (hfNewCustState   != null) hfNewCustState.Value   = "";
            if (hfNewCustPinCode != null) hfNewCustPinCode.Value = "";
            if (hfNewCustGSTIN   != null) hfNewCustGSTIN.Value   = "";

            // Rebuild dropdowns + select the new customer, then show the DC form
            BindCustomers("RT");
            BuildCustomerData("RT");
            BuildProductData();
            if (ddlCustomer != null)
            {
                var li = ddlCustomer.Items.FindByValue(newCustId.ToString());
                if (li != null) ddlCustomer.SelectedValue = newCustId.ToString();
            }
            pnlForm.Visible = true;
            if (pnlCreateRetailBar != null) pnlCreateRetailBar.Visible = true;
            if (pnlLocked != null) pnlLocked.Visible = false;
            ConfigureTransportModesForTab();
            if (lblFormTitle != null) lblFormTitle.Text = "New Retail Order";
        }

        protected void rptConsigTabs_Command(object s, CommandEventArgs e)
        {
            if (e.CommandName == "SelectConsig")
            {
                int csgId = Convert.ToInt32(e.CommandArgument);
                hfActiveConsig.Value = csgId.ToString();
                hfActiveTab.Value = "consig";
                if (pnlCreateRetailBar != null) pnlCreateRetailBar.Visible = false;
                BindCustomers("DI,ST");
                BuildCustomerData("DI,ST");
                LoadConsigDCs(csgId);
                ResetDCForm();
                BindConsigTabs();
                ConfigureTransportModesForTab();
            }
        }

        protected void ddlDispatched_Changed(object s, EventArgs e)
        {
            int csgId = Convert.ToInt32(ddlDispatched.SelectedValue);
            if (csgId > 0)
            {
                hfActiveConsig.Value = csgId.ToString();
                hfActiveTab.Value = "dispatched";
                if (pnlCreateRetailBar != null) pnlCreateRetailBar.Visible = false;
                LoadConsigDCs(csgId);
                pnlForm.Visible = false;
                BindConsigTabs();
                ConfigureTransportModesForTab();
            }
        }

        protected void ddlArchived_Changed(object s, EventArgs e)
        {
            int csgId = Convert.ToInt32(ddlArchived.SelectedValue);
            if (csgId > 0)
            {
                hfActiveConsig.Value = csgId.ToString();
                hfActiveTab.Value = "archived";
                if (pnlCreateRetailBar != null) pnlCreateRetailBar.Visible = false;
                LoadConsigDCs(csgId);
                pnlForm.Visible = false;
                BindConsigTabs();
                ConfigureTransportModesForTab();
            }
        }

        void ResetDCForm()
        {
            hfDCID.Value = "0";
            txtDCNumber.Text = "";
            ddlCustomer.SelectedIndex = 0;
            txtDCDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtRemarks.Text = "";
            hfLines.Value = "[]";
            lblFormTitle.Text = "New Delivery Challan";
            if (btnDeleteDC != null) btnDeleteDC.Visible = false;
            if (btnCreateInvoiceDraft != null) btnCreateInvoiceDraft.Visible = false;
            if (ddlTransport != null) ddlTransport.SelectedIndex = 0;
            if (txtCourierName != null) txtCourierName.Text = "";
            if (txtTrackingNo != null) txtTrackingNo.Text = "";
        }

        protected void btnCreateConsignment_Click(object s, EventArgs e)
        {
            try
            {
                DateTime dt = DateTime.Parse(txtConsigDate.Text);
                string userText = txtConsigText.Text.Trim();
                if (string.IsNullOrEmpty(userText))
                { ShowAlert("Please enter a consignment identifier (e.g. ROTN, BLORE).", false); return; }

                int newId = PKDatabaseHelper.CreateConsignment(dt, userText, "", UserID);
                hfActiveConsig.Value = newId.ToString();
                hfActiveTab.Value = "consig";
                txtConsigText.Text = "";
                ResetDCForm();
                LoadConsigDCs(newId);
                BindConsigTabs();
                BindDispatchedDropdown();

                var csg = PKDatabaseHelper.GetConsignmentById(newId);
                ShowAlert("Consignment created: " + (csg != null ? csg["ConsignmentCode"].ToString() : ""), true);
            }
            catch (Exception ex) { ShowAlert("Error creating consignment: " + ex.Message, false); }
        }

        void LoadConsigDCs(int consignmentId)
        {
            if (pnlConsigDCs == null) return;
            var csg = PKDatabaseHelper.GetConsignmentById(consignmentId);
            if (csg == null) { pnlConsigDCs.Visible = false; return; }

            var dcs = PKDatabaseHelper.GetDCsByConsignment(consignmentId);
            pnlConsigDCs.Visible = true;
            if (pnlConsigContent != null) pnlConsigContent.Visible = true;
            if (lblConsigDCTitle != null)
                lblConsigDCTitle.Text = csg["ConsignmentCode"] + " — " + dcs.Rows.Count + " DC(s)";
            if (rptConsigDCs != null) { rptConsigDCs.DataSource = dcs; rptConsigDCs.DataBind(); }
            if (pnlBulkResult != null) pnlBulkResult.Visible = false;
            if (pnlConsigEmpty != null) pnlConsigEmpty.Visible = dcs.Rows.Count == 0;

            string status = csg["Status"].ToString();

            // Auto-check READY status
            if (status == "OPEN")
            {
                try { PKDatabaseHelper.UpdateConsignmentReadyStatus(consignmentId); } catch { }
                csg = PKDatabaseHelper.GetConsignmentById(consignmentId);
                status = csg["Status"].ToString();
            }

            // Status badge
            if (lblConsigStatus != null)
            {
                lblConsigStatus.Text = status;
                switch (status)
                {
                    case "OPEN": lblConsigStatus.Style["background"] = "#fff3cd"; lblConsigStatus.Style["color"] = "#856404"; break;
                    case "READY": lblConsigStatus.Style["background"] = "#d4edda"; lblConsigStatus.Style["color"] = "#155724"; break;
                    case "DISPATCHED": lblConsigStatus.Style["background"] = "#cce5ff"; lblConsigStatus.Style["color"] = "#004085"; break;
                    case "ARCHIVED": lblConsigStatus.Style["background"] = "#e2e3e5"; lblConsigStatus.Style["color"] = "#383d41"; break;
                }
            }

            // Button visibility
            bool hasFinalised = false;
            foreach (DataRow r in dcs.Rows)
                if (r["Status"].ToString() == "FINALISED") { hasFinalised = true; break; }

            if (btnBulkInvoice != null) btnBulkInvoice.Visible = hasFinalised && status == "OPEN";
            if (btnSyncFromZoho != null) btnSyncFromZoho.Visible = hasFinalised && (status == "DISPATCHED" || status == "READY" || status == "OPEN");
            if (btnDispatchConsig != null) btnDispatchConsig.Visible = (status == "READY");
            if (pnlDispatchForm != null) pnlDispatchForm.Visible = false;
            if (btnArchiveConsig != null) btnArchiveConsig.Visible = (status == "DISPATCHED");

            // Hide DC form for dispatched/archived
            if (status == "DISPATCHED" || status == "ARCHIVED")
            {
                pnlForm.Visible = false;
                string vn = csg["VehicleNumber"] != DBNull.Value ? csg["VehicleNumber"].ToString() : "";
                if (!string.IsNullOrEmpty(vn) && lblConsigDCTitle != null)
                    lblConsigDCTitle.Text += " | Vehicle: " + vn;
            }
            else
            {
                pnlForm.Visible = true;
            }
        }

        void LoadRetailDCs()
        {
            var dcs = PKDatabaseHelper.GetRetailDCs();
            if (pnlConsigDCs != null)
            {
                pnlConsigDCs.Visible = true;
                if (pnlConsigContent != null) pnlConsigContent.Visible = dcs.Rows.Count > 0;
                if (lblConsigDCTitle != null) lblConsigDCTitle.Text = "Retail Orders — " + dcs.Rows.Count + " DC(s)";
                if (lblConsigStatus != null) { lblConsigStatus.Text = ""; lblConsigStatus.Visible = false; }
                if (rptConsigDCs != null) { rptConsigDCs.DataSource = dcs; rptConsigDCs.DataBind(); }
                if (pnlConsigEmpty != null) pnlConsigEmpty.Visible = dcs.Rows.Count == 0;
                if (btnBulkInvoice != null) btnBulkInvoice.Visible = false;
                if (btnDispatchConsig != null) btnDispatchConsig.Visible = false;
                if (btnArchiveConsig != null) btnArchiveConsig.Visible = false;
                if (btnSyncFromZoho != null) btnSyncFromZoho.Visible = false;
                if (pnlDispatchForm != null) pnlDispatchForm.Visible = false;
                if (pnlBulkResult != null) pnlBulkResult.Visible = false;
            }
            // Show form for retail even when consignment content not visible
            pnlForm.Visible = true;
        }

        protected string GetTransportLabel(object mode, object courier)
        {
            string m = mode != null && mode != DBNull.Value ? mode.ToString() : "";
            string c = courier != null && courier != DBNull.Value ? courier.ToString() : "";
            if (m == "FULL_LOAD") return "<span style='color:#155724;'>Own Vehicle</span>";
            if (m == "COURIER") return "<span style='color:#004085;'>Courier" + (!string.IsNullOrEmpty(c) ? ": " + c : "") + "</span>";
            return "—";
        }

        protected void btnDispatchConsig_Click(object s, EventArgs e)
        {
            if (pnlDispatchForm != null) pnlDispatchForm.Visible = true;
        }

        protected void btnConfirmDispatch_Click(object s, EventArgs e)
        {
            int csgId = Convert.ToInt32(hfActiveConsig.Value);
            if (csgId <= 0) return;
            string vehicleNo = txtVehicleNo != null ? txtVehicleNo.Text.Trim().ToUpper() : "";
            if (string.IsNullOrEmpty(vehicleNo))
            { ShowAlert("Please enter a vehicle number.", false); return; }

            try
            {
                PKDatabaseHelper.DispatchConsignment(csgId, vehicleNo);
                ShowAlert("Consignment dispatched with Vehicle: " + vehicleNo, true);
                BindConsigTabs();
                BindDispatchedDropdown();
                LoadConsigDCs(csgId);
            }
            catch (Exception ex) { ShowAlert("Dispatch error: " + ex.Message, false); }
        }

        protected void btnArchiveConsig_Click(object s, EventArgs e)
        {
            int csgId = Convert.ToInt32(hfActiveConsig.Value);
            if (csgId <= 0) return;
            try
            {
                PKDatabaseHelper.ArchiveConsignment(csgId);
                ShowAlert("Consignment archived.", true);
                hfActiveConsig.Value = "0";
                hfActiveTab.Value = "retail";
                if (pnlConsigContent != null) pnlConsigContent.Visible = false;
                pnlForm.Visible = true;
                BindConsigTabs();
                BindDispatchedDropdown();
                BindArchivedDropdown();
            }
            catch (Exception ex) { ShowAlert("Archive error: " + ex.Message, false); }
        }

        protected void btnSyncFromZoho_Click(object s, EventArgs e)
        {
            int csgId = Convert.ToInt32(hfActiveConsig.Value);
            if (csgId <= 0) { ShowAlert("No consignment selected.", false); return; }

            try
            {
                var results = StockApp.DAL.ZohoHelper.SyncConsignmentBack(csgId);
                var sb = new System.Text.StringBuilder();
                sb.Append("<div style='font-size:11px;'>");
                int totalChanges = 0, totalAlerts = 0;

                foreach (var r in results)
                {
                    string icon = r.Success ? (r.Changes.Count > 0 ? "&#x1F504;" : "&#x2705;") : "&#x274C;";
                    string color = r.StockAlerts.Count > 0 ? "#e74c3c" : r.Changes.Count > 0 ? "#0078d4" : "#27ae60";
                    sb.Append("<div style='margin-bottom:6px;color:" + color + ";'>" + icon + " <strong>" + Server.HtmlEncode(r.ZohoInvoiceNo ?? "") + "</strong>");
                    if (r.Changes.Count > 0)
                    {
                        totalChanges += r.Changes.Count;
                        foreach (var c in r.Changes)
                            sb.Append("<div style='margin-left:20px;color:#333;'>" + Server.HtmlEncode(c) + "</div>");
                    }
                    else
                        sb.Append(" — No changes");
                    if (r.StockAlerts.Count > 0)
                    {
                        totalAlerts += r.StockAlerts.Count;
                        foreach (var a in r.StockAlerts)
                            sb.Append("<div style='margin-left:20px;color:#e74c3c;font-weight:700;'>STOCK ALERT: " + Server.HtmlEncode(a) + "</div>");
                    }
                    sb.Append("</div>");
                }

                sb.Append("<div style='margin-top:8px;font-weight:700;'>Summary: " + totalChanges + " change(s)");
                if (totalAlerts > 0)
                    sb.Append(", <span style='color:#e74c3c;'>" + totalAlerts + " stock alert(s)</span>");
                sb.Append("</div></div>");

                if (pnlBulkResult != null) { pnlBulkResult.Visible = true; litBulkResult.Text = sb.ToString(); }
                LoadConsigDCs(csgId);
            }
            catch (Exception ex) { ShowAlert("Sync error: " + ex.Message, false); }
        }

        protected string GetInvoiceStatusBadge(object dcIdObj)
        {
            try
            {
                int dcId = Convert.ToInt32(dcIdObj);
                var invLog = StockApp.DAL.ZohoHelper.GetInvoiceLogForDC(dcId);
                if (invLog != null && invLog["PushStatus"].ToString() == "Pushed")
                {
                    string invNo = invLog["ZohoInvoiceNo"] != System.DBNull.Value ? invLog["ZohoInvoiceNo"].ToString() : "";
                    return "<span style='color:#27ae60;font-weight:600;font-size:10px;'>✓ " + invNo + "</span>";
                }
                else if (invLog != null && invLog["PushStatus"].ToString() == "Error")
                {
                    string err = invLog["ErrorMessage"] != System.DBNull.Value ? invLog["ErrorMessage"].ToString() : "Error";
                    if (err.Length > 40) err = err.Substring(0, 40) + "…";
                    return "<span style='color:#e74c3c;font-size:10px;' title='" + Server.HtmlEncode(err) + "'>✗ Failed</span>";
                }
            }
            catch { }
            return "<span style='color:#999;font-size:10px;'>— Pending</span>";
        }

        protected void btnBulkInvoice_Click(object s, EventArgs e)
        {
            int csgId = Convert.ToInt32(hfActiveConsig.Value);
            if (csgId <= 0) { ShowAlert("No consignment selected.", false); return; }

            var dcs = PKDatabaseHelper.GetDCsByConsignment(csgId);
            int success = 0, failed = 0, skipped = 0;
            var sb = new System.Text.StringBuilder();
            sb.Append("<div style='font-size:11px;'>");

            foreach (DataRow dc in dcs.Rows)
            {
                int dcId = Convert.ToInt32(dc["DCID"]);
                string dcNum = dc["DCNumber"].ToString();
                string custName = dc["CustomerName"].ToString();
                string status = dc["Status"].ToString();

                if (status != "FINALISED")
                {
                    skipped++;
                    sb.Append("<div style='color:#999;'>⊘ " + dcNum + " (" + custName + ") — Draft, skipped</div>");
                    continue;
                }

                try
                {
                    string channel = dc.Table.Columns.Contains("Channel") && dc["Channel"] != DBNull.Value
                        ? dc["Channel"].ToString() : "GT";
                    string result = StockApp.DAL.ZohoHelper.CreateInvoiceFromDC(dcId, channel, UserID);
                    if (result.StartsWith("OK:"))
                    {
                        success++;
                        sb.Append("<div style='color:#27ae60;'>✓ " + dcNum + " (" + custName + ") — Invoice " + result.Substring(3) + " created</div>");
                    }
                    else if (result.StartsWith("UPDATED:"))
                    {
                        success++;
                        sb.Append("<div style='color:#0078d4;'>✓ " + dcNum + " (" + custName + ") — Invoice " + result.Substring(8) + " updated</div>");
                    }
                    else
                    {
                        failed++;
                        sb.Append("<div style='color:#e74c3c;'>✗ " + dcNum + " (" + custName + ") — " + Server.HtmlEncode(result) + "</div>");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    sb.Append("<div style='color:#e74c3c;'>✗ " + dcNum + " (" + custName + ") — " + Server.HtmlEncode(ex.Message) + "</div>");
                }
            }

            sb.Append("<div style='margin-top:8px;font-weight:700;'>Summary: " + success + " created, " + failed + " failed, " + skipped + " skipped</div>");
            sb.Append("</div>");

            if (pnlBulkResult != null) { pnlBulkResult.Visible = true; litBulkResult.Text = sb.ToString(); }
            LoadConsigDCs(csgId); // refresh DC list with updated invoice status
        }

        void BuildProductData()
        {
            var dt = PKDatabaseHelper.GetFGStockForShipment();
            var sb = new System.Text.StringBuilder("{");
            bool first = true;
            foreach (DataRow r in dt.Rows)
            {
                string pid = r["ProductID"].ToString();
                string unitSizes = r["UnitsPerContainer"] == DBNull.Value ? "1" : r["UnitsPerContainer"].ToString();
                string firstUnitSize = "1";
                if (!string.IsNullOrEmpty(unitSizes))
                {
                    string[] sizes = unitSizes.Split(',');
                    firstUnitSize = sizes[0].Trim();
                }
                int jpc = Convert.ToInt32(r["ContainersPerCase"]);
                int availJars = Convert.ToInt32(r["AvailableFGJars"]);
                int availCases = Convert.ToInt32(r["AvailableCases"]);
                int availLoose = Convert.ToInt32(r["AvailableLooseJars"]);
                int unitSize = 1;
                int.TryParse(firstUnitSize, out unitSize);
                if (unitSize <= 0) unitSize = 1;

                // Get MRP, HSN, GST from product
                int productId = Convert.ToInt32(pid);
                decimal mrpPcs = PKDatabaseHelper.GetProductMRP(productId, "PCS");
                decimal mrpJar = PKDatabaseHelper.GetProductMRP(productId, "JAR");
                decimal mrpBox = PKDatabaseHelper.GetProductMRP(productId, "BOX");
                decimal mrpCase = PKDatabaseHelper.GetProductMRP(productId, "CASE");
                string hsn = r.Table.Columns.Contains("HSNCode") && r["HSNCode"] != DBNull.Value ? r["HSNCode"].ToString() : "";
                decimal gstRate = r.Table.Columns.Contains("GSTRate") && r["GSTRate"] != DBNull.Value ? Convert.ToDecimal(r["GSTRate"]) : 0;

                if (!first) sb.Append(",");
                sb.Append("\"" + pid + "\":{");
                sb.Append("\"name\":\"" + Esc(r["ProductName"].ToString()) + "\",");
                sb.Append("\"code\":\"" + Esc(r["ProductCode"].ToString()) + "\",");
                sb.Append("\"unitSize\":" + unitSize + ",");
                sb.Append("\"jarsPerCase\":" + jpc + ",");
                sb.Append("\"availJars\":" + availJars + ",");
                sb.Append("\"availCases\":" + availCases + ",");
                sb.Append("\"availLoose\":" + availLoose + ",");
                sb.Append("\"hsn\":\"" + Esc(hsn) + "\",");
                sb.Append("\"gstRate\":" + gstRate.ToString("0.##") + ",");
                sb.Append("\"mrpPcs\":" + mrpPcs.ToString("0.##") + ",");
                sb.Append("\"mrpJar\":" + mrpJar.ToString("0.##") + ",");
                sb.Append("\"mrpBox\":" + mrpBox.ToString("0.##") + ",");
                sb.Append("\"mrpCase\":" + mrpCase.ToString("0.##") + ",");
                string ct = r.Table.Columns.Contains("ContainerType") && r["ContainerType"] != DBNull.Value
                    ? r["ContainerType"].ToString() : "JAR";
                sb.Append("\"containerType\":\"" + Esc(ct) + "\"");
                sb.Append("}");
                first = false;
            }
            sb.Append("}");
            if (hfProductData != null) hfProductData.Value = sb.ToString();
        }

        void BuildCustomerData(string typeFilter = null)
        {
            DataTable dt;
            if (!string.IsNullOrEmpty(typeFilter))
                dt = PKDatabaseHelper.GetActiveCustomersByType(typeFilter);
            else
                dt = PKDatabaseHelper.GetActiveCustomers();
            var sb = new System.Text.StringBuilder("{");
            bool first = true;
            foreach (DataRow r in dt.Rows)
            {
                int cid = Convert.ToInt32(r["CustomerID"]);
                string gstin = r["GSTIN"] != DBNull.Value ? r["GSTIN"].ToString() : "";
                string state = r["State"] != DBNull.Value ? r["State"].ToString() : "";
                decimal smPct = 0, gtPct = 0;
                var margins = PKDatabaseHelper.GetCustomerMargins(cid);
                if (margins != null)
                {
                    smPct = Convert.ToDecimal(margins["SuperMarketPct"]);
                    gtPct = Convert.ToDecimal(margins["GTPct"]);
                }

                if (!first) sb.Append(",");
                sb.Append("\"" + cid + "\":{");
                sb.Append("\"gstin\":\"" + Esc(gstin) + "\",");
                sb.Append("\"state\":\"" + Esc(state) + "\",");
                sb.Append("\"sm\":" + smPct.ToString("0.##") + ",");
                sb.Append("\"gt\":" + gtPct.ToString("0.##"));
                sb.Append("}");
                first = false;
            }
            sb.Append("}");
            if (hfCustomerData != null) hfCustomerData.Value = sb.ToString();
        }

        string Esc(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "").Replace("\r", "");
        }

        string GetCustomerTypeFilter()
        {
            string tab = hfActiveTab != null ? hfActiveTab.Value : "retail";
            return tab == "retail" ? "RT" : "DI,ST";
        }

        void BindDCList()
        {
            string tab = hfActiveTab != null ? hfActiveTab.Value : "retail";
            if (tab == "retail")
            {
                LoadRetailDCs();
            }
            else
            {
                int csgId = 0;
                int.TryParse(hfActiveConsig.Value, out csgId);
                if (csgId > 0)
                {
                    BindConsigTabs();
                    LoadConsigDCs(csgId);
                }
            }
        }

        // ── DRAFT SAVE ──
        protected void btnDraftSave_Click(object s, EventArgs e)
        {
            if (!ValidateForm()) return;

            int customerId = Convert.ToInt32(ddlCustomer.SelectedValue);
            DateTime dcDate = DateTime.Parse(txtDCDate.Text);
            string remarks = txtRemarks.Text.Trim();
            string channel = ddlChannel != null ? ddlChannel.SelectedValue : "GT";
            int dcId = Convert.ToInt32(hfDCID.Value);

            var lineData = ParseLines();
            if (lineData == null || lineData.Length == 0)
            { ShowAlert("Please add at least one product line.", false); return; }

            // Consignment validation — mandatory for consignment tabs, not for retail
            int consignmentId = 0;
            int.TryParse(hfActiveConsig.Value, out consignmentId);
            string activeTab = hfActiveTab != null ? hfActiveTab.Value : "retail";
            if (activeTab != "retail" && consignmentId <= 0 && dcId <= 0)
            { ShowAlert("Please select a Consignment before saving.", false); return; }

            // Stock validation — source-based (CASE or LOOSE)
            var fgStock = PKDatabaseHelper.GetFGStockForShipment();
            var productNeeds = new System.Collections.Generic.Dictionary<int, int[]>(); // pid → [casesNeeded, looseNeeded]
            foreach (var line in lineData)
            {
                DataRow stockRow = null;
                foreach (DataRow r in fgStock.Rows)
                    if (Convert.ToInt32(r["ProductID"]) == line.ProductID) { stockRow = r; break; }
                if (stockRow == null)
                { ShowAlert("Product ID " + line.ProductID + " has no FG stock.", false); return; }

                int jpc = Convert.ToInt32(stockRow["ContainersPerCase"]);
                if (jpc <= 0) jpc = 12;

                if (!productNeeds.ContainsKey(line.ProductID))
                    productNeeds[line.ProductID] = new int[] { 0, 0 };

                string src = string.IsNullOrEmpty(line.Source) ? "CASE" : line.Source;
                if (src == "CASE")
                {
                    int cases = line.SellingForm == "CASE" ? line.Qty : (int)Math.Ceiling((double)line.Qty / jpc);
                    productNeeds[line.ProductID][0] += cases;
                }
                else
                {
                    productNeeds[line.ProductID][1] += line.Qty;
                }
            }

            foreach (var kvp in productNeeds)
            {
                int pid = kvp.Key;
                int casesNeeded = kvp.Value[0];
                int looseNeeded = kvp.Value[1];

                DataRow stockRow = null;
                foreach (DataRow r in fgStock.Rows)
                    if (Convert.ToInt32(r["ProductID"]) == pid) { stockRow = r; break; }
                if (stockRow == null) continue;

                int availCases = Convert.ToInt32(stockRow["AvailableCases"]);
                int availLoose = Convert.ToInt32(stockRow["AvailableLooseJars"]);
                string prodName = stockRow["ProductName"].ToString();

                // If editing existing DC, add back this DC's allocation
                if (dcId > 0)
                {
                    var existingLines = PKDatabaseHelper.GetDCLines(dcId);
                    foreach (DataRow el in existingLines.Rows)
                    {
                        if (Convert.ToInt32(el["ProductID"]) != pid) continue;
                        string elSrc = el.Table.Columns.Contains("Source") && el["Source"] != DBNull.Value
                            ? el["Source"].ToString() : "CASE";
                        int elQty = Convert.ToInt32(el["TotalPcs"]);
                        int jpc2 = Convert.ToInt32(stockRow["ContainersPerCase"]);
                        if (jpc2 <= 0) jpc2 = 12;
                        if (elSrc == "CASE") availCases += (int)Math.Ceiling((double)elQty / jpc2);
                        else availLoose += elQty;
                    }
                }

                if (casesNeeded > availCases)
                {
                    ShowAlert("Insufficient CASES for " + prodName + ". Need " + casesNeeded + ", available " + availCases + ".", false);
                    return;
                }
                if (looseNeeded > availLoose)
                {
                    ShowAlert("Insufficient loose stock for " + prodName + ". Need " + looseNeeded + ", available " + availLoose + ".", false);
                    return;
                }
            }

            // Get customer GSTIN
            var custRow = PKDatabaseHelper.GetCustomerById(customerId);
            string custGSTIN = custRow != null && custRow["GSTIN"] != DBNull.Value ? custRow["GSTIN"].ToString() : "";
            string custState = custRow != null && custRow["State"] != DBNull.Value ? custRow["State"].ToString() : "";
            string orgState = hfOrgState != null ? hfOrgState.Value : "Tamil Nadu";
            bool isInterState = !string.IsNullOrEmpty(custState) &&
                custState.Trim().ToLower() != orgState.Trim().ToLower();

            // Calculate totals
            decimal subTotal = 0, totalCGST = 0, totalSGST = 0, totalIGST = 0, grandTotal = 0;
            foreach (var line in lineData)
            {
                subTotal += line.TaxableVal;
                if (isInterState)
                    totalIGST += line.GSTAmt;
                else
                {
                    totalCGST += Math.Round(line.GSTAmt / 2, 2);
                    totalSGST += Math.Round(line.GSTAmt / 2, 2);
                }
                grandTotal += line.LineTotal;
            }

            try
            {
                if (dcId == 0)
                {
                    dcId = PKDatabaseHelper.CreateDeliveryChallan(customerId, dcDate, remarks, UserID, consignmentId);
                }
                else
                {
                    PKDatabaseHelper.UpdateDCHeader(dcId, customerId, dcDate, remarks);
                    PKDatabaseHelper.DeleteDCLines(dcId);
                }

                // Update DC header with channel, GSTIN, and totals
                PKDatabaseHelper.UpdateDCPricing(dcId, channel, custGSTIN, isInterState,
                    subTotal, totalCGST, totalSGST, totalIGST, grandTotal);

                // Save lines with pricing — selling form approach
                foreach (var line in lineData)
                {
                    decimal cgst = isInterState ? 0 : Math.Round(line.GSTAmt / 2, 2);
                    decimal sgst = isInterState ? 0 : Math.Round(line.GSTAmt / 2, 2);
                    decimal igst = isInterState ? line.GSTAmt : 0;
                    PKDatabaseHelper.AddDCLineWithPricing(dcId, line.ProductID, 0, 0,
                        0, line.Qty, line.HSN, line.GSTRate, line.MRP, line.MarginPct,
                        line.UnitRate, line.TaxableVal, cgst, sgst, igst, line.LineTotal);
                    // Save selling form
                    PKDatabaseHelper.UpdateDCLineSellingForm(dcId, line.ProductID, line.SellingForm, line.Source ?? "CASE");
                }

                hfDCID.Value = dcId.ToString();
                var dc = PKDatabaseHelper.GetDCById(dcId);
                if (dc != null) txtDCNumber.Text = dc["DCNumber"].ToString();

                // Save transport details
                string transport = ddlTransport != null ? ddlTransport.SelectedValue : "";
                string courier = txtCourierName != null ? txtCourierName.Text.Trim() : "";
                string tracking = txtTrackingNo != null ? txtTrackingNo.Text.Trim() : "";
                if (!string.IsNullOrEmpty(transport))
                    PKDatabaseHelper.UpdateDCTransport(dcId, transport, courier, tracking);

                ShowAlert("Delivery Challan saved as Draft. Grand Total: ₹" + grandTotal.ToString("N2"), true);
                BuildProductData();
                BuildCustomerData(GetCustomerTypeFilter());
                BindDCList();
                // Show invoice button after save
                if (btnCreateInvoiceDraft != null)
                {
                    btnCreateInvoiceDraft.Visible = true;
                    try
                    {
                        var invLog = StockApp.DAL.ZohoHelper.GetInvoiceLogForDC(dcId);
                        bool hasInv = invLog != null && invLog["PushStatus"].ToString() == "Pushed";
                        btnCreateInvoiceDraft.Text = hasInv ? "Update Invoice in Zoho" : "Create Invoice in Zoho";
                    }
                    catch { }
                }
                if (btnDeleteDC != null) btnDeleteDC.Visible = true;
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        // ── FINALISE ──
        protected void btnFinalise_Click(object s, EventArgs e)
        {
            int dcId = Convert.ToInt32(hfDCID.Value);
            if (dcId == 0)
            {
                // First save as draft, then finalise
                btnDraftSave_Click(s, e);
                dcId = Convert.ToInt32(hfDCID.Value);
                if (dcId == 0) return; // save failed
            }

            try
            {
                // Generate invoice number
                string invoiceNo = PKDatabaseHelper.GenerateInvoiceNumber();
                PKDatabaseHelper.SetDCInvoiceNumber(dcId, invoiceNo);

                // Finalise the DC
                PKDatabaseHelper.FinaliseDeliveryChallan(dcId, UserID);

                // Auto-push invoice to Zoho Books
                string channel = ddlChannel != null ? ddlChannel.SelectedValue : "GT";
                string zohoResult = "";
                try
                {
                    zohoResult = StockApp.DAL.ZohoHelper.CreateInvoiceFromDC(dcId, channel, UserID);
                }
                catch (Exception zex) { zohoResult = "Zoho error: " + zex.Message; }

                string msg = "DC finalised. Invoice: " + invoiceNo + ".";
                if (zohoResult.StartsWith("OK:"))
                    msg += " Zoho invoice " + zohoResult.Substring(3) + " created.";
                else if (!string.IsNullOrEmpty(zohoResult))
                    msg += " Zoho: " + zohoResult;

                ShowAlert(msg, true);
                LoadDC(dcId);
                BindDCList();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        // ── CREATE ZOHO INVOICE (from Draft) ──
        protected void btnCreateInvoiceDraft_Click(object s, EventArgs e)
        {
            // Save draft first
            btnDraftSave_Click(s, e);
            int dcId = Convert.ToInt32(hfDCID.Value);
            if (dcId == 0) return;

            string channel = ddlChannel != null ? ddlChannel.SelectedValue : "GT";
            try
            {
                string result = StockApp.DAL.ZohoHelper.CreateInvoiceFromDC(dcId, channel, UserID);
                if (result.StartsWith("OK:"))
                    ShowAlert("Draft saved. Zoho Invoice " + result.Substring(3) + " created.", true);
                else if (result.StartsWith("UPDATED:"))
                    ShowAlert("Draft saved. Zoho Invoice " + result.Substring(8) + " updated.", true);
                else
                    ShowAlert("Draft saved. Zoho: " + result, false);
                LoadDC(dcId);
            }
            catch (Exception ex) { ShowAlert("Invoice error: " + ex.Message, false); }
        }

        // ── CREATE/UPDATE ZOHO INVOICE ──
        protected void btnCreateInvoice_Click(object s, EventArgs e)
        {
            int dcId = Convert.ToInt32(hfDCID.Value);
            if (dcId == 0) { ShowAlert("No DC selected.", false); return; }

            string channel = ddlChannel != null ? ddlChannel.SelectedValue : "GT";

            try
            {
                string result = StockApp.DAL.ZohoHelper.CreateInvoiceFromDC(dcId, channel, UserID);
                if (result.StartsWith("OK:"))
                    ShowAlert("Zoho Invoice " + result.Substring(3) + " created successfully.", true);
                else if (result.StartsWith("UPDATED:"))
                    ShowAlert("Zoho Invoice " + result.Substring(8) + " updated successfully.", true);
                else
                    ShowAlert(result, false);
                LoadDC(dcId);
            }
            catch (Exception ex) { ShowAlert("Invoice error: " + ex.Message, false); }
        }

        // ── DOWNLOAD INVOICE PDF ──
        protected void btnDownloadInvoicePDF_Click(object s, EventArgs e)
        {
            string zohoInvId = ViewState["ZohoInvoiceID"] as string;
            if (string.IsNullOrEmpty(zohoInvId))
            {
                int dcId = Convert.ToInt32(hfDCID.Value);
                if (dcId > 0)
                {
                    var invLog = StockApp.DAL.ZohoHelper.GetInvoiceLogForDC(dcId);
                    if (invLog != null && invLog["ZohoInvoiceID"] != DBNull.Value)
                        zohoInvId = invLog["ZohoInvoiceID"].ToString();
                }
            }

            if (string.IsNullOrEmpty(zohoInvId))
            { ShowAlert("No Zoho invoice found for this DC.", false); return; }

            try
            {
                byte[] pdfBytes = StockApp.DAL.ZohoHelper.GetInvoicePDF(zohoInvId);
                Response.Clear();
                Response.ContentType = "application/pdf";
                string dcNum = hfDCID.Value;
                Response.AddHeader("Content-Disposition", "attachment; filename=Invoice_DC" + dcNum + ".pdf");
                Response.BinaryWrite(pdfBytes);
                Response.End();
            }
            catch (Exception ex)
            {
                ShowAlert("PDF download error: " + ex.Message, false);
            }
        }

        // ── NEW ──
        protected void btnNew_Click(object s, EventArgs e)
        {
            hfDCID.Value = "0";
            hfLines.Value = "";
            txtDCNumber.Text = "";
            txtDCDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtRemarks.Text = "";
            if (ddlCustomer != null) ddlCustomer.SelectedIndex = 0;
            lblFormTitle.Text = "New Delivery Challan";
            pnlForm.Visible = true;
            pnlLocked.Visible = false;
            btnDraftSave.Visible = true;
            btnFinalise.Visible = true;
            if (btnDeleteDC != null) btnDeleteDC.Visible = false;
            BuildProductData();
            if (pnlAlert != null) pnlAlert.Visible = false;
        }

        // ── DELETE DRAFT DC ──
        protected void btnDeleteDC_Click(object s, EventArgs e)
        {
            int dcId = Convert.ToInt32(hfDCID.Value);
            if (dcId == 0) { ShowAlert("No DC selected to delete.", false); return; }

            try
            {
                var dc = PKDatabaseHelper.GetDCById(dcId);
                if (dc == null) { ShowAlert("DC not found.", false); return; }
                if (dc["Status"].ToString() != "DRAFT")
                { ShowAlert("Only DRAFT DCs can be deleted. Finalised DCs cannot be removed.", false); return; }

                string dcNumber = dc["DCNumber"].ToString();
                PKDatabaseHelper.DeleteDraftDC(dcId);

                // Reset form to new
                hfDCID.Value = "0";
                hfLines.Value = "";
                txtDCNumber.Text = "";
                txtDCDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtRemarks.Text = "";
                if (ddlCustomer != null) ddlCustomer.SelectedIndex = 0;
                lblFormTitle.Text = "New Delivery Challan";

                ShowAlert(dcNumber + " deleted. All reserved stock has been freed.", true);
                BuildProductData();
                BindDCList();
            }
            catch (Exception ex) { ShowAlert("Error deleting DC: " + ex.Message, false); }
        }

        // ── UNCONVERT DC (from the DC form — for SA-originated DCs) ──────
        // Fix 5: When the user opens a DRAFT DC that was created from a Sales Force shipment order,
        // show an "Unconvert DC" button. Pressing it deletes the DC and flips the SA shipment back to "Order".
        protected void btnUnconvertDCFromForm_Click(object s, EventArgs e)
        {
            int dcId = Convert.ToInt32(hfDCID.Value);
            if (dcId == 0) { ShowAlert("No DC selected.", false); return; }
            try
            {
                var dc = PKDatabaseHelper.GetDCById(dcId);
                if (dc == null) { ShowAlert("DC not found.", false); return; }
                if (dc["Status"].ToString() != "DRAFT")
                { ShowAlert("Only DRAFT DCs can be unconverted.", false); return; }

                string rem = dc["Remarks"] == DBNull.Value ? "" : dc["Remarks"].ToString();
                var m = System.Text.RegularExpressions.Regex.Match(rem, @"^SH-(\d{5})");
                if (!m.Success)
                { ShowAlert("This DC was not created from a Sales Force order.", false); return; }

                int shipId = Convert.ToInt32(m.Groups[1].Value);
                PKDatabaseHelper.UnconvertSAShipmentDC(shipId);

                // Reset form state — the DC no longer exists
                hfDCID.Value = "0";
                hfLines.Value = "";
                txtDCNumber.Text = "";
                txtDCDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtRemarks.Text = "";
                if (ddlCustomer != null) ddlCustomer.SelectedIndex = 0;
                lblFormTitle.Text = "New Delivery Challan";
                pnlForm.Visible = false;

                ShowAlert("DC unconverted. SH-" + shipId.ToString("D5") + " is back in Order status — SA team can edit it again.", true);
                BuildProductData();
                BindDCList();
                BindSAOrders();
                // Stay on the SA Orders sub-tab so the user sees the restored order
                if (hfSubTab != null) hfSubTab.Value = "sa";
            }
            catch (Exception ex) { ShowAlert("Error unconverting DC: " + ex.Message, false); }
        }

        // ── LOAD EXISTING DC ──
        protected void rptDCs_ItemCommand(object src, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "EditDC")
            {
                int dcId = Convert.ToInt32(e.CommandArgument);
                LoadDC(dcId);
            }
        }

        void LoadDC(int dcId)
        {
            var dc = PKDatabaseHelper.GetDCById(dcId);
            if (dc == null) return;

            hfDCID.Value = dcId.ToString();
            txtDCNumber.Text = dc["DCNumber"].ToString();
            txtDCDate.Text = Convert.ToDateTime(dc["DCDate"]).ToString("yyyy-MM-dd");
            txtRemarks.Text = dc["Remarks"] == DBNull.Value ? "" : dc["Remarks"].ToString();
            try { ddlCustomer.SelectedValue = dc["CustomerID"].ToString(); } catch { }

            // Restore channel
            if (ddlChannel != null && dc.Table.Columns.Contains("Channel") && dc["Channel"] != DBNull.Value)
            {
                try { ddlChannel.SelectedValue = dc["Channel"].ToString(); } catch { }
            }

            string status = dc["Status"].ToString();
            bool isDraft = status == "DRAFT";

            // Load lines into hfLines as JSON — include pricing
            var lines = PKDatabaseHelper.GetDCLines(dcId);
            var sb = new System.Text.StringBuilder("[");
            bool first = true;
            foreach (DataRow r in lines.Rows)
            {
                if (!first) sb.Append(",");
                sb.Append("{\"pid\":\"" + r["ProductID"] + "\",");
                sb.Append("\"name\":\"" + Esc(r["ProductName"].ToString()) + "\",");
                sb.Append("\"code\":\"" + Esc(r["ProductCode"].ToString()) + "\",");
                string sellingForm = r.Table.Columns.Contains("SellingForm") && r["SellingForm"] != DBNull.Value
                    ? r["SellingForm"].ToString() : "JAR";
                int qty = r.Table.Columns.Contains("TotalPcs") ? Convert.ToInt32(r["TotalPcs"]) : 0;
                sb.Append("\"form\":\"" + sellingForm + "\",");
                string sourceVal = r.Table.Columns.Contains("Source") && r["Source"] != DBNull.Value
                    ? r["Source"].ToString() : "CASE";
                sb.Append("\"source\":\"" + sourceVal + "\",");
                sb.Append("\"qty\":" + qty + ",");
                // Pricing fields — use saved values or defaults
                string hsn = r.Table.Columns.Contains("HSNCode") && r["HSNCode"] != DBNull.Value ? r["HSNCode"].ToString() : "";
                decimal gstRate = r.Table.Columns.Contains("GSTRate") && r["GSTRate"] != DBNull.Value ? Convert.ToDecimal(r["GSTRate"]) : 0;
                decimal mrp = r.Table.Columns.Contains("MRP") && r["MRP"] != DBNull.Value ? Convert.ToDecimal(r["MRP"]) : 0;
                decimal marginPct = r.Table.Columns.Contains("MarginPct") && r["MarginPct"] != DBNull.Value ? Convert.ToDecimal(r["MarginPct"]) : 0;
                decimal unitRate = r.Table.Columns.Contains("UnitRate") && r["UnitRate"] != DBNull.Value ? Convert.ToDecimal(r["UnitRate"]) : 0;
                decimal taxableVal = r.Table.Columns.Contains("TaxableValue") && r["TaxableValue"] != DBNull.Value ? Convert.ToDecimal(r["TaxableValue"]) : 0;
                decimal gstAmt = 0;
                if (r.Table.Columns.Contains("CGSTAmt")) gstAmt += r["CGSTAmt"] != DBNull.Value ? Convert.ToDecimal(r["CGSTAmt"]) : 0;
                if (r.Table.Columns.Contains("SGSTAmt")) gstAmt += r["SGSTAmt"] != DBNull.Value ? Convert.ToDecimal(r["SGSTAmt"]) : 0;
                if (r.Table.Columns.Contains("IGSTAmt")) gstAmt += r["IGSTAmt"] != DBNull.Value ? Convert.ToDecimal(r["IGSTAmt"]) : 0;
                decimal lineTotal = r.Table.Columns.Contains("LineTotal") && r["LineTotal"] != DBNull.Value ? Convert.ToDecimal(r["LineTotal"]) : 0;
                sb.Append("\"hsn\":\"" + Esc(hsn) + "\",");
                sb.Append("\"gstRate\":" + gstRate.ToString("0.##") + ",");
                sb.Append("\"mrp\":" + mrp.ToString("0.##") + ",");
                sb.Append("\"marginPct\":" + marginPct.ToString("0.##") + ",");
                sb.Append("\"rate\":" + unitRate.ToString("0.##") + ",");
                sb.Append("\"taxableVal\":" + taxableVal.ToString("0.##") + ",");
                sb.Append("\"gstAmt\":" + gstAmt.ToString("0.##") + ",");
                sb.Append("\"lineTotal\":" + lineTotal.ToString("0.##") + "}");
                first = false;
            }
            sb.Append("]");
            hfLines.Value = sb.ToString();

            if (isDraft)
            {
                lblFormTitle.Text = "Edit DC: " + dc["DCNumber"];
                pnlForm.Visible = true;
                pnlLocked.Visible = false;
                btnDraftSave.Visible = true;
                btnFinalise.Visible = true;
                if (btnDeleteDC != null) btnDeleteDC.Visible = true;

                // Fix 5: Show Unconvert button when this DC originated from an SA shipment order.
                // Detection: Remarks starts with "SH-" followed by a 5-digit shipment number (set by ConvertSAShipmentToDC).
                if (btnUnconvertDCFromForm != null)
                {
                    string rem = dc["Remarks"] == DBNull.Value ? "" : dc["Remarks"].ToString();
                    btnUnconvertDCFromForm.Visible = System.Text.RegularExpressions.Regex.IsMatch(rem, @"^SH-\d{5}");
                }

                // Restore consignment selection
                if (dc.Table.Columns.Contains("ConsignmentID") && dc["ConsignmentID"] != DBNull.Value)
                {
                    hfActiveConsig.Value = dc["ConsignmentID"].ToString();
                }

                // Restore transport mode
                if (ddlTransport != null && dc.Table.Columns.Contains("TransportMode") && dc["TransportMode"] != DBNull.Value)
                {
                    string tm = dc["TransportMode"].ToString();
                    if (ddlTransport.Items.FindByValue(tm) != null) ddlTransport.SelectedValue = tm;
                }
                if (txtCourierName != null && dc.Table.Columns.Contains("CourierName") && dc["CourierName"] != DBNull.Value)
                    txtCourierName.Text = dc["CourierName"].ToString();
                if (txtTrackingNo != null && dc.Table.Columns.Contains("TrackingNumber") && dc["TrackingNumber"] != DBNull.Value)
                    txtTrackingNo.Text = dc["TrackingNumber"].ToString();

                if (btnCreateInvoiceDraft != null)
                {
                    btnCreateInvoiceDraft.Visible = true;
                    var invLog = StockApp.DAL.ZohoHelper.GetInvoiceLogForDC(dcId);
                    bool hasInvoice = invLog != null && invLog["PushStatus"].ToString() == "Pushed";
                    btnCreateInvoiceDraft.Text = hasInvoice ? "Update Invoice in Zoho" : "Create Invoice in Zoho";
                }
            }
            else
            {
                pnlForm.Visible = false;
                pnlLocked.Visible = true;

                // Fix 5: finalised DC cannot be unconverted
                if (btnUnconvertDCFromForm != null) btnUnconvertDCFromForm.Visible = false;

                // Populate read-only view
                if (lblLockedTitle != null)
                {
                    string invNo = dc.Table.Columns.Contains("InvoiceNumber") && dc["InvoiceNumber"] != DBNull.Value
                        ? dc["InvoiceNumber"].ToString() : "";
                    lblLockedTitle.Text = "Delivery Challan — " + dc["DCNumber"]
                        + (!string.IsNullOrEmpty(invNo) ? " | Invoice: " + invNo : "");
                }
                if (lblViewDCNum != null) lblViewDCNum.Text = dc["DCNumber"].ToString();
                if (lblViewDate != null) lblViewDate.Text = Convert.ToDateTime(dc["DCDate"]).ToString("dd-MMM-yyyy");
                if (lblViewCustomer != null)
                {
                    string channel = dc.Table.Columns.Contains("Channel") && dc["Channel"] != DBNull.Value
                        ? dc["Channel"].ToString() : "";
                    string channelLabel = channel == "SM" ? "Super Market" : channel == "GT" ? "General Trade" : "";
                    lblViewCustomer.Text = dc["CustomerName"] + " (" + dc["CustomerCode"] + ")"
                        + (!string.IsNullOrEmpty(channelLabel) ? " — " + channelLabel : "");
                }

                string rem = dc["Remarks"] == DBNull.Value ? "" : dc["Remarks"].ToString();
                if (pnlViewRemarks != null) { pnlViewRemarks.Visible = !string.IsNullOrEmpty(rem); }
                if (lblViewRemarks != null) lblViewRemarks.Text = rem;

                if (rptViewLines != null) { rptViewLines.DataSource = lines; rptViewLines.DataBind(); }

                // Zoho Invoice status
                if (pnlCreateInvoice != null && pnlInvoiceStatus != null && pnlInvoiceError != null)
                {
                    try
                    {
                        var invLog = StockApp.DAL.ZohoHelper.GetInvoiceLogForDC(dcId);
                        if (invLog != null && invLog["PushStatus"].ToString() == "Pushed")
                        {
                            string zohoInvId = invLog["ZohoInvoiceID"].ToString();
                            string zohoInvNo = invLog["ZohoInvoiceNo"] != DBNull.Value ? invLog["ZohoInvoiceNo"].ToString() : "";
                            string zohoStatus = invLog["ZohoStatus"] != DBNull.Value ? invLog["ZohoStatus"].ToString() : "draft";

                            pnlCreateInvoice.Visible = false;
                            pnlInvoiceStatus.Visible = true;
                            pnlInvoiceError.Visible = false;

                            if (lblInvoiceNo != null) lblInvoiceNo.Text = zohoInvNo;
                            if (lblInvoiceZohoStatus != null)
                            {
                                lblInvoiceZohoStatus.Text = zohoStatus.ToUpper();
                                lblInvoiceZohoStatus.ForeColor = zohoStatus == "paid" ? System.Drawing.Color.Green
                                    : zohoStatus == "overdue" ? System.Drawing.Color.Red
                                    : System.Drawing.Color.FromArgb(0, 120, 212);
                            }

                            // Grand total from DC header
                            decimal gt = dc.Table.Columns.Contains("GrandTotal") && dc["GrandTotal"] != DBNull.Value
                                ? Convert.ToDecimal(dc["GrandTotal"]) : 0;
                            if (lblInvoiceAmount != null)
                                lblInvoiceAmount.Text = "₹" + gt.ToString("N2");

                            string orgId = StockApp.DAL.ZohoHelper.GetOrgId();
                            if (lnkViewInZoho != null)
                                lnkViewInZoho.NavigateUrl = "https://books.zoho.in/app/" + orgId + "/invoices/" + zohoInvId;

                            ViewState["ZohoInvoiceID"] = zohoInvId;
                        }
                        else
                        {
                            pnlCreateInvoice.Visible = true;
                            pnlInvoiceStatus.Visible = false;
                            pnlInvoiceError.Visible = false;

                            if (invLog != null && invLog["PushStatus"].ToString() == "Error")
                            {
                                pnlInvoiceError.Visible = true;
                                if (lblInvoiceError != null)
                                    lblInvoiceError.Text = invLog["ErrorMessage"] != DBNull.Value ? invLog["ErrorMessage"].ToString() : "Unknown error";
                            }
                        }
                    }
                    catch
                    {
                        pnlCreateInvoice.Visible = true;
                        pnlInvoiceStatus.Visible = false;
                        pnlInvoiceError.Visible = false;
                    }
                }
            }

            BuildProductData();
            BuildCustomerData(GetCustomerTypeFilter());
            if (pnlAlert != null) pnlAlert.Visible = false;
        }

        // ── SA SHIPMENT ORDERS ──────────────────────────────────────────

        void BindSAOrders()
        {
            int csgId = 0;
            int.TryParse(hfActiveConsig.Value, out csgId);
            DataTable dt;
            if (csgId > 0)
                dt = PKDatabaseHelper.GetSAShipmentOrdersByConsignment(csgId);
            else
                dt = PKDatabaseHelper.GetSAShipmentOrders();
            bool hasRows = dt.Rows.Count > 0;
            if (pnlSAEmpty != null) pnlSAEmpty.Visible = !hasRows;
            if (pnlSAList != null) pnlSAList.Visible = hasRows;
            if (rptSAOrders != null) { rptSAOrders.DataSource = dt; rptSAOrders.DataBind(); }
        }

        protected string GetSAStatusBadge(string status)
        {
            switch (status)
            {
                case "Order": return "<span class='badge-order'>Order</span>";
                case "DC": return "<span class='badge-dc'>DC</span>";
                case "Shipped": return "<span class='badge-shipped'>Shipped</span>";
                default: return "<span class='badge-draft'>" + status + "</span>";
            }
        }

        protected void rptSAOrders_ItemCommand(object src, RepeaterCommandEventArgs e)
        {
            // Fix 2: any action triggered from the SA Orders sub-tab stays on SA Orders
            if (hfSubTab != null) hfSubTab.Value = "sa";

            int shipId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "ViewSAOrder")
            {
                LoadSAOrderDetail(shipId);
            }
            else if (e.CommandName == "EditSAOrder")
            {
                LoadSAOrderDetail(shipId);
            }
            else if (e.CommandName == "ConvertDC")
            {
                DoConvertToDC(shipId);
            }
            else if (e.CommandName == "UnconvertDC")
            {
                DoUnconvertDC(shipId);
            }
            else if (e.CommandName == "Dispatch")
            {
                DoCompleteDispatch(shipId);
            }
        }

        void LoadSAOrderDetail(int shipId)
        {
            // Fix 2: persist sub-tab across postback — we're on SA Orders
            if (hfSubTab != null) hfSubTab.Value = "sa";

            hfSAShipId.Value = shipId.ToString();
            lblSAOrderId.Text = "SH-" + shipId.ToString("D5");

            // Get shipment header
            DataRow order = PKDatabaseHelper.GetSAShipmentById(shipId);
            if (order == null) return;

            lblSACustomer.Text = order["CustomerName"].ToString();
            lblSADate.Text = Convert.ToDateTime(order["ShipmentDate"]).ToString("dd-MMM-yyyy");
            lblSAArea.Text = order["AreaName"] + " (" + order["ZoneName"] + " / " + order["RegionName"] + ")";
            if (lblSAChannel != null) lblSAChannel.Text = order["ChannelName"].ToString();
            if (lblSATransport != null) lblSATransport.Text = order["TransportMode"].ToString();

            string status = order["Status"].ToString();
            lblSAStatus.Text = status;

            bool canEdit = (status == "Order" || status == "DC");
            bool isShipped = (status == "Shipped");

            if (btnConvertDC != null) btnConvertDC.Visible = (status == "Order");
            if (btnUnconvertDC != null) btnUnconvertDC.Visible = (status == "DC");
            if (btnDispatch != null) btnDispatch.Visible = (status == "DC");
            if (btnSaveSAEdit != null) btnSaveSAEdit.Visible = canEdit;
            if (pnlSAEditLines != null) pnlSAEditLines.Visible = canEdit;

            // Load FG stock check table
            var stockLines = PKDatabaseHelper.CheckFGStockForSAOrder(shipId);
            if (rptSALines != null) { rptSALines.DataSource = stockLines; rptSALines.DataBind(); }

            // Load editable lines (must come after BuildSAStockProducts so ItemDataBound can see it)
            if (canEdit)
            {
                // Fix 3: Build product options with stock in the label — matches DC form pattern
                // Cache the per-product stock breakdown on the page so ItemDataBound can reuse it.
                BuildSAEditProductOptions();

                var editLines = PKDatabaseHelper.GetSAShipmentLines(shipId);
                if (rptSAEditLines != null) { rptSAEditLines.DataSource = editLines; rptSAEditLines.DataBind(); }
            }

            pnlSADetail.Visible = true;
        }

        /// <summary>Fix 3: Build the product option HTML used both for repeater rows and JS-added lines.
        /// Label format matches the DC form: "Name (CODE) — N cases + N loose jars".
        /// Pulls from GetFGStockForShipment() — the same source of truth as the DC product dropdown.</summary>
        private void BuildSAEditProductOptions()
        {
            var stock = PKDatabaseHelper.GetFGStockForShipment();
            // Cache for rptSAEditLines_ItemDataBound
            ViewState["SAEditStockList"] = stock;

            var sb = new System.Text.StringBuilder();
            foreach (DataRow r in stock.Rows)
            {
                string pid = r["ProductID"].ToString();
                sb.Append("<option value='").Append(pid).Append("'>")
                  .Append(FormatSAEditOptionLabel(r))
                  .Append("</option>");
            }
            if (hfSAProductOptions != null) hfSAProductOptions.Value = sb.ToString();
        }

        /// <summary>Shared formatter so the JS-added rows and the ItemDataBound rows look identical.</summary>
        private string FormatSAEditOptionLabel(DataRow r)
        {
            string name = r["ProductName"].ToString();
            string code = r["ProductCode"].ToString();
            int availCases = r.Table.Columns.Contains("AvailableCases") && r["AvailableCases"] != DBNull.Value
                ? Convert.ToInt32(r["AvailableCases"]) : 0;
            int availLoose = r.Table.Columns.Contains("AvailableLooseJars") && r["AvailableLooseJars"] != DBNull.Value
                ? Convert.ToInt32(r["AvailableLooseJars"]) : 0;
            string ct = r.Table.Columns.Contains("ContainerType") && r["ContainerType"] != DBNull.Value
                ? r["ContainerType"].ToString() : "JAR";
            string looseLabel = ct == "BOX" ? "loose boxes" : "loose jars";
            string suffix = availCases + " cases";
            if (availLoose > 0) suffix += " + " + availLoose + " " + looseLabel;
            return name + " (" + code + ") — " + suffix;
        }

        protected void rptSAEditLines_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var row = (DataRowView)e.Item.DataItem;
            int savedPid = row["ProductID"] != DBNull.Value ? Convert.ToInt32(row["ProductID"]) : 0;
            string savedForm = row.Row.Table.Columns.Contains("SellingForm") && row["SellingForm"] != DBNull.Value
                ? row["SellingForm"].ToString() : "JAR";
            string savedSource = row.Row.Table.Columns.Contains("Source") && row["Source"] != DBNull.Value
                ? row["Source"].ToString() : "CASE";

            // Product dropdown with stock breakdown in label (matches DC form)
            var lit = e.Item.FindControl("litSAEditProduct") as Literal;
            if (lit != null)
            {
                var stock = ViewState["SAEditStockList"] as DataTable;
                if (stock == null)
                {
                    // Fallback — should have been built by BuildSAEditProductOptions() but guard
                    BuildSAEditProductOptions();
                    stock = ViewState["SAEditStockList"] as DataTable;
                }
                var sb = new System.Text.StringBuilder();
                if (stock != null)
                {
                    foreach (DataRow r in stock.Rows)
                    {
                        int pid = Convert.ToInt32(r["ProductID"]);
                        string sel = (pid == savedPid) ? " selected" : "";
                        sb.Append("<option value='").Append(pid).Append("'").Append(sel).Append(">")
                          .Append(FormatSAEditOptionLabel(r))
                          .Append("</option>");
                    }
                }
                lit.Text = sb.ToString();
            }

            // Selling Form dropdown — saved value pre-selected
            var litForm = e.Item.FindControl("litSAEditForm") as Literal;
            if (litForm != null)
            {
                var fsb = new System.Text.StringBuilder();
                string[] forms = new[] { "JAR", "BOX", "PCS", "CASE" };
                foreach (var f in forms)
                {
                    string sel = f == savedForm ? " selected" : "";
                    fsb.Append("<option value='").Append(f).Append("'").Append(sel).Append(">").Append(f).Append("</option>");
                }
                litForm.Text = fsb.ToString();
            }

            // Source dropdown — saved value pre-selected. Labels match DC form.
            var litSrc = e.Item.FindControl("litSAEditSource") as Literal;
            if (litSrc != null)
            {
                string selCase = savedSource == "CASE" ? " selected" : "";
                string selLoose = savedSource == "LOOSE" ? " selected" : "";
                litSrc.Text = "<option value='CASE'" + selCase + ">From Cases</option>"
                            + "<option value='LOOSE'" + selLoose + ">From Loose</option>";
            }
        }

        protected void btnSaveSAEdit_Click(object s, EventArgs e)
        {
            // Fix 2: stay on SA Orders sub-tab after save
            if (hfSubTab != null) hfSubTab.Value = "sa";

            int shipId = Convert.ToInt32(hfSAShipId.Value);
            if (shipId == 0) return;

            string[] pids    = Request.Form.GetValues("sa_edit_product");
            string[] qtys    = Request.Form.GetValues("sa_edit_qty");
            string[] forms   = Request.Form.GetValues("sa_edit_form");
            string[] sources = Request.Form.GetValues("sa_edit_source");

            if (pids == null || qtys == null)
            { ShowAlert("No product lines to save.", false); return; }

            var productIds  = new System.Collections.Generic.List<int>();
            var quantities  = new System.Collections.Generic.List<int>();
            var sellingForms= new System.Collections.Generic.List<string>();
            var sourceVals  = new System.Collections.Generic.List<string>();

            for (int i = 0; i < pids.Length; i++)
            {
                int pid = 0; int.TryParse(pids[i], out pid);
                int qty = 0;
                if (i < qtys.Length) int.TryParse(qtys[i], out qty);
                string form = (forms != null && i < forms.Length && !string.IsNullOrEmpty(forms[i])) ? forms[i] : "JAR";
                string src  = (sources != null && i < sources.Length && !string.IsNullOrEmpty(sources[i])) ? sources[i] : "CASE";
                if (pid > 0 && qty > 0)
                {
                    productIds.Add(pid); quantities.Add(qty);
                    sellingForms.Add(form); sourceVals.Add(src);
                }
            }

            if (productIds.Count == 0)
            { ShowAlert("Please add at least one product with quantity.", false); return; }

            PKDatabaseHelper.UpdateSAShipmentLines(shipId, productIds.ToArray(), quantities.ToArray(),
                sellingForms.ToArray(), sourceVals.ToArray());
            ShowAlert("SH-" + shipId.ToString("D5") + " updated.", true);
            LoadSAOrderDetail(shipId);
            BindSAOrders();
        }

        void DoConvertToDC(int shipId)
        {
            // Check FG stock for all line items
            var stockCheck = PKDatabaseHelper.CheckFGStockForSAOrder(shipId);
            foreach (DataRow r in stockCheck.Rows)
            {
                decimal required = Convert.ToDecimal(r["RequiredQty"]);
                decimal available = Convert.ToDecimal(r["AvailableQty"]);
                if (available < required)
                {
                    ShowAlert("Cannot convert to DC — insufficient FG stock for " + r["ProductName"] +
                        " (Need: " + required + ", Available: " + available + ").", false);
                    LoadSAOrderDetail(shipId);
                    BindSAOrders();
                    return;
                }
            }

            int csgId = 0;
            int.TryParse(hfActiveConsig.Value, out csgId);
            int dcId = PKDatabaseHelper.ConvertSAShipmentToDC(shipId, csgId);
            ShowAlert("SH-" + shipId.ToString("D5") + " converted to Delivery Challan. SA side is now read-only.", true);
            pnlSADetail.Visible = false;
            BindSAOrders();
            BindDCList();

            // Auto-load the newly created DC for editing — user lands on DC sub-tab
            if (dcId > 0)
            {
                if (hfSubTab != null) hfSubTab.Value = "dc";
                LoadDC(dcId);
            }
        }

        protected void btnConvertDC_Click(object s, EventArgs e)
        {
            if (hfSubTab != null) hfSubTab.Value = "sa";
            int shipId = Convert.ToInt32(hfSAShipId.Value);
            if (shipId > 0) DoConvertToDC(shipId);
        }

        void DoUnconvertDC(int shipId)
        {
            if (hfSubTab != null) hfSubTab.Value = "sa";
            PKDatabaseHelper.UnconvertSAShipmentDC(shipId);
            ShowAlert("SH-" + shipId.ToString("D5") + " reverted to Order status. SA team can now edit again.", true);
            pnlSADetail.Visible = false;
            BindSAOrders();
        }

        protected void btnUnconvertDC_Click(object s, EventArgs e)
        {
            if (hfSubTab != null) hfSubTab.Value = "sa";
            int shipId = Convert.ToInt32(hfSAShipId.Value);
            if (shipId > 0) DoUnconvertDC(shipId);
        }

        void DoCompleteDispatch(int shipId)
        {
            if (hfSubTab != null) hfSubTab.Value = "sa";
            try
            {
                PKDatabaseHelper.CompleteSAShipmentDispatch(shipId, UserID);
                ShowAlert("SH-" + shipId.ToString("D5") + " dispatched. FG stock has been deducted.", true);
                pnlSADetail.Visible = false;
                BindSAOrders();
                BuildProductData(); // Refresh FG stock data
            }
            catch (Exception ex)
            {
                ShowAlert("Error dispatching: " + ex.Message, false);
            }
        }

        protected void btnDispatch_Click(object s, EventArgs e)
        {
            if (hfSubTab != null) hfSubTab.Value = "sa";
            int shipId = Convert.ToInt32(hfSAShipId.Value);
            if (shipId > 0) DoCompleteDispatch(shipId);
        }

        protected void btnCloseSADetail_Click(object s, EventArgs e)
        {
            if (hfSubTab != null) hfSubTab.Value = "sa";
            pnlSADetail.Visible = false;
        }

        // ── HELPERS ──
        bool ValidateForm()
        {
            if (ddlCustomer.SelectedValue == "0")
            { ShowAlert("Please select a customer.", false); return false; }
            if (string.IsNullOrEmpty(txtDCDate.Text))
            { ShowAlert("Please enter a DC date.", false); return false; }
            return true;
        }

        class DCLineData
        {
            public int ProductID, Qty;
            public string SellingForm, Source, HSN;
            public decimal GSTRate, MRP, MarginPct, UnitRate, TaxableVal, GSTAmt, LineTotal;
        }

        DCLineData[] ParseLines()
        {
            string raw = hfLines.Value;
            if (string.IsNullOrEmpty(raw) || raw == "[]") return null;

            try
            {
                var list = new System.Collections.Generic.List<DCLineData>();
                raw = raw.Trim(new char[] { '[', ']' });
                if (string.IsNullOrEmpty(raw)) return null;

                string[] items = raw.Split(new string[] { "},{" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in items)
                {
                    string clean = item.Trim(new char[] { '{', '}' });
                    var ld = new DCLineData();

                    foreach (string pair in clean.Split(','))
                    {
                        string[] kv = pair.Split(new char[] { ':' }, 2);
                        if (kv.Length < 2) continue;
                        string key = kv[0].Trim().Trim('"');
                        string val = kv[1].Trim().Trim('"');

                        if (key == "pid") int.TryParse(val, out ld.ProductID);
                        else if (key == "form") ld.SellingForm = val;
                        else if (key == "source") ld.Source = val;
                        else if (key == "qty") int.TryParse(val, out ld.Qty);
                        else if (key == "hsn") ld.HSN = val;
                        else if (key == "gstRate") decimal.TryParse(val, out ld.GSTRate);
                        else if (key == "mrp") decimal.TryParse(val, out ld.MRP);
                        else if (key == "marginPct") decimal.TryParse(val, out ld.MarginPct);
                        else if (key == "rate") decimal.TryParse(val, out ld.UnitRate);
                        else if (key == "taxableVal") decimal.TryParse(val, out ld.TaxableVal);
                        else if (key == "gstAmt") decimal.TryParse(val, out ld.GSTAmt);
                        else if (key == "lineTotal") decimal.TryParse(val, out ld.LineTotal);
                    }

                    if (string.IsNullOrEmpty(ld.SellingForm)) ld.SellingForm = "JAR";
                    if (ld.ProductID > 0 && ld.Qty > 0)
                        list.Add(ld);
                }
                return list.ToArray();
            }
            catch { return null; }
        }

        protected string GetGSTDisplay(object cgst, object sgst, object igst, object gstRate)
        {
            decimal c = cgst != null && cgst != DBNull.Value ? Convert.ToDecimal(cgst) : 0;
            decimal s = sgst != null && sgst != DBNull.Value ? Convert.ToDecimal(sgst) : 0;
            decimal i = igst != null && igst != DBNull.Value ? Convert.ToDecimal(igst) : 0;
            decimal rate = gstRate != null && gstRate != DBNull.Value ? Convert.ToDecimal(gstRate) : 0;
            decimal total = c + s + i;
            if (total <= 0) return "—";
            string label = i > 0 ? "IGST " + rate.ToString("0.#") + "%"
                : "C+S " + (rate / 2).ToString("0.#") + "%+" + (rate / 2).ToString("0.#") + "%";
            return string.Format("₹{0:N2}<div style=\"font-size:9px;color:#999;\">{1}</div>", total, label);
        }

        void ShowAlert(string m, bool ok)
        {
            if (lblAlert != null) lblAlert.Text = m;
            if (pnlAlert != null)
            {
                pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger");
                pnlAlert.Visible = true;
            }
        }

        // ── PRINT / DOWNLOAD DC ──
        protected void btnPrintDC_Click(object s, EventArgs e)
        {
            int dcId = Convert.ToInt32(hfDCID.Value);
            if (dcId == 0) { ShowAlert("Please save the DC first before downloading.", false); return; }

            var dc = PKDatabaseHelper.GetDCById(dcId);
            if (dc == null) { ShowAlert("DC not found.", false); return; }

            var lines = PKDatabaseHelper.GetDCLines(dcId);
            string dcNumber = dc["DCNumber"].ToString();
            string custName = dc["CustomerName"].ToString();
            string custCode = dc["CustomerCode"].ToString();
            string dcDate = Convert.ToDateTime(dc["DCDate"]).ToString("dd-MMM-yyyy");
            string status = dc["Status"].ToString();
            string remarks = dc["Remarks"] == DBNull.Value ? "" : dc["Remarks"].ToString();

            int totalCases = 0, totalLoose = 0, totalPcs = 0;
            var lineRows = new System.Text.StringBuilder();
            int sno = 1;
            foreach (DataRow r in lines.Rows)
            {
                int cs = Convert.ToInt32(r["Cases"]);
                int lj = Convert.ToInt32(r["LooseJars"]);
                int jpc = Convert.ToInt32(r["JarsPerCase"]);
                int tp = Convert.ToInt32(r["TotalPcs"]);
                totalCases += cs; totalLoose += lj; totalPcs += tp;
                lineRows.Append("<tr>");
                lineRows.Append("<td style='text-align:center;'>" + sno++ + "</td>");
                lineRows.Append("<td><strong>" + r["ProductName"] + "</strong><br/><span style='font-size:10px;color:#888;'>" + r["ProductCode"] + "</span></td>");
                lineRows.Append("<td style='text-align:right;'>" + cs + "</td>");
                lineRows.Append("<td style='text-align:right;'>" + lj + "</td>");
                lineRows.Append("<td style='text-align:center;'>" + jpc + "</td>");
                lineRows.Append("<td style='text-align:right;font-weight:700;'>" + tp.ToString("N0") + "</td>");
                lineRows.Append("</tr>");
            }

            string html = @"<!DOCTYPE html><html><head><meta charset='utf-8'/>
<title>Delivery Challan - " + dcNumber + @"</title>
<style>
@page{size:A4;margin:15mm 12mm;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'Segoe UI',Arial,sans-serif;font-size:12px;color:#222;padding:20px;}
.header{display:flex;justify-content:space-between;align-items:flex-start;border-bottom:3px solid #e67e22;padding-bottom:12px;margin-bottom:16px;}
.company{font-size:18px;font-weight:800;letter-spacing:1px;}
.company-sub{font-size:10px;color:#666;margin-top:2px;}
.dc-title{text-align:right;}
.dc-title h1{font-size:20px;color:#e67e22;margin:0;letter-spacing:2px;}
.dc-title .dc-num{font-size:14px;font-weight:700;margin-top:4px;}
.dc-title .dc-status{font-size:10px;padding:2px 8px;border-radius:4px;display:inline-block;margin-top:4px;}
.status-draft{background:#fef3cd;color:#856404;}
.status-final{background:#d4edda;color:#155724;}
.info-grid{display:grid;grid-template-columns:1fr 1fr;gap:10px;margin-bottom:16px;font-size:11px;}
.info-box{border:1px solid #ddd;border-radius:6px;padding:10px 12px;}
.info-label{font-size:9px;font-weight:700;text-transform:uppercase;color:#888;letter-spacing:.5px;}
.info-val{font-size:13px;font-weight:600;margin-top:2px;}
table{width:100%;border-collapse:collapse;margin-bottom:16px;}
th{background:#f8f8f8;font-size:9px;font-weight:700;text-transform:uppercase;letter-spacing:.5px;color:#666;padding:8px 10px;border-bottom:2px solid #ddd;text-align:left;}
th.num{text-align:right;}th.ctr{text-align:center;}
td{padding:8px 10px;border-bottom:1px solid #eee;font-size:12px;}
tfoot td{border-top:2px solid #333;font-weight:700;font-size:13px;}
.footer{margin-top:40px;display:grid;grid-template-columns:1fr 1fr;gap:20px;}
.sig-box{border-top:1px solid #999;padding-top:6px;text-align:center;font-size:10px;color:#666;}
.remarks{font-size:11px;color:#555;margin-bottom:12px;padding:8px;background:#f9f9f9;border-radius:4px;}
@media print{body{padding:0;}.no-print{display:none !important;}}
</style></head><body>
<div class='no-print' style='text-align:center;margin-bottom:16px;'>
<button onclick='window.print();' style='padding:10px 30px;font-size:14px;font-weight:700;background:#e67e22;color:#fff;border:none;border-radius:8px;cursor:pointer;'>Print / Save as PDF</button>
<button onclick='window.close();' style='padding:10px 20px;font-size:14px;background:#eee;border:1px solid #ddd;border-radius:8px;cursor:pointer;margin-left:8px;'>Close</button>
</div>
<div class='header'>
<div><div class='company'>SIRIMIRI NUTRITION</div><div class='company-sub'>Food Products Pvt. Ltd.</div></div>
<div class='dc-title'><h1>DELIVERY CHALLAN</h1><div class='dc-num'>" + dcNumber + @"</div>
<div class='dc-status " + (status == "FINALISED" ? "status-final" : "status-draft") + "'>" + status + @"</div></div>
</div>
<div class='info-grid'>
<div class='info-box'><div class='info-label'>Customer</div><div class='info-val'>" + custName + @"</div><div style='font-size:10px;color:#888;'>" + custCode + @"</div></div>
<div class='info-box'><div class='info-label'>DC Date</div><div class='info-val'>" + dcDate + @"</div></div>
</div>" +
(string.IsNullOrEmpty(remarks) ? "" : "<div class='remarks'><strong>Remarks:</strong> " + remarks + "</div>") +
@"<table>
<thead><tr><th class='ctr'>S.No</th><th>Product</th><th class='num'>Cases</th><th class='num'>Loose Jars</th><th class='ctr'>Jars/Case</th><th class='num'>Total Pcs</th></tr></thead>
<tbody>" + lineRows.ToString() + @"</tbody>
<tfoot><tr><td></td><td>Total</td><td style='text-align:right;'>" + totalCases + @"</td><td style='text-align:right;'>" + totalLoose + @"</td><td></td><td style='text-align:right;'>" + totalPcs.ToString("N0") + @"</td></tr></tfoot>
</table>
<div class='footer'>
<div><div class='sig-box'>Prepared By</div></div>
<div><div class='sig-box'>Received By (Customer Signature)</div></div>
</div>
</body></html>";

            Response.Clear();
            Response.ContentType = "text/html";
            Response.Write(html);
            Response.End();
        }

        // ── PROJECTIONS (Read-only) ────────────────────────────────────────

        void LoadProjDropdowns()
        {
            if (ddlProjMonth == null || ddlProjYear == null) return;
            ddlProjMonth.Items.Clear();
            string[] months = { "January","February","March","April","May","June","July","August","September","October","November","December" };
            for (int i = 0; i < 12; i++)
                ddlProjMonth.Items.Add(new System.Web.UI.WebControls.ListItem(months[i], (i + 1).ToString()));
            ddlProjMonth.SelectedValue = DateTime.Now.Month.ToString();

            ddlProjYear.Items.Clear();
            int yr = DateTime.Now.Year;
            for (int y = yr - 1; y <= yr + 1; y++)
                ddlProjYear.Items.Add(new System.Web.UI.WebControls.ListItem(y.ToString(), y.ToString()));
            ddlProjYear.SelectedValue = yr.ToString();
        }

        void BindProjections()
        {
            if (ddlProjMonth == null || ddlProjYear == null) return;
            int month = Convert.ToInt32(ddlProjMonth.SelectedValue);
            int year = Convert.ToInt32(ddlProjYear.SelectedValue);
            var dt = PKDatabaseHelper.GetSAProjections(month, year);
            if (rptProjections != null) { rptProjections.DataSource = dt; rptProjections.DataBind(); }
            if (pnlProjEmpty != null) pnlProjEmpty.Visible = dt.Rows.Count == 0;
        }

        protected DataTable GetProjectionLines(object projectionId)
        {
            return PKDatabaseHelper.GetSAProjectionLines(Convert.ToInt32(projectionId));
        }

        protected void ddlProjMonth_Changed(object s, EventArgs e) { BindProjections(); }
        protected void ddlProjYear_Changed(object s, EventArgs e) { BindProjections(); }
    }
}
