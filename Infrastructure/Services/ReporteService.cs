using Application.Interfaces;
using Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastReport;
using FastReport.Export.PdfSimple;
using System.Data;

namespace Infrastructure.Services
{
    public class ReporteService : IReporteService
    {
        private IPedidoRepository _pedidoRepository;
        private readonly string _rutaReporte;

        public ReporteService(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository ?? throw new ArgumentNullException(nameof(pedidoRepository));
            _rutaReporte = Path.Combine(AppContext.BaseDirectory, "Reportes", "Pedidos.frx");
        }

        public async Task<byte[]> GenerarDetallePedidoPdfAsync(int pedidoId)
        {
            using var report = await BuildReportAsync(pedidoId);
            using var ms = new MemoryStream();
            var pdf = new PDFSimpleExport();
            report.Prepare();
            report.Export(pdf, ms);
            return ms.ToArray();
        }

        private async Task<Report> BuildReportAsync(int pedidoId)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(pedidoId);
            if (pedido == null) throw new Exception("Pedido no encontrado");

            var report = new Report();
            if(!File.Exists(_rutaReporte))
            {
                throw new FileNotFoundException("Plantilla de reporte no encontrada", _rutaReporte);
            }

            report.Load(_rutaReporte);

            var dtPedido = new DataTable("Pedido");
            dtPedido.Columns.Add("PedidoId", typeof(int));
            dtPedido.Columns.Add("Total", typeof(decimal));

            dtPedido.Rows.Add(
                pedido.PedidoId,
                pedido.Total
            );

            // DataTable Detalles (detalle)
            var dtDetalles = new DataTable("Detalles");
            dtDetalles.Columns.Add("ProductoCodigo", typeof(string));
            dtDetalles.Columns.Add("ProductoNombre", typeof(string));
            dtDetalles.Columns.Add("Cantidad", typeof(int));

            foreach (var d in pedido.Detalles)
            {
                var subtotalLinea = (d.Cantidad * d.PrecioUnitario) - d.Descuento;
                var iva = d.TieneIVA ? subtotalLinea * 0.15m : 0m;

                dtDetalles.Rows.Add(
                    d.Producto?.Codigo ?? d.ProductoId.ToString(),
                    d.Producto?.Nombre ?? "",
                    d.Cantidad,
                    d.PrecioUnitario
                );
            }

            // Registrar datos
            report.RegisterData(dtPedido, "Pedido");
            report.RegisterData(dtDetalles, "Detalles");

            // Asegurar que estén habilitadas las fuentes
            var dsPedido = report.GetDataSource("Pedido");
            if (dsPedido != null) dsPedido.Enabled = true;

            var dsDetalles = report.GetDataSource("Detalles");
            if (dsDetalles != null) dsDetalles.Enabled = true;

            return report;
        }
    }
}
