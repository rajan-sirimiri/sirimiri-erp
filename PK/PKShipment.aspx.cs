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
        protected Panel pnlAlert, pnlForm, pnlLocked, pnlEmpty, pnlList, pnlViewRemarks;
        protected Panel pnlSAEmpty, pnlSAList, pnlSADetail, pnlSAEditLines;
        protected HiddenField hfDCID, hfLines, hfProductData, hfSAShipId, hfSAProductOptions;
        protected TextBox txtDCNumber, txtDCDate, txtRemarks;
        protected DropDownList ddlCustomer;
        protected Button btnDraftSave, btnFinalise, btnNew, btnNewFromLocked, btnPrintDC, btnDownloadFromView, btnDeleteDC;
        protected Button btnCreateInvoice;
        protected Button btnDownloadInvoicePDF;
        protected DropDownList ddlChannel;
        protected Panel pnlCreateInvoice, pnlInvoiceStatus, pnlInvoiceError;
        protected Label lblInvoiceNo, lblInvoiceZohoStatus, lblInvoiceAmount, lblInvoiceError;
        protected HyperLink lnkViewInZoho;
        protected Button btnConvertDC, btnDispatch, btnUnconvertDC, btnCloseSADetail, btnSaveSAEdit;
        protected Repeater rptDCs, rptViewLines, rptSAOrders, rptSALines, rptSAEditLines, rptProjections;
        protected Panel pnlProjEmpty;
        protected DropDownList ddlProjMonth, ddlProjYear;
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
                BindCustomers();
                BuildProductData();
                txtDCDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                if (btnDeleteDC != null) btnDeleteDC.Visible = false;
                BindDCList();
                LoadProjDropdowns();
                BindProjections();
            }
            BindSAOrders();
        }

        void BindCustomers()
        {
            var dt = PKDatabaseHelper.GetActiveCustomers();
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

                if (!first) sb.Append(",");
                sb.Append("\"" + pid + "\":{");
                sb.Append("\"name\":\"" + Esc(r["ProductName"].ToString()) + "\",");
                sb.Append("\"code\":\"" + Esc(r["ProductCode"].ToString()) + "\",");
                sb.Append("\"unitSize\":" + unitSize + ",");
                sb.Append("\"jarsPerCase\":" + jpc + ",");
                sb.Append("\"availJars\":" + availJars + ",");
                sb.Append("\"availCases\":" + availCases + ",");
                sb.Append("\"availLoose\":" + availLoose);
                sb.Append("}");
                first = false;
            }
            sb.Append("}");
            if (hfProductData != null) hfProductData.Value = sb.ToString();
        }

        string Esc(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "").Replace("\r", "");
        }

        void BindDCList()
        {
            var dt = PKDatabaseHelper.GetRecentDCs();
            bool hasRows = dt.Rows.Count > 0;
            if (pnlEmpty != null) pnlEmpty.Visible = !hasRows;
            if (pnlList != null) pnlList.Visible = hasRows;
            if (rptDCs != null) { rptDCs.DataSource = dt; rptDCs.DataBind(); }
        }

        // ── DRAFT SAVE ──
        protected void btnDraftSave_Click(object s, EventArgs e)
        {
            if (!ValidateForm()) return;

            int customerId = Convert.ToInt32(ddlCustomer.SelectedValue);
            DateTime dcDate = DateTime.Parse(txtDCDate.Text);
            string remarks = txtRemarks.Text.Trim();
            int dcId = Convert.ToInt32(hfDCID.Value);

            var lineData = ParseLines();
            if (lineData == null || lineData.Length == 0)
            { ShowAlert("Please add at least one product line.", false); return; }

            // Validate FG stock — cases and loose jars separately
            var fgStock = PKDatabaseHelper.GetFGStockForShipment();
            foreach (var line in lineData)
            {
                DataRow stockRow = null;
                foreach (DataRow r in fgStock.Rows)
                    if (Convert.ToInt32(r["ProductID"]) == line[0]) { stockRow = r; break; }
                if (stockRow == null)
                { ShowAlert("Product ID " + line[0] + " has no FG stock.", false); return; }

                int availCases = Convert.ToInt32(stockRow["AvailableCases"]);
                int availLoose = Convert.ToInt32(stockRow["AvailableLooseJars"]);
                int lineCases = line[1];
                int lineLoose = line[2];

                // If editing existing DC, add back this DC's own allocation
                if (dcId > 0)
                {
                    var existingLines = PKDatabaseHelper.GetDCLines(dcId);
                    foreach (DataRow el in existingLines.Rows)
                        if (Convert.ToInt32(el["ProductID"]) == line[0])
                        {
                            availCases += Convert.ToInt32(el["Cases"]);
                            availLoose += Convert.ToInt32(el["LooseJars"]);
                        }
                }
                if (lineCases > availCases)
                { ShowAlert("Insufficient CASES for " + (stockRow["ProductName"] ?? "product") + ". Need " + lineCases + ", available " + availCases + ".", false); return; }
                if (lineLoose > availLoose)
                { ShowAlert("Insufficient loose JARS for " + (stockRow["ProductName"] ?? "product") + ". Need " + lineLoose + ", available " + availLoose + ".", false); return; }
            }

            try
            {
                if (dcId == 0)
                {
                    dcId = PKDatabaseHelper.CreateDeliveryChallan(customerId, dcDate, remarks, UserID);
                }
                else
                {
                    PKDatabaseHelper.UpdateDCHeader(dcId, customerId, dcDate, remarks);
                    PKDatabaseHelper.DeleteDCLines(dcId);
                }

                foreach (var line in lineData)
                    PKDatabaseHelper.AddDCLine(dcId, line[0], line[1], line[2], line[3], line[4]);

                hfDCID.Value = dcId.ToString();
                // Reload DC number
                var dc = PKDatabaseHelper.GetDCById(dcId);
                if (dc != null) txtDCNumber.Text = dc["DCNumber"].ToString();

                ShowAlert("Delivery Challan saved as Draft.", true);
                BuildProductData();
                BindDCList();
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
                PKDatabaseHelper.FinaliseDeliveryChallan(dcId, UserID);
                ShowAlert("Delivery Challan finalised. No further changes allowed.", true);
                LoadDC(dcId);
                BindDCList();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        // ── CREATE ZOHO INVOICE ──
        protected void btnCreateInvoice_Click(object s, EventArgs e)
        {
            int dcId = Convert.ToInt32(hfDCID.Value);
            if (dcId == 0) { ShowAlert("No DC selected.", false); return; }

            string channel = ddlChannel != null ? ddlChannel.SelectedValue : "GT";

            try
            {
                string result = StockApp.DAL.ZohoHelper.CreateInvoiceFromDC(dcId, channel, UserID);
                if (result.StartsWith("OK:"))
                {
                    string invNo = result.Substring(3);
                    ShowAlert("Zoho Invoice " + invNo + " created successfully.", true);
                }
                else
                {
                    ShowAlert(result, false);
                }
                LoadDC(dcId);
            }
            catch (Exception ex) { ShowAlert("Invoice creation error: " + ex.Message, false); }
        }

        // ── DOWNLOAD INVOICE PDF ──
        protected void btnDownloadInvoicePDF_Click(object s, EventArgs e)
        {
            string zohoInvId = ViewState["ZohoInvoiceID"] as string;
            if (string.IsNullOrEmpty(zohoInvId))
            {
                // Try to get from DB
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

            string status = dc["Status"].ToString();
            bool isDraft = status == "DRAFT";

            // Load lines into hfLines as JSON
            var lines = PKDatabaseHelper.GetDCLines(dcId);
            var sb = new System.Text.StringBuilder("[");
            bool first = true;
            foreach (DataRow r in lines.Rows)
            {
                if (!first) sb.Append(",");
                sb.Append("{\"pid\":\"" + r["ProductID"] + "\",");
                sb.Append("\"name\":\"" + Esc(r["ProductName"].ToString()) + "\",");
                sb.Append("\"code\":\"" + Esc(r["ProductCode"].ToString()) + "\",");
                sb.Append("\"cases\":" + r["Cases"] + ",");
                sb.Append("\"loose\":" + r["LooseJars"] + ",");
                sb.Append("\"jpc\":" + r["JarsPerCase"] + ",");
                sb.Append("\"unitSize\":1,");
                sb.Append("\"totalPcs\":" + r["TotalPcs"] + "}");
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
            }
            else
            {
                pnlForm.Visible = false;
                pnlLocked.Visible = true;

                // Populate read-only view
                if (lblLockedTitle != null) lblLockedTitle.Text = "Delivery Challan — " + dc["DCNumber"];
                if (lblViewDCNum != null) lblViewDCNum.Text = dc["DCNumber"].ToString();
                if (lblViewDate != null) lblViewDate.Text = Convert.ToDateTime(dc["DCDate"]).ToString("dd-MMM-yyyy");
                if (lblViewCustomer != null) lblViewCustomer.Text = dc["CustomerName"] + " (" + dc["CustomerCode"] + ")";

                string rem = dc["Remarks"] == DBNull.Value ? "" : dc["Remarks"].ToString();
                if (pnlViewRemarks != null) { pnlViewRemarks.Visible = !string.IsNullOrEmpty(rem); }
                if (lblViewRemarks != null) lblViewRemarks.Text = rem;

                if (rptViewLines != null) { rptViewLines.DataSource = lines; rptViewLines.DataBind(); }

                // Zoho Invoice status
                if (pnlCreateInvoice != null && pnlInvoiceStatus != null)
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
                            if (pnlInvoiceError != null) pnlInvoiceError.Visible = false;

                            if (lblInvoiceNo != null) lblInvoiceNo.Text = zohoInvNo;
                            if (lblInvoiceZohoStatus != null)
                            {
                                lblInvoiceZohoStatus.Text = zohoStatus.ToUpper();
                                lblInvoiceZohoStatus.ForeColor = zohoStatus == "paid" ? System.Drawing.Color.Green
                                    : zohoStatus == "overdue" ? System.Drawing.Color.Red
                                    : System.Drawing.Color.FromArgb(0, 120, 212);
                            }

                            // Zoho Books URL — India domain
                            string orgId = StockApp.DAL.ZohoHelper.GetOrgId();
                            if (lnkViewInZoho != null)
                                lnkViewInZoho.NavigateUrl = "https://books.zoho.in/app/" + orgId + "/invoices/" + zohoInvId;

                            // Store ZohoInvoiceID for PDF download
                            ViewState["ZohoInvoiceID"] = zohoInvId;
                        }
                        else
                        {
                            pnlCreateInvoice.Visible = true;
                            pnlInvoiceStatus.Visible = false;
                            if (pnlInvoiceError != null) pnlInvoiceError.Visible = false;

                            if (invLog != null && invLog["PushStatus"].ToString() == "Error")
                            {
                                if (pnlInvoiceError != null) pnlInvoiceError.Visible = true;
                                if (lblInvoiceError != null)
                                    lblInvoiceError.Text = invLog["ErrorMessage"] != DBNull.Value ? invLog["ErrorMessage"].ToString() : "Unknown error";
                            }
                        }
                    }
                    catch
                    {
                        pnlCreateInvoice.Visible = true;
                        pnlInvoiceStatus.Visible = false;
                        if (pnlInvoiceError != null) pnlInvoiceError.Visible = false;
                    }
                }
            }

            BuildProductData();
            if (pnlAlert != null) pnlAlert.Visible = false;
        }

        // ── SA SHIPMENT ORDERS ──────────────────────────────────────────

        void BindSAOrders()
        {
            var dt = PKDatabaseHelper.GetSAShipmentOrders();
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

            // Load editable lines
            if (canEdit)
            {
                var editLines = PKDatabaseHelper.GetSAShipmentLines(shipId);
                if (rptSAEditLines != null) { rptSAEditLines.DataSource = editLines; rptSAEditLines.DataBind(); }

                // Build product options HTML for JS addSAEditLine
                DataTable products = PKDatabaseHelper.ExecuteQueryPublic(
                    "SELECT ProductID, ProductCode, ProductName FROM PP_Products WHERE IsActive=1 AND ProductType='Core' ORDER BY ProductName;");
                var sb = new System.Text.StringBuilder();
                foreach (DataRow r in products.Rows)
                    sb.Append("<option value='" + r["ProductID"] + "'>" + r["ProductName"] + " (" + r["ProductCode"] + ")</option>");
                if (hfSAProductOptions != null) hfSAProductOptions.Value = sb.ToString();
            }

            pnlSADetail.Visible = true;
        }

        protected void rptSAEditLines_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var row = (DataRowView)e.Item.DataItem;
            int savedPid = row["ProductID"] != DBNull.Value ? Convert.ToInt32(row["ProductID"]) : 0;

            var lit = e.Item.FindControl("litSAEditProduct") as Literal;
            if (lit != null)
            {
                DataTable products = PKDatabaseHelper.ExecuteQueryPublic(
                    "SELECT ProductID, ProductCode, ProductName FROM PP_Products WHERE IsActive=1 AND ProductType='Core' ORDER BY ProductName;");
                var sb = new System.Text.StringBuilder();
                foreach (DataRow r in products.Rows)
                {
                    int pid = Convert.ToInt32(r["ProductID"]);
                    string sel = (pid == savedPid) ? " selected" : "";
                    sb.Append("<option value='" + pid + "'" + sel + ">" + r["ProductName"] + " (" + r["ProductCode"] + ")</option>");
                }
                lit.Text = sb.ToString();
            }
        }

        protected void btnSaveSAEdit_Click(object s, EventArgs e)
        {
            int shipId = Convert.ToInt32(hfSAShipId.Value);
            if (shipId == 0) return;

            string[] pids = Request.Form.GetValues("sa_edit_product");
            string[] qtys = Request.Form.GetValues("sa_edit_qty");

            if (pids == null || qtys == null)
            { ShowAlert("No product lines to save.", false); return; }

            var productIds = new System.Collections.Generic.List<int>();
            var quantities = new System.Collections.Generic.List<int>();

            for (int i = 0; i < pids.Length; i++)
            {
                int pid = 0; int.TryParse(pids[i], out pid);
                int qty = 0; int.TryParse(qtys[i], out qty);
                if (pid > 0 && qty > 0) { productIds.Add(pid); quantities.Add(qty); }
            }

            if (productIds.Count == 0)
            { ShowAlert("Please add at least one product with quantity.", false); return; }

            PKDatabaseHelper.UpdateSAShipmentLines(shipId, productIds.ToArray(), quantities.ToArray());
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

            int dcId = PKDatabaseHelper.ConvertSAShipmentToDC(shipId);
            ShowAlert("SH-" + shipId.ToString("D5") + " converted to Delivery Challan. SA side is now read-only.", true);
            pnlSADetail.Visible = false;
            BindSAOrders();
            BindDCList();

            // Auto-load the newly created DC for editing
            if (dcId > 0) LoadDC(dcId);
        }

        protected void btnConvertDC_Click(object s, EventArgs e)
        {
            int shipId = Convert.ToInt32(hfSAShipId.Value);
            if (shipId > 0) DoConvertToDC(shipId);
        }

        void DoUnconvertDC(int shipId)
        {
            PKDatabaseHelper.UnconvertSAShipmentDC(shipId);
            ShowAlert("SH-" + shipId.ToString("D5") + " reverted to Order status. SA team can now edit again.", true);
            pnlSADetail.Visible = false;
            BindSAOrders();
        }

        protected void btnUnconvertDC_Click(object s, EventArgs e)
        {
            int shipId = Convert.ToInt32(hfSAShipId.Value);
            if (shipId > 0) DoUnconvertDC(shipId);
        }

        void DoCompleteDispatch(int shipId)
        {
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
            int shipId = Convert.ToInt32(hfSAShipId.Value);
            if (shipId > 0) DoCompleteDispatch(shipId);
        }

        protected void btnCloseSADetail_Click(object s, EventArgs e)
        {
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

        int[][] ParseLines()
        {
            string raw = hfLines.Value;
            if (string.IsNullOrEmpty(raw) || raw == "[]") return null;

            try
            {
                // Simple JSON array parse: [{pid,cases,loose,jpc,unitSize,totalPcs},...]
                var list = new System.Collections.Generic.List<int[]>();
                raw = raw.Trim(new char[] { '[', ']' });
                if (string.IsNullOrEmpty(raw)) return null;

                // Split by },{ pattern
                string[] items = raw.Split(new string[] { "},{" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in items)
                {
                    string clean = item.Trim(new char[] { '{', '}' });
                    int pid = 0, cases = 0, loose = 0, jpc = 12, us = 1, totalPcs = 0;

                    foreach (string pair in clean.Split(','))
                    {
                        string[] kv = pair.Split(':');
                        if (kv.Length < 2) continue;
                        string key = kv[0].Trim().Trim('"');
                        string val = kv[1].Trim().Trim('"');

                        if (key == "pid") int.TryParse(val, out pid);
                        else if (key == "cases") int.TryParse(val, out cases);
                        else if (key == "loose") int.TryParse(val, out loose);
                        else if (key == "jpc") int.TryParse(val, out jpc);
                        else if (key == "unitSize") int.TryParse(val, out us);
                        else if (key == "totalPcs") int.TryParse(val, out totalPcs);
                    }

                    if (pid > 0 && totalPcs > 0)
                        list.Add(new int[] { pid, cases, loose, jpc, totalPcs });
                }
                return list.ToArray();
            }
            catch { return null; }
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
