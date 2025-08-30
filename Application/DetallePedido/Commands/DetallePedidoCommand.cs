using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DetallePedido.Commands
{
    public class DetallePedidoCommand
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal? Descuento { get; set; }
        public bool TieneIVA { get; set; }
    }
}
