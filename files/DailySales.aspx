<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DailySales.aspx.cs" Inherits="StockApp.DailySales" ResponseEncoding="UTF-8" ContentType="text/html; charset=utf-8" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>Sirimiri - Daily Sales Entry</title>
    <link rel="preconnect" href="https://fonts.googleapis.com"/>
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <style>
        :root{--accent:#C0392B;--accent-dark:#a93226;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--text:#1a1a1a;--muted:#666;--radius:14px}
        *{box-sizing:border-box;margin:0;padding:0}
        body{background:var(--bg);font-family:'DM Sans',sans-serif;min-height:100vh}
        nav{background:var(--accent);display:flex;align-items:center;padding:0 24px;height:52px;gap:12px}
        .nav-group{position:relative}
        .nav-item{color:#fff;font-size:13px;font-weight:600;padding:8px 14px;border-radius:6px;cursor:pointer;display:flex;align-items:center;gap:6px;text-decoration:none}
        .nav-item:hover{background:rgba(255,255,255,.15)}
        .nav-dropdown{display:none;position:absolute;top:100%;left:0;background:#fff;border-radius:8px;min-width:220px;box-shadow:0 4px 20px rgba(0,0,0,.15);z-index:999;overflow:hidden}
        .nav-group:hover .nav-dropdown{display:block}
        .nav-dropdown a{display:block;padding:10px 16px;font-size:13px;color:var(--text);text-decoration:none}
        .nav-dropdown a:hover{background:var(--bg);color:var(--accent)}
        .nav-right{margin-left:auto;display:flex;align-items:center;gap:20px;font-size:13px}
        .nav-right a{color:#fff;font-weight:700;text-decoration:none;opacity:.9}
        .btn-signout{border:1.5px solid rgba(255,255,255,.6);padding:5px 14px;border-radius:6px}
        .user-label{color:#fff;opacity:.9;font-weight:500}
        .logo-area{background:#fff;display:flex;align-items:center;justify-content:space-between;padding:16px 24px 0}
        .logo-area img{height:72px;object-fit:contain;filter:drop-shadow(0 2px 8px rgba(204,30,30,.20))}
        .bis-label{font-family:'Bebas Neue',cursive;font-size:22px;letter-spacing:.12em;color:var(--text);text-align:center;line-height:1.25}
        .accent-bar{height:4px;background:linear-gradient(90deg,var(--accent-dark),#e63030,var(--accent-dark))}
        .page-wrap{max-width:860px;margin:0 auto;padding:28px 16px 60px}
        .page-header{display:flex;align-items:center;justify-content:space-between;margin-bottom:22px}
        .page-title{font-family:'Bebas Neue',cursive;font-size:32px;letter-spacing:.08em;color:var(--text)}
        .date-badge{background:var(--accent);color:#fff;font-family:'Bebas Neue',cursive;font-size:14px;letter-spacing:.08em;padding:6px 16px;border-radius:20px}
        .card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 16px rgba(0,0,0,.08);margin-bottom:22px;overflow:hidden}
        .card-head{background:var(--accent);padding:12px 20px}
        .card-head h2{font-family:'Bebas Neue',cursive;font-size:18px;letter-spacing:.08em;color:#fff}
        .card-body{padding:20px}
        .field-row{display:grid;grid-template-columns:1fr 1fr 1fr;gap:16px}
        @media(max-width:640px){.field-row{grid-template-columns:1fr}}
        .field label{display:block;font-size:11px;font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:var(--muted);margin-bottom:6px}
        .field select{width:100%;padding:10px 12px;border:1.5px solid var(--border);border-radius:8px;font-size:14px;background:#fff;color:var(--text)}
        .field select:focus{outline:none;border-color:var(--accent)}
        /* Multi-select distributor */
        .field select[multiple]{height:140px;padding:6px}
        .field select[multiple] option{padding:6px 8px;border-radius:4px;margin-bottom:2px}
        .field select[multiple] option:checked{background:var(--accent);color:#fff}
        .multi-hint{font-size:11px;color:var(--muted);margin-top:4px}
        /* Product table */
        .product-table{width:100%;border-collapse:collapse}
        .product-table th{font-size:11px;font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:var(--muted);padding:10px 12px;text-align:left;border-bottom:2px solid var(--border)}
        .product-table td{padding:10px 12px;border-bottom:1px solid var(--border);vertical-align:middle}
        .product-table tr:last-child td{border-bottom:none}
        .product-table tr:hover td{background:#fafafa}
        .product-name{font-size:14px;font-weight:500;color:var(--text)}
        .product-meta{font-size:11px;color:var(--muted);margin-top:2px}
        .qty-wrap{display:flex;align-items:center;gap:8px}
        .qty-input{width:90px;padding:8px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:15px;font-weight:600;text-align:center;color:var(--text)}
        .qty-input:focus{outline:none;border-color:var(--accent);background:#fff8f8}
        .qty-input:invalid{border-color:#e74c3c}
        .units-label{font-size:13px;color:var(--muted);font-weight:500}
        /* Buttons */
        .btn-row{display:flex;gap:12px;margin-top:8px}
        .btn-save{padding:12px 32px;background:var(--accent);color:#fff;border:none;border-radius:8px;font-size:15px;font-weight:600;cursor:pointer;letter-spacing:.04em}
        .btn-save:hover{background:var(--accent-dark)}
        .btn-cancel{padding:12px 28px;background:#fff;color:var(--text);border:1.5px solid var(--border);border-radius:8px;font-size:15px;font-weight:600;cursor:pointer}
        .btn-cancel:hover{background:var(--bg)}
        /* Messages */
        .msg-ok{background:#f0fdf4;border:1px solid #86efac;border-radius:8px;padding:12px 16px;color:#166534;font-size:14px;margin-bottom:16px}
        .msg-err{background:#fff8f8;border:1px solid #fca5a5;border-radius:8px;padding:12px 16px;color:#991b1b;font-size:14px;margin-bottom:16px}
        .empty-products{text-align:center;padding:40px;color:var(--muted);font-size:14px}
    </style>
</head>
<body>
<form id="form1" runat="server">

    <!-- NAV -->
    <nav>
        <a href="ERPHome.aspx" style="text-decoration:none;color:#fff;font-size:13px;font-weight:600;padding:14px 18px;letter-spacing:.04em;text-transform:uppercase;">&#x2302; Home</a>
        <div class="nav-group">
            <span class="nav-item">&#9776; Home</span>
            <div class="nav-dropdown">
                <a href="StockEntry.aspx">Distributor Stock Position Entry</a>
                <a href="DailySales.aspx" style="color:var(--accent);font-weight:600;">Daily Sales Entry</a>
            </div>
        </div>
        <asp:Panel ID="pnlAdminMenu" runat="server" Visible="false" CssClass="nav-group">
            <span class="nav-item">&#9881; Admin</span>
            <div class="nav-dropdown">
                <a href="UserAdmin.aspx">User Management</a>
                <a href="ProductMaster.aspx">Product Master</a>
            </div>
        </asp:Panel>
        <div class="nav-right">
            <span class="user-label"><asp:Label ID="lblUserInfo" runat="server" /></span>
            <a href="Logout.aspx" class="btn-signout">Sign Out</a>
        </div>
    </nav>

    <!-- LOGO -->
    <div class="logo-area">
        <img src="https://vimarsa.in/StockApp/sirimiri-logo.png" alt="Sirimiri" onerror="this.style.display='none'" />
        <div class="bis-label">SIRIMIRI NUTRITION FOOD PRODUCTS<br/><span style="font-size:14px;letter-spacing:.14em;">BUSINESS INTELLIGENCE SYSTEM</span></div>
        <div></div>
    </div>
    <div class="accent-bar"></div>

    <div class="page-wrap">
        <div class="page-header">
            <div class="page-title">DAILY SALES ENTRY</div>
            <div class="date-badge"><%= DateTime.Now.ToString("dddd, d MMM yyyy").ToUpper() %></div>
        </div>

        <!-- Messages -->
        <asp:Panel ID="pnlResult" runat="server" Visible="false">
            <div class="msg-ok"><asp:Label ID="lblResultMsg" runat="server" /></div>
        </asp:Panel>
        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="msg-err"><asp:Label ID="lblErrorMsg" runat="server" /></div>
        </asp:Panel>

        <!-- DISTRIBUTOR SELECTION -->
        <div class="card">
            <div class="card-head"><h2>SELECT DISTRIBUTOR</h2></div>
            <div class="card-body">
                <div class="field-row">
                    <div class="field">
                        <label>State</label>
                        <asp:DropDownList ID="ddlState" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlState_SelectedIndexChanged" />
                    </div>
                    <div class="field">
                        <label>City</label>
                        <asp:DropDownList ID="ddlCity" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlCity_SelectedIndexChanged" />
                    </div>
                    <div class="field">
                        <label>Distributor</label>
                        <asp:ListBox ID="lstDistributor" runat="server" SelectionMode="Multiple" Rows="6" />
                        <div class="multi-hint">Hold Ctrl / Cmd to select multiple</div>
                    </div>
                </div>
            </div>
        </div>

        <!-- PRODUCT ENTRY -->
        <asp:Panel ID="pnlSalesEntry" runat="server">
        <div class="card">
            <div class="card-head"><h2>ENTER QUANTITIES SOLD</h2></div>
            <div class="card-body">
                <asp:HiddenField ID="hfProductData" runat="server" />
                <table class="product-table">
                    <thead>
                        <tr>
                            <th>Product</th>
                            <th style="width:180px;">Quantity</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptProducts" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="product-name"><%# Eval("ProductName") %></div>
                                        <div class="product-meta">
                                            <%# Eval("MRP", "Rs. {0:N2}") %>
                                            <%# Convert.ToString(Eval("HSNCode")) != "" ? " &nbsp;|&nbsp; HSN: " + Eval("HSNCode") : "" %>
                                        </div>
                                        <asp:HiddenField ID="hfProductId" runat="server" Value='<%# Eval("ProductID") %>' />
                                    </td>
                                    <td>
                                        <div class="qty-wrap">
                                            <asp:TextBox ID="txtQty" runat="server"
                                                CssClass="qty-input"
                                                TextMode="Number"
                                                Text="0" />
                                            <span class="units-label">Units</span>
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>

                <br/>
                <div class="btn-row">
                    <asp:Button ID="btnSave" runat="server" Text="SAVE" CssClass="btn-save" OnClick="btnSave_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="CANCEL" CssClass="btn-cancel" OnClick="btnCancel_Click" CausesValidation="false" />
                </div>
            </div>
        </div>
        </asp:Panel>

    </div>
</form>
</body>
</html>
