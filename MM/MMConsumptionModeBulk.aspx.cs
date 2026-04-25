using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMConsumptionModeBulk : Page
    {
        protected Label        lblNavUser;
        protected Label        lblAlert;
        protected Panel        pnlAlert;
        protected Panel        pnlEmpty;
        protected HiddenField  hfChanges;
        protected Repeater     rptMaterials;
        protected Button       btnSaveAll;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            // Module access — gate behind one of the master access flags
            string role = Session["MM_Role"]?.ToString() ?? "";
            bool hasAccess =
                MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_PM_MASTER") ||
                MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_CM_MASTER") ||
                MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_ST_MASTER");
            if (!hasAccess) { Response.Redirect("MMHome.aspx"); return; }

            lblNavUser.Text = Session["MM_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
                LoadMaterials();
        }

        private void LoadMaterials()
        {
            DataTable dt = MMDatabaseHelper.GetMaterialsForBulkEdit();
            if (dt != null && dt.Rows.Count > 0)
            {
                rptMaterials.DataSource = dt;
                rptMaterials.DataBind();
                pnlEmpty.Visible = false;
            }
            else
            {
                rptMaterials.DataSource = null;
                rptMaterials.DataBind();
                pnlEmpty.Visible = true;
            }
        }

        protected void btnSaveAll_Click(object sender, EventArgs e)
        {
            string json = (hfChanges.Value ?? "{}").Trim();
            if (string.IsNullOrEmpty(json) || json == "{}")
            {
                ShowAlert("No changes to save.", false);
                return;
            }

            try
            {
                var ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                var changes = ser.Deserialize<Dictionary<string, string>>(json);
                if (changes == null || changes.Count == 0)
                {
                    ShowAlert("No changes to save.", false);
                    return;
                }

                int saved = 0;
                foreach (var kv in changes)
                {
                    // key shape: "PM_12" / "CN_45" / "ST_3"
                    string key = kv.Key ?? "";
                    string mode = kv.Value ?? "";
                    int us = key.IndexOf('_');
                    if (us <= 0 || us == key.Length - 1) continue;
                    string type = key.Substring(0, us);
                    int id;
                    if (!int.TryParse(key.Substring(us + 1), out id)) continue;
                    if (type != "PM" && type != "CN" && type != "ST") continue;
                    if (mode != "AT_ISSUE" && mode != "IN_PRODUCTION") continue;

                    MMDatabaseHelper.SetConsumptionMode(type, id, mode);
                    saved++;
                }

                ShowAlert(saved + " material" + (saved == 1 ? "" : "s") + " updated.", true);
                hfChanges.Value = "{}";
                LoadMaterials();
            }
            catch (Exception ex)
            {
                ShowAlert("Save failed: " + ex.Message, false);
            }
        }

        private void ShowAlert(string msg, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text    = msg;
            // Apply class via inline style on the inner div using CssClass on the Panel won't work because we used a fixed div. Use literal:
            string cls = success ? "alert alert-success" : "alert alert-danger";
            ScriptManager.RegisterStartupScript(this, GetType(), "alertCls",
                "var a=document.getElementById('divAlert'); if(a){ a.className='" + cls + "'; }", true);
        }
    }
}
