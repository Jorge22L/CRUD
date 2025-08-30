using Application.DetallePedido.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Pedidos.Commands
{
    public class CrearPedidoCommand
    {
        public int ClienteId { get; set; }
        public string FormaPago { get; set; } = string.Empty;
        public string Estado { get; set; }
        public List<DetallePedidoCommand> Detalles { get; set; } = new List<DetallePedidoCommand>();
    }
}
