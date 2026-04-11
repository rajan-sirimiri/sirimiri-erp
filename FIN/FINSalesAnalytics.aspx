<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINSalesAnalytics.aspx.cs" Inherits="FINApp.FINSalesAnalytics" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Sales Analytics</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;700&display=swap" rel="stylesheet"/>
<script src="https://cdnjs.cloudflare.com/ajax/libs/react/18.2.0/umd/react.production.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/react-dom/18.2.0/umd/react-dom.production.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/recharts/2.12.7/Recharts.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/babel-standalone/7.23.9/babel.min.js"></script>
<style>
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:#f7f5f2;color:#0f0f0f;-webkit-font-smoothing:antialiased;}
</style>
</head>
<body>
<form id="form1" runat="server">
<div id="analytics-root"></div>
<script type="text/babel">
const { useState, useEffect, useCallback } = React;
const { LineChart, Line, BarChart, Bar, PieChart, Pie, Cell, AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, ComposedChart } = Recharts;

const API = 'FINAnalyticsAPI.ashx';
const C = ['#cc1e1e','#1a9e6a','#1e5fcc','#d68b00','#7c3aed','#e67e22','#16a085','#8e44ad','#2c3e50','#f39c12','#c0392b','#27ae60','#3498db','#d35400','#7f8c8d'];

const fmt = (v) => {
  if (!v && v !== 0) return '₹0';
  if (v >= 1e7) return '₹'+(v/1e7).toFixed(1)+'Cr';
  if (v >= 1e5) return '₹'+(v/1e5).toFixed(1)+'L';
  if (v >= 1e3) return '₹'+(v/1e3).toFixed(1)+'K';
  return '₹'+Math.round(v).toLocaleString('en-IN');
};

const fM = (ym) => {
  if (!ym || ym.length < 7) return ym || '';
  const mo = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
  const p = ym.split('-');
  return mo[parseInt(p[1])-1]+' '+p[0].slice(2);
};

const q = async (action, params = {}) => {
  let url = API+'?action='+action;
  Object.entries(params).forEach(([k,v]) => { if(v) url += '&'+k+'='+encodeURIComponent(v); });
  const r = await fetch(url);
  return r.json();
};

// ── Reusable Components ──
const KPI = ({value, label, color='#9a9590', delta}) => (
  <div style={{flex:1,minWidth:140,background:'#fff',borderRadius:10,padding:'16px 18px',borderLeft:'4px solid '+color,boxShadow:'0 1px 4px rgba(0,0,0,.05)'}}>
    <div style={{fontFamily:"'Bebas Neue'",fontSize:28,lineHeight:1,letterSpacing:'.03em'}}>{value}</div>
    <div style={{fontSize:9,fontWeight:700,letterSpacing:'.1em',textTransform:'uppercase',color:'#9a9590',marginTop:4}}>{label}</div>
    {delta !== undefined && delta !== null && <div style={{fontSize:11,fontWeight:700,marginTop:3,color:delta>=0?'#1a9e6a':'#cc1e1e'}}>{delta>=0?'▲':'▼'} {Math.abs(delta).toFixed(0)}%</div>}
  </div>
);

const SH = ({children, badge}) => (
  <div style={{fontFamily:"'Bebas Neue'",fontSize:20,letterSpacing:'.08em',margin:'28px 0 12px',paddingBottom:6,borderBottom:'3px solid #0f0f0f',display:'flex',alignItems:'baseline',gap:10}}>
    {children}
    {badge && <span style={{fontFamily:"'DM Sans'",fontSize:10,fontWeight:700,background:'#cc1e1e',color:'#fff',padding:'3px 10px',borderRadius:20}}>{badge}</span>}
  </div>
);

const CTooltip = ({active, payload, label}) => {
  if (!active || !payload?.length) return null;
  return (
    <div style={{background:'#0f0f0f',color:'#fff',padding:'10px 14px',borderRadius:8,fontSize:11,boxShadow:'0 4px 20px rgba(0,0,0,.3)'}}>
      <div style={{fontWeight:700,marginBottom:4,fontSize:12}}>{label}</div>
      {payload.map((p,i) => (
        <div key={i} style={{display:'flex',gap:8,alignItems:'center',padding:'2px 0'}}>
          <div style={{width:8,height:8,borderRadius:'50%',background:p.color}} />
          <span style={{opacity:.7}}>{p.name}:</span>
          <span style={{fontWeight:700}}>{fmt(p.value)}</span>
        </div>
      ))}
    </div>
  );
};

const Card = ({children, style}) => (
  <div style={{background:'#fff',borderRadius:10,padding:20,marginBottom:18,boxShadow:'0 1px 4px rgba(0,0,0,.05)',...style}}>{children}</div>
);

