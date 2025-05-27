$(document).ready(function () {
    const historialModalEl = document.getElementById('historialCompletoModal');
    const historialModal = new bootstrap.Modal(historialModalEl);
    const historialBody = $('#historialCompletoBody');
    const paginationUl = $('#historialPagination');
    let currentPage = 1;
    const pageSize = 10;

    // Cierra cualquier otro modal antes de abrir el historial
    function closeOtherModals() {
        $('.modal.show').each(function () {
            if (this !== historialModalEl) {
                bootstrap.Modal.getInstance(this)?.hide();
            }
        });
    }

    $('#verHistorialCompletoBtn').on('click', function () {
        closeOtherModals();
        currentPage = 1;
        loadHistorial(currentPage);
        // Oculta overlay de carga si está visible
        $('#globalLoaderOverlay').hide();
        setTimeout(() => historialModal.show(), 100); // Pequeño delay para evitar doble apertura
    });

    // Oculta overlay de carga y cualquier backdrop de Bootstrap al abrir/cerrar el modal
    function hideAllOverlays() {
        $('#globalLoaderOverlay').hide();
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');
    }
    historialModalEl.addEventListener('show.bs.modal', function () {
        hideAllOverlays();
    });
    historialModalEl.addEventListener('hidden.bs.modal', function () {
        hideAllOverlays();
    });

    function animateTableRows(newRowsHtml) {
        const tbody = $('#historialCompletoBody');
        const currentRows = tbody.children('tr');
        if (currentRows.length > 0) {
            // Fade out si hay filas
            currentRows.animate({ opacity: 0 }, 180, function () {
                tbody.empty();
                tbody.html(newRowsHtml);
                tbody.children('tr').css('opacity', 0).animate({ opacity: 1 }, 220);
            });
        } else {
            // Si no hay filas, solo muestra
            tbody.empty();
            tbody.html(newRowsHtml);
            tbody.children('tr').css('opacity', 0).animate({ opacity: 1 }, 220);
        }
    }

    function loadHistorial(page) {
        $.ajax({
            url: '/Home/ObtenerHistorialMovimientos',
            type: 'GET',
            data: {
                pagina: page,
                tamanoPagina: pageSize
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
                            <td>${movimiento.observaciones || 'N/A'}</td>
                        </tr>`;
                        newRowsHtml += row;
                    });
                    animateTableRows(newRowsHtml);
                    renderPagination(response.totalRegistros, page);
                } else {
                    animateTableRows('<tr><td colspan="6" class="text-center">No hay movimientos registrados.</td></tr>');
                    paginationUl.empty();
                }
            },
            error: function (xhr, status, error) {
                console.error("Error al cargar el historial:", error);
                animateTableRows('<tr><td colspan="6" class="text-center">Error al cargar el historial. Intente nuevamente.</td></tr>');
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
});
