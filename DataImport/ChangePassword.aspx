<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="DataImport.ChangePassword" ResponseEncoding="UTF-8" ContentType="text/html; charset=utf-8" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>Sirimiri &mdash; Change Password</title>
    <link rel="preconnect" href="https://fonts.googleapis.com"/>
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <style>
        :root { --accent:#C0392B; --bg:#f0f0f0; --surface:#fff;
                --border:#e0e0e0; --text:#1a1a1a; --muted:#666; }
        * { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); font-family:'DM Sans',sans-serif;
               min-height:100vh; display:flex; align-items:center; justify-content:center; }
        .page-wrap { width:100%; max-width:520px; padding:16px; }
        .card { background:var(--surface); border-radius:16px;
                box-shadow:0 8px 40px rgba(0,0,0,.13); overflow:hidden; }
        .card-header { background:#fff; padding:20px 32px 0;
                       display:flex; align-items:center; justify-content:space-between; }
        .card-header img { height:80px; object-fit:contain;
                           filter:drop-shadow(0 2px 8px rgba(204,30,30,.20)); }
        .company-block { text-align:center; flex:1; }
        .company-name { font-family:'Bebas Neue',cursive; font-size:22px;
                        letter-spacing:.12em; color:var(--text); text-transform:uppercase; line-height:1.25; }
        .company-sub  { font-family:'Bebas Neue',cursive; font-size:14px;
                        letter-spacing:.12em; color:var(--muted); text-transform:uppercase; }
        .accent-bar { width:100%; height:4px;
                      background:linear-gradient(90deg,#a93226,#e63030,#a93226); }
        .card-body { padding:28px 40px 36px; }
        h1 { font-family:'Bebas Neue',cursive; font-size:24px; letter-spacing:.08em;
             color:var(--text); margin-bottom:4px; }
        .sub { font-size:13px; color:var(--muted); margin-bottom:20px; }
        .field { margin-bottom:16px; }
        .field label { display:block; font-size:11px; font-weight:700;
                       letter-spacing:.1em; text-transform:uppercase;
                       color:var(--muted); margin-bottom:6px; }
        .field input { width:100%; padding:11px 14px; border:1.5px solid var(--border);
                       border-radius:8px; font-size:14px; font-family:inherit; outline:none; }
        .field input:focus { border-color:var(--accent); }
        .strength-bar { height:4px; border-radius:2px; margin-top:6px;
                        background:var(--border); overflow:hidden; }
        .strength-fill { height:100%; width:0; transition:width .3s,background .3s; border-radius:2px; }
        .strength-label { font-size:11px; color:var(--muted); margin-top:4px; }
        .btn-change { width:100%; padding:13px; background:var(--accent); color:#fff;
                      border:none; border-radius:9px; font-size:14px; font-weight:700;
                      font-family:inherit; cursor:pointer; letter-spacing:.08em;
                      text-transform:uppercase; margin-top:8px; }
        .btn-change:hover { background:#a93226; }
        .error-box { background:#fdf0f0; border:1.5px solid var(--accent);
                     border-radius:8px; padding:10px 14px; margin-bottom:14px;
                     font-size:13px; color:var(--accent); }
    </style>
</head>
<body>
<form id="form1" runat="server">
<div class="page-wrap">
<div class="card">
    <div class="card-header">
        <img src="../StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri"
             onerror="this.src='Sirimiri_Logo-16_9-72ppi-01.png'"/>
        <div class="company-block">
            <div class="company-name">SIRIMIRI Nutrition Food Products</div>
            <div class="company-sub">Business Intelligence System</div>
        </div>
        <div style="width:80px;"></div>
    </div>
    <div class="accent-bar"></div>
    <div class="card-body">
        <h1>Set New Password</h1>
        <p class="sub">Please set a new password to continue.</p>

        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="error-box"><asp:Label ID="lblError" runat="server"/></div>
        </asp:Panel>

        <div class="field">
            <label>Current Password</label>
            <asp:TextBox ID="txtCurrent" runat="server" TextMode="Password" placeholder="Current password"/>
        </div>
        <div class="field">
            <label>New Password</label>
            <asp:TextBox ID="txtNew" runat="server" TextMode="Password"
                ClientIDMode="Static" placeholder="Min 8 chars, 1 uppercase, 1 number"
                onkeyup="checkStrength(this.value)"/>
            <div class="strength-bar"><div id="strengthFill" class="strength-fill"></div></div>
            <div id="strengthLabel" class="strength-label"></div>
        </div>
        <div class="field">
            <label>Confirm New Password</label>
            <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" placeholder="Repeat new password"/>
        </div>

        <asp:Button ID="btnChange" runat="server" Text="SET NEW PASSWORD"
            CssClass="btn-change" OnClick="btnChange_Click"/>
    </div>
</div>
</div>
</form>
<script>
function checkStrength(v) {
    var s = 0;
    if (v.length >= 8) s++;
    if (/[A-Z]/.test(v)) s++;
    if (/[0-9]/.test(v)) s++;
    if (/[^A-Za-z0-9]/.test(v)) s++;
    var fill = document.getElementById('strengthFill');
    var lbl  = document.getElementById('strengthLabel');
    var labels = ['','Weak','Fair','Good','Strong'];
    var colors = ['','#e74c3c','#e67e22','#27ae60','#27ae60'];
    fill.style.width = (s * 25) + '%';
    fill.style.background = colors[s] || '#ccc';
    lbl.textContent = labels[s] || '';
}
</script>
</body>
</html>
