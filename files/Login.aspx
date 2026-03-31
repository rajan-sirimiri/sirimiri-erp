<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="StockApp.Login" %>
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8"/>
<link rel="preconnect" href="https://fonts.googleapis.com"/>
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Login — Sirimiri Stock App</title>
<style>
:root { --accent:#C0392B; --surface:#fff; --bg:#f0f0f0; --border:#e0e0e0; --text:#1a1a1a; --muted:#666; }
* { box-sizing:border-box; margin:0; padding:0; }
body { background:var(--bg); font-family:'DM Sans',sans-serif; min-height:100vh;
       display:flex; align-items:center; justify-content:center; }
.page-wrap { width:100%; max-width:540px; padding:16px; }
.card { background:var(--surface); border-radius:16px;
        box-shadow:0 8px 40px rgba(0,0,0,.13); overflow:hidden; width:100%; }

/* Logo header — white bg, matches StockEntry */
.card-header {
    background:#fff;
    padding:20px 32px 0;
    display:flex;
    align-items:center;
    justify-content:space-between;
    border-bottom:none;
}
.card-header img {
    height:80px;
    width:auto;
    object-fit:contain;
    filter:drop-shadow(0 2px 8px rgba(204,30,30,0.20));
}
.company-block { text-align:center; flex:1; }
.company-name {
    font-family:'Bebas Neue',cursive;
    font-size:22px;
    letter-spacing:.12em;
    color:var(--text);
    text-transform:uppercase;
    line-height:1.25;
}
.company-sub {
    font-family:'Bebas Neue',cursive;
    font-size:14px;
    letter-spacing:.12em;
    color:var(--muted);
    text-transform:uppercase;
}
/* Red accent bar under header */
.accent-bar { width:100%; height:4px;
    background:linear-gradient(90deg,#a93226,#e63030,#a93226); }

.card-body { padding:32px 40px 40px; }
h1 { font-family:'Bebas Neue',cursive; font-size:26px; letter-spacing:.08em;
     color:var(--text); margin-bottom:4px; }
.sub { font-size:13px; color:var(--muted); margin-bottom:24px; }
.field { margin-bottom:18px; }
.field label { display:block; font-size:12px; font-weight:600;
               text-transform:uppercase; letter-spacing:.05em;
               color:var(--muted); margin-bottom:6px; }
.field input {
    width:100%; padding:11px 14px; border:1.5px solid var(--border);
    border-radius:8px; font-size:14px; color:var(--text);
    transition:border .2s; outline:none; background:var(--surface);
}
.field input:focus { border-color:var(--accent); }
.field.pwd-wrap { position:relative; }
.field.pwd-wrap input { padding-right:44px; }
.toggle-pwd {
    position:absolute; right:12px; top:50%; transform:translateY(-50%);
    background:none; border:none; cursor:pointer; color:var(--muted);
    padding:4px; line-height:1;
}
.btn-login {
    width:100%; padding:12px; background:var(--accent); color:#fff;
    border:none; border-radius:8px; font-size:14px; font-weight:700;
    letter-spacing:.04em; cursor:pointer; transition:opacity .2s;
    text-transform:uppercase; margin-top:8px;
}
.btn-login:hover { opacity:.88; }
.error-msg {
    background:#fdecea; border:1px solid #f5c6c2; border-radius:8px;
    padding:10px 14px; font-size:13px; color:#c0392b; margin-bottom:18px;
    display:none;
}
.error-msg.show { display:block; }
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
    <h1>Sign In</h1>
    <p class="sub">Enter your credentials to continue</p>

    <asp:Panel ID="pnlError" runat="server" CssClass="error-msg show" Visible="false">
        <asp:Label ID="lblError" runat="server" />
    </asp:Panel>

    <div class="field">
        <label for="txtUsername">Username</label>
        <asp:TextBox ID="txtUsername" runat="server" CssClass="" placeholder="Enter username" />
    </div>
    <div class="field pwd-wrap">
        <label for="txtPassword">Password</label>
        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"
            placeholder="Enter password" ClientIDMode="Static" />
        <button type="button" class="toggle-pwd" onclick="togglePwd()" tabindex="-1">
            <svg id="eyeIcon" xmlns="http://www.w3.org/2000/svg" width="18" height="18"
                viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
                stroke-linecap="round" stroke-linejoin="round">
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                <circle cx="12" cy="12" r="3"/>
            </svg>
        </button>
    </div>

    <asp:Button ID="btnLogin" runat="server" Text="Sign In"
        CssClass="btn-login" OnClick="btnLogin_Click" />
</div>
<script>
function togglePwd() {
    var inp = document.getElementById('txtPassword');
    inp.type = inp.type === 'password' ? 'text' : 'password';
}
</script>
    </div>
</div>
</div>
</form>
</body>
</html>
