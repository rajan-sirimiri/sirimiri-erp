<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HROrgChart.aspx.cs" Inherits="HRModule.HROrgChart" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri &mdash; Org Chart</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#0d9488;--accent-dark:#0f766e;--accent-light:#ccfbf1;--teal:#0f6e56;--warn:#f39c12;--danger:#c0392b;--success:#0f6e56;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
html,body{height:100%;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);overflow:hidden;}

nav{background:#1a1a1a;display:flex;align-items:center;padding:0 28px;height:52px;gap:6px;flex-shrink:0;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;margin-right:10px;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.1em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:14px;}
.nav-user{font-size:12px;color:#999;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}
.nav-link:hover{opacity:1;}
.nav-link.active{opacity:1;color:var(--accent-light);}

.page-header{background:var(--surface);border-bottom:2px solid var(--accent);padding:16px 40px;flex-shrink:0;}
.page-icon{font-size:24px;margin-bottom:2px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:26px;letter-spacing:.06em;line-height:1;}
.page-title span{color:var(--accent);}
.page-sub{font-size:11px;color:var(--text-muted);margin-top:4px;}

.shell{display:flex;flex-direction:column;height:100vh;}
.workspace{position:relative;flex:1;overflow:hidden;background:#f7f7f4;background-image:radial-gradient(circle, #e0ddd6 1px, transparent 1px);background-size:24px 24px;}

/* ----- Floating toolbar (top-left of canvas) ----- */
.toolbar{position:absolute;top:14px;left:14px;z-index:10;background:var(--surface);border:1px solid var(--border);border-radius:10px;box-shadow:0 4px 16px rgba(0,0,0,.06);padding:8px 10px;display:flex;align-items:center;gap:8px;}
.toolbar input[type=text]{font-family:inherit;font-size:13px;padding:6px 10px;border:1px solid var(--border);border-radius:6px;background:#fafafa;width:240px;outline:none;}
.toolbar input[type=text]:focus{border-color:var(--accent);background:#fff;}
.toolbar select{font-family:inherit;font-size:12px;padding:6px 8px;border:1px solid var(--border);border-radius:6px;background:#fafafa;outline:none;}
.tb-btn{border:1px solid var(--border);background:#fafafa;border-radius:6px;width:28px;height:28px;font-size:14px;cursor:pointer;display:inline-flex;align-items:center;justify-content:center;color:var(--text-muted);font-family:inherit;}
.tb-btn:hover{background:#fff;color:var(--accent-dark);border-color:var(--accent);}
.tb-divider{width:1px;height:22px;background:var(--border);}
.tb-stat{font-size:11px;color:var(--text-muted);padding:0 4px;font-variant-numeric:tabular-nums;}
.tb-stat b{color:var(--text);font-weight:600;}

/* ----- Floating legend (top-right) ----- */
.legend{position:absolute;top:14px;right:14px;z-index:10;background:var(--surface);border:1px solid var(--border);border-radius:10px;box-shadow:0 4px 16px rgba(0,0,0,.06);padding:10px 12px;font-size:11px;color:var(--text-muted);max-width:240px;}
.legend-title{font-family:'Bebas Neue',sans-serif;font-size:11px;letter-spacing:.12em;color:var(--accent);margin-bottom:6px;}
.legend-row{display:flex;align-items:center;gap:6px;margin-top:4px;}
.legend-swatch{width:14px;height:14px;border-radius:3px;flex-shrink:0;border:1px solid rgba(0,0,0,.08);}

/* ----- The SVG canvas itself ----- */
#chart{width:100%;height:100%;cursor:grab;}
#chart:active{cursor:grabbing;}

/* ----- Node visuals ----- */
.node{cursor:pointer;}
.node-rect{stroke:var(--border);stroke-width:1;fill:#fff;rx:8;ry:8;transition:stroke .15s,filter .15s;}
.node:hover .node-rect{stroke:var(--accent);filter:drop-shadow(0 2px 8px rgba(13,148,136,.18));}
.node.dim .node-rect{opacity:.3;}
.node.match .node-rect{stroke:var(--accent);stroke-width:2;}
.node.selected .node-rect{stroke:var(--accent-dark);stroke-width:2.5;filter:drop-shadow(0 4px 12px rgba(15,118,110,.25));}
.node.inactive .node-rect{fill:#fafafa;stroke-dasharray:3,3;}
.node-stripe{rx:0;ry:0;}
.node-code{font-family:'Bebas Neue',sans-serif;font-size:10px;letter-spacing:.06em;fill:var(--text-muted);}
.node-name{font-family:'DM Sans',sans-serif;font-size:13px;font-weight:600;fill:var(--text);}
.node.inactive .node-name{fill:var(--text-muted);text-decoration:line-through;}
.node-desig{font-family:'DM Sans',sans-serif;font-size:11px;fill:var(--text-muted);}
.node-territory{font-family:'DM Sans',sans-serif;font-size:10px;fill:var(--text-dim);}
.node-reports-pill{font-family:'DM Sans',sans-serif;font-size:9.5px;font-weight:600;fill:#fff;text-anchor:middle;}
.node-reports-bg{fill:var(--accent-dark);}
.node-inactive-pill{font-family:'DM Sans',sans-serif;font-size:8.5px;font-weight:600;fill:#fff;text-anchor:middle;letter-spacing:.04em;}
.node-inactive-bg{fill:#9ca3af;}

.link{fill:none;stroke:#cbd5d0;stroke-width:1.4;}
.link.dim{opacity:.25;}
.link.highlight{stroke:var(--accent);stroke-width:2;}

/* ----- Side drawer ----- */
.drawer{position:absolute;top:0;right:0;height:100%;width:420px;max-width:90vw;background:var(--surface);box-shadow:-12px 0 28px rgba(0,0,0,.08);transform:translateX(100%);transition:transform .22s cubic-bezier(.4,.0,.2,1);z-index:20;display:flex;flex-direction:column;}
.drawer.open{transform:translateX(0);}
.drawer-head{padding:18px 22px 14px;border-bottom:1px solid var(--border);background:#fafafa;}
.drawer-close{position:absolute;top:14px;right:16px;width:30px;height:30px;border:none;background:transparent;font-size:20px;color:var(--text-muted);cursor:pointer;border-radius:6px;}
.drawer-close:hover{background:var(--border);color:var(--text);}
.drawer-code{font-family:'Bebas Neue',sans-serif;font-size:12px;letter-spacing:.12em;color:var(--accent);}
.drawer-name{font-family:'DM Sans',sans-serif;font-size:22px;font-weight:600;color:var(--text);margin-top:4px;line-height:1.15;}
.drawer-desig{font-size:13px;color:var(--text-muted);margin-top:3px;}
.drawer-status-pill{display:inline-block;font-size:9.5px;font-weight:600;padding:2px 9px;border-radius:8px;letter-spacing:.06em;text-transform:uppercase;margin-left:6px;vertical-align:middle;}
.pill-active{background:#e8f7f1;color:#0f6e56;}
.pill-inactive{background:#fdecea;color:#c0392b;}
.drawer-body{flex:1;overflow-y:auto;padding:14px 22px 30px;}
.section{margin-top:14px;}
.section h4{font-family:'Bebas Neue',sans-serif;font-size:11px;letter-spacing:.14em;color:var(--accent);padding-bottom:5px;border-bottom:1px solid var(--border);margin-bottom:8px;}
.kv{display:grid;grid-template-columns:auto 1fr;gap:5px 14px;font-size:13px;}
.kv dt{color:var(--text-muted);font-size:11.5px;text-transform:uppercase;letter-spacing:.04em;align-self:center;}
.kv dd{color:var(--text);font-weight:500;word-break:break-word;}
.kv dd.muted{color:var(--text-dim);font-weight:400;}

.drawer-loading{padding:40px 22px;text-align:center;color:var(--text-muted);font-size:13px;}
.drawer-error{padding:30px 22px;color:var(--danger);font-size:13px;background:#fdecea;border-radius:8px;margin:14px 22px;}

.banner{position:absolute;top:14px;left:50%;transform:translateX(-50%);z-index:30;background:var(--surface);border:1px solid var(--border);border-radius:8px;padding:12px 18px;font-size:13px;box-shadow:0 4px 16px rgba(0,0,0,.08);display:none;}
.banner.show{display:block;}
.banner.error{border-color:#f5b7b1;color:#c0392b;background:#fdecea;}

/* Fade-in for the whole canvas after data loads */
.fade-in{animation:fadeIn .35s ease-out;}
@keyframes fadeIn{from{opacity:0}to{opacity:1}}

@media (max-width:768px){
    .drawer{width:100%;}
    .toolbar input[type=text]{width:140px;}
    .legend{display:none;}
}
</style>
</head>
<body>
<form id="form1" runat="server">

<div class="shell">

<nav>
    <a class="nav-logo" href="HREmployee.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">HUMAN RESOURCES</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="HREmployee.aspx" class="nav-link">Employees</a>
        <a href="HRDepartment.aspx" class="nav-link">Departments</a>
        <a href="HREmployeeImport.aspx" class="nav-link">Import</a>
        <a href="HROrgChart.aspx" class="nav-link active">Org Chart</a>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#8592; ERP Home</a>
        <a href="HRLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <div class="page-icon">&#x1F333;</div>
    <div class="page-title">ORG <span>CHART</span></div>
    <div class="page-sub">Reporting hierarchy. Pan with click-drag, zoom with scroll. Click any card for full details.</div>
</div>

<div class="workspace">

    <div class="toolbar">
        <input type="text" id="searchBox" placeholder="Search by name, code, or designation..." autocomplete="off"/>
        <button type="button" class="tb-btn" id="btnZoomIn"  title="Zoom in">+</button>
        <button type="button" class="tb-btn" id="btnZoomOut" title="Zoom out">&minus;</button>
        <button type="button" class="tb-btn" id="btnFit"     title="Fit to view">&#x26F6;</button>
        <span class="tb-divider"></span>
        <span class="tb-stat">People <b id="statTotal">&hellip;</b></span>
        <span class="tb-stat">Active <b id="statActive">&hellip;</b></span>
        <span class="tb-stat">Orphans <b id="statOrphan">&hellip;</b></span>
    </div>

    <div class="legend">
        <div class="legend-title">LEGEND</div>
        <div class="legend-row"><span class="legend-swatch" style="background:#fff;border:1px solid var(--border);"></span> Active employee</div>
        <div class="legend-row"><span class="legend-swatch" style="background:#fafafa;border:1px dashed #999;"></span> Inactive (DOL set)</div>
        <div class="legend-row"><span class="legend-swatch" style="background:var(--accent-dark);"></span> Direct-reports count</div>
        <div class="legend-row" style="margin-top:8px;font-size:10px;color:var(--text-dim);">Department colour-band on left edge</div>
    </div>

    <div id="banner" class="banner"></div>

    <svg id="chart"></svg>

    <!-- Side drawer (employee detail) -->
    <aside id="drawer" class="drawer" aria-hidden="true">
        <div class="drawer-head">
            <button type="button" class="drawer-close" id="btnDrawerClose" aria-label="Close">&times;</button>
            <div class="drawer-code" id="dCode">&hellip;</div>
            <div class="drawer-name" id="dName">&hellip;</div>
            <div class="drawer-desig" id="dDesig">&hellip;</div>
        </div>
        <div class="drawer-body" id="drawerBody">
            <div class="drawer-loading">Loading details &hellip;</div>
        </div>
    </aside>

</div>
</div>

</form>

<!-- D3 v7 from cdnjs (matches Session 10 pattern of cdnjs-only externals) -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/d3/7.8.5/d3.min.js"></script>

<script>
(function () {
    'use strict';

    // -----------------------------------------------------------------------
    // State
    // -----------------------------------------------------------------------
    let nodesData = [];         // raw flat array from server
    let nodesById = new Map();  // id -> raw record
    let rootHierarchy = null;   // d3.hierarchy node
    let svg, gRoot, gLinks, gNodes;
    let zoomBehaviour;
    let selectedId = null;

    // Stable colour for each department — same dept always maps to same colour.
    // Picked to be distinguishable on a light grey background.
    const DEPT_COLOURS = [
        '#0d9488', '#7c3aed', '#dc2626', '#ea580c', '#ca8a04',
        '#65a30d', '#0891b2', '#be185d', '#475569', '#9333ea',
        '#16a34a', '#0369a1'
    ];
    const deptColourMap = new Map();
    function colourForDept(deptName) {
        if (!deptName) return '#9ca3af';
        if (deptColourMap.has(deptName)) return deptColourMap.get(deptName);
        const colour = DEPT_COLOURS[deptColourMap.size % DEPT_COLOURS.length];
        deptColourMap.set(deptName, colour);
        return colour;
    }

    // Node card dimensions. Wide enough for "Regional Sales Manager" etc.
    const NODE_W = 220;
    const NODE_H = 96;

    // -----------------------------------------------------------------------
    // Boot
    // -----------------------------------------------------------------------
    document.addEventListener('DOMContentLoaded', init);

    async function init() {
        svg = d3.select('#chart');
        gRoot = svg.append('g').attr('class', 'fade-in');
        gLinks = gRoot.append('g').attr('class', 'links');
        gNodes = gRoot.append('g').attr('class', 'nodes');

        // Pan + zoom
        zoomBehaviour = d3.zoom()
            .scaleExtent([0.2, 2.5])
            .on('zoom', (event) => {
                gRoot.attr('transform', event.transform);
            });
        svg.call(zoomBehaviour);

        // Toolbar wiring
        document.getElementById('btnZoomIn').addEventListener('click',  () => svg.transition().duration(180).call(zoomBehaviour.scaleBy, 1.25));
        document.getElementById('btnZoomOut').addEventListener('click', () => svg.transition().duration(180).call(zoomBehaviour.scaleBy, 0.8));
        document.getElementById('btnFit').addEventListener('click', fitToView);

        document.getElementById('searchBox').addEventListener('input', onSearch);
        document.getElementById('btnDrawerClose').addEventListener('click', closeDrawer);

        // Resize handler — update SVG viewBox so it fills the workspace
        window.addEventListener('resize', sizeSvg);
        sizeSvg();

        try {
            await loadTree();
        } catch (err) {
            showBanner('Failed to load org chart: ' + (err && err.message ? err.message : err), true);
        }
    }

    function sizeSvg() {
        const w = document.querySelector('.workspace').clientWidth;
        const h = document.querySelector('.workspace').clientHeight;
        svg.attr('width', w).attr('height', h);
    }

    // -----------------------------------------------------------------------
    // Data load
    // -----------------------------------------------------------------------
    async function loadTree() {
        const resp = await fetch('HROrgChart.ashx?action=tree', { credentials: 'same-origin' });
        if (resp.status === 401) {
            window.location.href = 'HRLogin.aspx';
            return;
        }
        if (!resp.ok) throw new Error('HTTP ' + resp.status);
        const data = await resp.json();
        nodesData = data.nodes || [];

        nodesById.clear();
        nodesData.forEach(n => nodesById.set(n.id, n));

        updateStats();
        buildHierarchy();
        render();
        // Fit on next frame so layout has settled.
        requestAnimationFrame(fitToView);
    }

    function updateStats() {
        const total = nodesData.length;
        const active = nodesData.filter(n => n.active).length;
        // "Orphan" = has a non-null mgrId that doesn't exist in nodesById
        // (FK should prevent this, but UI surfaces any drift)
        // PLUS anyone with no mgrId at all who isn't the explicit root EMP001
        const orphans = nodesData.filter(n => {
            if (n.mgrId == null) return n.code !== 'EMP001';
            return !nodesById.has(n.mgrId);
        }).length;
        document.getElementById('statTotal').textContent = total;
        document.getElementById('statActive').textContent = active;
        document.getElementById('statOrphan').textContent = orphans;
    }

    // -----------------------------------------------------------------------
    // Hierarchy construction
    //
    // The data is a flat list with mgrId pointers. Multiple roots (mgrId=null)
    // are possible — at minimum EMP001 (Rajan, MD) is always a root. Anyone
    // else with a null mgrId becomes a separate sub-tree.
    //
    // d3.hierarchy needs a single root, so we synthesize a virtual "ALL" root
    // that owns every real root underneath it. The virtual root is rendered
    // invisibly so the user just sees natural sub-trees.
    // -----------------------------------------------------------------------
    function buildHierarchy() {
        const VIRTUAL_ROOT_ID = -1;
        const virtual = { id: VIRTUAL_ROOT_ID, code: '', name: 'All', _virtual: true };

        // Cycle-safe parent pointer: anyone whose mgrId points to themselves
        // (or back into themselves transitively) gets reparented to the virtual root.
        // We detect cycles by walking up from each node and watching for revisits.
        function parentOf(node) {
            if (node.id === VIRTUAL_ROOT_ID) return null;
            const mgrId = node.mgrId;
            if (mgrId == null || !nodesById.has(mgrId) || mgrId === node.id) {
                return VIRTUAL_ROOT_ID;
            }
            // Walk up to detect cycles.
            const seen = new Set([node.id]);
            let cursor = mgrId;
            while (cursor != null && nodesById.has(cursor)) {
                if (seen.has(cursor)) {
                    // Cycle. Break it by reparenting to virtual root.
                    return VIRTUAL_ROOT_ID;
                }
                seen.add(cursor);
                cursor = nodesById.get(cursor).mgrId;
            }
            return mgrId;
        }

        const stratifyData = [virtual, ...nodesData];
        const stratify = d3.stratify()
            .id(d => String(d.id))
            .parentId(d => {
                const p = parentOf(d);
                return p == null ? null : String(p);
            });

        rootHierarchy = stratify(stratifyData);
    }

    // -----------------------------------------------------------------------
    // Rendering
    // -----------------------------------------------------------------------
    function render() {
        if (!rootHierarchy) return;

        // d3.tree gives us node positions. nodeSize gives us spacing per node.
        const treeLayout = d3.tree().nodeSize([NODE_W + 24, NODE_H + 50]);
        treeLayout(rootHierarchy);

        // Filter: don't render the virtual root or its links to it.
        const visibleNodes = rootHierarchy.descendants().filter(n => n.data.id !== -1);
        const visibleLinks = rootHierarchy.links().filter(l => l.source.data.id !== -1);

        // ----- LINKS -----
        const linkGen = d3.linkVertical()
            .x(d => d.x)
            .y(d => d.y);

        const linkSel = gLinks.selectAll('path.link').data(visibleLinks, d => d.target.data.id);
        linkSel.exit().remove();
        linkSel.enter()
            .append('path')
            .attr('class', 'link')
            .merge(linkSel)
            .attr('d', linkGen);

        // ----- NODES -----
        const nodeSel = gNodes.selectAll('g.node').data(visibleNodes, d => d.data.id);
        nodeSel.exit().remove();

        const nodeEnter = nodeSel.enter()
            .append('g')
            .attr('class', d => 'node' + (d.data.active ? '' : ' inactive'))
            .attr('data-id', d => d.data.id)
            .attr('transform', d => `translate(${d.x - NODE_W / 2},${d.y - NODE_H / 2})`)
            .on('click', (event, d) => {
                event.stopPropagation();
                openDrawer(d.data.id);
            });

        // Card background
        nodeEnter.append('rect')
            .attr('class', 'node-rect')
            .attr('width', NODE_W)
            .attr('height', NODE_H);

        // Department colour-band (left edge)
        nodeEnter.append('rect')
            .attr('class', 'node-stripe')
            .attr('width', 4)
            .attr('height', NODE_H)
            .attr('fill', d => colourForDept(d.data.dept));

        // Code (top-left, small caps)
        nodeEnter.append('text')
            .attr('class', 'node-code')
            .attr('x', 14).attr('y', 18)
            .text(d => d.data.code);

        // Department label (top-right, small caps)
        nodeEnter.append('text')
            .attr('class', 'node-code')
            .attr('x', NODE_W - 14).attr('y', 18)
            .attr('text-anchor', 'end')
            .text(d => (d.data.dept || '').toUpperCase());

        // Name
        nodeEnter.append('text')
            .attr('class', 'node-name')
            .attr('x', 14).attr('y', 38)
            .text(d => truncate(d.data.name || '', 26));

        // Designation
        nodeEnter.append('text')
            .attr('class', 'node-desig')
            .attr('x', 14).attr('y', 55)
            .text(d => truncate(d.data.designation || '', 30));

        // Territory line: Zone / Region / Area / Location, joined by "·"
        nodeEnter.append('text')
            .attr('class', 'node-territory')
            .attr('x', 14).attr('y', 72)
            .text(d => formatTerritory(d.data));

        // Joining date (bottom-left corner)
        nodeEnter.append('text')
            .attr('class', 'node-territory')
            .attr('x', 14).attr('y', 88)
            .attr('fill', '#aaa')
            .text(d => d.data.doj ? 'joined ' + formatDate(d.data.doj) : '');

        // Direct-reports pill (bottom-right of card)
        const pillNodes = nodeEnter.filter(d => d.data.reports > 0);
        pillNodes.append('rect')
            .attr('class', 'node-reports-bg')
            .attr('x', NODE_W - 36).attr('y', NODE_H - 22)
            .attr('width', 28).attr('height', 14)
            .attr('rx', 7).attr('ry', 7);
        pillNodes.append('text')
            .attr('class', 'node-reports-pill')
            .attr('x', NODE_W - 22).attr('y', NODE_H - 12)
            .text(d => d.data.reports);

        // Inactive pill (top-right corner of card, only for inactive)
        const inactiveSel = nodeEnter.filter(d => !d.data.active);
        inactiveSel.append('rect')
            .attr('class', 'node-inactive-bg')
            .attr('x', NODE_W - 56).attr('y', 4)
            .attr('width', 50).attr('height', 13)
            .attr('rx', 6).attr('ry', 6);
        inactiveSel.append('text')
            .attr('class', 'node-inactive-pill')
            .attr('x', NODE_W - 31).attr('y', 13)
            .text('INACTIVE');

        // Existing nodes: just update position (in case of re-render)
        nodeSel.attr('transform', d => `translate(${d.x - NODE_W / 2},${d.y - NODE_H / 2})`);
    }

    // -----------------------------------------------------------------------
    // Pan/zoom helpers
    // -----------------------------------------------------------------------
    function fitToView() {
        if (!rootHierarchy) return;
        const visible = rootHierarchy.descendants().filter(n => n.data.id !== -1);
        if (visible.length === 0) return;

        let minX = Infinity, maxX = -Infinity, minY = Infinity, maxY = -Infinity;
        visible.forEach(n => {
            if (n.x - NODE_W / 2 < minX) minX = n.x - NODE_W / 2;
            if (n.x + NODE_W / 2 > maxX) maxX = n.x + NODE_W / 2;
            if (n.y - NODE_H / 2 < minY) minY = n.y - NODE_H / 2;
            if (n.y + NODE_H / 2 > maxY) maxY = n.y + NODE_H / 2;
        });
        const treeW = maxX - minX;
        const treeH = maxY - minY;

        const w = svg.node().clientWidth;
        const h = svg.node().clientHeight;
        const padding = 60;
        const scale = Math.min(
            (w - padding * 2) / treeW,
            (h - padding * 2) / treeH,
            1.0
        );
        const tx = w / 2 - (minX + treeW / 2) * scale;
        const ty = h / 2 - (minY + treeH / 2) * scale;

        svg.transition().duration(450).call(
            zoomBehaviour.transform,
            d3.zoomIdentity.translate(tx, ty).scale(scale)
        );
    }

    function panToNode(id) {
        const target = rootHierarchy.descendants().find(n => n.data.id === id);
        if (!target) return;
        const w = svg.node().clientWidth;
        const h = svg.node().clientHeight;
        const scale = 1.0;
        const tx = w / 2 - target.x * scale;
        const ty = h / 2 - target.y * scale;
        svg.transition().duration(380).call(
            zoomBehaviour.transform,
            d3.zoomIdentity.translate(tx, ty).scale(scale)
        );
    }

    // -----------------------------------------------------------------------
    // Search
    // -----------------------------------------------------------------------
    let searchTimer = null;
    function onSearch(e) {
        const q = e.target.value.trim().toLowerCase();
        clearTimeout(searchTimer);
        searchTimer = setTimeout(() => applySearch(q), 120);
    }

    function applySearch(q) {
        const allNodes = gNodes.selectAll('g.node');
        const allLinks = gLinks.selectAll('path.link');

        if (!q) {
            allNodes.classed('dim', false).classed('match', false);
            allLinks.classed('dim', false).classed('highlight', false);
            return;
        }

        const matchIds = new Set();
        nodesData.forEach(n => {
            const hay = ((n.code || '') + ' ' + (n.name || '') + ' ' + (n.designation || '') + ' ' + (n.dept || '')).toLowerCase();
            if (hay.indexOf(q) !== -1) matchIds.add(n.id);
        });

        // Also include ancestors of matches so the path from root is visible
        const visibleIds = new Set(matchIds);
        matchIds.forEach(id => {
            let cursor = nodesById.get(id);
            while (cursor && cursor.mgrId != null) {
                visibleIds.add(cursor.mgrId);
                cursor = nodesById.get(cursor.mgrId);
            }
        });

        allNodes
            .classed('dim',   d => !visibleIds.has(d.data.id))
            .classed('match', d => matchIds.has(d.data.id));
        allLinks
            .classed('dim',       l => !visibleIds.has(l.target.data.id))
            .classed('highlight', l => matchIds.has(l.target.data.id));

        // Pan to first match
        if (matchIds.size > 0) {
            panToNode(matchIds.values().next().value);
        }
    }

    // -----------------------------------------------------------------------
    // Side drawer (detail panel)
    // -----------------------------------------------------------------------
    async function openDrawer(id) {
        // Highlight the selected node
        if (selectedId !== null) {
            gNodes.select(`g.node[data-id="${selectedId}"]`).classed('selected', false);
        }
        selectedId = id;
        gNodes.select(`g.node[data-id="${id}"]`).classed('selected', true);

        const drawer = document.getElementById('drawer');
        const body   = document.getElementById('drawerBody');
        drawer.classList.add('open');
        drawer.setAttribute('aria-hidden', 'false');
        body.innerHTML = '<div class="drawer-loading">Loading details &hellip;</div>';

        // Optimistic header from the cached tree record so it doesn't flash
        const cached = nodesById.get(id);
        if (cached) {
            document.getElementById('dCode').textContent  = cached.code || '';
            document.getElementById('dName').textContent  = cached.name || '';
            document.getElementById('dDesig').textContent = cached.designation || '';
        }

        try {
            const resp = await fetch('HROrgChart.ashx?action=detail&id=' + encodeURIComponent(id), { credentials: 'same-origin' });
            if (resp.status === 401) { window.location.href = 'HRLogin.aspx'; return; }
            if (!resp.ok) throw new Error('HTTP ' + resp.status);
            const r = await resp.json();
            renderDrawer(r);
        } catch (err) {
            body.innerHTML = '<div class="drawer-error">Failed to load details: ' + escapeHtml(err.message || err) + '</div>';
        }
    }

    function closeDrawer() {
        const drawer = document.getElementById('drawer');
        drawer.classList.remove('open');
        drawer.setAttribute('aria-hidden', 'true');
        if (selectedId !== null) {
            gNodes.select(`g.node[data-id="${selectedId}"]`).classed('selected', false);
            selectedId = null;
        }
    }

    function renderDrawer(r) {
        document.getElementById('dCode').textContent  = r.code  || '';
        document.getElementById('dName').innerHTML    = escapeHtml(r.name || '') +
            (r.active
                ? ' <span class="drawer-status-pill pill-active">Active</span>'
                : ' <span class="drawer-status-pill pill-inactive">Inactive</span>');
        document.getElementById('dDesig').textContent = (r.designation || '') + (r.dept ? ' · ' + r.dept : '');

        const body = document.getElementById('drawerBody');
        body.innerHTML = '';

        // ----- Identity -----
        body.appendChild(section('Identity', kvList([
            ['Father', r.fatherName],
            ['Gender', genderLabel(r.gender)],
            ['DOB',    formatDate(r.dob)],
            ['DOJ',    formatDate(r.doj)],
            ['DOL',    r.dol ? formatDate(r.dol) : '—'],
            ['Type',   r.empType]
        ])));

        // ----- Reporting -----
        body.appendChild(section('Reporting', kvList([
            ['Reports to',
                r.mgrName
                    ? `<a href="javascript:void(0)" data-jump="${r.mgrId}">${escapeHtml(r.mgrCode || '')} — ${escapeHtml(r.mgrName)}</a>`
                    : (r.managerText
                        ? `<span class="muted">${escapeHtml(r.managerText)} (unresolved)</span>`
                        : '<span class="muted">— (top of org)</span>'),
                true],
            ['Direct reports', r.directReports]
        ])));

        // Make the manager link jump the chart to that node
        body.querySelectorAll('a[data-jump]').forEach(a => {
            a.addEventListener('click', () => {
                const targetId = parseInt(a.getAttribute('data-jump'), 10);
                if (!isNaN(targetId)) {
                    closeDrawer();
                    panToNode(targetId);
                    setTimeout(() => openDrawer(targetId), 380);
                }
            });
        });

        // ----- Territory -----
        if (r.zone || r.region || r.area || r.location) {
            body.appendChild(section('Territory', kvList([
                ['Zone',     r.zone],
                ['Region',   r.region],
                ['Area',     r.area],
                ['Location', r.location]
            ])));
        }

        // ----- Contact -----
        body.appendChild(section('Contact', kvList([
            ['Mobile',     r.mobile],
            ['Alt Mobile', r.altMobile],
            ['Email',      r.email],
            ['Address',    r.address],
            ['City',       r.city],
            ['State',      r.state],
            ['Pincode',    r.pincode]
        ])));

        // ----- KYC -----
        body.appendChild(section('KYC & Statutory', kvList([
            ['Aadhaar', r.aadhaar ? formatAadhaar(r.aadhaar) : ''],
            ['PAN',     r.pan],
            ['UAN',     r.uan],
            ['PF No',   r.pfNo],
            ['ESI No',  r.esiNo]
        ])));

        // ----- Bank -----
        if (r.bankAcNo || r.bankName || r.ifsc) {
            body.appendChild(section('Bank', kvList([
                ['A/c No',    r.bankAcNo],
                ['Bank Name', r.bankName],
                ['IFSC',      r.ifsc]
            ])));
        }

        // ----- Salary -----
        const grossNum = parseFloat(r.gross || '0');
        if (grossNum > 0) {
            body.appendChild(section('Compensation', kvList([
                ['Basic',      formatMoney(r.basic)],
                ['HRA',        formatMoney(r.hra)],
                ['Conveyance', formatMoney(r.conveyance)],
                ['Other',      formatMoney(r.other)],
                ['Gross',      '<b>' + formatMoney(r.gross) + '</b>', true]
            ])));
        }

        // ----- Audit -----
        body.appendChild(section('Audit', kvList([
            ['Created', (r.createdBy || '?') + (r.createdAt ? ' · ' + formatDate(r.createdAt) : '')],
            ['Modified', r.modifiedAt
                ? (r.modifiedBy || '?') + ' · ' + formatDate(r.modifiedAt)
                : '<span class="muted">never</span>',
                true]
        ])));
    }

    // -----------------------------------------------------------------------
    // Drawer DOM helpers
    // -----------------------------------------------------------------------
    function section(title, contents) {
        const s = document.createElement('div');
        s.className = 'section';
        const h = document.createElement('h4');
        h.textContent = title;
        s.appendChild(h);
        s.appendChild(contents);
        return s;
    }

    function kvList(pairs) {
        const dl = document.createElement('dl');
        dl.className = 'kv';
        pairs.forEach(([key, value, isHtml]) => {
            const v = (value == null || value === '' ? null : value);
            if (v == null) return;  // skip blanks — keeps the panel tidy
            const dt = document.createElement('dt');
            dt.textContent = key;
            const dd = document.createElement('dd');
            if (isHtml) dd.innerHTML = String(v);
            else dd.textContent = String(v);
            dl.appendChild(dt);
            dl.appendChild(dd);
        });
        return dl;
    }

    // -----------------------------------------------------------------------
    // Misc utilities
    // -----------------------------------------------------------------------
    function truncate(s, n) {
        if (!s) return '';
        return s.length > n ? s.substring(0, n - 1) + '…' : s;
    }

    function formatDate(s) {
        if (!s) return '';
        // Accept YYYY-MM-DD or full ISO; render as DD-MMM-YYYY
        const d = new Date(s);
        if (isNaN(d.getTime())) return s;
        const months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
        return d.getDate().toString().padStart(2, '0') + '-' + months[d.getMonth()] + '-' + d.getFullYear();
    }

    function formatTerritory(d) {
        const parts = [];
        if (d.zone)     parts.push(d.zone);
        if (d.region && d.region !== d.zone) parts.push(d.region);
        if (d.location) parts.push(d.location);
        return truncate(parts.join(' · '), 30);
    }

    function formatAadhaar(a) {
        if (!a) return '';
        const s = a.replace(/\s+/g, '');
        if (s.length !== 12) return a;
        return s.substring(0,4) + ' ' + s.substring(4,8) + ' ' + s.substring(8,12);
    }

    function formatMoney(s) {
        const n = parseFloat(s || '0');
        return '₹ ' + Math.round(n).toLocaleString('en-IN');
    }

    function genderLabel(g) {
        if (g === 'M') return 'Male';
        if (g === 'F') return 'Female';
        if (g === 'O') return 'Other';
        return g || '';
    }

    function escapeHtml(s) {
        if (s == null) return '';
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function showBanner(msg, isError) {
        const b = document.getElementById('banner');
        b.textContent = msg;
        b.classList.toggle('error', !!isError);
        b.classList.add('show');
        setTimeout(() => b.classList.remove('show'), 6000);
    }

    // Close drawer on Esc
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') closeDrawer();
    });

})();
</script>

<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
