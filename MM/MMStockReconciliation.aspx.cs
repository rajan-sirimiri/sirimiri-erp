using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMStockReconciliation : Page
    {
        protected Label        lblNavUser, lblAlert, lblTotalItems, lblEntered, lblPending, lblReconStatus;
        protected Panel        pnlAlert, pnlEmpty;
        protected TextBox      txtReconDate;
        protected Repeater     rptStock;
        protected Button       btnLoadDate, btnReconcile, btnSaveRow, btnDownload;
        protected Button       btnTabRM, btnTabPM, btnTabCM, btnTabST;
        protected HiddenField  hfTab, hfReconciled, hfSaveData;

        private Dictionary<int, decimal> _physMap;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            // Module access check
            string __role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(__role, "MM", "MM_RECON"))
            { Response.Redirect("MMHome.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                txtReconDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                BindStock();
            }
            else
            {
                // Postback — nothing special needed here
            }
        }

        private void HandleRowSave(string arg)
        {
            // Format: SAVE_ROW:materialId:qty
            string[] parts = arg.Split(':');
            if (parts.Length < 3) return;

            int matId;
            decimal qty;
            if (!int.TryParse(parts[1], out matId) || !decimal.TryParse(parts[2], out qty)) return;

            DateTime sessionDate;
            if (!DateTime.TryParse(txtReconDate.Text, out sessionDate))
                sessionDate = DateTime.Today;

            string matType = hfTab.Value;
            int userId = Convert.ToInt32(Session["MM_UserID"]);

            try
            {
                MMDatabaseHelper.SavePhysicalStock(sessionDate, matType, matId, qty, userId);
                // Rebind to show updated status
                BindStock();
                ShowAlert("Saved physical count for item.", true);
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void btnSaveRow_Click(object sender, EventArgs e)
        {
            string data = hfSaveData.Value ?? "";
            if (string.IsNullOrEmpty(data)) return;
            HandleRowSave("SAVE_ROW:" + data);
            hfSaveData.Value = "";
        }

        protected void btnTab_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            hfTab.Value = btn.CommandArgument;
            hfReconciled.Value = "0";
            SetActiveTab();
            BindStock();
            pnlAlert.Visible = false;
        }

        protected void btnLoadDate_Click(object sender, EventArgs e)
        {
            hfReconciled.Value = "0";
            BindStock();
            pnlAlert.Visible = false;
        }

        protected void btnReconcile_Click(object sender, EventArgs e)
        {
            DateTime sessionDate;
            if (!DateTime.TryParse(txtReconDate.Text, out sessionDate))
            {
                ShowAlert("Please select a valid date.", false);
                return;
            }

            string matType = hfTab.Value;
            DataTable stock = MMDatabaseHelper.GetStockForReconciliation(matType);
            LoadPhysicalMap(sessionDate, matType);

            int saved = 0;
            int warnings = 0;
            foreach (DataRow r in stock.Rows)
            {
                int matId = Convert.ToInt32(r["MaterialID"]);
                decimal sysQty = Convert.ToDecimal(r["SystemStock"]);
                if (!_physMap.ContainsKey(matId)) continue;

                decimal physQty = _physMap[matId];
                try
                {
                    MMDatabaseHelper.SaveReconciliationSnapshot(sessionDate, matType, matId, physQty, sysQty);
                    saved++;
                    decimal pct = sysQty != 0 ? Math.Abs((physQty - sysQty) / sysQty) * 100 : 0;
                    if (pct > 1) warnings++;
                }
                catch { }
            }

            hfReconciled.Value = "1";
            btnDownload.Visible = saved > 0;
            BindStock();

            if (saved == 0)
                ShowAlert("No physical counts entered yet. Please enter counts and save before reconciling.", false);
            else
                ShowAlert("Reconciled " + saved + " item" + (saved == 1 ? "" : "s") + "." +
                    (warnings > 0 ? " " + warnings + " item" + (warnings == 1 ? "" : "s") + " with >1% variance (shown in red)." : " All within tolerance."), 
                    warnings == 0);
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            DateTime sessionDate;
            if (!DateTime.TryParse(txtReconDate.Text, out sessionDate))
                sessionDate = DateTime.Today;

            string matType = hfTab.Value;
            string typeName = "Raw Materials";
            if (matType == "PM") typeName = "Packing Materials";
            else if (matType == "CM") typeName = "Consumables";
            else if (matType == "ST") typeName = "Stationary";

            LoadPhysicalMap(sessionDate, matType);
            DataTable stock = MMDatabaseHelper.GetStockForReconciliation(matType);

            // Build HTML report
            var sb = new System.Text.StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
            sb.Append("<title>Stock Reconciliation Report</title>");
            sb.Append("<style>");
            sb.Append("body{font-family:'Segoe UI',Arial,sans-serif;margin:30px;color:#1a1a1a;}");
            sb.Append("h1{font-size:20px;margin-bottom:4px;}h2{font-size:14px;color:#666;margin-bottom:20px;font-weight:normal;}");
            sb.Append("table{width:100%;border-collapse:collapse;font-size:12px;}");
            sb.Append("th{background:#f5f5f5;border:1px solid #ddd;padding:8px 10px;text-align:left;font-size:10px;text-transform:uppercase;letter-spacing:.05em;color:#666;}");
            sb.Append("td{border:1px solid #ddd;padding:7px 10px;}");
            sb.Append(".num{text-align:right;font-weight:600;}");
            sb.Append(".warn{background:#fdf3f2;color:#c0392b;font-weight:700;}");
            sb.Append(".ok{color:#1a9e6a;}");
            sb.Append(".header-bar{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:20px;}");
            sb.Append(".logo{font-size:24px;font-weight:700;letter-spacing:.08em;}");
            sb.Append(".logo span{color:#e07b00;}");
            sb.Append(".summary{font-size:11px;color:#666;margin-bottom:12px;}");
            sb.Append("</style></head><body>");
            sb.Append("<div class='header-bar'>");
            sb.Append("<div><div class='logo'>SIRIMIRI <span>NUTRITION</span></div>");
            sb.Append("<h1>Stock Reconciliation Report</h1>");
            sb.Append("<h2>" + typeName + " — " + sessionDate.ToString("dd MMM yyyy") + "</h2></div></div>");

            int totalItems = stock.Rows.Count;
            int counted = 0;
            int variances = 0;

            sb.Append("<table><tr><th>#</th><th>Material</th><th>Code</th><th>UOM</th>");
            sb.Append("<th style='text-align:right;'>Physical</th><th style='text-align:right;'>System</th>");
            sb.Append("<th style='text-align:right;'>Variance</th><th style='text-align:right;'>%</th></tr>");

            int idx = 0;
            foreach (DataRow r in stock.Rows)
            {
                idx++;
                int matId = Convert.ToInt32(r["MaterialID"]);
                decimal sysQty = Convert.ToDecimal(r["SystemStock"]);
                decimal physQty = _physMap.ContainsKey(matId) ? _physMap[matId] : -1;

                if (physQty < 0) continue; // skip items without physical count
                counted++;

                decimal variance = physQty - sysQty;
                decimal pct = sysQty != 0 ? (variance / sysQty) * 100 : (physQty != 0 ? 100 : 0);
                bool isWarn = Math.Abs(pct) > 1;
                if (isWarn) variances++;

                string cls = isWarn ? " class='warn'" : "";
                sb.Append("<tr" + cls + ">");
                sb.Append("<td>" + idx + "</td>");
                sb.Append("<td>" + r["Name"] + "</td>");
                sb.Append("<td>" + r["Code"] + "</td>");
                sb.Append("<td>" + r["UOM"] + "</td>");
                sb.Append("<td class='num'>" + physQty.ToString("N2") + "</td>");
                sb.Append("<td class='num'>" + sysQty.ToString("N2") + "</td>");
                sb.Append("<td class='num'>" + variance.ToString("N2") + "</td>");
                sb.Append("<td class='num'>" + pct.ToString("N1") + "%</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            sb.Append("<div class='summary' style='margin-top:16px;'>Total: " + counted + " items counted. " + variances + " with >1% variance.</div>");
            sb.Append("<div class='summary'>Generated: " + DateTime.Now.ToString("dd MMM yyyy HH:mm") + "</div>");
            sb.Append("</body></html>");

            // Send as downloadable HTML file
            Response.Clear();
            Response.ContentType = "text/html";
            Response.AddHeader("Content-Disposition",
                "attachment; filename=StockRecon_" + matType + "_" + sessionDate.ToString("yyyy-MM-dd") + ".html");
            Response.Write(sb.ToString());
            Response.End();
        }

        private void SetActiveTab()
        {
            btnTabRM.CssClass = hfTab.Value == "RM" ? "tab-btn active" : "tab-btn";
            btnTabPM.CssClass = hfTab.Value == "PM" ? "tab-btn active" : "tab-btn";
            btnTabCM.CssClass = hfTab.Value == "CM" ? "tab-btn active" : "tab-btn";
            btnTabST.CssClass = hfTab.Value == "ST" ? "tab-btn active" : "tab-btn";
        }

        private void BindStock()
        {
            SetActiveTab();
            string matType = hfTab.Value;

            DateTime sessionDate;
            if (!DateTime.TryParse(txtReconDate.Text, out sessionDate))
                sessionDate = DateTime.Today;

            // Load physical counts for this session
            LoadPhysicalMap(sessionDate, matType);

            DataTable dt = MMDatabaseHelper.GetStockForReconciliation(matType);
            pnlEmpty.Visible = dt.Rows.Count == 0;
            rptStock.DataSource = dt;
            rptStock.DataBind();

            lblTotalItems.Text = dt.Rows.Count.ToString();
            int entered = _physMap.Count;
            lblEntered.Text = entered.ToString();
            lblPending.Text = (dt.Rows.Count - entered).ToString();
        }

        private void LoadPhysicalMap(DateTime sessionDate, string matType)
        {
            _physMap = new Dictionary<int, decimal>();
            DataTable phys = MMDatabaseHelper.GetPhysicalStock(sessionDate, matType);
            foreach (DataRow r in phys.Rows)
            {
                int mid = Convert.ToInt32(r["MaterialID"]);
                decimal qty = Convert.ToDecimal(r["PhysicalQty"]);
                _physMap[mid] = qty;
            }
        }

        // Helper methods called from ASPX
        protected string GetPhysicalQty(object materialIdObj)
        {
            if (_physMap == null) return "";
            int mid = Convert.ToInt32(materialIdObj);
            return _physMap.ContainsKey(mid) ? _physMap[mid].ToString("0.####") : "";
        }

        protected bool IsPhysicalSaved(object materialIdObj)
        {
            if (_physMap == null) return false;
            int mid = Convert.ToInt32(materialIdObj);
            return _physMap.ContainsKey(mid);
        }

        protected string RenderPhysicalCell(object materialIdObj)
        {
            int mid = Convert.ToInt32(materialIdObj);
            if (_physMap != null && _physMap.ContainsKey(mid))
                return "<span class='phys-saved'>" + _physMap[mid].ToString("0.####") + "</span>";
            else
                return "<input type='text' class='phys-input' data-mid='" + mid + "' value='' placeholder='0'/>";
        }

        protected string RenderActionCell(object materialIdObj)
        {
            int mid = Convert.ToInt32(materialIdObj);
            if (_physMap != null && _physMap.ContainsKey(mid))
                return "<span class='save-done'>&#x2714; Done</span>";
            else
                return "<button type='button' class='btn-save-row' onclick='saveRow(this);'>Save</button>";
        }

        private void ShowAlert(string msg, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }
    }
}