const CardHead = ({children}) => (
  <div style={{fontFamily:"'Bebas Neue'",fontSize:14,color:'#9a9590',letterSpacing:'.06em',marginBottom:10}}>{children}</div>
);

const DT = ({columns, data, maxH=450}) => (
  <div style={{overflow:'auto',maxHeight:maxH,border:'1px solid #e8e5e0',borderRadius:8}}>
    <table style={{width:'100%',borderCollapse:'collapse',fontSize:11}}>
      <thead><tr>{columns.map((c,i) => (
        <th key={i} style={{fontSize:9,fontWeight:700,letterSpacing:'.1em',textTransform:'uppercase',color:'#9a9590',padding:'10px 8px',textAlign:c.num?'right':'left',borderBottom:'2px solid #e8e5e0',background:'#faf9f7',position:'sticky',top:0,zIndex:1}}>{c.label}</th>
      ))}</tr></thead>
      <tbody>{data.map((row,ri) => (
        <tr key={ri} style={{background:ri%2===0?'transparent':'#faf9f7'}}>
          {columns.map((c,ci) => (
            <td key={ci} style={{padding:'7px 8px',borderBottom:'1px solid #f2f0ed',textAlign:c.num?'right':'left',fontFamily:c.mono?"'JetBrains Mono'":"inherit",fontSize:c.mono?10:11,fontWeight:c.bold?700:400,color:c.color?c.color(row):'inherit'}}>
              {c.render ? c.render(row,ri) : row[c.key]}
            </td>
          ))}
        </tr>
      ))}</tbody>
    </table>
  </div>
);

const TagDIST = ({type}) => (
  <span style={{fontSize:9,fontWeight:700,padding:'2px 7px',borderRadius:4,background:type==='DI'?'#eafaf1':'#ebf5fb',color:type==='DI'?'#1a9e6a':'#1e5fcc'}}>{type}</span>
);

const buildPivot = (items, months) => {
  if (!items || !months) return [];
  return months.map((m, mi) => {
    const row = { month: fM(m) };
    items.forEach(item => { row[item.name] = item.monthly[mi] || 0; });
    return row;
  });
};

const pctArrow = (curr, prev) => {
  if (!prev || prev === 0) return null;
  const p = ((curr-prev)/prev*100).toFixed(0);
  return <span style={{color:p>=0?'#1a9e6a':'#cc1e1e',fontWeight:700}}>{p>=0?'▲':'▼'} {Math.abs(p)}%</span>;
};

const TABS = [{id:'all',label:'All Data'},{id:'fy',label:'FY 2025-26'},{id:'product',label:'Product View'},{id:'distributor',label:'Distributor View'}];

