using System; using System.Data; using System.Web.UI; using System.Web.UI.WebControls; using PKApp.DAL;
namespace PKApp {
    public partial class PKPrimaryPacking : Page {
        protected Label lblUser, lblAlert, lblPending;
        protected Panel pnlAlert, pnlBatchEmpty, pnlBatchTable, pnlLogEmpty, pnlLogTable;
        protected DropDownList ddlBatch, ddlPM;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtQtyPack, txtPMQty;
        protected Repeater rptBatches, rptLog;
        protected Button btnPack;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e) {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack) { BindBatchDropdown(); BindPMDropdown(); BindBatchList(); BindLog(); }
        }

        void BindBatchDropdown() {
            var dt = PKDatabaseHelper.GetCompletedBatchesForPacking();
            ddlBatch.Items.Clear();
            ddlBatch.Items.Add(new ListItem("-- Select Batch --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlBatch.Items.Add(new ListItem(
                    r["ProductName"] + " — Batch " + r["BatchNo"] + " (" + string.Format("{0:0.###}", r["PendingQty"]) + " " + r["OutputUnit"] + " pending)",
                    r["ExecutionID"] + "|" + r["ProductID"] + "|" + r["OrderID"] + "|" + r["BatchNo"] + "|" + r["PendingQty"]));
        }

        void BindPMDropdown() {
            var dt = PKDatabaseHelper.GetActivePackingMaterials();
            ddlPM.Items.Clear();
            ddlPM.Items.Add(new ListItem("-- None --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlPM.Items.Add(new ListItem(r["PMName"] + " (" + r["Abbreviation"] + ")", r["PMID"].ToString()));
        }

        protected void ddlBatch_Changed(object s, EventArgs e) {
            string val = ddlBatch.SelectedValue;
            if (val == "0") { lblPending.Text = "—"; return; }
            string[] parts = val.Split('|');
            if (parts.Length >= 5) lblPending.Text = string.Format("{0:0.###}", decimal.Parse(parts[4]));
        }

        void BindBatchList() {
            var dt = PKDatabaseHelper.GetCompletedBatchesForPacking();
            pnlBatchEmpty.Visible = dt.Rows.Count == 0;
            pnlBatchTable.Visible = dt.Rows.Count > 0;
            rptBatches.DataSource = dt; rptBatches.DataBind();
        }

        void BindLog() {
            var dt = PKDatabaseHelper.GetPrimaryPackingToday();
            pnlLogEmpty.Visible = dt.Rows.Count == 0;
            pnlLogTable.Visible = dt.Rows.Count > 0;
            rptLog.DataSource = dt; rptLog.DataBind();
        }

        protected void btnPack_Click(object s, EventArgs e) {
            string batchVal = ddlBatch.SelectedValue;
            if (batchVal == "0") { ShowAlert("Please select a batch.", false); return; }
            decimal qty;
            if (!decimal.TryParse(txtQtyPack.Value, out qty) || qty <= 0) { ShowAlert("Enter valid qty.", false); return; }
            string[] parts = batchVal.Split('|');
            int execId = int.Parse(parts[0]), productId = int.Parse(parts[1]);
            int orderId = int.Parse(parts[2]), batchNo = int.Parse(parts[3]);
            decimal pending = decimal.Parse(parts[4]);
            if (qty > pending) { ShowAlert("Qty exceeds pending (" + pending.ToString("0.###") + ").", false); return; }
            try {
                int fgId = PKDatabaseHelper.AddFGStock(productId, qty, execId, orderId, batchNo, UserID);
                int pmId; decimal pmQty;
                if (int.TryParse(ddlPM.SelectedValue, out pmId) && pmId > 0 &&
                    decimal.TryParse(txtPMQty.Value, out pmQty) && pmQty > 0)
                    PKDatabaseHelper.RecordPMConsumption(fgId, productId, pmId, pmQty, UserID);
                txtQtyPack.Value = ""; txtPMQty.Value = "";
                ShowAlert("Packing recorded — " + qty.ToString("0.###") + " units added to FG stock.", true);
                BindBatchDropdown(); BindBatchList(); BindLog();
            } catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        void ShowAlert(string m, bool ok) { lblAlert.Text = m; pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger"); pnlAlert.Visible = true; }
    }
}
