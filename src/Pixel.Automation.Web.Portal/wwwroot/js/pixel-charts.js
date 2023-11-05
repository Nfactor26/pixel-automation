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
    var chartPlaceHolder = document.querySelector(containerId);
    chartPlaceHolder.innerHTML = '';
    var chart = new ApexCharts(chartPlaceHolder, options);

    chart.render();
}

function generateRadarChart(containerId, chartData) {    
    var options = {
        series: chartData.series,  
        chart: {
            type: 'radar',
            height: chartData.height,
            width: chartData.width,
            toolbar: {
                show: true              
            },
            zoom: {
                enabled: true
            }
        },        
        stroke: {
            show: true,
            width: 2,
            colors: [],
            dashArray: 0
        },
        colors: ['#FF4560'],
        markers: {
            size: 4,
            colors: ['#fff'],
            strokeColor: '#FF4560',
            strokeWidth: 2,
        },
        tooltip: {
            y: {
                formatter: function (val) {
                    return val
                }
            }
        },       
        responsive: [{
            breakpoint: 300,
            options: {
                legend: {
                    position: 'bottom',
                    offsetX: -10,
                    offsetY: 0
                }
            }
        }],        
        xaxis: {
            categories: chartData.xAxis.categories           
        },        
        yaxis: {           
            labels: {
                formatter: function (val) {
                    return val.toFixed(2)
                }
            },
        },
        legend: {
            show: false,
            position: 'right',
            offsetY: 40
        },
        fill: {
            opacity: 0.2
        },
        dataLabels: {
            enabled: true,
            formatter: function (val, opts) {
                return val.toFixed(2)
            },
            background: {
                enabled: true,
                borderRadius: 2,
            }
        },
        plotOptions: {
            radar: {
                size: 100,
                polygons: {
                    strokeColors: '#e9e9e9',
                    fill: {
                        colors: ['#f8f8f8', '#fff']
                    }
                }
            }
        },
    };
    var chartPlaceHolder = document.querySelector(containerId);
    chartPlaceHolder.innerHTML = '';
    var chart = new ApexCharts(chartPlaceHolder, options);
    chart.render();
}

function generateBarChart(containerId, chartData) {   
    var options = {
        series: chartData.series,
        chart: {
            type: 'bar',
            height: chartData.height,
            width: chartData.width,
            stacked: true,
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true,
                    customIcons: []
                }
            },
            zoom: {
                enabled: false
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
                borderRadius: 2,
                horizontal: false,
                columnWidth: '10%',
                distributed: chartData.plotOptions.distributed,
            },
        },
        xaxis: {
            categories: chartData.xAxis.categories,
            tickPlacement: 'on'
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

    var chartPlaceHolder = document.querySelector(containerId);
    chartPlaceHolder.innerHTML = '';
    var chart = new ApexCharts(chartPlaceHolder, options);
    chart.render();
}