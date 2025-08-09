using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class DetallePedido
    {
        [Key]
        public int DetalleId { get; set; }
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; } = null!;
        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public bool TieneIVA { get; set; }

        // Campos calculados
        [NotMapped]
        public decimal SubTotalLinea => (Cantidad * PrecioUnitario) - Descuento;

        [NotMapped]
        public decimal IVA => TieneIVA ? SubTotalLinea * 0.15m : 0;

    }
}
