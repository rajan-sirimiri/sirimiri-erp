<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINSalesAnalytics.aspx.cs" Inherits="FINApp.FINSalesAnalytics" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — CDN Test</title>
</head>
<body>
<form id="form1" runat="server">
<div id="status" style="padding:40px;font-family:monospace;font-size:14px;">Testing CDN libraries...</div>
<script>
var log = '';
function check(name, obj) {
    var ok = typeof obj !== 'undefined';
    log += name + ': ' + (ok ? '✅ LOADED' : '❌ MISSING') + '\n';
}
</script>

<script src="https://cdnjs.cloudflare.com/ajax/libs/react/18.2.0/umd/react.production.min.js" onerror="log+='React CDN: ❌ FAILED TO LOAD\n'"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/react-dom/18.2.0/umd/react-dom.production.min.js" onerror="log+='ReactDOM CDN: ❌ FAILED TO LOAD\n'"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/babel-standalone/7.23.9/babel.min.js" onerror="log+='Babel CDN: ❌ FAILED TO LOAD\n'"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/recharts/2.12.7/Recharts.min.js" onerror="log+='Recharts CDN (cdnjs): ❌ FAILED TO LOAD\n'"></script>
<script src="https://unpkg.com/recharts@2.12.7/umd/Recharts.js" onerror="log+='Recharts CDN (unpkg): ❌ FAILED TO LOAD\n'"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js" onerror="log+='Chart.js CDN: ❌ FAILED TO LOAD\n'"></script>

<script>
setTimeout(function() {
    check('React', window.React);
    check('ReactDOM', window.ReactDOM);
    check('Babel', window.Babel);
    check('Recharts (cdnjs)', window.Recharts);
    check('Chart.js', window.Chart);
    
    // Also test API
    fetch('FINAnalyticsAPI.ashx?action=overview').then(function(r){ return r.json(); }).then(function(d){
        log += '\nAPI Test: ' + (d.error ? '❌ ' + d.error : '✅ totalSales=' + d.totalSales);
        document.getElementById('status').innerText = log;
    }).catch(function(e){
        log += '\nAPI Test: ❌ ' + e.message;
        document.getElementById('status').innerText = log;
    });
    
    document.getElementById('status').innerText = log + '\n\nTesting API...';
}, 3000);
</script>
</form>
</body>
</html>
