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
<script src="https://cdnjs.cloudflare.com/ajax/libs/babel-standalone/7.23.9/babel.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js"></script>
<style>
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:#f7f5f2;color:#0f0f0f;-webkit-font-smoothing:antialiased;}
</style>
</head>
<body>
<form id="form1" runat="server">
<div id="root"></div>
<script type="text/babel">
const {useState, useEffect, useRef, useCallback} = React;
const API = 'FINAnalyticsAPI.ashx';
const CL = ['#cc1e1e','#1a9e6a','#1e5fcc','#d68b00','#7c3aed','#e67e22','#16a085','#8e44ad','#2c3e50','#f39c12','#c0392b','#27ae60','#3498db','#d35400','#7f8c8d'];

const fmt = v => {
  if(!v&&v!==0) return '₹0';
  if(v>=1e7) return '₹'+(v/1e7).toFixed(1)+'Cr';
  if(v>=1e5) return '₹'+(v/1e5).toFixed(1)+'L';
  if(v>=1e3) return '₹'+(v/1e3).toFixed(1)+'K';
  return '₹'+Math.round(v).toLocaleString('en-IN');
};
const fM = ym => {
  if(!ym||ym.length<7) return ym||'';
  const mo=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
  const p=ym.split('-');
  return mo[parseInt(p[1])-1]+' '+p[0].slice(2);
};
const q = async (action,params={}) => {
  let url=API+'?action='+action;
  Object.entries(params).forEach(([k,v])=>{if(v)url+='&'+k+'='+encodeURIComponent(v);});
  return (await fetch(url)).json();
};
const pctArr = (c,p) => {
  if(!p||p===0) return null;
  const v=((c-p)/p*100).toFixed(0);
  return React.createElement('span',{style:{color:v>=0?'#1a9e6a':'#cc1e1e',fontWeight:700}},(v>=0?'▲':'▼')+' '+Math.abs(v)+'%');
};

// ── Chart.js React wrapper ──
function CChart({type,data,options,height=300}) {
  const ref = useRef(null);
  const chart = useRef(null);
  useEffect(() => {
    if(!ref.current) return;
    if(chart.current) chart.current.destroy();
    const ctx = ref.current.getContext('2d');
    const dfl = {responsive:true,maintainAspectRatio:false,
      plugins:{legend:{position:'bottom',labels:{boxWidth:10,padding:8,font:{family:"'DM Sans'",size:10}}},
        tooltip:{backgroundColor:'#0f0f0f',titleFont:{family:"'DM Sans'",size:12,weight:'bold'},bodyFont:{family:"'JetBrains Mono'",size:10},padding:12,cornerRadius:8,callbacks:{label:ctx=>ctx.dataset.label+': '+fmt(ctx.parsed.y||ctx.parsed||0)}}},
      scales:type==='doughnut'||type==='pie'?{}:{y:{ticks:{callback:v=>fmt(v),font:{family:"'JetBrains Mono'",size:9}},grid:{color:'#f0ede8'}},x:{ticks:{font:{size:9}},grid:{display:false}}}};
    chart.current = new Chart(ctx,{type,data,options:Object.assign({},dfl,options||{})});
    return () => { if(chart.current) chart.current.destroy(); };
  },[type,JSON.stringify(data),JSON.stringify(options)]);
  return React.createElement('div',{style:{height}},React.createElement('canvas',{ref}));
}

// ── UI Components ──
const KPI = ({value,label,color='#9a9590',delta}) => (
  <div style={{flex:1,minWidth:140,background:'#fff',borderRadius:10,padding:'16px 18px',borderLeft:'4px solid '+color,boxShadow:'0 1px 4px rgba(0,0,0,.05)'}}>
    <div style={{fontFamily:"'Bebas Neue'",fontSize:28,lineHeight:1,letterSpacing:'.03em'}}>{value}</div>
    <div style={{fontSize:9,fontWeight:700,letterSpacing:'.1em',textTransform:'uppercase',color:'#9a9590',marginTop:4}}>{label}</div>
    {delta!==undefined&&delta!==null&&<div style={{fontSize:11,fontWeight:700,marginTop:3,color:delta>=0?'#1a9e6a':'#cc1e1e'}}>{delta>=0?'▲':'▼'} {Math.abs(delta).toFixed(0)}%</div>}
  </div>
);
const SH = ({children,badge}) => (
  <div style={{fontFamily:"'Bebas Neue'",fontSize:20,letterSpacing:'.08em',margin:'28px 0 12px',paddingBottom:6,borderBottom:'3px solid #0f0f0f',display:'flex',alignItems:'baseline',gap:10}}>
    {children}{badge&&<span style={{fontFamily:"'DM Sans'",fontSize:10,fontWeight:700,background:'#cc1e1e',color:'#fff',padding:'3px 10px',borderRadius:20}}>{badge}</span>}
  </div>
);
const Card = ({children,style}) => <div style={{background:'#fff',borderRadius:10,padding:20,marginBottom:18,boxShadow:'0 1px 4px rgba(0,0,0,.05)',...style}}>{children}</div>;
const CH = ({children}) => <div style={{fontFamily:"'Bebas Neue'",fontSize:14,color:'#9a9590',letterSpacing:'.06em',marginBottom:10}}>{children}</div>;
const Tag = ({type}) => <span style={{fontSize:9,fontWeight:700,padding:'2px 7px',borderRadius:4,background:type==='DI'?'#eafaf1':'#ebf5fb',color:type==='DI'?'#1a9e6a':'#1e5fcc'}}>{type}</span>;

