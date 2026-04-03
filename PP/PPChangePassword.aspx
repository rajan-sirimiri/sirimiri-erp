<%@ Page Language="C#" AutoEventWireup="true" Inherits="PPApp.PPChangePassword" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Change Password &mdash; PP</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--accent:#2980b9;--teal:#1a9e6a;--red:#e74c3c;--text:#1a1a1a;--text-muted:#666;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);min-height:100vh;display:flex;align-items:center;justify-content:center;}
.pwd-card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:32px;width:100%;max-width:420px;margin:20px;}
.pwd-title{font-family:'Bebas Neue',sans-serif;font-size:24px;letter-spacing:.06em;margin-bottom:6px;}
.pwd-sub{font-size:12px;color:var(--text-muted);margin-bottom:24px;}
.form-group{margin-bottom:16px;}
.form-group label{display:block;font-size:11px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);margin-bottom:5px;}
.form-group input{width:100%;padding:10px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:14px;outline:none;}
.form-group input:focus{border-color:var(--accent);}
.btn{border:none;border-radius:8px;padding:12px 24px;font-size:13px;font-weight:700;cursor:pointer;width:100%;}
.btn-primary{background:var(--accent);color:#fff;}.btn-primary:hover{background:#1a6a9e;}
.alert-danger{background:#fdf3f2;color:var(--red);border:1px solid #f5c6cb;padding:10px 14px;border-radius:8px;font-size:13px;font-weight:600;margin-bottom:14px;}
.pwd-rules{font-size:11px;color:var(--text-muted);margin-bottom:16px;line-height:1.6;}
</style></head><body>
<form id="form1" runat="server">
<div class="pwd-card">
    <div class="pwd-title">Change Password</div>
    <div class="pwd-sub">Production Planning &mdash; Your password must be changed before continuing.</div>
    <asp:Panel ID="pnlError" runat="server" Visible="false"><div class="alert-danger"><asp:Label ID="lblError" runat="server"/></div></asp:Panel>
    <div class="form-group"><label>Current Password</label><asp:TextBox ID="txtCurrent" runat="server" TextMode="Password"/></div>
    <div class="form-group"><label>New Password</label><asp:TextBox ID="txtNew" runat="server" TextMode="Password"/></div>
    <div class="form-group"><label>Confirm New Password</label><asp:TextBox ID="txtConfirm" runat="server" TextMode="Password"/></div>
    <div class="pwd-rules">Min 8 characters, at least one uppercase letter and one number.</div>
    <asp:Button ID="btnChange" runat="server" Text="Change Password" CssClass="btn btn-primary" OnClick="btnChange_Click" CausesValidation="false"/>
</div>
</form>
</body></html>