function App() {
  const [tab, setTab] = useState('all');
  const [loading, setLoading] = useState(true);
  const [ov, setOv] = useState(null);
  const [trend, setTrend] = useState([]);
  const [states, setStates] = useState(null);
  const [products, setProducts] = useState([]);
  const [alerts, setAlerts] = useState(null);
  const [showRcpt, setShowRcpt] = useState(false);
  const [selState, setSelState] = useState('');
  const [cityData, setCityData] = useState(null);
  const [ptData, setPtData] = useState(null);
  const [distList, setDistList] = useState(null);
  const [dd, setDD] = useState(null);
  const [ddName, setDDName] = useState('');
  const [pvState, setPvState] = useState('ALL');
  const [pvData, setPvData] = useState(null);
  const [pvSel, setPvSel] = useState(['ALL']);
  const [allProd, setAllProd] = useState([]);
  const [dvState, setDvState] = useState('ALL');
  const [dvData, setDvData] = useState(null);
  const [dvPF, setDvPF] = useState([]);
  const [dvPD, setDvPD] = useState(null);

  const dp = tab==='fy'?{dateFrom:'2025-04-01',dateTo:'2026-03-31'}:(tab==='distributor'?{dateFrom:'2025-04-01',dateTo:'2026-03-31'}:{});

  const loadMain = async () => {
    setLoading(true);
    try {
      const [o,t,s,p,a] = await Promise.all([q('overview',dp),q('monthlyTrend',dp),q('stateBreakdown',dp),q('topProducts',dp),q('alerts')]);
      setOv(o); setTrend(Array.isArray(t)?t:[]); setStates(s&&s.states?s:{months:[],states:[]}); setProducts(Array.isArray(p)?p:[]); setAlerts(a);
    } catch(e) { console.error(e); }
    setLoading(false);
  };

  const loadPV = async () => {
    setLoading(true);
    const [n,s] = await Promise.all([allProd.length?Promise.resolve(allProd):q('productList'),states?Promise.resolve(states):q('stateBreakdown')]);
    if(Array.isArray(n)) setAllProd(n); if(!states && s) setStates(s);
    const d = await q('productView',{state:'ALL',...dp});
    setPvData(d);
    setLoading(false);
  };

  const loadDV = async () => {
    setLoading(true);
    if(!states){ const s=await q('stateBreakdown'); setStates(s); }
    if(!allProd.length){ const n=await q('productList'); if(Array.isArray(n)) setAllProd(n); }
    const d = await q('distView',{state:'ALL',dateFrom:'2025-04-01',dateTo:'2026-03-31'});
    setDvData(d);
    setLoading(false);
  };

  useEffect(() => {
    setCityData(null); setPtData(null); setDistList(null); setDD(null); setDvPD(null);
    if(tab==='all'||tab==='fy') loadMain();
    else if(tab==='product') loadPV();
    else if(tab==='distributor') loadDV();
  }, [tab]);

  const tcd = trend.map(t => ({month:fM(t.month),Sales:t.sales,Receipts:t.receipts||0}));

  if (loading) return (
    <div style={{display:'flex',alignItems:'center',justifyContent:'center',height:'60vh',color:'#9a9590'}}>
      <div style={{textAlign:'center'}}>
        <div style={{fontSize:24,fontFamily:"'Bebas Neue'",letterSpacing:'.1em',marginBottom:8}}>Loading Analytics</div>
        <div style={{fontSize:12}}>Fetching data...</div>
      </div>
    </div>
  );

  return (
    <div>
      {/* NAV */}
      <div style={{background:'#0f0f0f',padding:'12px 28px',display:'flex',alignItems:'center',gap:16}}>
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" style={{height:28,background:'#fff',borderRadius:4,padding:'2px 6px'}} onError={e=>e.target.style.display='none'} />
        <span style={{fontFamily:"'Bebas Neue'",color:'#fff',fontSize:18,letterSpacing:'.1em'}}>FINANCE</span>
        <span style={{fontFamily:"'Bebas Neue'",color:'rgba(255,255,255,.4)',fontSize:14,letterSpacing:'.08em'}}>SALES ANALYTICS</span>
        <div style={{flex:1}} />
        <a href="FINHome.aspx" style={{color:'rgba(255,255,255,.6)',fontSize:12,textDecoration:'none'}}>← FIN Home</a>
        <a href="/StockApp/ERPHome.aspx" style={{color:'rgba(255,255,255,.6)',fontSize:12,textDecoration:'none'}}>← ERP</a>
        <a href="FINLogout.aspx" style={{color:'rgba(255,255,255,.6)',fontSize:12,textDecoration:'none'}}>Sign Out</a>
      </div>
      <div style={{background:'#fff',borderBottom:'3px solid #cc1e1e',padding:'20px 40px',display:'flex',alignItems:'center',gap:14}}>
        <span style={{fontSize:28}}>📊</span>
        <div>
          <div style={{fontFamily:"'Bebas Neue'",fontSize:28,letterSpacing:'.07em'}}>SALES <span style={{color:'#cc1e1e'}}>ANALYTICS</span></div>
          <div style={{fontSize:12,color:'#9a9590'}}>Revenue trends, product performance, distributor intelligence</div>
        </div>
      </div>

      <div style={{maxWidth:1300,margin:'0 auto',padding:'24px 24px 80px'}}>
        {/* TABS */}
        <div style={{display:'flex',borderRadius:10,overflow:'hidden',border:'2px solid #0f0f0f',marginBottom:24}}>
          {TABS.map(t => (
            <button key={t.id} onClick={()=>setTab(t.id)} style={{flex:1,padding:'13px 10px',border:'none',fontFamily:"'Bebas Neue'",fontSize:14,letterSpacing:'.08em',cursor:'pointer',background:tab===t.id?'#0f0f0f':'#fff',color:tab===t.id?'#fff':'#0f0f0f',transition:'all .15s'}}>{t.label}</button>
          ))}
        </div>

        {/* ═══ TAB 1/2: ALL / FY ═══ */}
        {(tab==='all'||tab==='fy') && ov && (<>
          <div style={{display:'flex',gap:12,marginBottom:24,flexWrap:'wrap'}}>
            <KPI value={fmt(ov.totalSales)} label={tab==='fy'?'FY 25-26 Revenue':'Total Revenue'} color="#cc1e1e" />
            <KPI value={fmt(ov.thisMonth)} label="Latest Month" color="#1a9e6a" delta={ov.growthPct} />
            <KPI value={(ov.totalInvoices||0).toLocaleString()} label="Invoices" color="#1e5fcc" />
            <KPI value={ov.totalCustomers} label="Customers" color="#d68b00" />
            <KPI value={fmt(ov.totalReceipts||0)} label="Receipts" color="#7c3aed" />
            <KPI value={ov.monthCount} label="Months" />
          </div>

          <SH badge={ov.monthCount+' MONTHS'}>Revenue Trend</SH>
          <Card>
            <label style={{fontSize:12,fontWeight:600,cursor:'pointer',display:'flex',alignItems:'center',gap:6,marginBottom:10}}>
              <input type="checkbox" checked={showRcpt} onChange={()=>setShowRcpt(!showRcpt)} style={{width:16,height:16,accentColor:'#1a9e6a'}} /> Show Receipts
            </label>
            <ResponsiveContainer width="100%" height={300}>
              <ComposedChart data={tcd}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0ede8" />
                <XAxis dataKey="month" tick={{fontSize:10}} />
                <YAxis tickFormatter={fmt} tick={{fontSize:9,fontFamily:"'JetBrains Mono'"}} />
                <Tooltip content={<CTooltip />} />
                <Legend />
                <Area type="monotone" dataKey="Sales" stroke="#cc1e1e" fill="rgba(204,30,30,.08)" strokeWidth={2.5} dot={{r:4,fill:'#cc1e1e'}} />
                {showRcpt && <Line type="monotone" dataKey="Receipts" stroke="#1a9e6a" strokeWidth={2.5} strokeDasharray="6 3" dot={{r:3,fill:'#1a9e6a'}} />}
              </ComposedChart>
            </ResponsiveContainer>
          </Card>

          <SH>State Performance</SH>
          <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:18,marginBottom:18}}>
            <Card><CardHead>Revenue Share</CardHead>
              <ResponsiveContainer width="100%" height={260}>
                <PieChart><Pie data={states.states.map(s=>({name:s.name,value:s.total}))} cx="50%" cy="50%" innerRadius={60} outerRadius={100} dataKey="value" label={({name,percent})=>name+' '+( percent*100).toFixed(0)+'%'} labelLine={false} style={{fontSize:9}}>
                  {states.states.map((_,i) => <Cell key={i} fill={C[i%C.length]} />)}
                </Pie><Tooltip formatter={v=>fmt(v)} /></PieChart>
              </ResponsiveContainer>
            </Card>
            <Card><CardHead>Monthly by State</CardHead>
              <ResponsiveContainer width="100%" height={260}>
                <BarChart data={buildPivot(states.states,states.months)}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0ede8" />
                  <XAxis dataKey="month" tick={{fontSize:9}} /><YAxis tickFormatter={fmt} tick={{fontSize:9}} />
                  <Tooltip content={<CTooltip />} /><Legend wrapperStyle={{fontSize:10}} />
                  {states.states.map((s,i) => <Bar key={s.name} dataKey={s.name} stackId="a" fill={C[i%C.length]} />)}
                </BarChart>
              </ResponsiveContainer>
            </Card>
          </div>

          <Card><CardHead>State × Month</CardHead>
            <DT columns={[{label:'#',render:(_,i)=>i+1},{label:'State',key:'name',bold:true},{label:'Total',num:true,mono:true,bold:true,render:r=>fmt(r.total)},...states.months.map((m,mi)=>({label:fM(m),num:true,mono:true,render:r=>{const v=r.monthly[mi];return v>0?fmt(v):'—';}})),{label:'Trend',num:true,render:r=>{const m=r.monthly;return m.length>=2?pctArrow(m[m.length-1],m[m.length-2]):null;}}]} data={states.states} />
          </Card>

          <Card><CardHead>Drill Down: State → City</CardHead>
            <select value={selState} onChange={async e=>{setSelState(e.target.value);if(!e.target.value){setCityData(null);return;}const d=await q('cityBreakdown',{state:e.target.value,...dp});setCityData(d);}} style={{padding:'8px 14px',border:'1.5px solid #e8e5e0',borderRadius:8,fontSize:13,marginBottom:14,minWidth:200}}>
              <option value="">— Select State —</option>
              {states.states.map(s => <option key={s.name} value={s.name}>{s.name} ({fmt(s.total)})</option>)}
            </select>
            {cityData?.cities?.length>0 && (<>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={buildPivot(cityData.cities.slice(0,12),cityData.months)}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0ede8" />
                  <XAxis dataKey="month" tick={{fontSize:9}} /><YAxis tickFormatter={fmt} tick={{fontSize:9}} />
                  <Tooltip content={<CTooltip />} /><Legend wrapperStyle={{fontSize:9}} />
                  {cityData.cities.slice(0,12).map((c,i)=><Bar key={c.name} dataKey={c.name} stackId="a" fill={C[i%C.length]} />)}
                </BarChart>
              </ResponsiveContainer>
              <DT columns={[{label:'#',render:(_,i)=>i+1},{label:'City',key:'name',bold:true},{label:'Total',num:true,mono:true,bold:true,render:r=>fmt(r.total)},...cityData.months.map((m,mi)=>({label:fM(m),num:true,mono:true,render:r=>{const v=r.monthly[mi];return v>0?fmt(v):'—';}}))]} data={cityData.cities} />
            </>)}
          </Card>

          <SH>Product Performance</SH>
          <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:18,marginBottom:18}}>
            <Card><CardHead>Top Products</CardHead>
              <ResponsiveContainer width="100%" height={320}>
                <BarChart data={products.slice(0,10)} layout="vertical">
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0ede8" />
                  <XAxis type="number" tickFormatter={fmt} tick={{fontSize:9}} />
                  <YAxis type="category" dataKey="name" width={150} tick={{fontSize:9}} />
                  <Tooltip formatter={v=>fmt(v)} />
                  <Bar dataKey="sales" fill="#cc1e1ecc" radius={[0,4,4,0]} />
                </BarChart>
              </ResponsiveContainer>
            </Card>
            <Card><CardHead>Product Mix</CardHead>
              <ResponsiveContainer width="100%" height={320}>
                <PieChart><Pie data={products.slice(0,8).map(p=>({name:p.name,value:p.sales}))} cx="50%" cy="50%" innerRadius={55} outerRadius={100} dataKey="value">
                  {products.slice(0,8).map((_,i)=><Cell key={i} fill={C[i]} />)}
                </Pie><Tooltip formatter={v=>fmt(v)} /><Legend wrapperStyle={{fontSize:9}} /></PieChart>
              </ResponsiveContainer>
            </Card>
          </div>

          <Card>
            <DT columns={[{label:'#',render:(_,i)=>i+1},{label:'Product',key:'name',bold:true},{label:'Revenue',num:true,mono:true,bold:true,render:r=>fmt(r.sales)},{label:'Qty',num:true,mono:true,render:r=>Math.round(r.qty).toLocaleString()},{label:'Invoices',num:true,render:r=>r.invoices},{label:'Customers',num:true,render:r=>r.customers},{label:'Share',num:true,render:r=>{const t=products.reduce((s,p)=>s+p.sales,0);return t>0?(r.sales/t*100).toFixed(1)+'%':'';}}]} data={products} />
          </Card>

          {alerts?.silentDistributors?.length>0 && (<>
            <SH>⚠ Silent Distributors — 45+ Days</SH>
            <div style={{background:'#fff8f0',border:'1.5px solid #ffd6a0',borderRadius:10,padding:'16px 20px',marginBottom:18}}>
              {alerts.silentDistributors.map((d,i)=>(
                <div key={i} style={{display:'flex',alignItems:'center',gap:10,padding:'6px 0',borderBottom:i<alerts.silentDistributors.length-1?'1px solid #f5efe5':'none',fontSize:12}}>
                  <span style={{fontFamily:"'JetBrains Mono'",fontSize:11,fontWeight:700,color:'#cc1e1e',minWidth:45}}>{d.daysSilent}d</span>
                  <span style={{flex:1,fontWeight:600}}>{d.name}</span>
                  <span style={{color:'#9a9590',fontSize:11}}>{d.city}, {d.state}</span>
                  <span style={{fontFamily:"'JetBrains Mono'",fontSize:10}}>{fmt(d.totalSales)}</span>
                </div>
              ))}
            </div>
          </>)}

          <SH>Distributor Intelligence</SH>
          <Card>
            <select onChange={async e=>{if(!e.target.value){setDistList(null);return;}const d=await q('distributors',{state:e.target.value,...dp});setDistList(Array.isArray(d)?d:[]);}} style={{padding:'8px 14px',border:'1.5px solid #e8e5e0',borderRadius:8,fontSize:13,marginBottom:14,minWidth:200}}>
              <option value="">— Select State —</option>
              {states.states.map(s=><option key={s.name} value={s.name}>{s.name}</option>)}
            </select>
            {distList?.length>0 && (<>
              <ResponsiveContainer width="100%" height={Math.max(300,Math.min(distList.length,20)*28)}>
                <BarChart data={distList.filter(d=>d.sales>0).slice(0,20)} layout="vertical">
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0ede8" />
                  <XAxis type="number" tickFormatter={fmt} tick={{fontSize:9}} />
                  <YAxis type="category" dataKey="name" width={180} tick={{fontSize:9}} />
                  <Tooltip formatter={v=>fmt(v)} />
                  <Bar dataKey="sales" radius={[0,4,4,0]}>{distList.filter(d=>d.sales>0).slice(0,20).map((_,i)=><Cell key={i} fill={C[i%C.length]+'cc'} />)}</Bar>
                </BarChart>
              </ResponsiveContainer>
              <DT columns={[{label:'#',render:(_,i)=>i+1},{label:'Distributor',bold:true,render:r=><span style={{cursor:'pointer',borderBottom:'1px dashed #9a9590'}} onClick={async()=>{setDDName(r.name);const d=await q('distDetail',{customerId:r.id,...dp});setDD(d);}}>{r.name}</span>},{label:'Type',render:r=><TagDIST type={r.type} />},{label:'City',key:'city'},{label:'Revenue',num:true,mono:true,render:r=>fmt(r.sales)},{label:'Orders',num:true,render:r=>r.orders},{label:'Active',num:true,render:r=>r.activeMonths},{label:'Last',num:true,render:r=>r.lastOrder||'—'},{label:'Days',num:true,color:r=>r.daysSinceLast>45?'#cc1e1e':r.daysSinceLast<=30?'#1a9e6a':'inherit',render:r=>r.daysSinceLast+'d'},{label:'Repeat',num:true,color:r=>(r.activeMonths/13*100)>=70?'#1a9e6a':(r.activeMonths/13*100)<40?'#cc1e1e':'inherit',render:r=>(r.activeMonths/13*100).toFixed(0)+'%'}]} data={distList} />
            </>)}
            {dd && (
              <div style={{background:'#faf9f7',border:'1.5px solid #e8e5e0',borderRadius:10,padding:20,marginTop:14}}>
                <div style={{fontFamily:"'Bebas Neue'",fontSize:18,letterSpacing:'.06em',marginBottom:12}}>{ddName}</div>
                <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:18}}>
                  <div><CardHead>Monthly Sales</CardHead>
                    <ResponsiveContainer width="100%" height={200}>
                      <BarChart data={dd.monthly?.map(m=>({month:fM(m.month),Sales:m.sales}))}>
                        <XAxis dataKey="month" tick={{fontSize:9}} /><YAxis tickFormatter={fmt} tick={{fontSize:9}} />
                        <Tooltip formatter={v=>fmt(v)} /><Bar dataKey="Sales" fill="#cc1e1ecc" radius={[4,4,0,0]} />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                  <div><CardHead>Product Mix</CardHead>
                    <ResponsiveContainer width="100%" height={200}>
                      <PieChart><Pie data={dd.products?.slice(0,6).map(p=>({name:p.name,value:p.sales}))} cx="50%" cy="50%" innerRadius={30} outerRadius={70} dataKey="value">
                        {dd.products?.slice(0,6).map((_,i)=><Cell key={i} fill={C[i]} />)}
                      </Pie><Tooltip formatter={v=>fmt(v)} /></PieChart>
                    </ResponsiveContainer>
                  </div>
                </div>
              </div>
            )}
          </Card>
        </>)}

        {/* ═══ TAB 3: PRODUCT VIEW ═══ */}
        {tab==='product' && (<>
          <SH>Product Performance View</SH>
          <Card>
            <div style={{display:'flex',gap:10,alignItems:'center',flexWrap:'wrap',marginBottom:14}}>
              <span style={{fontSize:10,fontWeight:700,letterSpacing:'.08em',textTransform:'uppercase',color:'#9a9590'}}>State</span>
              <select value={pvState} onChange={async e=>{setPvState(e.target.value);const d=await q('productView',{state:e.target.value,...dp});setPvData(d);}} style={{padding:'8px 14px',border:'1.5px solid #e8e5e0',borderRadius:8,fontSize:13,minWidth:180}}>
                <option value="ALL">All States</option>
                {states?.states?.map(s=><option key={s.name} value={s.name}>{s.name}</option>)}
              </select>
            </div>
            <div style={{fontSize:10,fontWeight:700,letterSpacing:'.08em',textTransform:'uppercase',color:'#9a9590',marginBottom:8}}>Select Products</div>
            <div style={{display:'flex',flexWrap:'wrap',gap:6,marginBottom:10}}>
              <button onClick={()=>setPvSel(['ALL'])} style={{padding:'6px 14px',border:'1.5px solid '+(pvSel.includes('ALL')?'#cc1e1e':'#e8e5e0'),borderRadius:20,fontSize:11,fontWeight:600,cursor:'pointer',background:pvSel.includes('ALL')?'#cc1e1e':'#fff',color:pvSel.includes('ALL')?'#fff':'#cc1e1e'}}>All Products</button>
              {allProd.map(p=>(
                <button key={p} onClick={()=>{
                  if(pvSel.includes('ALL')) setPvSel([p]);
                  else if(pvSel.includes(p)){const n=pvSel.filter(x=>x!==p);setPvSel(n.length?n:['ALL']);}
                  else setPvSel([...pvSel,p]);
                }} style={{padding:'6px 14px',border:'1.5px solid '+(pvSel.includes(p)?'#0f0f0f':'#e8e5e0'),borderRadius:20,fontSize:11,fontWeight:600,cursor:'pointer',background:pvSel.includes(p)?'#0f0f0f':'#fff',color:pvSel.includes(p)?'#fff':'#0f0f0f'}}>{p}</button>
              ))}
            </div>
          </Card>

          {pvData?.products && (()=>{
            const fp = pvSel.includes('ALL') ? pvData.products : pvData.products.filter(p=>pvSel.includes(p.name));
            const fs = pvSel.includes('ALL') ? (pvData.summary||[]) : (pvData.summary||[]).filter(p=>pvSel.includes(p.name));
            return (<>
              <Card>
                <ResponsiveContainer width="100%" height={380}>
                  <LineChart data={buildPivot(fp.slice(0,12),pvData.months)}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f0ede8" />
                    <XAxis dataKey="month" tick={{fontSize:9}} /><YAxis tickFormatter={fmt} tick={{fontSize:9}} />
                    <Tooltip content={<CTooltip />} /><Legend wrapperStyle={{fontSize:9}} />
                    {fp.slice(0,12).map((p,i)=><Line key={p.name} type="monotone" dataKey={p.name} stroke={C[i%C.length]} strokeWidth={2} dot={{r:3}} />)}
                  </LineChart>
                </ResponsiveContainer>
              </Card>
              <Card>
                <DT columns={[{label:'#',render:(_,i)=>i+1},{label:'Product',key:'name',bold:true},{label:'Revenue',num:true,mono:true,bold:true,render:r=>fmt(r.total)},{label:'Qty',num:true,mono:true,render:r=>{const s=fs.find(x=>x.name===r.name);return s?Math.round(s.qty).toLocaleString():'';}},{label:'Invoices',num:true,render:r=>{const s=fs.find(x=>x.name===r.name);return s?.invoices||'';}},{label:'Share',num:true,render:r=>{const t=fp.reduce((s,p)=>s+p.total,0);return t>0?(r.total/t*100).toFixed(1)+'%':'';}},...pvData.months.map((m,mi)=>({label:fM(m),num:true,mono:true,render:r=>{const v=r.monthly[mi];return v>0?fmt(v):'—';}})),{label:'Trend',num:true,render:r=>{const m=r.monthly;return m.length>=2?pctArrow(m[m.length-1],m[m.length-2]):null;}}]} data={fp} />
              </Card>
            </>);
          })()}
        </>)}

        {/* ═══ TAB 4: DISTRIBUTOR VIEW ═══ */}
        {tab==='distributor' && dvData && (<>
          <SH>Distributor View</SH>
          <Card>
            <div style={{display:'flex',gap:10,alignItems:'center',marginBottom:14}}>
              <span style={{fontSize:10,fontWeight:700,letterSpacing:'.08em',textTransform:'uppercase',color:'#9a9590'}}>State</span>
              <select value={dvState} onChange={async e=>{setDvState(e.target.value);const d=await q('distView',{state:e.target.value,dateFrom:'2025-04-01',dateTo:'2026-03-31'});setDvData(d);}} style={{padding:'8px 14px',border:'1.5px solid #e8e5e0',borderRadius:8,fontSize:13,minWidth:180}}>
                <option value="ALL">All States</option>
                {states?.states?.map(s=><option key={s.name} value={s.name}>{s.name}</option>)}
              </select>
            </div>
          </Card>

          {dvData.distributors?.length>0 && (
            <Card><CardHead>1. Monthly Sales by Distributor</CardHead>
              <ResponsiveContainer width="100%" height={340}>
                <BarChart data={buildPivot(dvData.distributors.slice(0,8),dvData.months)}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0ede8" />
                  <XAxis dataKey="month" tick={{fontSize:9}} /><YAxis tickFormatter={fmt} tick={{fontSize:9}} />
                  <Tooltip content={<CTooltip />} /><Legend wrapperStyle={{fontSize:9}} />
                  {dvData.distributors.slice(0,8).map((d,i)=><Bar key={d.name} dataKey={d.name} stackId="a" fill={C[i%C.length]} />)}
                </BarChart>
              </ResponsiveContainer>
              <DT columns={[{label:'#',render:(_,i)=>i+1},{label:'Distributor',key:'name',bold:true},{label:'Total',num:true,mono:true,bold:true,render:r=>fmt(r.total)},...dvData.months.map((m,mi)=>({label:fM(m),num:true,mono:true,render:r=>{const v=r.monthly[mi];return v>0?fmt(v):'—';}})),{label:'Trend',num:true,render:r=>{const m=r.monthly;return m.length>=2?pctArrow(m[m.length-1],m[m.length-2]):null;}}]} data={dvData.distributors.slice(0,30)} />
            </Card>
          )}

          {dvData.summary?.length>0 && (
            <Card><CardHead>2. Orders & Gap Analysis</CardHead>
              <DT columns={[{label:'#',render:(_,i)=>i+1},{label:'Distributor',key:'name',bold:true},{label:'Type',render:r=><TagDIST type={r.type} />},{label:'City',key:'city'},{label:'Orders',num:true,render:r=>r.orders},{label:'Revenue',num:true,mono:true,render:r=>fmt(r.sales)},{label:'Avg Order',num:true,mono:true,render:r=>r.orders>0?fmt(r.sales/r.orders):'—'},{label:'Avg Gap',num:true,color:r=>r.avgGap>0&&r.avgGap<=30?'#1a9e6a':r.avgGap>90?'#cc1e1e':'inherit',render:r=>r.avgGap>0?r.avgGap.toFixed(0)+'d':'—'},{label:'Min',num:true,render:r=>r.minGap>0?r.minGap+'d':'—'},{label:'Max',num:true,render:r=>r.maxGap>0?r.maxGap+'d':'—'},{label:'Since Last',num:true,color:r=>r.daysSinceLast>45?'#cc1e1e':r.daysSinceLast<=30?'#1a9e6a':'inherit',render:r=>r.daysSinceLast+'d'}]} data={dvData.summary} />
            </Card>
          )}

          {dvData.regularity && (
            <Card><CardHead>3. Regular Ordering Distributors</CardHead>
              <div style={{display:'flex',gap:10,flexWrap:'wrap',marginBottom:10}}>
                {[60,90,120,180,270,360].map(b=>(
                  <div key={b} style={{padding:'10px 20px',borderRadius:20,fontSize:13,fontWeight:600,background:(dvData.regularity['d'+b]||0)>0?'#1a9e6a':'#e8e5e0',color:(dvData.regularity['d'+b]||0)>0?'#fff':'#9a9590'}}>
                    <strong>{dvData.regularity['d'+b]||0}</strong> within <strong>{b}</strong> days
                  </div>
                ))}
              </div>
              <div style={{fontSize:11,color:'#9a9590'}}>Based on average gap between orders. Distributors with 2+ orders counted.</div>
            </Card>
          )}

          <Card><CardHead>4. Distributor Orders by Product</CardHead>
            <div style={{display:'flex',gap:10,alignItems:'flex-start',marginBottom:14}}>
              <select multiple value={dvPF} onChange={e=>setDvPF(Array.from(e.target.selectedOptions,o=>o.value))} style={{minWidth:260,height:100,fontSize:11,border:'1.5px solid #e8e5e0',borderRadius:8,padding:4}}>
                {allProd.map(p=><option key={p} value={p}>{p}</option>)}
              </select>
              <button onClick={async()=>{if(!dvPF.length)return;const d=await q('distOrdersByProduct',{state:dvState,products:dvPF.join('|'),dateFrom:'2025-04-01',dateTo:'2026-03-31'});setDvPD(d);}} style={{padding:'8px 18px',border:'none',borderRadius:8,background:'#0f0f0f',color:'#fff',fontWeight:700,cursor:'pointer',fontSize:12}}>Apply</button>
            </div>
            {dvPD?.distributors?.length>0 && (<>
              <ResponsiveContainer width="100%" height={340}>
                <BarChart data={buildPivot(dvPD.distributors.slice(0,10),dvPD.months)}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0ede8" />
                  <XAxis dataKey="month" tick={{fontSize:9}} /><YAxis tickFormatter={fmt} tick={{fontSize:9}} />
                  <Tooltip content={<CTooltip />} /><Legend wrapperStyle={{fontSize:9}} />
                  {dvPD.distributors.slice(0,10).map((d,i)=><Bar key={d.name} dataKey={d.name} stackId="a" fill={C[i%C.length]} />)}
                </BarChart>
              </ResponsiveContainer>
              <DT columns={[{label:'#',render:(_,i)=>i+1},{label:'Distributor',key:'name',bold:true},{label:'Total',num:true,mono:true,bold:true,render:r=>fmt(r.total)},...dvPD.months.map((m,mi)=>({label:fM(m),num:true,mono:true,render:r=>{const v=r.monthly[mi];return v>0?fmt(v):'—';}}))]} data={dvPD.distributors} />
            </>)}
          </Card>
        </>)}
      </div>
    </div>
  );
}

ReactDOM.render(<App />, document.getElementById('analytics-root'));
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
