using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Usuario.Response
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string[] Errors { get; set; }
    }
}
