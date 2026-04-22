using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    /// <summary>
    /// Manage service categories — rename or merge. Categories are derived from
    /// mm_suppliers.ServiceCategory (no dedicated table), so rename is a mass-update
    /// across providers and merge is a rename into an existing category name.
    /// Finance role required.
    /// </summary>
    public partial class FINServiceCategoryManage : System.Web.UI.Page
    {
        protected Label lblNavUser, lblAlert, lblCount;
        protected Label lblRenameFrom, lblMergeFrom;
        protected HtmlGenericControl alertBox;
        protected Panel pnlAlert, pnlRename, pnlMerge;
        protected HiddenField hfRenameFrom, hfMergeFrom;
        protected TextBox txtNewName;
        protected DropDownList ddlMergeTarget;
        protected Button btnRenameSave, btnRenameCancel, btnMergeSave, btnMergeCancel;
        protected Repeater rptCategories;

        private string UserRole  => Session["FIN_Role"]?.ToString() ?? "";
        private bool   IsFinance => FINConsignments.IsFinanceRole(UserRole);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }
            if (!IsFinance)
            {
                Response.Redirect("FINHome.aspx");
                return;
            }

            if (lblNavUser != null)
                lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                LoadCategories();
            }
        }

        // ══════════════════════════════════════════════════════════════
        // List
        // ══════════════════════════════════════════════════════════════

        private void LoadCategories()
        {
            DataTable dt = FINDatabaseHelper.GetCategoryUsageCounts();
            rptCategories.DataSource = dt;
            rptCategories.DataBind();
            lblCount.Text = dt.Rows.Count + " categor" + (dt.Rows.Count == 1 ? "y" : "ies");
        }

        // ══════════════════════════════════════════════════════════════
        // Row actions — open Rename / Merge panel
        // ══════════════════════════════════════════════════════════════

        protected void rptCategories_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string cat = e.CommandArgument?.ToString() ?? "";
            if (string.IsNullOrEmpty(cat)) return;

            HideAllPanels();
            pnlAlert.Visible = false;

            if (e.CommandName == "Rename")
            {
                hfRenameFrom.Value = cat;
                lblRenameFrom.Text = cat;
                txtNewName.Text = cat;  // pre-fill so operator tweaks rather than retypes
                pnlRename.Visible = true;
            }
            else if (e.CommandName == "Merge")
            {
                hfMergeFrom.Value = cat;
                lblMergeFrom.Text = cat;
                PopulateMergeTargets(cat);
                pnlMerge.Visible = true;
            }
        }

        private void PopulateMergeTargets(string excludeCat)
        {
            ddlMergeTarget.Items.Clear();
            ddlMergeTarget.Items.Add(new ListItem("-- Select target --", ""));
            foreach (var c in FINDatabaseHelper.GetServiceCategories())
            {
                if (string.Equals(c, excludeCat, StringComparison.OrdinalIgnoreCase)) continue;
                ddlMergeTarget.Items.Add(new ListItem(c, c));
            }
        }

        // ══════════════════════════════════════════════════════════════
        // Save / Cancel
        // ══════════════════════════════════════════════════════════════

        protected void btnRenameSave_Click(object sender, EventArgs e)
        {
            string from = (hfRenameFrom.Value ?? "").Trim();
            string to   = (txtNewName.Text   ?? "").Trim();

            if (string.IsNullOrEmpty(from)) { ShowAlert("Source category missing.", false); return; }
            if (string.IsNullOrEmpty(to))   { ShowAlert("New name is required.", false);    return; }
            if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
            {
                ShowAlert("New name is the same as the current name. Nothing to do.", false);
                return;
            }

            try
            {
                int affected = FINDatabaseHelper.RenameServiceCategory(from, to);
                string msg = "Renamed '" + from + "' to '" + to + "'";
                if (affected > 0) msg += " (" + affected + " provider" + (affected == 1 ? "" : "s") + " updated)";
                else              msg += " (no providers were using this category yet)";
                ShowAlert(msg + ".", true);

                HideAllPanels();
                LoadCategories();
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void btnMergeSave_Click(object sender, EventArgs e)
        {
            string from = (hfMergeFrom.Value       ?? "").Trim();
            string to   = (ddlMergeTarget.SelectedValue ?? "").Trim();

            if (string.IsNullOrEmpty(from)) { ShowAlert("Source category missing.", false); return; }
            if (string.IsNullOrEmpty(to))   { ShowAlert("Pick a target category to merge into.", false); return; }
            if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
            {
                ShowAlert("Source and target are the same. Nothing to do.", false);
                return;
            }

            try
            {
                int affected = FINDatabaseHelper.RenameServiceCategory(from, to);
                string msg = "Merged '" + from + "' into '" + to + "'";
                if (affected > 0) msg += " (" + affected + " provider" + (affected == 1 ? "" : "s") + " reassigned)";
                else              msg += " (no providers were using the source category)";
                ShowAlert(msg + ".", true);

                HideAllPanels();
                LoadCategories();
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            pnlAlert.Visible = false;
        }

        // ══════════════════════════════════════════════════════════════
        // Helpers
        // ══════════════════════════════════════════════════════════════

        private void HideAllPanels()
        {
            pnlRename.Visible = false;
            pnlMerge.Visible  = false;
            hfRenameFrom.Value = "";
            hfMergeFrom.Value  = "";
            txtNewName.Text    = "";
        }

        private void ShowAlert(string message, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = message;
            alertBox.Attributes["class"] = "alert " + (success ? "alert-success" : "alert-danger");
        }
    }
}
