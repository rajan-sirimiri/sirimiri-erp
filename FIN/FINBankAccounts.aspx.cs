using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    /// <summary>
    /// Bank Accounts master CRUD + XLSX layout editor.
    /// Finance role required. Links to Zoho Books Chart of Accounts so postings
    /// can use the correct ledger as Dr/Cr.
    /// </summary>
    public partial class FINBankAccounts : System.Web.UI.Page
    {
        // Nav + alerts
        protected Label lblNavUser, lblFormTitle, lblAlert, lblCount;
        protected HtmlGenericControl alertBox;
        protected Panel pnlAlert, pnlEmpty, pnlLayout;

        // Form state
        protected HiddenField hfBankID;
        protected TextBox txtCode, txtName, txtAcctNo, txtBranch;
        protected DropDownList ddlZohoAccount;

        // Layout fields
        protected TextBox txtHeaderRow, txtFirstData, txtDateCol, txtDateFormat;
        protected TextBox txtDescCol, txtRefCol;
        protected RadioButton rbModeTwoCol, rbModeFlag, rbModeSigned;
        protected TextBox txtDebitCol, txtCreditCol, txtAmountCol, txtFlagCol, txtBalanceCol;

        // Buttons
        protected Button btnSave, btnClear, btnToggleActive, btnSaveLayout;

        // List
        protected Repeater rptBanks;

        private string UserRole  => Session["FIN_Role"]?.ToString() ?? "";
        private bool   IsFinance => FINConsignments.IsFinanceRole(UserRole);
        private int    CurrentUserID => Convert.ToInt32(Session["FIN_UserID"] ?? 0);

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
                LoadZohoAccounts();
                LoadBanks();
                SetFormMode(false);
            }
        }

        // ── Zoho Chart of Accounts dropdown ───────────────────────────

        private void LoadZohoAccounts()
        {
            DataTable dt = FINDatabaseHelper.GetChartOfAccounts(activeOnly: true);
            ddlZohoAccount.Items.Clear();
            ddlZohoAccount.Items.Add(new ListItem("-- Select Zoho account --", ""));
            foreach (DataRow r in dt.Rows)
            {
                string zid   = r["ZohoAccountID"].ToString();
                string zname = r["AccountName"].ToString();
                string atype = r["AccountType"] == DBNull.Value ? "" : r["AccountType"].ToString();
                // Only surface bank / cash / credit-card type accounts (accounts suitable as a bank posting target)
                string lowerType = atype.ToLowerInvariant();
                if (!(lowerType.Contains("bank") || lowerType.Contains("cash") || lowerType.Contains("credit")))
                    continue;
                ddlZohoAccount.Items.Add(new ListItem(zname + "  (" + atype + ")", zid + "|" + zname));
            }
        }

        // ── List ──────────────────────────────────────────────────────

        private void LoadBanks()
        {
            DataTable dt = FINDatabaseHelper.GetAllBankAccounts();
            if (dt.Rows.Count > 0)
            {
                rptBanks.DataSource = dt;
                rptBanks.DataBind();
                ((Panel)FindControl("pnlEmpty")).Visible = false;
                lblCount.Text = dt.Rows.Count + " bank" + (dt.Rows.Count == 1 ? "" : "s");
            }
            else
            {
                rptBanks.DataSource = null;
                rptBanks.DataBind();
                ((Panel)FindControl("pnlEmpty")).Visible = true;
                lblCount.Text = "0 banks";
            }
        }

        /// <summary>Used by the repeater to show whether a bank has a saved XLSX layout.</summary>
        protected string RenderLayoutBadge(object bankIdObj)
        {
            if (bankIdObj == null || bankIdObj == DBNull.Value) return "";
            int bankId = Convert.ToInt32(bankIdObj);
            DataRow layout = FINDatabaseHelper.GetBankLayout(bankId);
            bool configured = layout != null
                && layout["IsConfigured"] != DBNull.Value
                && Convert.ToInt32(layout["IsConfigured"]) == 1;
            return configured
                ? "<span class='badge-layout yes'>Configured</span>"
                : "<span class='badge-layout no'>Not set</span>";
        }

        // ── Form helpers ──────────────────────────────────────────────

        private void SetFormMode(bool isEdit)
        {
            lblFormTitle.Text = isEdit ? "Edit Bank Account" : "New Bank Account";
            btnToggleActive.Visible = isEdit;
            pnlLayout.Visible = isEdit;
        }

        private void ShowAlert(string message, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = message;
            alertBox.Attributes["class"] = "alert " + (success ? "alert-success" : "alert-danger");
        }

        private void ClearForm()
        {
            hfBankID.Value = "0";
            txtCode.Text = "";
            txtName.Text = "";
            txtAcctNo.Text = "";
            txtBranch.Text = "";
            ddlZohoAccount.SelectedValue = "";
            ClearLayoutForm();
            SetFormMode(false);
        }

        private void ClearLayoutForm()
        {
            txtHeaderRow.Text = "1";
            txtFirstData.Text = "2";
            txtDateCol.Text = "";
            txtDateFormat.Text = "dd/MM/yyyy";
            txtDescCol.Text = "";
            txtRefCol.Text = "";
            rbModeTwoCol.Checked = true;
            rbModeFlag.Checked = false;
            rbModeSigned.Checked = false;
            txtDebitCol.Text = "";
            txtCreditCol.Text = "";
            txtAmountCol.Text = "";
            txtFlagCol.Text = "";
            txtBalanceCol.Text = "";
        }

        private void LoadLayoutForm(int bankId)
        {
            DataRow row = FINDatabaseHelper.GetBankLayout(bankId);
            if (row == null)
            {
                ClearLayoutForm();
                return;
            }
            txtHeaderRow.Text = row["HeaderRow"] == DBNull.Value ? "1" : row["HeaderRow"].ToString();
            txtFirstData.Text = row["FirstDataRow"] == DBNull.Value ? "2" : row["FirstDataRow"].ToString();
            txtDateCol.Text   = row["DateCol"]  == DBNull.Value ? "" : row["DateCol"].ToString();
            txtDateFormat.Text= row["DateFormat"] == DBNull.Value ? "dd/MM/yyyy" : row["DateFormat"].ToString();
            txtDescCol.Text   = row["DescCol"]  == DBNull.Value ? "" : row["DescCol"].ToString();
            txtRefCol.Text    = row["RefCol"]   == DBNull.Value ? "" : row["RefCol"].ToString();

            string mode = row["AmountMode"] == DBNull.Value ? "TWO_COL" : row["AmountMode"].ToString();
            rbModeTwoCol.Checked = (mode == "TWO_COL");
            rbModeFlag.Checked   = (mode == "FLAG");
            rbModeSigned.Checked = (mode == "SIGNED");

            txtDebitCol.Text   = row["DebitCol"]   == DBNull.Value ? "" : row["DebitCol"].ToString();
            txtCreditCol.Text  = row["CreditCol"]  == DBNull.Value ? "" : row["CreditCol"].ToString();
            txtAmountCol.Text  = row["AmountCol"]  == DBNull.Value ? "" : row["AmountCol"].ToString();
            txtFlagCol.Text    = row["FlagCol"]    == DBNull.Value ? "" : row["FlagCol"].ToString();
            txtBalanceCol.Text = row["BalanceCol"] == DBNull.Value ? "" : row["BalanceCol"].ToString();
        }

        // ── Save / clear / toggle ─────────────────────────────────────

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ShowAlert("Bank Name is required.", false);
                return;
            }

            // Decompose the composite Zoho selection (stored as "id|name" in the dropdown)
            string zohoId = "", zohoName = "";
            string sel = ddlZohoAccount.SelectedValue ?? "";
            if (!string.IsNullOrEmpty(sel))
            {
                int pipe = sel.IndexOf('|');
                if (pipe > 0)
                {
                    zohoId   = sel.Substring(0, pipe);
                    zohoName = sel.Substring(pipe + 1);
                }
            }

            int bankId = Convert.ToInt32(hfBankID.Value);

            try
            {
                if (bankId == 0)
                {
                    FINDatabaseHelper.AddBankAccount(name, txtAcctNo.Text.Trim(), txtBranch.Text.Trim(),
                                                     zohoId, zohoName, CurrentUserID);
                    ShowAlert("Bank '" + name + "' added. Now configure the XLSX column layout below.", true);
                }
                else
                {
                    FINDatabaseHelper.UpdateBankAccount(bankId, name, txtAcctNo.Text.Trim(), txtBranch.Text.Trim(),
                                                        zohoId, zohoName);
                    ShowAlert("Bank '" + name + "' updated.", true);
                }
                // Reload list. If edit-mode, keep the form populated so user can continue with layout.
                LoadBanks();
                if (bankId == 0)
                {
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlAlert.Visible = false;
        }

        protected void btnToggleActive_Click(object sender, EventArgs e)
        {
            int bankId = Convert.ToInt32(hfBankID.Value);
            if (bankId == 0) return;

            DataRow dr = FINDatabaseHelper.GetBankAccountById(bankId);
            if (dr == null) return;

            bool currentlyActive = Convert.ToBoolean(dr["IsActive"]);
            FINDatabaseHelper.ToggleBankAccountActive(bankId, !currentlyActive);
            ShowAlert("Bank " + (currentlyActive ? "deactivated" : "activated") + ".", true);
            ClearForm();
            LoadBanks();
        }

        protected void btnSaveLayout_Click(object sender, EventArgs e)
        {
            int bankId = Convert.ToInt32(hfBankID.Value);
            if (bankId == 0)
            {
                ShowAlert("Save the bank first, then configure the layout.", false);
                return;
            }

            int headerRow, firstData;
            if (!int.TryParse(txtHeaderRow.Text.Trim(), out headerRow) || headerRow < 1) headerRow = 1;
            if (!int.TryParse(txtFirstData.Text.Trim(), out firstData)  || firstData  < 1) firstData  = headerRow + 1;

            string mode = rbModeFlag.Checked ? "FLAG"
                        : rbModeSigned.Checked ? "SIGNED"
                        : "TWO_COL";

            // Basic validation per mode
            if (mode == "TWO_COL" && (string.IsNullOrEmpty(txtDebitCol.Text) || string.IsNullOrEmpty(txtCreditCol.Text)))
            {
                ShowAlert("TWO_COL mode requires both Debit and Credit column letters.", false);
                return;
            }
            if (mode == "FLAG" && (string.IsNullOrEmpty(txtAmountCol.Text) || string.IsNullOrEmpty(txtFlagCol.Text)))
            {
                ShowAlert("FLAG mode requires Amount and Flag column letters.", false);
                return;
            }
            if (mode == "SIGNED" && string.IsNullOrEmpty(txtAmountCol.Text))
            {
                ShowAlert("SIGNED mode requires the Amount column letter.", false);
                return;
            }
            if (string.IsNullOrEmpty(txtDateCol.Text))
            {
                ShowAlert("Date column is required.", false);
                return;
            }

            try
            {
                FINDatabaseHelper.SaveBankLayout(bankId, headerRow, firstData,
                    txtDateCol.Text, txtDescCol.Text, txtRefCol.Text,
                    mode,
                    txtDebitCol.Text, txtCreditCol.Text,
                    txtAmountCol.Text, txtFlagCol.Text,
                    txtBalanceCol.Text,
                    string.IsNullOrEmpty(txtDateFormat.Text) ? "dd/MM/yyyy" : txtDateFormat.Text.Trim());
                ShowAlert("XLSX layout saved. You can now upload statements for this bank.", true);
                LoadBanks();
            }
            catch (Exception ex)
            {
                ShowAlert("Error saving layout: " + ex.Message, false);
            }
        }

        // ── Edit from list ────────────────────────────────────────────

        protected void rptBanks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int bankId = Convert.ToInt32(e.CommandArgument);
                DataRow dr = FINDatabaseHelper.GetBankAccountById(bankId);
                if (dr == null) return;

                hfBankID.Value  = bankId.ToString();
                txtCode.Text    = dr["BankCode"].ToString();
                txtName.Text    = dr["BankName"].ToString();
                txtAcctNo.Text  = dr["AccountNumber"] == DBNull.Value ? "" : dr["AccountNumber"].ToString();
                txtBranch.Text  = dr["Branch"]        == DBNull.Value ? "" : dr["Branch"].ToString();

                string zid   = dr["ZohoAccountID"]   == DBNull.Value ? "" : dr["ZohoAccountID"].ToString();
                string zname = dr["ZohoAccountName"] == DBNull.Value ? "" : dr["ZohoAccountName"].ToString();
                string want  = string.IsNullOrEmpty(zid) ? "" : zid + "|" + zname;
                ListItem sel = ddlZohoAccount.Items.FindByValue(want);
                if (sel != null) ddlZohoAccount.SelectedValue = want;
                else ddlZohoAccount.SelectedValue = "";

                bool isActive = Convert.ToBoolean(dr["IsActive"]);
                btnToggleActive.Text = isActive ? "Deactivate" : "Activate";

                LoadLayoutForm(bankId);
                SetFormMode(true);
                pnlAlert.Visible = false;
            }
        }
    }
}
