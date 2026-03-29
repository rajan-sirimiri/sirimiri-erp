<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKLogin" %>
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8"/>
<link rel="preconnect" href="https://fonts.googleapis.com"/>
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Login &mdash; Packing &amp; Shipments</title>
<style>
:root { --accent:#e67e22; --surface:#fff; --bg:#f5f5f5; --border:#e0e0e0; --text:#1a1a1a; --muted:#666; --radius:12px; }
*, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
body { background:var(--bg); font-family:'DM Sans',sans-serif; min-height:100vh;
       display:flex; align-items:center; justify-content:center; }
body::before {
    content:''; position:fixed; inset:0;
    background-image: linear-gradient(var(--border) 1px,transparent 1px), linear-gradient(90deg,var(--border) 1px,transparent 1px);
    background-size:48px 48px; opacity:.06; pointer-events:none;
}
.page-wrap { width:100%; max-width:440px; padding:16px; position:relative; z-index:1; animation:fadeUp .5s ease both; }
@keyframes fadeUp { from{opacity:0;transform:translateY(24px)} to{opacity:1;transform:translateY(0)} }

.card { background:var(--surface); border-radius:16px; box-shadow:0 8px 40px rgba(0,0,0,.12); overflow:hidden; }

/* Header */
.card-header { background:#fff; padding:24px 32px 16px; display:flex; align-items:center; justify-content:space-between; }
.card-header img { height:72px; width:auto; object-fit:contain; filter:drop-shadow(0 2px 8px rgba(230,126,34,.2)); }
.company-block { text-align:center; flex:1; }
.company-name  { font-family:'Bebas Neue',sans-serif; font-size:18px; letter-spacing:.10em; color:var(--text); line-height:1.2; }
.company-sub   { font-family:'Bebas Neue',sans-serif; font-size:13px; letter-spacing:.10em; color:var(--muted); }
.module-badge  { background:rgba(230,126,34,.08); border:1px solid rgba(230,126,34,.2);
                 border-radius:6px; padding:6px 12px; text-align:center; }
.module-badge-label { font-size:9px; font-weight:700; text-transform:uppercase; letter-spacing:.12em; color:var(--muted); }
.module-badge-name  { font-family:'Bebas Neue',sans-serif; font-size:14px; letter-spacing:.08em; color:var(--accent); }

/* Red bar */
.accent-bar { height:3px; background:linear-gradient(90deg,var(--accent),#f39c12,var(--accent)); }

/* Form area */
.card-body { padding:28px 32px 32px; }
.card-title    { font-family:'Bebas Neue',sans-serif; font-size:26px; letter-spacing:.07em; color:var(--text); margin-bottom:4px; }
.card-subtitle { font-size:13px; color:var(--muted); margin-bottom:24px; }

.field { margin-bottom:16px; }
.field label { display:block; font-size:11px; font-weight:700; text-transform:uppercase; letter-spacing:.07em; color:var(--muted); margin-bottom:5px; }
.field input  { width:100%; padding:10px 14px; border:1.5px solid var(--border); border-radius:8px;
                font-size:14px; font-family:inherit; color:var(--text); background:#fafafa; transition:border-color .2s; }
.field input:focus { outline:none; border-color:var(--accent); background:#fff; box-shadow:0 0 0 3px rgba(230,126,34,.1); }

.btn-login { width:100%; padding:12px; background:var(--accent); color:#fff; border:none;
             border-radius:8px; font-size:14px; font-weight:700; font-family:inherit;
             letter-spacing:.05em; text-transform:uppercase; cursor:pointer;
             margin-top:8px; transition:background .2s; }
.btn-login:hover { background:#cf6d17; }

.error-msg { background:#fef2f2; border:1px solid #fecaca; border-radius:8px;
             padding:10px 14px; color:#b91c1c; font-size:13px; margin-bottom:16px; }
</style>
</head>
<body>
<div class="page-wrap">
  <div class="card">
    <div class="card-header">
      <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri"
           onerror="this.style.display='none'" />
      <div class="company-block">
        <div class="company-name">Sirimiri Nutrition<br/>Food Products</div>
        <div class="company-sub">Enterprise Resource Planning</div>
      </div>
      <div class="module-badge">
        <div class="module-badge-label">Module</div>
        <div class="module-badge-name">Packing &amp;<br/>Shipments</div>
      </div>
    </div>
    <div class="accent-bar"></div>
    <form id="form1" runat="server">
    <div class="card-body">
      <div class="card-title">Sign In</div>
      <div class="card-subtitle">Packing &amp; Shipments Module</div>
      <asp:Panel ID="pnlErr" runat="server" Visible="false">
        <div class="error-msg"><asp:Label ID="lblErr" runat="server" /></div>
      </asp:Panel>
      <div class="field">
        <label>Username</label>
        <asp:TextBox ID="txtUser" runat="server" placeholder="Enter username" />
      </div>
      <div class="field">
        <label>Password</label>
        <asp:TextBox ID="txtPass" runat="server" TextMode="Password" placeholder="Enter password" />
      </div>
      <asp:Button ID="btnLogin" runat="server" Text="Sign In" CssClass="btn-login" OnClick="btnLogin_Click" />
    </div>
    </form>
  </div>
</div>
</body>
</html>
