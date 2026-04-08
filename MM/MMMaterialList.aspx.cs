using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMMaterialList : Page
    {
        protected Label      lblDate, lblUser;
        protected Panel      pnlRM, pnlPM, pnlCN, pnlST;
        protected Repeater   rptRM, rptPM, rptCN, rptST;
        protected Label      lblRMCount, lblPMCount, lblCNCount, lblSTCount;
        protected CheckBox   chkRM, chkPM, chkCN, chkST, chkInactive;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            lblDate.Text = DateTime.Now.ToString("dd-MMM-yyyy HH:mm");
            lblUser.Text = Session["MM_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
                BindAll();
        }

        protected void chkFilter_Changed(object sender, EventArgs e)
        {
            BindAll();
        }

        private void BindAll()
        {
            string activeFilter = chkInactive.Checked ? "" : " WHERE IsActive=1";
            string activeFilterAnd = chkInactive.Checked ? "" : " AND r.IsActive=1";

            // Raw Materials
            pnlRM.Visible = chkRM.Checked;
            if (chkRM.Checked)
            {
                DataTable rm = MMDatabaseHelper.ExecuteQueryPublic(
                    "SELECT r.RMID, r.RMCode, r.RMName, r.Description, r.HSNCode, r.GSTRate," +
                    " r.ReorderLevel, r.IsActive, u.Abbreviation" +
                    " FROM MM_RawMaterials r JOIN MM_UOM u ON u.UOMID=r.UOMID" +
                    activeFilterAnd.Replace("r.", "r.") +
                    " ORDER BY r.RMName;");
                rptRM.DataSource = rm;
                rptRM.DataBind();
                lblRMCount.Text = rm.Rows.Count.ToString();
            }

            // Packing Materials
            pnlPM.Visible = chkPM.Checked;
            if (chkPM.Checked)
            {
                DataTable pm = MMDatabaseHelper.ExecuteQueryPublic(
                    "SELECT p.PMID, p.PMCode, p.PMName, p.PMCategory, p.Description, p.HSNCode, p.GSTRate," +
                    " p.ReorderLevel, p.IsActive, u.Abbreviation" +
                    " FROM MM_PackingMaterials p JOIN MM_UOM u ON u.UOMID=p.UOMID" +
                    activeFilterAnd.Replace("r.", "p.") +
                    " ORDER BY p.PMName;");
                rptPM.DataSource = pm;
                rptPM.DataBind();
                lblPMCount.Text = pm.Rows.Count.ToString();
            }

            // Consumables
            pnlCN.Visible = chkCN.Checked;
            if (chkCN.Checked)
            {
                DataTable cn = MMDatabaseHelper.ExecuteQueryPublic(
                    "SELECT c.ConsumableID, c.ConsumableCode, c.ConsumableName, c.Description, c.HSNCode, c.GSTRate," +
                    " c.ReorderLevel, c.IsActive, u.Abbreviation" +
                    " FROM MM_Consumables c JOIN MM_UOM u ON u.UOMID=c.UOMID" +
                    activeFilterAnd.Replace("r.", "c.") +
                    " ORDER BY c.ConsumableName;");
                rptCN.DataSource = cn;
                rptCN.DataBind();
                lblCNCount.Text = cn.Rows.Count.ToString();
            }

            // Stationaries
            pnlST.Visible = chkST.Checked;
            if (chkST.Checked)
            {
                DataTable st = MMDatabaseHelper.ExecuteQueryPublic(
                    "SELECT s.StationaryID, s.StationaryCode, s.StationaryName, s.Description, s.HSNCode, s.GSTRate," +
                    " s.ReorderLevel, s.IsActive, u.Abbreviation" +
                    " FROM MM_Stationaries s JOIN MM_UOM u ON u.UOMID=s.UOMID" +
                    activeFilterAnd.Replace("r.", "s.") +
                    " ORDER BY s.StationaryName;");
                rptST.DataSource = st;
                rptST.DataBind();
                lblSTCount.Text = st.Rows.Count.ToString();
            }
        }
    }
}