const DT = ({cols,data,maxH=450}) => (
  <div style={{overflow:'auto',maxHeight:maxH,border:'1px solid #e8e5e0',borderRadius:8}}>
    <table style={{width:'100%',borderCollapse:'collapse',fontSize:11}}>
      <thead><tr>{cols.map((c,i)=><th key={i} style={{fontSize:9,fontWeight:700,letterSpacing:'.1em',textTransform:'uppercase',color:'#9a9590',padding:'10px 8px',textAlign:c.n?'right':'left',borderBottom:'2px solid #e8e5e0',background:'#faf9f7',position:'sticky',top:0,zIndex:1}}>{c.l}</th>)}</tr></thead>
      <tbody>{data.map((r,ri)=><tr key={ri} style={{background:ri%2?'#faf9f7':'transparent'}}>{cols.map((c,ci)=><td key={ci} style={{padding:'7px 8px',borderBottom:'1px solid #f2f0ed',textAlign:c.n?'right':'left',fontFamily:c.m?"'JetBrains Mono'":"inherit",fontSize:c.m?10:11,fontWeight:c.b?700:400,color:c.c?c.c(r):'inherit'}}>{c.r?c.r(r,ri):r[c.k]}</td>)}</tr>)}</tbody>
    </table>
  </div>
);

const buildPivot = (items,months) => {
  if(!items||!months) return {labels:[],datasets:[]};
  return {
    labels:months.map(fM),
    datasets:items.slice(0,10).map((it,i)=>({label:it.name,data:it.monthly,backgroundColor:CL[i%CL.length]+'cc',borderColor:CL[i%CL.length],borderWidth:1.5,fill:false,tension:.3,pointRadius:3}))
  };
};

const TABS = [{id:'all',l:'All Data'},{id:'fy',l:'FY 2025-26'},{id:'product',l:'Product View'},{id:'distributor',l:'Distributor View'}];

// ── FY helpers ──
const getFYList = () => {
  const now = new Date();
  const curFYStart = now.getMonth() >= 3 ? now.getFullYear() : now.getFullYear() - 1;
  const list = [];
  for (let y = curFYStart; y >= 2023; y--) {
    list.push({id:'fy-'+y, label:'FY '+(y%100)+'-'+((y+1)%100), dateFrom:y+'-04-01', dateTo:(y+1)+'-03-31'});
  }
  return list;
};
const FY_LIST = getFYList();
const FY_DEFAULT = FY_LIST.length > 0 ? FY_LIST[0] : null;

// ── DateRangeSelector ──
function DateRangeSelector({value,onChange}) {
  const mode = value.mode || 'fy';
  const selFY = value.fyId || (FY_DEFAULT ? FY_DEFAULT.id : '');
  const custFrom = value.custFrom || '';
  const custTo = value.custTo || '';

  const setMode = m => {
    if(m==='fy') {
      const fy = FY_LIST.find(f=>f.id===selFY) || FY_DEFAULT;
      if(fy) onChange({mode:'fy', fyId:fy.id, dateFrom:fy.dateFrom, dateTo:fy.dateTo, custFrom, custTo});
    } else if(m==='custom') {
      onChange({mode:'custom', fyId:selFY, dateFrom:custFrom, dateTo:custTo, custFrom, custTo});
    } else {
      onChange({mode:'all', fyId:selFY, dateFrom:'', dateTo:'', custFrom, custTo});
    }
  };

  const pillStyle = (active) => ({padding:'7px 16px',border:'1.5px solid '+(active?'#0f0f0f':'#e8e5e0'),borderRadius:20,fontSize:11,fontWeight:700,cursor:'pointer',background:active?'#0f0f0f':'#fff',color:active?'#fff':'#0f0f0f',transition:'all .15s',letterSpacing:'.03em'});

  return (
    <div style={{display:'flex',gap:8,alignItems:'center',flexWrap:'wrap',marginBottom:14}}>
      <span style={{fontSize:10,fontWeight:700,letterSpacing:'.08em',textTransform:'uppercase',color:'#9a9590'}}>Period</span>
      <button type="button" onClick={()=>setMode('all')} style={pillStyle(mode==='all')}>All Time</button>
      {FY_LIST.map(fy=>(
        <button key={fy.id} type="button" onClick={()=>{onChange({mode:'fy',fyId:fy.id,dateFrom:fy.dateFrom,dateTo:fy.dateTo,custFrom,custTo});}} style={pillStyle(mode==='fy'&&selFY===fy.id)}>{fy.label}</button>
      ))}
      <button type="button" onClick={()=>setMode('custom')} style={pillStyle(mode==='custom')}>Custom</button>
      {mode==='custom'&&<>
        <input type="date" value={custFrom} onChange={e=>{const v=e.target.value;onChange({mode:'custom',fyId:selFY,dateFrom:v,dateTo:custTo,custFrom:v,custTo});}} style={{padding:'6px 10px',border:'1.5px solid #e8e5e0',borderRadius:8,fontSize:12,fontFamily:"'DM Sans'"}} />
        <span style={{color:'#9a9590',fontSize:11}}>to</span>
        <input type="date" value={custTo} onChange={e=>{const v=e.target.value;onChange({mode:'custom',fyId:selFY,dateFrom:custFrom,dateTo:v,custFrom,custTo:v});}} style={{padding:'6px 10px',border:'1.5px solid #e8e5e0',borderRadius:8,fontSize:12,fontFamily:"'DM Sans'"}} />
        <button type="button" onClick={()=>{if(custFrom&&custTo)onChange({mode:'custom',fyId:selFY,dateFrom:custFrom,dateTo:custTo,custFrom,custTo});}} style={{padding:'7px 16px',border:'none',borderRadius:8,background:'#cc1e1e',color:'#fff',fontWeight:700,cursor:'pointer',fontSize:11}}>Apply</button>
      </>}
    </div>
  );
}

