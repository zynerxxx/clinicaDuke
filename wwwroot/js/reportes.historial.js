$(document).ready(function () {
    const historialModalEl = document.getElementById('historialCompletoModal');
    const historialModal = new bootstrap.Modal(historialModalEl);
    const historialBody = $('#historialCompletoBody');
    const paginationUl = $('#historialPagination');
    let currentPage = 1;
    const pageSize = 10;

    function loadHistorial(page) {
        $.ajax({
            url: '/Reportes/ObtenerHistorialMovimientosPorNombre',
            type: 'GET',
            data: {
                pagina: page,
                tamanoPagina: pageSize,
                producto: '', // Siempre enviar producto vacío
                fechaInicio: '', // Siempre enviar fecha vacía
                fechaFin: '' // Siempre enviar fecha vacía
            },
            success: function (response) {
                let newRowsHtml = '';
                if (response && response.historial && response.historial.length > 0) {
                    response.historial.forEach(function (movimiento) {
                        const fechaFormateada = (movimiento.fecha && !isNaN(Date.parse(movimiento.fecha)))
                            ? new Date(movimiento.fecha).toLocaleString('es-ES', {
                                year: 'numeric', month: '2-digit', day: '2-digit',
                                hour: '2-digit', minute: '2-digit'
                            })
                            : (movimiento.fecha || 'Sin fecha');
                        const row = `<tr>
                            <td>${movimiento.producto}</td>
                            <td><span class="badge bg-${movimiento.tipo === 'Entrada' ? 'success' : 'danger'}">${movimiento.tipo}</span></td>
                            <td>${movimiento.tipo === 'Entrada' ? '+' : ''}${movimiento.cantidad}</td>
                            <td>${fechaFormateada}</td>
                            <td>${movimiento.usuario || '-'}</td>
                            <td>${movimiento.observaciones || 'N/A'}</td>
                        </tr>`;
                        newRowsHtml += row;
                    });
                    historialBody.html(newRowsHtml);
                    renderPagination(response.totalRegistros, page);
                } else {
                    historialBody.html('<tr><td colspan="6" class="text-center">No hay movimientos registrados.</td></tr>');
                    paginationUl.empty();
                }
            },
            error: function (xhr, status, error) {
                historialBody.html('<tr><td colspan="6" class="text-center">Error al cargar el historial. Intente nuevamente.</td></tr>');
                paginationUl.empty();
            }
        });
    }

    function renderPagination(totalItems, currentPage) {
        paginationUl.empty();
        const totalPages = Math.ceil(totalItems / pageSize);
        if (totalPages <= 1) return;
        paginationUl.append(
            `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${currentPage - 1}">Anterior</a>
            </li>`
        );
        for (let i = 1; i <= totalPages; i++) {
            paginationUl.append(
                `<li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>`
            );
        }
        paginationUl.append(
            `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${currentPage + 1}">Siguiente</a>
            </li>`
        );
        paginationUl.find('a.page-link').on('click', function (e) {
            e.preventDefault();
            const page = $(this).data('page');
            if (page > 0 && page <= totalPages) {
                loadHistorial(page);
            }
        });
    }

    $('#historialCompletoModal').on('show.bs.modal', function () {
        loadHistorial(1);
    });
});
