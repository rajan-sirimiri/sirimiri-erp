using System;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKCustomer : Page
    {
        protected Label lblUser, lblAlert, lblCount, lblFormTitle;
        protected Panel pnlAlert, pnlEmpty;
        protected HiddenField hfCustID;
        protected DropDownList ddlCustomerType;
        protected TextBox txtCode, txtName, txtContact, txtPhone, txtEmail;
        protected TextBox txtGSTIN, txtCity, txtState, txtPinCode, txtAddress;
        protected TextBox txtSMMargin, txtGTMargin;
        protected Button btnSave, btnClear, btnToggle, btnUpload;
        protected LinkButton lnkTemplate;
        protected LinkButton lnkExportAll;
        protected FileUpload fuExcel;
        protected Repeater rptList;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null)
            { Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery)); return; }

            // Module access check
            string __role = Session["PK_Role"]?.ToString() ?? "";
            if (!PKDatabaseHelper.RoleHasModuleAccess(__role, "PK", "PK_CUSTOMER"))
            { Response.Redirect("PKHome.aspx"); return; }
            if (lblUser != null) lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack) { LoadCustomerTypes(); BindList(); }
        }

        void LoadCustomerTypes()
        {
            if (ddlCustomerType == null) return;
            var dt = PKDatabaseHelper.GetCustomerTypes();
            ddlCustomerType.Items.Clear();
            ddlCustomerType.Items.Add(new ListItem("-- Select Type --", ""));
            foreach (DataRow r in dt.Rows)
                ddlCustomerType.Items.Add(new ListItem(r["TypeName"].ToString(), r["TypeCode"].ToString()));
        }

        void BindList()
        {
            var dt = PKDatabaseHelper.GetAllCustomers();
            if (pnlEmpty != null) pnlEmpty.Visible = dt.Rows.Count == 0;
            if (rptList != null) { rptList.DataSource = dt; rptList.DataBind(); }
            if (lblCount != null) lblCount.Text = dt.Rows.Count.ToString();
        }

        protected void btnSave_Click(object s, EventArgs e)
        {
            string customerType = ddlCustomerType != null ? ddlCustomerType.SelectedValue : "";
            if (string.IsNullOrEmpty(customerType))
            { ShowAlert("Please select a Customer Type.", false); return; }

            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ShowAlert("Name is required.", false); return; }

            int id = Convert.ToInt32(hfCustID.Value);
            string contact = txtContact.Text.Trim();
            string phone   = txtPhone.Text.Trim();
            string email   = txtEmail.Text.Trim();
            string address = txtAddress.Text.Trim();
            string city    = txtCity.Text.Trim();
            string state   = txtState.Text.Trim();
            string pinCode = txtPinCode != null ? txtPinCode.Text.Trim() : "";
            string gstin   = txtGSTIN.Text.Trim().ToUpper();

            try
            {
                int custId = id;
                if (id == 0)
                {
                    custId = PKDatabaseHelper.AddCustomer(customerType, name, contact, phone,
                        email, address, city, state, pinCode, gstin);
                    ShowAlert("Customer '" + name + "' added.", true);
                }
                else
                {
                    PKDatabaseHelper.UpdateCustomer(id, txtCode.Text.Trim(), customerType,
                        name, contact, phone, email, address, city, state, pinCode, gstin);
                    custId = id;
                    ShowAlert("Customer updated.", true);
                }

                // Save margins for Stockist/Distributor
                if (custId > 0 && (customerType == "ST" || customerType == "DI"))
                {
                    decimal smPct = 0, gtPct = 0;
                    decimal.TryParse(txtSMMargin != null ? txtSMMargin.Text.Trim() : "", out smPct);
                    decimal.TryParse(txtGTMargin != null ? txtGTMargin.Text.Trim() : "", out gtPct);
                    PKDatabaseHelper.SaveCustomerMargins(custId, smPct, gtPct);
                }

                ClearForm(); BindList();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        protected void btnClear_Click(object s, EventArgs e) { ClearForm(); if (pnlAlert != null) pnlAlert.Visible = false; }

        protected void btnToggle_Click(object s, EventArgs e)
        {
            int id = Convert.ToInt32(hfCustID.Value); if (id == 0) return;
            bool cur = btnToggle.Text == "Deactivate";
            PKDatabaseHelper.ToggleCustomer(id, !cur);
            ShowAlert("Customer " + (!cur ? "activated" : "deactivated") + ".", true);
            ClearForm(); BindList();
        }

        protected void rptList_Cmd(object src, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Edit") return;
            int id = Convert.ToInt32(e.CommandArgument);
            var row = PKDatabaseHelper.GetCustomerById(id);
            if (row == null) return;

            hfCustID.Value = id.ToString();
            txtCode.Text   = row["CustomerCode"].ToString();
            txtName.Text   = row["CustomerName"].ToString();
            txtContact.Text = row["ContactPerson"] == DBNull.Value ? "" : row["ContactPerson"].ToString();
            txtPhone.Text   = row["Phone"]    == DBNull.Value ? "" : row["Phone"].ToString();
            txtEmail.Text   = row["Email"]    == DBNull.Value ? "" : row["Email"].ToString();
            txtGSTIN.Text   = row["GSTIN"]    == DBNull.Value ? "" : row["GSTIN"].ToString();
            txtCity.Text    = row["City"]     == DBNull.Value ? "" : row["City"].ToString();
            txtState.Text   = row["State"]    == DBNull.Value ? "" : row["State"].ToString();
            txtAddress.Text = row["Address"]  == DBNull.Value ? "" : row["Address"].ToString();
            if (txtPinCode != null)
                txtPinCode.Text = row.Table.Columns.Contains("PinCode") && row["PinCode"] != DBNull.Value
                    ? row["PinCode"].ToString() : "";
            if (ddlCustomerType != null)
            {
                string custType = row.Table.Columns.Contains("CustomerType") && row["CustomerType"] != DBNull.Value
                    ? row["CustomerType"].ToString() : "";
                try { ddlCustomerType.SelectedValue = custType; } catch { ddlCustomerType.SelectedIndex = 0; }
            }
            lblFormTitle.Text = "Edit Customer";
            btnToggle.Text = Convert.ToBoolean(row["IsActive"]) ? "Deactivate" : "Activate";
            btnToggle.Visible = true;

            // Load margins
            if (txtSMMargin != null && txtGTMargin != null)
            {
                var margins = PKDatabaseHelper.GetCustomerMargins(id);
                if (margins != null)
                {
                    txtSMMargin.Text = Convert.ToDecimal(margins["SuperMarketPct"]).ToString("0.##");
                    txtGTMargin.Text = Convert.ToDecimal(margins["GTPct"]).ToString("0.##");
                }
                else { txtSMMargin.Text = ""; txtGTMargin.Text = ""; }
            }

            if (pnlAlert != null) pnlAlert.Visible = false;
        }

        // ── EXCEL UPLOAD ──
        protected void btnUpload_Click(object s, EventArgs e)
        {
            if (fuExcel == null || !fuExcel.HasFile)
            { ShowAlert("Please select an Excel file.", false); return; }

            string ext = Path.GetExtension(fuExcel.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls")
            { ShowAlert("Only .xlsx or .xls files are supported.", false); return; }

            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ext);
                fuExcel.SaveAs(tempPath);

                int imported = 0, skipped = 0;
                var errors = new System.Collections.Generic.List<string>();

                using (var fs = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
                {
                    DataTable dt;
                    if (ext == ".xlsx")
                    {
                        var wb = new ClosedXML.Excel.XLWorkbook(fs);
                        var ws = wb.Worksheet(1);
                        dt = new DataTable();
                        bool firstRow = true;
                        foreach (var row in ws.RowsUsed())
                        {
                            if (firstRow)
                            {
                                foreach (var cell in row.CellsUsed())
                                    dt.Columns.Add(cell.GetString().Trim());
                                firstRow = false;
                            }
                            else
                            {
                                var dr = dt.NewRow();
                                for (int i = 0; i < dt.Columns.Count && i < row.CellCount(); i++)
                                    dr[i] = row.Cell(i + 1).GetString().Trim();
                                dt.Rows.Add(dr);
                            }
                        }
                    }
                    else
                    { ShowAlert("Only .xlsx files are supported. Please save as .xlsx.", false); return; }

                    // Expected columns: CustomerType, Name, ContactPerson, Phone, Email, Address, City, State, PinCode, GSTIN
                    foreach (DataRow dr in dt.Rows)
                    {
                        string name = GetCol(dr, "Name", "CustomerName");
                        if (string.IsNullOrEmpty(name)) { skipped++; continue; }

                        string custType = GetCol(dr, "CustomerType", "Type");
                        if (string.IsNullOrEmpty(custType)) custType = "RT"; // default to Retail

                        // Normalize type code
                        string typeUpper = custType.ToUpper().Trim();
                        if (typeUpper.StartsWith("STOCK")) typeUpper = "ST";
                        else if (typeUpper.StartsWith("DIST")) typeUpper = "DI";
                        else if (typeUpper.StartsWith("RET") || typeUpper.StartsWith("CUST")) typeUpper = "RT";
                        else if (typeUpper.Length > 2) typeUpper = typeUpper.Substring(0, 2);

                        try
                        {
                            PKDatabaseHelper.AddCustomer(typeUpper, name,
                                GetCol(dr, "ContactPerson", "Contact"),
                                GetCol(dr, "Phone", "Mobile"),
                                GetCol(dr, "Email"),
                                GetCol(dr, "Address"),
                                GetCol(dr, "City"),
                                GetCol(dr, "State"),
                                GetCol(dr, "PinCode", "PIN"),
                                GetCol(dr, "GSTIN", "GST"));
                            imported++;
                        }
                        catch (Exception ex2)
                        {
                            errors.Add("Row " + (imported + skipped + 1) + ": " + ex2.Message);
                            skipped++;
                        }
                    }
                }

                try { File.Delete(tempPath); } catch { }

                string msg = imported + " customer(s) imported successfully.";
                if (skipped > 0) msg += " " + skipped + " skipped.";
                if (errors.Count > 0 && errors.Count <= 3)
                    msg += " Errors: " + string.Join("; ", errors);
                ShowAlert(msg, imported > 0);
                BindList();
            }
            catch (Exception ex) { ShowAlert("Upload error: " + ex.Message, false); }
        }

        string GetCol(DataRow dr, params string[] names)
        {
            foreach (string n in names)
            {
                if (dr.Table.Columns.Contains(n) && dr[n] != DBNull.Value)
                {
                    string val = dr[n].ToString().Trim();
                    if (!string.IsNullOrEmpty(val)) return val;
                }
            }
            return "";
        }

        // ── TEMPLATE DOWNLOAD ──
        protected void lnkTemplate_Click(object s, EventArgs e)
        {
            var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.AddWorksheet("Customers");
            string[] headers = { "CustomerType", "Name", "ContactPerson", "Phone", "Email", "Address", "City", "State", "PinCode", "GSTIN" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FEF9F3");
            }
            // Sample rows
            ws.Cell(2, 1).Value = "Stockist";   ws.Cell(2, 2).Value = "ABC Traders";
            ws.Cell(2, 3).Value = "Mr. Kumar";   ws.Cell(2, 4).Value = "9876543210";
            ws.Cell(2, 7).Value = "Bangalore";   ws.Cell(2, 8).Value = "Karnataka";
            ws.Cell(2, 9).Value = "560001";
            ws.Cell(3, 1).Value = "Distributor"; ws.Cell(3, 2).Value = "XYZ Distribution";
            ws.Cell(4, 1).Value = "Retail";      ws.Cell(4, 2).Value = "John Customer";

            // Add note
            ws.Cell(6, 1).Value = "CustomerType: Use Stockist/Distributor/Retail (or ST/DI/RT)";
            ws.Cell(6, 1).Style.Font.Italic = true;
            ws.Cell(6, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;

            ws.Columns().AdjustToContents();

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", "attachment; filename=Customer_Import_Template.xlsx");
            using (var ms = new MemoryStream())
            {
                wb.SaveAs(ms);
                ms.WriteTo(Response.OutputStream);
            }
            Response.End();
        }

        protected void lnkExportAll_Click(object s, EventArgs e)
        {
            var customers = PKDatabaseHelper.GetAllCustomers();
            if (customers.Rows.Count == 0) { ShowAlert("No customers to export.", false); return; }

            var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.AddWorksheet("Customers");

            // Headers
            string[] headers = { "CustomerCode", "CustomerType", "CustomerName", "ContactPerson",
                "Phone", "Email", "Address", "City", "State", "PinCode", "GSTIN", "IsActive" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#1a1a1a");
                ws.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            // Data rows
            int row = 2;
            foreach (System.Data.DataRow r in customers.Rows)
            {
                ws.Cell(row, 1).Value = r["CustomerCode"]?.ToString() ?? "";
                ws.Cell(row, 2).Value = r["CustomerType"]?.ToString() ?? "";
                ws.Cell(row, 3).Value = r["CustomerName"]?.ToString() ?? "";
                ws.Cell(row, 4).Value = r["ContactPerson"] != System.DBNull.Value ? r["ContactPerson"].ToString() : "";
                ws.Cell(row, 5).Value = r["Phone"] != System.DBNull.Value ? r["Phone"].ToString() : "";
                ws.Cell(row, 6).Value = r["Email"] != System.DBNull.Value ? r["Email"].ToString() : "";
                ws.Cell(row, 7).Value = customers.Columns.Contains("Address") && r["Address"] != System.DBNull.Value ? r["Address"].ToString() : "";
                ws.Cell(row, 8).Value = r["City"] != System.DBNull.Value ? r["City"].ToString() : "";
                ws.Cell(row, 9).Value = r["State"] != System.DBNull.Value ? r["State"].ToString() : "";
                ws.Cell(row, 10).Value = r["PinCode"] != System.DBNull.Value ? r["PinCode"].ToString() : "";
                ws.Cell(row, 11).Value = r["GSTIN"] != System.DBNull.Value ? r["GSTIN"].ToString() : "";
                ws.Cell(row, 12).Value = r["IsActive"] != System.DBNull.Value && Convert.ToInt32(r["IsActive"]) == 1 ? "Yes" : "No";
                row++;
            }

            ws.Columns().AdjustToContents();

            // Auto-filter
            ws.RangeUsed().SetAutoFilter();

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", "attachment; filename=All_Customers_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx");
            using (var ms = new MemoryStream())
            {
                wb.SaveAs(ms);
                ms.WriteTo(Response.OutputStream);
            }
            Response.End();
        }

        void ClearForm()
        {
            hfCustID.Value = "0";
            txtCode.Text = txtName.Text = txtContact.Text = "";
            txtPhone.Text = txtEmail.Text = txtGSTIN.Text = txtCity.Text = "";
            txtState.Text = txtAddress.Text = "";
            if (txtPinCode != null) txtPinCode.Text = "";
            if (txtSMMargin != null) txtSMMargin.Text = "";
            if (txtGTMargin != null) txtGTMargin.Text = "";
            if (ddlCustomerType != null) ddlCustomerType.SelectedIndex = 0;
            btnToggle.Visible = false;
            lblFormTitle.Text = "New Customer";
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
    }
}
