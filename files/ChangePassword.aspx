<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="StockApp.ChangePassword" %>
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8"/>
<link rel="preconnect" href="https://fonts.googleapis.com"/>
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Change Password — Sirimiri Stock App</title>
<style>
:root { --accent:#C0392B; --surface:#fff; --bg:#f0f0f0; --border:#e0e0e0; --text:#1a1a1a; --muted:#666; }
* { box-sizing:border-box; margin:0; padding:0; }
body { background:var(--bg); font-family:'DM Sans',sans-serif; min-height:100vh;
       display:flex; align-items:center; justify-content:center; }
.page-wrap { width:100%; max-width:540px; padding:16px; }
.card { background:var(--surface); border-radius:16px;
        box-shadow:0 8px 40px rgba(0,0,0,.13); overflow:hidden; width:100%; }
.card-header { background:#fff; padding:20px 32px 0;
               display:flex; align-items:center; justify-content:space-between; }
.card-header img { height:80px; width:auto; object-fit:contain;
                   filter:drop-shadow(0 2px 8px rgba(204,30,30,0.20)); }
.company-block { text-align:center; flex:1; }
.company-name { font-family:'Bebas Neue',cursive; font-size:22px; letter-spacing:.12em;
                color:var(--text); text-transform:uppercase; line-height:1.25; }
.company-sub  { font-family:'Bebas Neue',cursive; font-size:14px; letter-spacing:.12em;
                color:var(--muted); text-transform:uppercase; }
.accent-bar { width:100%; height:4px;
              background:linear-gradient(90deg,#a93226,#e63030,#a93226); }
.card-body { padding:28px 40px 36px; }
h1 { font-family:'Bebas Neue',cursive; font-size:24px; letter-spacing:.08em;
     color:var(--text); margin-bottom:4px; }
.sub { font-size:13px; color:var(--muted); margin-bottom:14px; }
.notice { background:#fff8e1; border:1px solid #ffe082; border-radius:8px;
          padding:10px 14px; font-size:13px; color:#7a5c00; margin-bottom:20px; }
.field { margin-bottom:16px; }
.field label { display:block; font-size:12px; font-weight:600; text-transform:uppercase;
               letter-spacing:.05em; color:var(--muted); margin-bottom:6px; }
.field input { width:100%; padding:11px 14px; border:1.5px solid var(--border);
               border-radius:8px; font-size:14px; color:var(--text); outline:none; }
.field input:focus { border-color:var(--accent); }
.strength { height:4px; border-radius:2px; margin-top:6px; background:#e0e0e0; }
.strength-bar { height:100%; border-radius:2px; width:0; transition:width .3s, background .3s; }
.strength-label { font-size:11px; color:var(--muted); margin-top:4px; }
.btn { width:100%; padding:12px; background:var(--accent); color:#fff; border:none;
       border-radius:8px; font-size:14px; font-weight:700; cursor:pointer;
       text-transform:uppercase; letter-spacing:.04em; margin-top:8px; }
.btn:hover { opacity:.88; }
.error-msg { background:#fdecea; border:1px solid #f5c6c2; border-radius:8px;
             padding:10px 14px; font-size:13px; color:#c0392b; margin-bottom:16px; }
.rules { font-size:12px; color:var(--muted); margin-top:6px; line-height:1.8; }
.rules span { display:block; }
</style>
</head>
<body>
<form id="form1" runat="server">
<div class="page-wrap">
<div class="card">
    <div class="card-header">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri Efficient Nutrition"/>
        <div class="company-block">
            <div class="company-name">SIRIMIRI Nutrition Food Products</div>
            <div class="company-sub">Business Intelligence System</div>
        </div>
        <div style="width:80px;"></div>
    </div>
    <div class="accent-bar"></div>
    <div class="card-body">
    <h1>Set New Password</h1>
    <p class="sub">Welcome! Please set a new password to continue.</p>
    <div class="notice">&#9888; You must change your temporary password before proceeding.</div>

    <asp:Panel ID="pnlError" runat="server" CssClass="error-msg" Visible="false">
        <asp:Label ID="lblError" runat="server" />
    </asp:Panel>

    <div class="field">
        <label>Current Password</label>
        <asp:TextBox ID="txtCurrent" runat="server" TextMode="Password" placeholder="Enter current password" />
    </div>
    <div class="field">
        <label>New Password</label>
        <asp:TextBox ID="txtNew" runat="server" TextMode="Password"
            placeholder="Min 8 chars, 1 upper, 1 number" onkeyup="checkStrength(this.value)" />
        <div class="strength"><div class="strength-bar" id="strengthBar"></div></div>
        <div class="strength-label" id="strengthLabel"></div>
        <div class="rules">
            <span>&#8226; At least 8 characters</span>
            <span>&#8226; At least one uppercase letter</span>
            <span>&#8226; At least one number</span>
        </div>
    </div>
    <div class="field">
        <label>Confirm New Password</label>
        <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" placeholder="Re-enter new password" />
    </div>

    <asp:Button ID="btnChange" runat="server" Text="Change Password"
        CssClass="btn" OnClick="btnChange_Click" />
</div>
<script>
function checkStrength(v) {
    var bar = document.getElementById('strengthBar');
    var lbl = document.getElementById('strengthLabel');
    var score = 0;
    if (v.length >= 8)             score++;
    if (/[A-Z]/.test(v))          score++;
    if (/[0-9]/.test(v))          score++;
    if (/[^A-Za-z0-9]/.test(v))   score++;
    var colors = ['#e74c3c','#e67e22','#f1c40f','#2ecc71'];
    var labels = ['Weak','Fair','Good','Strong'];
    bar.style.width   = (score * 25) + '%';
    bar.style.background = colors[score-1] || '#e0e0e0';
    lbl.textContent   = score > 0 ? labels[score-1] : '';
}
</script>
    </div>
</div>
</div>
</form>
</body>
</html>
