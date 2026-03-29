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
        protected Panel pnlAlert, pnlForm, pnlLocked, pnlEmpty, pnlList;
        protected HiddenField hfDCID, hfLines, hfProductData;
        protected TextBox txtDCNumber, txtDCDate, txtRemarks;
        protected DropDownList ddlCustomer;
        protected Button btnDraftSave, btnFinalise, btnNew, btnNewFromLocked;
        protected Repeater rptDCs;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null)
            { Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery)); return; }
            if (lblUser != null) lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack)
            {
                BindCustomers();
                BuildProductData();
                txtDCDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                BindDCList();
            }
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
                int unitSize = 1;
                int.TryParse(firstUnitSize, out unitSize);
                if (unitSize <= 0) unitSize = 1;

                if (!first) sb.Append(",");
                sb.Append("\"" + pid + "\":{");
                sb.Append("\"name\":\"" + Esc(r["ProductName"].ToString()) + "\",");
                sb.Append("\"code\":\"" + Esc(r["ProductCode"].ToString()) + "\",");
                sb.Append("\"unitSize\":" + unitSize + ",");
                sb.Append("\"jarsPerCase\":" + jpc + ",");
                sb.Append("\"availJars\":" + availJars);
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

            // Validate FG stock (in jars)
            var fgStock = PKDatabaseHelper.GetFGStockForShipment();
            foreach (var line in lineData)
            {
                DataRow stockRow = null;
                foreach (DataRow r in fgStock.Rows)
                    if (Convert.ToInt32(r["ProductID"]) == line[0]) { stockRow = r; break; }
                if (stockRow == null)
                { ShowAlert("Product ID " + line[0] + " has no FG stock.", false); return; }
                int availJars = Convert.ToInt32(stockRow["AvailableFGJars"]);
                int jpc = line[3]; // jarsPerCase
                int lineJars = (line[1] * jpc) + line[2]; // cases*jpc + looseJars
                // If editing existing DC, add back the previously saved jars for this DC
                if (dcId > 0)
                {
                    var existingLines = PKDatabaseHelper.GetDCLines(dcId);
                    foreach (DataRow el in existingLines.Rows)
                        if (Convert.ToInt32(el["ProductID"]) == line[0])
                            availJars += (Convert.ToInt32(el["Cases"]) * Convert.ToInt32(el["JarsPerCase"])) + Convert.ToInt32(el["LooseJars"]);
                }
                if (lineJars > availJars)
                { ShowAlert("Insufficient FG stock for product. Need " + lineJars + " jars, available " + availJars + ".", false); return; }
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
            BuildProductData();
            if (pnlAlert != null) pnlAlert.Visible = false;
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
            }
            else
            {
                lblFormTitle.Text = "DC: " + dc["DCNumber"] + " (Finalised)";
                pnlForm.Visible = false;
                pnlLocked.Visible = true;
            }

            BuildProductData();
            if (pnlAlert != null) pnlAlert.Visible = false;
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
    }
}
