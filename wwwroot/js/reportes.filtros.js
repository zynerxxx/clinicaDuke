function hayFiltrosActivos() {
    var form = document.querySelector('form.mb-4.p-3.rounded.shadow-sm');
    if (!form) return false;
    var inputs = form.querySelectorAll('input, select');
    for (var i = 0; i < inputs.length; i++) {
        var el = inputs[i];
        if (el.type !== 'submit' && el.name && el.name !== '__RequestVerificationToken') {
            // Log para depuración
            console.log('Filtro:', el.name, 'type:', el.type, 'tag:', el.tagName, 'valor:', el.value);
            // Para selects, si el valor actual es distinto al primer option, es filtro activo
            if (el.tagName === 'SELECT') {
                var firstOption = el.options[0] ? el.options[0].value : '';
                if (el.value !== firstOption) {
                    console.log('Filtro activo: select', el.name, el.value, 'firstOption:', firstOption);
                    return true;
                }
            }
            // Para inputs de tipo date, si tiene valor, es filtro activo
            if (el.type === 'date' && el.value && el.value !== '') {
                console.log('Filtro activo: date', el.name, el.value);
                return true;
            }
            // Para otros inputs, si tiene valor, es filtro activo
            if (el.tagName !== 'SELECT' && el.type !== 'date' && el.value && el.value !== '') {
                console.log('Filtro activo: input', el.name, el.value);
                return true;
            }
        }
    }
    return false;
}

function limpiarFiltros() {
    // Redirige siempre a la URL limpia de reportes
    window.location.href = '/Home/Reportes';
}

function animarLimpiarFiltros(btn) {
    // Mostrar loader central
    var loader = document.getElementById('loader-limpiar-filtros');
    if (loader) loader.style.display = 'flex';
    // Deshabilitar botón para evitar doble clic
    btn.disabled = true;
    setTimeout(function() {
        limpiarFiltros();
    }, 700); // Da tiempo a la animación antes de limpiar
}

window.addEventListener('DOMContentLoaded', function() {
    var btn = document.getElementById('btn-limpiar-filtros');
    var form = document.querySelector('form.mb-4.p-3.rounded.shadow-sm');
    console.log('btn-limpiar-filtros:', btn);
    console.log('formulario de filtros:', form);
    function actualizarBoton() {
        console.log('actualizarBoton, hayFiltrosActivos:', hayFiltrosActivos());
        if (hayFiltrosActivos()) {
            btn.style.display = 'block';
            btn.style.setProperty('display', 'block', 'important');
            btn.style.opacity = '0';
            setTimeout(function() { btn.style.opacity = '1'; btn.style.transition = 'opacity 0.4s'; }, 10);
        } else {
            btn.style.opacity = '0';
            setTimeout(function() { btn.style.display = 'none'; }, 400);
        }
    }
    if (btn && form) {
        actualizarBoton();
        form.addEventListener('input', actualizarBoton);
        form.addEventListener('change', actualizarBoton);
        form.addEventListener('submit', function(e) {
            // Elimina temporalmente el name de los campos vacíos o selects sin filtro para que no salgan en la URL
            var inputs = form.querySelectorAll('input, select');
            var removedNames = [];
            for (var i = 0; i < inputs.length; i++) {
                var el = inputs[i];
                if (el.type !== 'submit' && el.name) {
                    // Para inputs vacíos
                    if ((el.tagName === 'INPUT' && el.value === '') ||
                        // Para selects en el primer option (sin filtro)
                        (el.tagName === 'SELECT' && el.value === (el.options[0] ? el.options[0].value : '')) ) {
                        removedNames.push({el: el, name: el.name});
                        el.removeAttribute('name');
                    }
                }
            }
            setTimeout(function() {
                // Restaura los names después de enviar
                removedNames.forEach(function(item) {
                    item.el.setAttribute('name', item.name);
                });
            }, 100);
            setTimeout(actualizarBoton, 500);
        });
    } else {
        if (!btn) console.log('No se encontró el botón flotante de limpiar filtros');
        if (!form) console.log('No se encontró el formulario de filtros');
    }
});

window.mostrarDetalleMovimiento = function(producto, tipo, cantidad, fecha, usuario, observaciones) {
    document.getElementById('detalleProducto').textContent = producto || '';
    document.getElementById('detalleTipo').textContent = tipo || '';
    document.getElementById('detalleCantidad').textContent = cantidad || '';
    document.getElementById('detalleFecha').textContent = fecha || '';
    document.getElementById('detalleUsuario').textContent = usuario || '';
    document.getElementById('detalleObservaciones').textContent = observaciones || '';
};

