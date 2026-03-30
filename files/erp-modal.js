/* ═══════════════════════════════════════════════════════════
   SIRIMIRI ERP — Custom Modal Dialogs
   Replace browser alert() and confirm() with styled modals
   Include: <script src="/StockApp/erp-modal.js"></script>
   ═══════════════════════════════════════════════════════════ */

(function(){
// Inject CSS once
var style = document.createElement('style');
style.textContent = `
.erp-modal-overlay{position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,.45);z-index:99999;display:flex;align-items:center;justify-content:center;opacity:0;transition:opacity .2s;}
.erp-modal-overlay.show{opacity:1;}
.erp-modal{background:#fff;border-radius:14px;padding:0;width:420px;max-width:90vw;box-shadow:0 20px 60px rgba(0,0,0,.2);transform:scale(.9);transition:transform .2s;overflow:hidden;}
.erp-modal-overlay.show .erp-modal{transform:scale(1);}
.erp-modal-header{padding:18px 24px 0;display:flex;align-items:center;gap:12px;}
.erp-modal-icon{width:40px;height:40px;border-radius:10px;display:flex;align-items:center;justify-content:center;font-size:20px;flex-shrink:0;}
.erp-modal-icon.warn{background:#fef3cd;color:#d68b00;}
.erp-modal-icon.danger{background:#fdf3f2;color:#e74c3c;}
.erp-modal-icon.info{background:#e8f4fd;color:#2980b9;}
.erp-modal-icon.success{background:#eafaf1;color:#1a9e6a;}
.erp-modal-title{font-family:'Bebas Neue','DM Sans',sans-serif;font-size:18px;letter-spacing:.05em;color:#1a1a1a;}
.erp-modal-body{padding:12px 24px 20px;font-size:14px;color:#444;line-height:1.5;}
.erp-modal-footer{padding:0 24px 20px;display:flex;gap:8px;justify-content:flex-end;}
.erp-modal-btn{border:none;border-radius:8px;padding:10px 24px;font-size:13px;font-weight:700;cursor:pointer;font-family:inherit;transition:background .15s;}
.erp-modal-btn.primary{background:#e67e22;color:#fff;}.erp-modal-btn.primary:hover{background:#d35400;}
.erp-modal-btn.danger{background:#e74c3c;color:#fff;}.erp-modal-btn.danger:hover{background:#c0392b;}
.erp-modal-btn.success{background:#1a9e6a;color:#fff;}.erp-modal-btn.success:hover{background:#148a5b;}
.erp-modal-btn.cancel{background:#f0f0f0;color:#333;border:1px solid #ddd;}.erp-modal-btn.cancel:hover{background:#e0e0e0;}
`;
document.head.appendChild(style);

function createModal(opts){
    var overlay = document.createElement('div');
    overlay.className = 'erp-modal-overlay';
    var iconClass = opts.type || 'info';
    var iconMap = {warn:'⚠️',danger:'🚫',info:'ℹ️',success:'✅'};
    overlay.innerHTML =
        '<div class="erp-modal">' +
            '<div class="erp-modal-header">' +
                '<div class="erp-modal-icon '+iconClass+'">'+(iconMap[iconClass]||'ℹ️')+'</div>' +
                '<div class="erp-modal-title">'+(opts.title||'Confirm')+'</div>' +
            '</div>' +
            '<div class="erp-modal-body">'+(opts.message||'')+'</div>' +
            '<div class="erp-modal-footer" id="erpModalFooter"></div>' +
        '</div>';
    document.body.appendChild(overlay);
    requestAnimationFrame(function(){ overlay.className = 'erp-modal-overlay show'; });
    return overlay;
}

function closeModal(overlay){
    overlay.className = 'erp-modal-overlay';
    setTimeout(function(){ if(overlay.parentNode) overlay.parentNode.removeChild(overlay); }, 200);
}

// ── erpAlert: styled replacement for alert() ──
window.erpAlert = function(message, opts){
    opts = opts || {};
    var overlay = createModal({
        title: opts.title || 'Notice',
        message: message,
        type: opts.type || 'info'
    });
    var footer = overlay.querySelector('#erpModalFooter');
    var btnOk = document.createElement('button');
    btnOk.className = 'erp-modal-btn primary';
    btnOk.innerText = 'OK';
    btnOk.onclick = function(){ closeModal(overlay); if(opts.onOk) opts.onOk(); };
    footer.appendChild(btnOk);
    btnOk.focus();
};

// ── erpConfirm: styled replacement for confirm() ──
// Returns via callbacks since it's async (non-blocking)
// Usage: erpConfirm('Delete this?', { onOk: function(){ doDelete(); } });
window.erpConfirm = function(message, opts){
    opts = opts || {};
    var overlay = createModal({
        title: opts.title || 'Confirm',
        message: message,
        type: opts.type || 'warn'
    });
    var footer = overlay.querySelector('#erpModalFooter');

    var btnCancel = document.createElement('button');
    btnCancel.className = 'erp-modal-btn cancel';
    btnCancel.innerText = opts.cancelText || 'Cancel';
    btnCancel.onclick = function(){ closeModal(overlay); if(opts.onCancel) opts.onCancel(); };
    footer.appendChild(btnCancel);

    var btnOk = document.createElement('button');
    btnOk.className = 'erp-modal-btn ' + (opts.btnClass || 'primary');
    btnOk.innerText = opts.okText || 'OK';
    btnOk.onclick = function(){ closeModal(overlay); if(opts.onOk) opts.onOk(); };
    footer.appendChild(btnOk);
    btnOk.focus();
};

// ── erpConfirmSync: for use with OnClientClick="return erpConfirmSync(...)" ──
// Uses a hidden form field to work with ASP.NET postback
// Call: OnClientClick="return erpConfirmLink(this, 'Remove this item?')"
window.erpConfirmLink = function(el, message, opts){
    opts = opts || {};
    erpConfirm(message, {
        title: opts.title || 'Confirm',
        type: opts.type || 'warn',
        okText: opts.okText || 'Yes, proceed',
        cancelText: opts.cancelText || 'Cancel',
        btnClass: opts.btnClass || 'danger',
        onOk: function(){
            // Trigger the ASP.NET postback
            if(el.tagName === 'A' && el.href && el.href.indexOf('__doPostBack') >= 0){
                // LinkButton — evaluate the href javascript
                eval(el.href.replace('javascript:',''));
            } else if(el.tagName === 'INPUT' || el.tagName === 'BUTTON'){
                // Regular button — submit via __doPostBack
                __doPostBack(el.name, '');
            }
        }
    });
    return false; // Always prevent default — modal handles the action
};

})();
