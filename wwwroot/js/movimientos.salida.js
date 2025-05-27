document.addEventListener('DOMContentLoaded', function () {
    const productos = JSON.parse(document.getElementById('productosStockJson').textContent);
    const productoSelect = document.getElementById('productoId');
    const stockActualSpan = document.getElementById('stock-actual-producto-salida-valor');
    const cantidadSalidaDisplay = document.getElementById('cantidad-salida-display'); // Added
    const cantidadInput = document.getElementById('cantidad'); // Fix: correct id for cantidad input
    const feedback = document.getElementById('cantidadFeedback');
    const detalleNombre = document.getElementById('detalleNombre');
    const detalleStock = document.getElementById('detalleStock');
    let stockActual = null;

    function actualizarStock() {
        const prodId = productoSelect.value;
        const prod = productos.find(p => p.Id == prodId);
        if (prod) {
            stockActual = prod.CantidadActual ?? 0;
            cantidadInput.max = stockActual;
            feedback.textContent = `Stock disponible: ${stockActual}`;
            feedback.style.color = '#888';
            if(detalleNombre && detalleStock) {
                detalleNombre.textContent = prod.Nombre;
                detalleStock.textContent = stockActual;
            }
            stockActualSpan.textContent = stockActual; // Fix: use stockActual
            cantidadSalidaDisplay.textContent = ''; // Clear when product changes
        } else {
            stockActual = null;
            cantidadInput.removeAttribute('max');
            feedback.textContent = '';
            if(detalleNombre && detalleStock) {
                detalleNombre.textContent = '-';
                detalleStock.textContent = '-';
            }
        }
    }

    productoSelect.addEventListener('change', function () {
        actualizarStock();
        cantidadInput.value = '';
    });

    function updateCantidadSalidaDisplay() {
        const cantidad = parseInt(cantidadInput.value, 10);
        if (!isNaN(cantidad) && cantidad > 0) {
            cantidadSalidaDisplay.textContent = `-${cantidad}`;
        } else {
            cantidadSalidaDisplay.textContent = '';
        }
    }

    cantidadInput.addEventListener('input', function () {
        if (stockActual !== null) {
            if (cantidadInput.value > stockActual) {
                feedback.textContent = `No puedes retirar más de ${stockActual}`;
                feedback.style.color = '#ff5858';
            } else if (cantidadInput.value <= 0) {
                feedback.textContent = 'La cantidad debe ser mayor a cero.';
                feedback.style.color = '#ff5858';
            } else {
                feedback.textContent = 'Cantidad válida.';
                feedback.style.color = '#43e97b';
            }
        }
        updateCantidadSalidaDisplay();
    });

    // Animación al enviar
    document.getElementById('salidaForm').addEventListener('submit', function (e) {
        if (stockActual !== null && (cantidadInput.value > stockActual || cantidadInput.value <= 0)) {
            e.preventDefault();
            cantidadInput.classList.add('animate__shakeX');
            setTimeout(() => cantidadInput.classList.remove('animate__shakeX'), 600);
        } else {
            document.getElementById('submitBtn').classList.add('animate__tada');
        }
    });

    // Inicializar
    actualizarStock();
});
