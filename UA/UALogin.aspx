<%@ Page Language="C#" AutoEventWireup="true" Inherits="UAApp.UALogin" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="UTF-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>User Admin — Login</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:ital,wght@0,300;0,400;0,500;0,600;1,300&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--bg:#f5f5f5;--surface:#fff;--border:#e0e0e0;--accent:#cc1e1e;--accent-dark:#a81818;--teal:#1a9e6a;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;}
body{background:var(--bg);font-family:'DM Sans',sans-serif;min-height:100vh;display:flex;align-items:center;justify-content:center;}
.login-card{background:var(--surface);border:1px solid var(--border);border-radius:14px;padding:40px;width:380px;box-shadow:0 8px 32px rgba(0,0,0,.08);}
.login-bar{height:3px;background:linear-gradient(90deg,var(--accent),var(--accent-dark));border-radius:14px 14px 0 0;margin:-40px -40px 24px;width:calc(100% + 80px);}
.login-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.08em;text-align:center;margin-bottom:4px;}
.login-title span{color:var(--accent);}
.login-sub{text-align:center;font-size:12px;color:var(--text-muted);margin-bottom:24px;}
.form-group{margin-bottom:16px;}
.form-group label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);display:block;margin-bottom:4px;}
.form-group input{width:100%;padding:10px 14px;border:1.5px solid var(--border);border-radius:8px;font-size:14px;font-family:inherit;outline:none;}
.form-group input:focus{border-color:var(--accent);}
.btn-login{width:100%;padding:12px;border:none;border-radius:8px;background:linear-gradient(135deg,#1a1a1a,var(--accent));color:#fff;font-size:14px;font-weight:700;cursor:pointer;font-family:inherit;letter-spacing:.04em;}
.btn-login:hover{background:linear-gradient(135deg,#000,var(--accent-dark));}
.alert{padding:10px 14px;border-radius:8px;font-size:12px;margin-bottom:16px;background:#fdf3f2;color:#c0392b;border:1px solid #f5c6cb;}
</style>
</head>
<body>
<form id="form1" runat="server">
<div class="login-card">
    <div class="login-bar"></div>
    <div class="login-title">User <span>Administration</span></div>
    <div class="login-sub">Admin access only</div>
    <asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert"><asp:Label ID="lblAlert" runat="server"/></asp:Panel>
    <div class="form-group"><label>Username</label><asp:TextBox ID="txtUsername" runat="server" placeholder="Enter username"/></div>
    <div class="form-group"><label>Password</label><asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Enter password"/></div>
    <asp:Button ID="btnLogin" runat="server" Text="Sign In" CssClass="btn-login" OnClick="btnLogin_Click"/>
</div>
</form>
</body>
</html>
