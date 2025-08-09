using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Producto
    {
        [Key]
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre {  get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public int Existencias { get; set; }
        public bool? TieneIVA { get; set; }
    }
}
