using Application.Producto.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Producto.Validators
{
    public class ActualizarProductoCommandValidator : AbstractValidator<ActualizarProductoCommand>
    {
        public ActualizarProductoCommandValidator() 
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del producto es obligatorio")
                .MaximumLength(50).WithMessage("El nombre del producto no puede exceder los 100 caracteres");

            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("El código del producto es obligatorio")
                .MaximumLength(50).WithMessage("El código del producto no puede exceder los 50 caracteres");

            RuleFor(x => x.PrecioVenta)
                .GreaterThan(0).WithMessage("El precio del producto debe ser mayor que 0");

            RuleFor(x => x.Existencias)
                .GreaterThanOrEqualTo(0).WithMessage("Las existencias del producto no pueden ser negativas");
        }
    }
}
