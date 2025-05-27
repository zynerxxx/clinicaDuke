// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('.user-dropdown-item, .user-dropdown-name, .user-dropdown-role').forEach(function(el) {
        el.addEventListener('click', function(e) {
            e.stopPropagation();
        });
    });

    // Animación de carga global para los botones laterales
    var sidebarBtns = document.querySelectorAll('.dashboard-sidebar .sidebar-btn');
    var globalLoader = document.getElementById('globalLoaderOverlay');
    sidebarBtns.forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            if (btn.classList.contains('loading')) return;
            e.preventDefault();
            btn.classList.add('loading');
            if (globalLoader) {
                globalLoader.style.display = 'flex';
            }
            setTimeout(function() {
                window.location.href = btn.getAttribute('href');
            }, 600);
        });
    });

    var notifBtn = document.getElementById('notifDropdownBtn');
    var notifMenu = document.getElementById('notifDropdownMenu');
    var notifDot = document.getElementById('notifBadge');
    var notifBellBtn = notifBtn;
    // Usar sessionStorage para recordar si ya se abrió el menú en la sesión
    var notifBellHasAnimated = sessionStorage.getItem('notifBellHasAnimated') === 'true';
    if (notifBtn && notifMenu && notifDot && notifBellBtn) {
        // Solo animar si hay notificaciones y nunca se ha abierto el menú en la sesión
        if (notifDot.classList.contains('show') && !notifBellHasAnimated) {
            notifBellBtn.classList.add('notif-bell-animate');
        }
        notifMenu.addEventListener('show.bs.dropdown', function() {
            notifBellBtn.classList.remove('notif-bell-animate');
            sessionStorage.setItem('notifBellHasAnimated', 'true');
        });
        notifMenu.addEventListener('hide.bs.dropdown', function() {
            notifBellBtn.classList.remove('notif-bell-animate');
        });
    }
});

// Estilos para el loader (puedes mover esto a tu CSS global si prefieres)
var style = document.createElement('style');
style.innerHTML = `
.sidebar-btn.loading {
    pointer-events: none;
    opacity: 0.7;
    position: relative;
}
.sidebar-btn-loader {
    position: absolute;
    right: 16px;
    top: 50%;
    transform: translateY(-50%);
    font-size: 1.1em;
    color: #888;
}
`;
document.head.appendChild(style);
