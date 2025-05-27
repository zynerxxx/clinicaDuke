// Gráficas de pastel para reportes
window.initReportesCharts = function() {
    if (window.Chart && window.ChartDataLabels) {
        Chart.register(ChartDataLabels);

        // Paleta de colores amigable para daltonismo (Color Blind Safe)
        const colorPalette = [
            '#018571', // Verde azulado
            '#CC79A7', // Rosa
            '#0072B2', // Azul
            '#E69F00', // Naranja
            '#56B4E9', // Celeste
            '#D55E00', // Rojo
            '#F0E442', // Amarillo
            '#999999'  // Gris (para "Otros")
        ];

        // Función para agrupar datos pequeños en "Otros"
        function agruparDatosEnOtros(data, labels, umbralPorcentaje = 3) {
            const total = data.reduce((a, b) => a + b, 0);
            const grupos = {
                datos: [],
                etiquetas: [],
                otros: 0
            };

            data.forEach((valor, i) => {
                const porcentaje = (valor / total) * 100;
                if (porcentaje >= umbralPorcentaje) {
                    grupos.datos.push(valor);
                    grupos.etiquetas.push(labels[i]);
                } else {
                    grupos.otros += valor;
                }
            });

            if (grupos.otros > 0) {
                grupos.datos.push(grupos.otros);
                grupos.etiquetas.push('Otros');
            }

            return grupos;
        }

        // Configuración común para tooltips y datalabels
        const commonChartOptions = {
            responsive: true,
            maintainAspectRatio: false, // Puedes ajustarlo según necesites
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 20, // Añade espacio a la leyenda
                        font: {
                            size: 13 // Tamaño de fuente para la leyenda
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            let label = context.label || '';
                            if (label) {
                                label += ': ';
                            }
                            if (context.parsed !== null) {
                                label += context.formattedValue;
                                const total = context.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                                const percentage = (context.raw / total * 100).toFixed(1) + '%';
                                label += ' (' + percentage + ')';
                            }
                            return label;
                        }
                    },
                    bodyFont: { // Estilo para el cuerpo del tooltip
                        size: 13
                    },
                    titleFont: { // Estilo para el título del tooltip
                        size: 14
                    },
                    padding: 10 // Padding interno del tooltip
                },
                datalabels: { // Configuración para chartjs-plugin-datalabels
                    formatter: (value, ctx) => {
                        const total = ctx.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                        const percentage = (value / total * 100).toFixed(1) + '%';
                        // Solo mostrar porcentaje si es significativo (ej. > 3%) para evitar clutter
                        if ((value / total * 100) < 3) {
                            return '';
                        }
                        return percentage;
                    },
                    color: '#fff', // Color del texto del porcentaje
                    font: {
                        weight: 'bold',
                        size: 13
                    },
                    // Opcional: añadir un pequeño borde o sombra al texto para mejor legibilidad
                    textStrokeColor: 'black',
                    textStrokeWidth: 0.5,
                    align: 'center',
                    anchor: 'center'
                }
            },
            animation: { // Animaciones sutiles
                duration: 1000,
                easing: 'easeInOutQuart'
            },
            // Efecto hover en las porciones
            hoverOffset: 8, // Cuánto se "sale" la porción al hacer hover
        };

        // Movimientos por usuario
        var ctxUsuario = document.getElementById('graficaPorUsuario');
        if (ctxUsuario) {
            var dataUsuario = JSON.parse(ctxUsuario.getAttribute('data-chart') || '[]');
            var { datos: datosUsuario, etiquetas: etiquetasUsuario } = agruparDatosEnOtros(
                dataUsuario.map(x => x.valor || x.Valor),
                dataUsuario.map(x => x.etiqueta || x.Etiqueta)
            );
            var chartUsuario = new Chart(ctxUsuario, {
                type: 'pie',
                data: {
                    labels: etiquetasUsuario,
                    datasets: [{
                        data: datosUsuario,
                        backgroundColor: colorPalette.slice(0, datosUsuario.length), // Colores de la paleta
                        borderColor: '#fff', // Borde blanco para separar las porciones
                        borderWidth: 2,       // Ancho del borde
                        hoverBorderWidth: 3   // Borde más grueso al hacer hover
                    }]
                },
                options: {
                    ...commonChartOptions, // Usar opciones comunes
                    plugins: {
                        ...commonChartOptions.plugins, // Heredar plugins comunes
                    },
                    onClick: function(evt, elements) {
                        if (elements && elements.length > 0) {
                            // Si solo hay un segmento, no hacer nada
                            if (this.data.labels.length <= 1) return;
                            var idx = elements[0].index;
                            var usuario = this.data.labels[idx];
                            var select = document.querySelector('select[name="Usuario"]');
                            if (select) {
                                // Si ya está seleccionado, limpiar filtro (mostrar todos)
                                if (select.value === usuario) {
                                    select.value = '';
                                } else {
                                    select.value = usuario;
                                }
                                var form = select.closest('form');
                                if (form) {
                                    form.scrollIntoView({behavior: 'smooth'});
                                    form.submit();
                                }
                            }
                        }
                    }
                }
            });
        }
        // Movimientos por tipo
        var ctxTipo = document.getElementById('graficaPorTipo');
        if (ctxTipo) {
            var dataTipo = JSON.parse(ctxTipo.getAttribute('data-chart') || '[]');
            var { datos: datosTipo, etiquetas: etiquetasTipo } = agruparDatosEnOtros(
                dataTipo.map(x => x.valor || x.Valor),
                dataTipo.map(x => x.etiqueta || x.Etiqueta)
            );
            new Chart(ctxTipo, {
                type: 'pie',
                data: {
                    labels: etiquetasTipo,
                    datasets: [{
                        data: datosTipo,
                        backgroundColor: colorPalette.slice(0, datosTipo.length), // Colores de la paleta
                        borderColor: '#fff', // Borde blanco
                        borderWidth: 2,
                        hoverBorderWidth: 3
                    }]
                },
                options: {
                    ...commonChartOptions, // Usar opciones comunes
                    plugins: {
                        ...commonChartOptions.plugins, // Heredar plugins comunes
                    },
                    onClick: function(evt, elements) {
                        if (elements && elements.length > 0) {
                            // Si solo hay un segmento, no hacer nada
                            if (this.data.labels.length <= 1) return;
                            var idx = elements[0].index;
                            var tipo = this.data.labels[idx];
                            var select = document.querySelector('select[name="TipoMovimiento"]');
                            if (select) {
                                // Si ya está seleccionado, limpiar filtro (mostrar todos)
                                if (select.value === tipo) {
                                    select.value = '';
                                } else {
                                    select.value = tipo;
                                }
                                var form = select.closest('form');
                                if (form) {
                                    form.scrollIntoView({behavior: 'smooth'});
                                    form.submit();
                                }
                            }
                        }
                    }
                }
            });
        }
    } else {
        console.warn('Chart.js o ChartDataLabels no está cargado.');
    }
};

window.addEventListener('DOMContentLoaded', function() {
    window.initReportesCharts && window.initReportesCharts();
});
