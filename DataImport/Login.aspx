<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="DataImport.Login" ResponseEncoding="UTF-8" ContentType="text/html; charset=utf-8" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>Sirimiri &mdash; Data Import Login</title>
    <link rel="preconnect" href="https://fonts.googleapis.com"/>
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <style>
        :root { --accent:#C0392B; --accent-dark:#a93226; --bg:#f0f0f0;
                --surface:#fff; --border:#e0e0e0; --text:#1a1a1a; --muted:#666; }
        * { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); font-family:'DM Sans',sans-serif;
               min-height:100vh; display:flex; align-items:center; justify-content:center; }
        .page-wrap { width:100%; max-width:520px; padding:16px; }
        .card { background:var(--surface); border-radius:20px;
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
                      background:linear-gradient(90deg,var(--accent-dark),#e63030,var(--accent-dark)); }
        .card-body { padding:32px 40px 40px; }
        h1 { font-family:'Bebas Neue',cursive; font-size:26px; letter-spacing:.08em;
             color:var(--text); margin-bottom:4px; }
        .sub { font-size:13px; color:var(--muted); margin-bottom:24px; }
        .field { margin-bottom:18px; }
        .field label { display:block; font-size:11px; font-weight:700;
                       letter-spacing:.1em; text-transform:uppercase;
                       color:var(--muted); margin-bottom:6px; }
        .field input { width:100%; padding:11px 14px; border:1.5px solid var(--border);
                       border-radius:8px; font-size:14px; font-family:inherit;
                       transition:border-color .2s; }
        .field input:focus { outline:none; border-color:var(--accent); }
        .pwd-wrap { position:relative; }
        .pwd-wrap input { padding-right:44px; }
        .eye-btn { position:absolute; right:12px; top:50%; transform:translateY(-50%);
                   background:none; border:none; cursor:pointer; color:var(--muted);
                   padding:4px; }
        .btn-login { width:100%; padding:13px; background:var(--accent); color:#fff;
                     border:none; border-radius:9px; font-size:14px; font-weight:700;
                     font-family:'Bebas Neue',cursive; letter-spacing:.1em;
                     cursor:pointer; transition:background .2s; margin-top:8px; }
        .btn-login:hover { background:var(--accent-dark); }
        .error-box { background:#fdf0f0; border:1.5px solid var(--accent);
                     border-radius:8px; padding:10px 14px; margin-bottom:16px;
                     font-size:13px; color:var(--accent); }
        .import-badge { display:inline-block; background:var(--accent);
                        color:#fff; font-size:11px; font-weight:700;
                        padding:3px 12px; border-radius:20px; margin-bottom:16px;
                        letter-spacing:.08em; }
    </style>
</head>
<body>
<form id="form1" runat="server">
<div class="page-wrap">
<div class="card">
    <div class="card-header">
        <img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri Efficient Nutrition"/>
        <div class="company-block">
            <div class="company-name">SIRIMIRI Nutrition Food Products</div>
            <div class="company-sub">Business Intelligence System</div>
        </div>
        <div style="width:80px;"></div>
    </div>
    <div class="accent-bar"></div>
    <div class="card-body">
        <div class="import-badge">⬆#11014; DATA IMPORT PORTAL</div>
        <h1>Sign In</h1>
        <p class="sub">Manager or Admin access required</p>

        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="error-box">
                <asp:Label ID="lblError" runat="server"/>
            </div>
        </asp:Panel>

        <div class="field">
            <label>Username</label>
            <asp:TextBox ID="txtUsername" runat="server" ClientIDMode="Static"
                placeholder="Enter username"/>
        </div>
        <div class="field">
            <label>Password</label>
            <div class="pwd-wrap">
                <asp:TextBox ID="txtPassword" runat="server" ClientIDMode="Static"
                    TextMode="Password" placeholder="Enter password"/>
                <button type="button" class="eye-btn" onclick="togglePwd()">
                    <svg id="eyeIcon" xmlns="http://www.w3.org/2000/svg" width="18" height="18"
                         viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
                         stroke-linecap="round" stroke-linejoin="round">
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                        <circle cx="12" cy="12" r="3"/>
                    </svg>
                </button>
            </div>
        </div>

        <asp:Button ID="btnLogin" runat="server" Text="SIGN IN"
            CssClass="btn-login" OnClick="btnLogin_Click"/>
    </div>
</div>
</div>
</form>
<script>
function togglePwd() {
    var f = document.getElementById('txtPassword');
    f.type = f.type === 'password' ? 'text' : 'password';
}
</script>
</body>
</html>
