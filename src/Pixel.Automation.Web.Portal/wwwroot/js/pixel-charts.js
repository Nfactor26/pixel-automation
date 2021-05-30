function generateDonutChart(containerId, chartData) {
    var options = {
        chart: {
            type: 'donut',
            height: chartData.height,
            width: chartData.width,
            stacked: true,
            toolbar: {
                show: true
            },
            zoom: {
                enabled: true
            }
        },
        //responsive: [{
        //    breakpoint: 480,
        //    options: {
        //        legend: {
        //            position: 'bottom',
        //            offsetX: 40,
        //            offsetY: 40
        //        }
        //    }
        //}],
        series: chartData.series,
        labels: chartData.labels,
        dataLabels: {
            enabled: true,
            formatter: function (value, { seriesIndex, dataPointIndex, w }) {
                return w.globals.initialSeries[seriesIndex];
            },
        },
        legend: {
            show: false,
            position: 'right',
            offsetY: 60
        },
        plotOptions: {
            pie:
            {
                donut: {
                    customScale: 1,
                  /*  size: '55%',*/
                    labels: {
                        show: true,
                        total: {
                            show: true,
                            showAlways: true,
                            label: 'Total',
                            fontSize: '22px',
                            fontFamily: 'Helvetica, Arial, sans-serif',
                            fontWeight: 600,
                            color: 'black',
                            formatter: function (w) {
                                return w.globals.seriesTotals.reduce((a, b) => {
                                    return a + b
                                }, 0)
                            }
                        }
                    }
                }
            }
        },
        colors: ['#82EE5F', '#E91E63']
    }

    var chart = new ApexCharts(document.querySelector(containerId), options);

    chart.render();
}


function generateExecutionHistoryBarChart(containerId, chartData) {

    var options = {
        series: chartData.series,
        chart: {
            type: 'bar',
            height: 350,
            stacked: true,
            toolbar: {
                show: true
            },
            zoom: {
                enabled: true
            }
        },              
        responsive: [{
            breakpoint: 480,
            options: {
                legend: {
                    position: 'bottom',
                    offsetX: -10,
                    offsetY: 0
                }
            }
        }],    
        colors: chartData.colors,
        plotOptions: {
            bar: {
                borderRadius: 8,
                horizontal: false,
                columnWidth: '45%',
                distributed: false,
            },
        },
        xaxis: {
            categories: chartData.xAxis.categories           
        },
        legend: {
            show: false,
            position: 'right',
            offsetY: 40
        },
        fill: {
            opacity: 1
        }
    };

    var chart = new ApexCharts(document.querySelector(containerId), options);
    chart.render();
}


function generateBarChart(containerId, chartData) {

    var options = {
        series: chartData.series,
        chart: {
            type: 'bar',
            height: chartData.height,
            stacked: true,
            toolbar: {
                show: true
            },
            zoom: {
                enabled: true
            }
        },
        responsive: [{
            breakpoint: 480,
            options: {
                legend: {
                    position: 'bottom',
                    offsetX: -10,
                    offsetY: 0
                }
            }
        }],
        colors: chartData.colors,
        plotOptions: {
            bar: {
                borderRadius: 8,
                horizontal: false,
                columnWidth: '20%',
                distributed: chartData.plotOptions.distributed,
            },
        },
        xaxis: {
            categories: chartData.xAxis.categories,
            //labels: {
            //    style: {
            //        colors: chartData.colors.length ? chartData.colors : 'undefined',
            //        fontSize: '12px'
            //    }
            //}
        },
        legend: {
            show: false,
            position: 'right',
            offsetY: 40
        },
        fill: {
            opacity: 1
        }
    };

    var chart = new ApexCharts(document.querySelector(containerId), options);
    chart.render();
}