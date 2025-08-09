using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Cliente
    {
        public int ClienteID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Cedula {  get; set; }
        public string? Telefono {  get; set; } 
        public string? Direccion {  get; set; } 
        public bool EsConsumidorFinal {  get; set; }

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
