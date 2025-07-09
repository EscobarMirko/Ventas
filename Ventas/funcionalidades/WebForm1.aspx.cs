using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Data;
using System.ComponentModel;

namespace Ventas.funcionalidades
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        // Método que realiza el procesamiento del botón "Procesar".
        protected void btnProcesar_Click(object sender, EventArgs e)
        {
            // Validación del archivo.
            if (FileUpload1.HasFile)
            {
                using (var stream = FileUpload1.PostedFile.InputStream)
                {
                    IWorkbook workbook = new XSSFWorkbook(stream); // Carga el archivo .xlsx
                    ISheet sheet = workbook.GetSheetAt(0); // Obtiene la primera hoja del archivo .xlsx

                    var tablaVentas = new DataTable(); // Genera un DataTable para almacenar los datos de ventas, especificando las columnas que se van a utilizar.
                    tablaVentas.Columns.Add("Año");
                    tablaVentas.Columns.Add("MontoVenta", typeof(double));
                    tablaVentas.Columns.Add("Factura");
                    tablaVentas.Columns.Add("OrdenCompra");
                    tablaVentas.Columns.Add("Vendedor");

                    var ventasPorAnio = new Dictionary<string, double>(); // Acumula las ventas por año
                    var ventasPorVendedorAnio = new Dictionary<string, Dictionary<string, double>>(); // Acumula las ventas por vendedor en cada año

                    // Leer desde fila 1 (índice 0 = cabecera)
                    for (int i = 1; i <= sheet.LastRowNum; i++) // Recorrer las filas de la hoja de cálculo, comenzando desde la fila 1 (índice 0 es la cabecera).
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; // Si la fila es nula, se salta a la siguiente iteración.

                        // Extraer los datos de cada celda de la fila actual y convertirlos al tipo adecuado.
                        string anio = row.GetCell(0)?.ToString();
                        double monto = double.TryParse(row.GetCell(1)?.ToString(), out double val) ? val : 0;
                        string factura = row.GetCell(2)?.ToString();
                        string ordenCompra = row.GetCell(3)?.ToString();
                        string vendedor = row.GetCell(4)?.ToString();

                        tablaVentas.Rows.Add(anio, monto, factura, ordenCompra, vendedor); // Agregar fila a la tabla de detalles

                        // Acumular las ventas por año.
                        if (!ventasPorAnio.ContainsKey(anio))
                            ventasPorAnio[anio] = 0;
                        ventasPorAnio[anio] += monto;

                        // Acumular ventas por vendedor en cada año
                        if (!ventasPorVendedorAnio.ContainsKey(anio))
                            ventasPorVendedorAnio[anio] = new Dictionary<string, double>();

                        if (!ventasPorVendedorAnio[anio].ContainsKey(vendedor))
                            ventasPorVendedorAnio[anio][vendedor] = 0;

                        ventasPorVendedorAnio[anio][vendedor] += monto;
                    }

                    // Mostrar tabla
                    GridView1.DataSource = tablaVentas;
                    GridView1.DataBind();

                    // Generar gráficos
                    LiteralGrafico.Text = GenerarGrafico(ventasPorAnio); // Gráfico de ventas por año
                    LiteralGraficoVendedores.Text = GenerarGraficoPorVendedor(ventasPorVendedorAnio); // Gráfico de ventas por vendedor en cada año
                }
            }
        }

        // Método para generar gráfico de barras por año (suma total por año)
        private string GenerarGrafico(Dictionary<string, double> datos)
        {
            string labels = string.Join(",", datos.Keys.Select(a => $"'{a}'"));
            string values = string.Join(",", datos.Values);

            return $@"
            <div style='width: 100%; max-width: 700px; margin: 30px auto;'>
                <canvas id='graficoVentas'></canvas>
            </div>
            <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
            <script>
                var ctx = document.getElementById('graficoVentas').getContext('2d');
                new Chart(ctx, {{
                    type: 'bar',
                    data: {{
                        labels: [{labels}],
                        datasets: [{{
                            label: 'Ventas por Año',
                            data: [{values}],
                            backgroundColor: [
                                'rgba(75, 192, 192, 0.5)',
                                'rgba(255, 99, 132, 0.5)',
                                'rgba(255, 206, 86, 0.5)',
                                'rgba(54, 162, 235, 0.5)',
                                'rgba(153, 102, 255, 0.5)'
                            ],
                            borderColor: [
                                'rgba(75, 192, 192, 1)',
                                'rgba(255, 99, 132, 1)',
                                'rgba(255, 206, 86, 1)',
                                'rgba(54, 162, 235, 1)',
                                'rgba(153, 102, 255, 1)'
                            ],
                            borderWidth: 1
                        }}]
                    }},
                    options: {{
                        responsive: true,
                        plugins: {{
                            legend: {{
                                display: true,
                                position: 'top'
                            }},
                            title: {{
                                display: true,
                                text: 'Resumen de Ventas por Año',
                                font: {{
                                    size: 18
                                }}
                            }}
                        }},
                        scales: {{
                            y: {{
                                beginAtZero: true,
                                ticks: {{
                                    callback: function(value) {{
                                        return '$' + value.toLocaleString();
                                    }}
                                }}
                            }}
                        }}
                    }}
                }});
            </script>";
        }

        // Método para generar gráfico de ventas por vendedor en cada año
        private string GenerarGraficoPorVendedor(Dictionary<string, Dictionary<string, double>> datos)
        {
            var vendedores = datos.Values.SelectMany(dict => dict.Keys).Distinct().ToList(); // Lista única de vendedores
            var anios = datos.Keys.OrderBy(x => x).ToList(); // Lista de años ordenada

            var datasets = new List<string>();
            var colores = new[] {
                "rgba(255, 99, 132, 0.5)", "rgba(54, 162, 235, 0.5)", "rgba(255, 206, 86, 0.5)",
                "rgba(75, 192, 192, 0.5)", "rgba(153, 102, 255, 0.5)", "rgba(255, 159, 64, 0.5)"
            };

            for (int i = 0; i < vendedores.Count; i++)
            {
                string vendedor = vendedores[i];
                string color = colores[i % colores.Length];

                // Construir los datos para cada vendedor por año
                var data = string.Join(",", anios.Select(anio =>
                    datos.ContainsKey(anio) && datos[anio].ContainsKey(vendedor)
                        ? datos[anio][vendedor].ToString()
                        : "0"
                ));

                datasets.Add($@"
                {{
                    label: '{vendedor}',
                    data: [{data}],
                    backgroundColor: '{color}',
                    borderColor: '{color.Replace("0.5", "1")}',
                    borderWidth: 1
                }}");
            }

            string labels = string.Join(",", anios.Select(a => $"'{a}'"));

            return $@"
            <div style='width: 100%; max-width: 700px; margin: 30px auto;'>
                <canvas id='graficoVendedores'></canvas>
            </div>
            <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
            <script>
                var ctx2 = document.getElementById('graficoVendedores').getContext('2d');
                new Chart(ctx2, {{
                    type: 'bar',
                    data: {{
                        labels: [{labels}],
                        datasets: [{string.Join(",", datasets)}]
                    }},
                    options: {{
                        responsive: true,
                        plugins: {{
                            legend: {{ position: 'top' }},
                            title: {{
                                display: true,
                                text: 'Ventas por Vendedor en Cada Año',
                                font: {{ size: 18 }}
                            }}
                        }},
                        scales: {{
                            y: {{
                                beginAtZero: true,
                                ticks: {{
                                    callback: function(value) {{
                                        return '$' + value.toLocaleString();
                                    }}
                                }}
                            }}
                        }}
                    }}
                }});
            </script>";
        }
    }
}

