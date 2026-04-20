using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    public partial class FINChartOfAccounts : System.Web.UI.Page
    {
        // Server controls declared in the .aspx markup
        protected Label lblNavUser;
        protected DropDownList ddlType, ddlStatus;
        protected TextBox txtSearch;
        protected LinkButton btnSync;
        protected Literal litTotalCount, litActiveCount, litLastSync, litListCount;
        protected PlaceHolder phBanner, phList;

        // Finance roles from the central list in FINConsignments
        bool IsFinance => FINConsignments.IsFinanceRole(Session["FIN_Role"]?.ToString() ?? "");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            if (lblNavUser != null)
                lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            // Only Finance / Super can trigger a sync; everyone else sees read-only
            btnSync.Visible = IsFinance;

            RefreshMeta();
            RenderList();
        }

        protected void Filter_Changed(object s, EventArgs e) { RenderList(); }

        protected void btnSync_Click(object s, EventArgs e)
        {
            if (!IsFinance)
            {
                ShowBanner("Only Finance / Super users can sync the chart of accounts.", "error");
                return;
            }
            try
            {
                int count = FINZohoHelper.SyncChartOfAccountsFromZoho();
                ShowBanner("Synced " + count + " account(s) from Zoho Books.", "success");
                RefreshMeta();
                RenderList();
            }
            catch (Exception ex)
            {
                ShowBanner("Sync failed: " + ex.Message, "error");
            }
        }

        void RefreshMeta()
        {
            var all = FINDatabaseHelper.GetChartOfAccounts(activeOnly: false, typeFilter: null);
            int total = all.Rows.Count;
            int active = 0;
            foreach (DataRow r in all.Rows)
                if (Convert.ToInt32(r["IsActive"]) == 1) active++;

            litTotalCount.Text  = total.ToString();
            litActiveCount.Text = active.ToString();

            DateTime last = FINDatabaseHelper.GetChartOfAccountsLastSync();
            if (last == DateTime.MinValue)
                litLastSync.Text = "<span class='meta-empty'>Never synced</span>";
            else
            {
                // Stale = older than 30 days
                string cls = (DateTime.Now - last).TotalDays > 30 ? "meta-stale" : "meta-ok";
                litLastSync.Text = "<span class='" + cls + "'>" + last.ToString("dd MMM yyyy, HH:mm") + "</span>";
            }
        }

        void RenderList()
        {
            phList.Controls.Clear();

            string statusMode = ddlStatus.SelectedValue;
            string typeFilter = string.IsNullOrEmpty(ddlType.SelectedValue) ? null : ddlType.SelectedValue;
            string search = (txtSearch.Text ?? "").Trim().ToLower();

            bool activeOnly = (statusMode == "active");
            var dt = FINDatabaseHelper.GetChartOfAccounts(activeOnly: activeOnly, typeFilter: typeFilter);

            // Client-side name/code search
            var filtered = new System.Collections.Generic.List<DataRow>();
            foreach (DataRow r in dt.Rows)
            {
                if (!string.IsNullOrEmpty(search))
                {
                    string name = (r["AccountName"] ?? "").ToString().ToLower();
                    string code = (r["AccountCode"] ?? "").ToString().ToLower();
                    if (!name.Contains(search) && !code.Contains(search)) continue;
                }
                filtered.Add(r);
            }

            litListCount.Text = filtered.Count.ToString();

            if (dt.Rows.Count == 0)
            {
                var empty = new Panel { CssClass = "tbl" };
                empty.Controls.Add(new LiteralControl(
                    "<div class='empty-state'>" +
                    "<strong>No accounts cached yet</strong>" +
                    "Click <b>Sync from Zoho</b> above to pull your chart of accounts into the ERP. This is a one-time setup before you can create journal entries." +
                    "</div>"));
                phList.Controls.Add(empty);
                return;
            }

            if (filtered.Count == 0)
            {
                var empty = new Panel { CssClass = "tbl" };
                empty.Controls.Add(new LiteralControl(
                    "<div class='empty-state'>" +
                    "<strong>No matches</strong>" +
                    "Try a different filter or clear the search box." +
                    "</div>"));
                phList.Controls.Add(empty);
                return;
            }

            var tbl = new Table { CssClass = "tbl" };
            var hdr = new TableHeaderRow();
            foreach (var h in new[] { "Code", "Account Name", "Type", "Status", "Last Sync" })
            {
                var cell = new TableHeaderCell();
                cell.Controls.Add(new LiteralControl(h));
                switch (h)
                {
                    case "Code":     cell.CssClass = "col-code"; break;
                    case "Type":     cell.CssClass = "col-type"; break;
                    case "Status":   cell.CssClass = "col-status"; break;
                    case "Last Sync":cell.CssClass = "col-date"; break;
                }
                hdr.Cells.Add(cell);
            }
            tbl.Rows.Add(hdr);

            foreach (var r in filtered)
            {
                var tr = new TableRow();

                var cCode = new TableCell { CssClass = "col-code" };
                cCode.Text = Server.HtmlEncode((r["AccountCode"] ?? "").ToString());
                tr.Cells.Add(cCode);

                var cName = new TableCell();
                cName.Text = "<strong>" + Server.HtmlEncode((r["AccountName"] ?? "").ToString()) + "</strong>";
                tr.Cells.Add(cName);

                var cType = new TableCell { CssClass = "col-type" };
                cType.Text = Server.HtmlEncode((r["AccountTypeName"] ?? r["AccountType"] ?? "").ToString());
                tr.Cells.Add(cType);

                var cStat = new TableCell { CssClass = "col-status" };
                bool active = Convert.ToInt32(r["IsActive"]) == 1;
                cStat.Text = active
                    ? "<span class='badge badge-active'>Active</span>"
                    : "<span class='badge badge-inactive'>Inactive</span>";
                tr.Cells.Add(cStat);

                var cDate = new TableCell { CssClass = "col-date" };
                cDate.Text = r["LastSyncAt"] == DBNull.Value
                    ? "—"
                    : Convert.ToDateTime(r["LastSyncAt"]).ToString("dd MMM HH:mm");
                tr.Cells.Add(cDate);

                tbl.Rows.Add(tr);
            }

            phList.Controls.Add(tbl);
        }

        void ShowBanner(string msg, string kind)
        {
            phBanner.Controls.Clear();
            string cls = "banner banner-" + (kind == "success" ? "success" : kind == "error" ? "error" : "info");
            phBanner.Controls.Add(new LiteralControl(
                "<div class='" + cls + "'>" + Server.HtmlEncode(msg) + "</div>"));
        }
    }
}
