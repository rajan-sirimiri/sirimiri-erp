<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINLogin" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Finance Login</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@400;600&display=swap" rel="stylesheet"/>
<style>
:root{--accent:#8e44ad;--accent-dark:#6c3483;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:#f0f0f0;display:flex;align-items:center;justify-content:center;min-height:100vh;}
.login-card{background:#fff;border-radius:18px;box-shadow:0 8px 40px rgba(0,0,0,.12);padding:40px 36px;width:360px;text-align:center;}
.login-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.08em;margin-bottom:4px;}
.login-title span{color:var(--accent);}
.login-sub{font-size:12px;color:#999;margin-bottom:28px;}
.form-group{margin-bottom:16px;text-align:left;}
.form-group label{font-size:10px;font-weight:700;text-transform:uppercase;letter-spacing:.08em;color:#999;display:block;margin-bottom:4px;}
.form-group input{width:100%;padding:11px 14px;border:1.5px solid #e0e0e0;border-radius:9px;font-size:14px;font-family:inherit;}
.form-group input:focus{border-color:var(--accent);outline:none;}
.btn-login{width:100%;padding:12px;border:none;border-radius:9px;background:var(--accent);color:#fff;font-size:14px;font-weight:700;cursor:pointer;font-family:inherit;letter-spacing:.04em;}
.btn-login:hover{background:var(--accent-dark);}
.error{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;border-radius:8px;padding:10px;font-size:12px;margin-bottom:12px;}
</style>
</head>
<body>
<form id="form1" runat="server">
<div class="login-card">
    <div class="login-title">SIRIMIRI <span>FINANCE</span></div>
    <div class="login-sub">Sign in to continue</div>
    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error"><asp:Label ID="lblError" runat="server"/></asp:Panel>
    <div class="form-group"><label>Username</label><asp:TextBox ID="txtUsername" runat="server" placeholder="Enter username"/></div>
    <div class="form-group"><label>Password</label><asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Enter password"/></div>
    <asp:Button ID="btnLogin" runat="server" Text="Sign In" CssClass="btn-login" OnClick="btnLogin_Click"/>
</div>
</form>
</body>
</html>
