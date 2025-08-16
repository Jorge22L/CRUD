using Application.Clientes.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clientes.Validators
{
    public class ActualizarClienteCommandValidator : AbstractValidator<ActualizarClienteCommand>
    {
        public ActualizarClienteCommandValidator()
        {
            RuleFor(x => x.Cedula)
                .NotEmpty().WithMessage("Cédula/Ruc es requerida")
                .MaximumLength(20).WithMessage("Máximo 20 caracteres");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("Nombre es requerido")
                .MaximumLength(100).WithMessage("Máximo 100 caracteres");
        }
    }
}
