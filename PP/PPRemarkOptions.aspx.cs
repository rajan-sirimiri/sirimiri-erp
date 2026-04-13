using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPRemarkOptions : Page
    {
        protected Label lblNavUser, lblAlert, lblCount, lblFormTitle;
        protected Panel pnlAlert, pnlEmpty, pnlTable;
        protected TextBox txtRemarkText, txtSortOrder;
        protected DropDownList ddlLine;
        protected Button btnSave, btnCancel;
        protected Repeater rptRemarks, rptLinePills;
        protected LinkButton lnkAll;
        protected HiddenField hfEditId;

        private int FilterLineId
        {
            get { return ViewState["FilterLineId"] != null ? (int)ViewState["FilterLineId"] : 0; }
            set { ViewState["FilterLineId"] = value; }
        }

        // Cache line codes for display
        private DataTable _lines;
        private DataTable Lines
        {
            get
            {
                if (_lines == null) _lines = PPDatabaseHelper.GetActiveProductionLines();
                return _lines;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }

            string role = Session["PP_Role"]?.ToString() ?? "";
            if (role != "Super" && role != "Admin")
            { Response.Redirect("PPHome.aspx"); return; }

            lblNavUser.Text = Session["PP_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                LoadLineDropdown();
                BindLinePills();
                BindRemarks();
            }
        }

        void LoadLineDropdown()
        {
            ddlLine.Items.Clear();
            foreach (DataRow r in Lines.Rows)
                ddlLine.Items.Add(new ListItem(r["LineName"].ToString(), r["LineID"].ToString()));
        }

        void BindLinePills()
        {
            rptLinePills.DataSource = Lines;
            rptLinePills.DataBind();
        }

        void BindRemarks()
        {
            DataTable dt = PPDatabaseHelper.GetAllRemarkOptions();

            // Apply filter
            if (FilterLineId > 0)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = "LineID = " + FilterLineId;
                dt = dv.ToTable();
            }

            int count = dt.Rows.Count;
            lblCount.Text = count.ToString();
            pnlEmpty.Visible = count == 0;
            pnlTable.Visible = count > 0;
            if (count > 0)
            {
                rptRemarks.DataSource = dt;
                rptRemarks.DataBind();
            }

            // Update filter pill active state
            if (lnkAll != null)
                lnkAll.CssClass = FilterLineId == 0 ? "filter-pill active" : "filter-pill";
        }

        protected void lnkFilter_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn == null) return;
            int lineId;
            int.TryParse(btn.CommandArgument, out lineId);
            FilterLineId = lineId;
            BindRemarks();
            BindLinePills();
        }

        // Called from repeater pills to set active CSS
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            // Set active class on correct pill after databind
            foreach (RepeaterItem item in rptLinePills.Items)
            {
                var lnk = item.FindControl(item.Controls[0] is LinkButton ? "" : "") as LinkButton;
                // Find the LinkButton in each item
                foreach (Control c in item.Controls)
                {
                    if (c is LinkButton)
                    {
                        var lb = (LinkButton)c;
                        int lid;
                        int.TryParse(lb.CommandArgument, out lid);
                        lb.CssClass = lid == FilterLineId ? "filter-pill active" : "filter-pill";
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string text = txtRemarkText.Text.Trim();
            if (string.IsNullOrEmpty(text))
            { ShowAlert("Remark text is required.", false); return; }

            int lineId = Convert.ToInt32(ddlLine.SelectedValue);
            int sortOrder;
            if (!int.TryParse(txtSortOrder.Text.Trim(), out sortOrder)) sortOrder = 1;

            int editId = Convert.ToInt32(hfEditId.Value);

            try
            {
                if (editId > 0)
                {
                    PPDatabaseHelper.UpdateRemarkOption(editId, lineId, text, sortOrder);
                    ShowAlert("Remark option updated.", true);
                    ResetForm();
                }
                else
                {
                    PPDatabaseHelper.AddRemarkOption(lineId, text, sortOrder);
                    ShowAlert("Remark option added: " + text, true);
                    txtRemarkText.Text = "";
                    int so;
                    int.TryParse(txtSortOrder.Text, out so);
                    txtSortOrder.Text = (so + 1).ToString();
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }

            BindRemarks();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
            BindRemarks();
        }

        protected void rptRemarks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int optionId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "ToggleOption")
            {
                PPDatabaseHelper.ToggleRemarkOption(optionId);
                ShowAlert("Status updated.", true);
                BindRemarks();
            }
            else if (e.CommandName == "EditOption")
            {
                DataRow row = PPDatabaseHelper.GetRemarkOptionById(optionId);
                if (row != null)
                {
                    hfEditId.Value = optionId.ToString();
                    txtRemarkText.Text = row["OptionText"].ToString();
                    txtSortOrder.Text = row["SortOrder"].ToString();
                    if (row["LineID"] != DBNull.Value)
                    {
                        string lineVal = row["LineID"].ToString();
                        if (ddlLine.Items.FindByValue(lineVal) != null)
                            ddlLine.SelectedValue = lineVal;
                    }
                    lblFormTitle.Text = "Edit Remark Option";
                    btnSave.Text = "Update";
                    btnCancel.Visible = true;
                }
                BindRemarks();
            }
        }

        void ResetForm()
        {
            hfEditId.Value = "0";
            txtRemarkText.Text = "";
            txtSortOrder.Text = "1";
            if (ddlLine.Items.Count > 0) ddlLine.SelectedIndex = 0;
            lblFormTitle.Text = "Add Remark Option";
            btnSave.Text = "+ Add";
            btnCancel.Visible = false;
        }

        /// <summary>Get LineCode for CSS class from LineID.</summary>
        protected string GetLineCode(object lineIdObj)
        {
            if (lineIdObj == null || lineIdObj == DBNull.Value) return "";
            int lineId = Convert.ToInt32(lineIdObj);
            foreach (DataRow r in Lines.Rows)
                if (Convert.ToInt32(r["LineID"]) == lineId)
                    return r["LineCode"].ToString();
            return "";
        }

        void ShowAlert(string msg, bool success)
        {
            string bg = success ? "#d1f5e0" : "#fdf3f2";
            string color = success ? "#155724" : "#842029";
            string border = success ? "#a3d9b1" : "#f5c2c7";
            string icon = success ? "&#10003;" : "&#9888;";
            lblAlert.Text = string.Format(
                "<div style='background:{0};color:{1};border:1px solid {2};padding:12px 18px;border-radius:8px;font-size:13px;'>" +
                "<strong>{3}</strong> {4}</div>", bg, color, border, icon, msg);
            pnlAlert.Visible = true;
        }
    }
}
