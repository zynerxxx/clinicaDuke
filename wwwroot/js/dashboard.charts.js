// Chart.js CDN loader for dashboard
(function() {
    if (!window.Chart) {
        var script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/chart.js';
        script.onload = function() {
            // Cargar plugin datalabels después de Chart.js
            var script2 = document.createElement('script');
            script2.src = 'https://cdn.jsdelivr.net/npm/chartjs-plugin-datalabels';
            script2.onload = function() {
                window.ChartDataLabels = window.ChartDataLabels || window['ChartDataLabels'];
                window.dispatchEvent(new Event('chartjs:loaded'));
            };
            document.head.appendChild(script2);
        };
        document.head.appendChild(script);
    } else {
        // Si Chart ya está, cargar solo datalabels
        var script2 = document.createElement('script');
        script2.src = 'https://cdn.jsdelivr.net/npm/chartjs-plugin-datalabels';
        script2.onload = function() {
            window.ChartDataLabels = window.ChartDataLabels || window['ChartDataLabels'];
            window.dispatchEvent(new Event('chartjs:loaded'));
        };
        document.head.appendChild(script2);
    }
})();
