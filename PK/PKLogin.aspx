<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKLogin" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Packing & Shipments</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<style>
:root{--accent:#e67e22;--text:#1a1a1a;--border:#e0e0e0;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:#f0f0f0;min-height:100vh;display:flex;align-items:center;justify-content:center;}
.login-box{background:#fff;border-radius:var(--radius);padding:40px 36px;width:360px;box-shadow:0 4px 24px rgba(0,0,0,.1);}
.logo{text-align:center;margin-bottom:24px;}
.logo-title{font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.1em;color:var(--text);}
.logo-sub{font-size:11px;color:#999;letter-spacing:.06em;text-transform:uppercase;}
label{display:block;font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:#666;margin-bottom:5px;margin-top:14px;}
input{width:100%;padding:10px 13px;border:1.5px solid var(--border);border-radius:8px;font-size:13px;font-family:inherit;outline:none;}
input:focus{border-color:var(--accent);}
.btn{width:100%;background:#1a1a1a;color:#fff;border:none;border-radius:8px;padding:12px;font-size:13px;font-weight:700;cursor:pointer;margin-top:20px;letter-spacing:.05em;}
.btn:hover{background:#333;}
.err{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;border-radius:8px;padding:10px 14px;font-size:12px;margin-top:12px;}
</style></head><body><form id="form1" runat="server">
<div class="login-box">
    <div class="logo">
        <img src="Sirimiri_Logo-16_9-72ppi-01.png" style="height:40px;" onerror="this.style.display='none'"/>
        <div class="logo-title">Packing &amp; Shipments</div>
        <div class="logo-sub">Sirimiri Nutrition</div>
    </div>
    <label>Username</label>
    <asp:TextBox ID="txtUser" runat="server"/>
    <label>Password</label>
    <asp:TextBox ID="txtPass" runat="server" TextMode="Password"/>
    <asp:Button ID="btnLogin" runat="server" Text="Sign In" CssClass="btn" OnClick="btnLogin_Click" CausesValidation="false"/>
    <asp:Panel ID="pnlErr" runat="server" Visible="false"><div class="err"><asp:Label ID="lblErr" runat="server"/></div></asp:Panel>
</div>
</form></body></html>