function App() {
  const [tab,setTab] = useState('all');
  const [loading,setLoading] = useState(true);
  const [ov,setOv] = useState(null);
  const [trend,setTrend] = useState([]);
  const [sts,setSts] = useState(null);
  const [prods,setProds] = useState([]);
  const [alerts,setAlerts] = useState(null);
  const [showR,setShowR] = useState(false);
  const [cityD,setCityD] = useState(null);
  const [distL,setDistL] = useState(null);
  const [dd,setDD] = useState(null);
  const [ddN,setDDN] = useState('');
  const [pvSt,setPvSt] = useState('ALL');
  const [pvD,setPvD] = useState(null);
  const [pvSel,setPvSel] = useState(['ALL']);
  const [allPN,setAllPN] = useState([]);
  const [dvSt,setDvSt] = useState('ALL');
  const [dvD,setDvD] = useState(null);
  const [dvPF,setDvPF] = useState([]);
  const [dvPD,setDvPD] = useState(null);

  // Date range state for Product View & Distributor View
  const initDR = () => FY_DEFAULT ? {mode:'fy',fyId:FY_DEFAULT.id,dateFrom:FY_DEFAULT.dateFrom,dateTo:FY_DEFAULT.dateTo,custFrom:'',custTo:''} : {mode:'all',fyId:'',dateFrom:'',dateTo:'',custFrom:'',custTo:''};
  const [pvDR,setPvDR] = useState(initDR);
  const [dvDR,setDvDR] = useState(initDR);
  const pvDateRef = useRef(pvDR);
  const dvDateRef = useRef(dvDR);

  const dp = (tab==='fy')?{dateFrom:'2025-04-01',dateTo:'2026-03-31'}:{};

  const loadMain = async () => {
    setLoading(true); setCityD(null); setDistL(null); setDD(null);
    try {
      const [o,t,s,p,a] = await Promise.all([q('overview',dp),q('monthlyTrend',dp),q('stateBreakdown',dp),q('topProducts',dp),q('alerts')]);
      setOv(o); setTrend(Array.isArray(t)?t:[]); setSts(s&&s.states?s:{months:[],states:[]}); setProds(Array.isArray(p)?p:[]); setAlerts(a);
    } catch(e){console.error(e);}
    setLoading(false);
  };

  const loadPV = async (dr) => {
    setLoading(true);
    const dateP = dr || pvDateRef.current;
    const pp = dateP.dateFrom ? {dateFrom:dateP.dateFrom,dateTo:dateP.dateTo} : {};
    const [n,s] = await Promise.all([allPN.length?Promise.resolve(allPN):q('productList'),sts?Promise.resolve(sts):q('stateBreakdown',pp)]);
    if(Array.isArray(n)) setAllPN(n); if(!sts&&s) setSts(s);
    const d = await q('productView',{state:pvSt,...pp}); setPvD(d);
    setLoading(false);
  };

  const loadDV = async (dr) => {
    setLoading(true);
    if(!sts){const s=await q('stateBreakdown');setSts(s);}
    if(!allPN.length){const n=await q('productList');if(Array.isArray(n))setAllPN(n);}
    const dateP = dr || dvDateRef.current;
    const pp = dateP.dateFrom ? {dateFrom:dateP.dateFrom,dateTo:dateP.dateTo} : {};
    const d=await q('distView',{state:dvSt,...pp}); setDvD(d);
    setLoading(false);
  };

  useEffect(()=>{
    if(tab==='all'||tab==='fy') loadMain();
    else if(tab==='product') loadPV(pvDR);
    else if(tab==='distributor') loadDV(dvDR);
  },[tab]);

  if(loading) return <div style={{display:'flex',alignItems:'center',justifyContent:'center',height:'60vh',color:'#9a9590'}}><div style={{textAlign:'center'}}><div style={{fontSize:24,fontFamily:"'Bebas Neue'",letterSpacing:'.1em',marginBottom:8}}>Loading Analytics</div><div style={{fontSize:12}}>Fetching data...</div></div></div>;

  return (
    <div>
      <div style={{background:'#0f0f0f',padding:'12px 28px',display:'flex',alignItems:'center',gap:16}}>
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" style={{height:28,background:'#fff',borderRadius:4,padding:'2px 6px'}} onError={e=>{e.target.style.display='none'}} />
        <span style={{fontFamily:"'Bebas Neue'",color:'#fff',fontSize:18,letterSpacing:'.1em'}}>FINANCE</span>
        <span style={{fontFamily:"'Bebas Neue'",color:'rgba(255,255,255,.4)',fontSize:14,letterSpacing:'.08em'}}>SALES ANALYTICS</span>
        <div style={{flex:1}} />
        <a href="FINHome.aspx" style={{color:'rgba(255,255,255,.6)',fontSize:12,textDecoration:'none'}}>← FIN Home</a>
        <a href="/StockApp/ERPHome.aspx" style={{color:'rgba(255,255,255,.6)',fontSize:12,textDecoration:'none',marginLeft:12}}>← ERP</a>
        <a href="FINLogout.aspx" style={{color:'rgba(255,255,255,.6)',fontSize:12,textDecoration:'none',marginLeft:12}}>Sign Out</a>
      </div>
      <div style={{background:'#fff',borderBottom:'3px solid #cc1e1e',padding:'20px 40px',display:'flex',alignItems:'center',gap:14}}>
        <span style={{fontSize:28}}>📊</span>
        <div><div style={{fontFamily:"'Bebas Neue'",fontSize:28,letterSpacing:'.07em'}}>SALES <span style={{color:'#cc1e1e'}}>ANALYTICS</span></div>
        <div style={{fontSize:12,color:'#9a9590'}}>Revenue trends, product performance, distributor intelligence</div></div>
      </div>

      <div style={{maxWidth:1300,margin:'0 auto',padding:'24px 24px 80px'}}>
        <div style={{display:'flex',borderRadius:10,overflow:'hidden',border:'2px solid #0f0f0f',marginBottom:24}}>
          {TABS.map(t=><button type="button" key={t.id} onClick={()=>setTab(t.id)} style={{flex:1,padding:'13px 10px',border:'none',fontFamily:"'Bebas Neue'",fontSize:14,letterSpacing:'.08em',cursor:'pointer',background:tab===t.id?'#0f0f0f':'#fff',color:tab===t.id?'#fff':'#0f0f0f',transition:'all .15s'}}>{t.l}</button>)}
        </div>

        {(tab==='all'||tab==='fy')&&ov&&<>
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
              <input type="checkbox" checked={showR} onChange={()=>setShowR(!showR)} style={{width:16,height:16,accentColor:'#1a9e6a'}} /> Show Receipts
            </label>
            <CChart type="line" height={300} data={{
              labels:trend.map(t=>fM(t.month)),
              datasets:[
                {label:'Sales Revenue',data:trend.map(t=>t.sales),borderColor:'#cc1e1e',backgroundColor:'rgba(204,30,30,.06)',fill:true,tension:.35,pointRadius:4,pointBackgroundColor:'#cc1e1e',borderWidth:2.5},
                ...(showR?[{label:'Receipts',data:trend.map(t=>t.receipts||0),borderColor:'#1a9e6a',backgroundColor:'rgba(26,158,106,.06)',fill:true,tension:.35,pointRadius:3,pointBackgroundColor:'#1a9e6a',borderWidth:2.5,borderDash:[6,3]}]:[])
              ]
            }} />
          </Card>

          <SH>State Performance</SH>
          <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:18,marginBottom:18}}>
            <Card><CH>Revenue Share</CH>
              <CChart type="doughnut" height={260} data={{labels:sts.states.map(s=>s.name),datasets:[{data:sts.states.map(s=>s.total),backgroundColor:CL.slice(0,sts.states.length),borderWidth:0}]}} options={{cutout:'55%',plugins:{tooltip:{callbacks:{label:ctx=>ctx.label+': '+fmt(ctx.parsed)}}}}} />
            </Card>
            <Card><CH>Monthly by State</CH>
              <CChart type="bar" height={260} data={buildPivot(sts.states,sts.months)} options={{scales:{x:{stacked:true},y:{stacked:true,ticks:{callback:v=>fmt(v)}}}}} />
            </Card>
          </div>

          <Card><CH>State × Month</CH>
            <DT cols={[{l:'#',r:(_,i)=>i+1},{l:'State',k:'name',b:1},{l:'Total',n:1,m:1,b:1,r:r=>fmt(r.total)},...sts.months.map((m,mi)=>({l:fM(m),n:1,m:1,r:r=>{const v=r.monthly[mi];return v>0?fmt(v):'—';}})),{l:'Trend',n:1,r:r=>{const m=r.monthly;return m.length>=2?pctArr(m[m.length-1],m[m.length-2]):null;}}]} data={sts.states} />
          </Card>

          <Card><CH>Drill Down: State → City</CH>
            <select onChange={async e=>{if(!e.target.value){setCityD(null);return;}const d=await q('cityBreakdown',{state:e.target.value,...dp});setCityD(d);}} style={{padding:'8px 14px',border:'1.5px solid #e8e5e0',borderRadius:8,fontSize:13,marginBottom:14,minWidth:200}}>
              <option value="">— Select State —</option>
              {sts.states.map(s=><option key={s.name} value={s.name}>{s.name} ({fmt(s.total)})</option>)}
            </select>
            {cityD?.cities?.length>0&&<>
              <CChart type="bar" height={300} data={buildPivot(cityD.cities,cityD.months)} options={{scales:{x:{stacked:true},y:{stacked:true,ticks:{callback:v=>fmt(v)}}}}} />
              <div style={{marginTop:14}}><DT cols={[{l:'#',r:(_,i)=>i+1},{l:'City',k:'name',b:1},{l:'Total',n:1,m:1,b:1,r:r=>fmt(r.total)},...cityD.months.map((m,mi)=>({l:fM(m),n:1,m:1,r:r=>{const v=r.monthly[mi];return v>0?fmt(v):'—';}}))]} data={cityD.cities} /></div>
            </>}
          </Card>

          <SH>Product Performance</SH>
          <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:18,marginBottom:18}}>
            <Card><CH>Top Products</CH>
              <CChart type="bar" height={320} data={{labels:prods.slice(0,10).map(p=>p.name.length>25?p.name.slice(0,22)+'...':p.name),datasets:[{data:prods.slice(0,10).map(p=>p.sales),backgroundColor:CL.slice(0,10).map(c=>c+'cc')}]}} options={{indexAxis:'y',plugins:{legend:{display:false}}}} />
            </Card>
            <Card><CH>Product Mix</CH>
              <CChart type="doughnut" height={320} data={{labels:prods.slice(0,8).map(p=>p.name),datasets:[{data:prods.slice(0,8).map(p=>p.sales),backgroundColor:CL.slice(0,8),borderWidth:0}]}} options={{cutout:'50%',plugins:{tooltip:{callbacks:{label:ctx=>ctx.label+': '+fmt(ctx.parsed)}}}}} />
            </Card>
          </div>
          <Card>
            <DT cols={[{l:'#',r:(_,i)=>i+1},{l:'Product',k:'name',b:1},{l:'Revenue',n:1,m:1,b:1,r:r=>fmt(r.sales)},{l:'Qty',n:1,m:1,r:r=>Math.round(r.qty).toLocaleString()},{l:'Invoices',n:1,r:r=>r.invoices},{l:'Customers',n:1,r:r=>r.customers},{l:'Share',n:1,r:r=>{const t=prods.reduce((s,p)=>s+p.sales,0);return t>0?(r.sales/t*100).toFixed(1)+'%':'';}}]} data={prods} />
          </Card>

          {alerts?.silentDistributors?.length>0&&<>
            <SH>⚠ Silent Distributors — 45+ Days</SH>
            <div style={{background:'#fff8f0',border:'1.5px solid #ffd6a0',borderRadius:10,padding:'16px 20px',marginBottom:18}}>
              {alerts.silentDistributors.map((d,i)=><div key={i} style={{display:'flex',alignItems:'center',gap:10,padding:'6px 0',borderBottom:i<alerts.silentDistributors.length-1?'1px solid #f5efe5':'none',fontSize:12}}>
                <span style={{fontFamily:"'JetBrains Mono'",fontSize:11,fontWeight:700,color:'#cc1e1e',minWidth:45}}>{d.daysSilent}d</span>
                <span style={{flex:1,fontWeight:600}}>{d.name}</span>
                <span style={{color:'#9a9590',fontSize:11}}>{d.city}, {d.state}</span>
                <span style={{fontFamily:"'JetBrains Mono'",fontSize:10}}>{fmt(d.totalSales)}</span>
              </div>)}
            </div>
          </>}

          <SH>Distributor Intelligence</SH>
          <Card>
            <select onChange={async e=>{if(!e.target.value){setDistL(null);return;}const d=await q('distributors',{state:e.target.value,...dp});setDistL(Array.isArray(d)?d:[]);setDD(null);}} style={{padding:'8px 14px',border:'1.5px solid #e8e5e0',borderRadius:8,fontSize:13,marginBottom:14,minWidth:200}}>
              <option value="">— Select State —</option>
              {sts.states.map(s=><option key={s.name} value={s.name}>{s.name}</option>)}
            </select>
            {distL?.length>0&&<>
              <CChart type="bar" height={Math.max(300,Math.min(distL.length,20)*28)} data={{labels:distL.filter(d=>d.sales>0).slice(0,20).map(d=>d.name.length>30?d.name.slice(0,27)+'...':d.name),datasets:[{data:distL.filter(d=>d.sales>0).slice(0,20).map(d=>d.sales),backgroundColor:distL.filter(d=>d.sales>0).slice(0,20).map((_,i)=>CL[i%CL.length]+'cc')}]}} options={{indexAxis:'y',plugins:{legend:{display:false}}}} />
              <div style={{marginTop:14}}><DT cols={[{l:'#',r:(_,i)=>i+1},{l:'Distributor',b:1,r:r=><span style={{cursor:'pointer',borderBottom:'1px dashed #9a9590'}} onClick={async()=>{setDDN(r.name);setDD(await q('distDetail',{customerId:r.id,...dp}));}}>{r.name}</span>},{l:'Type',r:r=><Tag type={r.type} />},{l:'City',k:'city'},{l:'Revenue',n:1,m:1,r:r=>fmt(r.sales)},{l:'Orders',n:1,r:r=>r.orders},{l:'Active',n:1,r:r=>r.activeMonths},{l:'Last',n:1,r:r=>r.lastOrder||'—'},{l:'Days',n:1,c:r=>r.daysSinceLast>45?'#cc1e1e':r.daysSinceLast<=30?'#1a9e6a':'inherit',r:r=>r.daysSinceLast+'d'},{l:'Repeat',n:1,c:r=>(r.activeMonths/13*100)>=70?'#1a9e6a':(r.activeMonths/13*100)<40?'#cc1e1e':'inherit',r:r=>(r.activeMonths/13*100).toFixed(0)+'%'}]} data={distL} /></div>
            </>}
            {dd&&<div style={{background:'#faf9f7',border:'1.5px solid #e8e5e0',borderRadius:10,padding:20,marginTop:14}}>
              <div style={{fontFamily:"'Bebas Neue'",fontSize:18,letterSpacing:'.06em',marginBottom:12}}>{ddN}</div>
              <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:18}}>
                <div><CH>Monthly Sales</CH><CChart type="bar" height={200} data={{labels:(dd.monthly||[]).map(m=>fM(m.month)),datasets:[{label:'Sales',data:(dd.monthly||[]).map(m=>m.sales),backgroundColor:'#cc1e1ecc',borderRadius:4}]}} options={{plugins:{legend:{display:false}}}} /></div>
                <div><CH>Product Mix</CH><CChart type="doughnut" height={200} data={{labels:(dd.products||[]).slice(0,6).map(p=>p.name),datasets:[{data:(dd.products||[]).slice(0,6).map(p=>p.sales),backgroundColor:CL.slice(0,6),borderWidth:0}]}} options={{cutout:'45%',plugins:{tooltip:{callbacks:{label:ctx=>ctx.label+': '+fmt(ctx.parsed)}}}}} /></div>
              </div>
            </div>}
          </Card>
        </>}

        {tab==='product'&&<>
          <SH>Product Performance View</SH>
          <Card>
            <DateRangeSelector value={pvDR} onChange={dr=>{setPvDR(dr);pvDateRef.current=dr;if(dr.mode!=='custom'){loadPV(dr);}}} />
            <div style={{display:'flex',gap:10,alignItems:'center',flexWrap:'wrap',marginBottom:14}}>
              <span style={{fontSize:10,fontWeight:700,letterSpacing:'.08em',textTransform:'uppercase',color:'#9a9590'}}>State</span>
              <select value={pvSt} onChange={async e=>{setPvSt(e.target.value);const pp=pvDR.dateFrom?{dateFrom:pvDR.dateFrom,dateTo:pvDR.dateTo}:{};setPvD(await q('productView',{state:e.target.value,...pp}));}} style={{padding:'8px 14px',border:'1.5px solid #e8e5e0',borderRadius:8,fontSize:13,minWidth:180}}>
                <option value="ALL">All States</option>
                {sts?.states?.map(s=><option key={s.name} value={s.name}>{s.name}</option>)}
              </select>
              <button type="button" onClick={()=>{pvDateRef.current=pvDR;loadPV(pvDR);}} style={{padding:'7px 16px',border:'none',borderRadius:8,background:'#0f0f0f',color:'#fff',fontWeight:700,cursor:'pointer',fontSize:11,letterSpacing:'.03em'}}>Refresh</button>
            </div>
            <div style={{fontSize:10,fontWeight:700,letterSpacing:'.08em',textTransform:'uppercase',color:'#9a9590',marginBottom:8}}>Select Products</div>
            <div style={{display:'flex',flexWrap:'wrap',gap:6,marginBottom:10}}>
              <button type="button" onClick={()=>setPvSel(['ALL'])} style={{padding:'6px 14px',border:'1.5px solid '+(pvSel.includes('ALL')?'#cc1e1e':'#e8e5e0'),borderRadius:20,fontSize:11,fontWeight:600,cursor:'pointer',background:pvSel.includes('ALL')?'#cc1e1e':'#fff',color:pvSel.includes('ALL')?'#fff':'#cc1e1e'}}>All Products</button>
              {allPN.map(p=><button type="button" key={p} onClick={()=>{if(pvSel.includes('ALL'))setPvSel([p]);else if(pvSel.includes(p)){const n=pvSel.filter(x=>x!==p);setPvSel(n.length?n:['ALL']);}else setPvSel([...pvSel,p]);}} style={{padding:'6px 14px',border:'1.5px solid '+(pvSel.includes(p)?'#0f0f0f':'#e8e5e0'),borderRadius:20,fontSize:11,fontWeight:600,cursor:'pointer',background:pvSel.includes(p)?'#0f0f0f':'#fff',color:pvSel.includes(p)?'#fff':'#0f0f0f'}}>{p}</button>)}
            </div>
          </Card>
          {pvD?.products&&(()=>{
            const fp=pvSel.includes('ALL')?pvD.products:pvD.products.filter(p=>pvSel.includes(p.name));
            const fs=pvSel.includes('ALL')?(pvD.summary||[]):(pvD.summary||[]).filter(p=>pvSel.includes(p.name));
            const cd={labels:(pvD.months||[]).map(fM),datasets:fp.slice(0,12).map((p,i)=>({label:p.name,data:p.monthly,borderColor:CL[i%CL.length],backgroundColor:'transparent',tension:.3,pointRadius:3,borderWidth:2,fill:false}))};
            return <>
              <Card><CChart type="line" height={380} data={cd} /></Card>
              <Card><DT cols={[{l:'#',r:(_,i)=>i+1},{l:'Product',k:'name',b:1},{l:'Revenue',n:1,m:1,b:1,r:r=>fmt(r.total)},{l:'Qty',n:1,m:1,r:r=>{const s=fs.find(x=>x.name===r.name);return s?Math.round(s.qty).toLocaleString():'';}},{l:'Invoices',n:1,r:r=>{const s=fs.find(x=>x.name===r.name);return s?.invoices||'';}},{l:'Share',n:1,r:r=>{const t=fp.reduce((s,p)=>s+p.total,0);return t>0?(r.total/t*100).toFixed(1)+'%':'';}},...(pvD.months||[]).map((m,mi)=>({l:fM(m),n:1,m:1,r:r=>{const v=r.monthly[mi];return v>0?fmt(v):'—';}})),{l:'Trend',n:1,r:r=>{const m=r.monthly;return m.length>=2?pctArr(m[m.length-1],m[m.length-2]):null;}}]} data={fp} /></Card>
            </>;
          })()}
        </>}

        {tab==='distributor'&&dvD&&<>
          <SH>Distributor View</SH>
          <Card>
            <DateRangeSelector value={dvDR} onChange={dr=>{setDvDR(dr);dvDateRef.current=dr;if(dr.mode!=='custom'){loadDV(dr);}}} />
            <div style={{display:'flex',gap:10,alignItems:'center',marginBottom:14}}>
              <span style={{fontSize:10,fontWeight:700,letterSpacing:'.08em',textTransform:'uppercase',color:'#9a9590'}}>State</span>
              <select value={dvSt} onChange={async e=>{setDvSt(e.target.value);const pp=dvDR.dateFrom?{dateFrom:dvDR.dateFrom,dateTo:dvDR.dateTo}:{};setDvD(await q('distView',{state:e.target.value,...pp}));setDvPD(null);}} style={{padding:'8px 14px',border:'1.5px solid #e8e5e0',borderRadius:8,fontSize:13,minWidth:180}}>
                <option value="ALL">All States</option>
                {sts?.states?.map(s=><option key={s.name} value={s.name}>{s.name}</option>)}
              </select>
              <button type="button" onClick={()=>{dvDateRef.current=dvDR;loadDV(dvDR);}} style={{padding:'7px 16px',border:'none',borderRadius:8,background:'#0f0f0f',color:'#fff',fontWeight:700,cursor:'pointer',fontSize:11,letterSpacing:'.03em'}}>Refresh</button>
            </div>
          </Card>

          {dvD.distributors?.length>0&&<Card><CH>1. Monthly Sales by Distributor</CH>
            <CChart type="bar" height={340} data={buildPivot(dvD.distributors,dvD.months)} options={{scales:{x:{stacked:true},y:{stacked:true,ticks:{callback:v=>fmt(v)}}}}} />
            <div style={{marginTop:14}}><DT cols={[{l:'#',r:(_,i)=>i+1},{l:'Distributor',k:'name',b:1},{l:'Total',n:1,m:1,b:1,r:r=>fmt(r.total)},...dvD.months.map((m,mi)=>({l:fM(m),n:1,m:1,r:r=>{const v=r.monthly[mi];return v>0?fmt(v):'—';}})),{l:'Trend',n:1,r:r=>{const m=r.monthly;return m.length>=2?pctArr(m[m.length-1],m[m.length-2]):null;}}]} data={dvD.distributors.slice(0,30)} /></div>
          </Card>}

          {dvD.summary?.length>0&&<Card><CH>2. Orders & Gap Analysis</CH>
            <DT cols={[{l:'#',r:(_,i)=>i+1},{l:'Distributor',k:'name',b:1},{l:'Type',r:r=><Tag type={r.type} />},{l:'City',k:'city'},{l:'Orders',n:1,r:r=>r.orders},{l:'Revenue',n:1,m:1,r:r=>fmt(r.sales)},{l:'Avg Order',n:1,m:1,r:r=>r.orders>0?fmt(r.sales/r.orders):'—'},{l:'Avg Gap',n:1,c:r=>r.avgGap>0&&r.avgGap<=30?'#1a9e6a':r.avgGap>90?'#cc1e1e':'inherit',r:r=>r.avgGap>0?r.avgGap.toFixed(0)+'d':'—'},{l:'Min',n:1,r:r=>r.minGap>0?r.minGap+'d':'—'},{l:'Max',n:1,r:r=>r.maxGap>0?r.maxGap+'d':'—'},{l:'Since',n:1,c:r=>r.daysSinceLast>45?'#cc1e1e':r.daysSinceLast<=30?'#1a9e6a':'inherit',r:r=>r.daysSinceLast+'d'}]} data={dvD.summary} />
          </Card>}

          {dvD.regularity&&<Card><CH>3. Regular Ordering Distributors</CH>
            <div style={{display:'flex',gap:10,flexWrap:'wrap',marginBottom:10}}>
              {[60,90,120,180,270,360].map(b=><div key={b} style={{padding:'10px 20px',borderRadius:20,fontSize:13,fontWeight:600,background:(dvD.regularity['d'+b]||0)>0?'#1a9e6a':'#e8e5e0',color:(dvD.regularity['d'+b]||0)>0?'#fff':'#9a9590'}}><strong>{dvD.regularity['d'+b]||0}</strong> within <strong>{b}</strong> days</div>)}
            </div>
            <div style={{fontSize:11,color:'#9a9590'}}>Based on average gap between orders. Distributors with 2+ orders counted.</div>
          </Card>}

          <Card><CH>4. Distributor Orders by Product</CH>
            <div style={{display:'flex',gap:10,alignItems:'flex-start',marginBottom:14}}>
              <select multiple value={dvPF} onChange={e=>setDvPF(Array.from(e.target.selectedOptions,o=>o.value))} style={{minWidth:260,height:100,fontSize:11,border:'1.5px solid #e8e5e0',borderRadius:8,padding:4}}>
                {allPN.map(p=><option key={p} value={p}>{p}</option>)}
              </select>
              <button type="button" onClick={async()=>{if(!dvPF.length)return;const pp=dvDR.dateFrom?{dateFrom:dvDR.dateFrom,dateTo:dvDR.dateTo}:{};setDvPD(await q('distOrdersByProduct',{state:dvSt,products:dvPF.join('|'),...pp}));}} style={{padding:'8px 18px',border:'none',borderRadius:8,background:'#0f0f0f',color:'#fff',fontWeight:700,cursor:'pointer',fontSize:12}}>Apply</button>
            </div>
            {dvPD?.distributors?.length>0&&<>
              <CChart type="bar" height={340} data={buildPivot(dvPD.distributors,dvPD.months)} options={{scales:{x:{stacked:true},y:{stacked:true,ticks:{callback:v=>fmt(v)}}}}} />
              <div style={{marginTop:14}}><DT cols={[{l:'#',r:(_,i)=>i+1},{l:'Distributor',k:'name',b:1},{l:'Total',n:1,m:1,b:1,r:r=>fmt(r.total)},...dvPD.months.map((m,mi)=>({l:fM(m),n:1,m:1,r:r=>{const v=r.monthly[mi];return v>0?fmt(v):'—';}}))]} data={dvPD.distributors} /></div>
            </>}
          </Card>
        </>}

      </div>
    </div>
  );
}

ReactDOM.render(<App />, document.getElementById('root'));
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
