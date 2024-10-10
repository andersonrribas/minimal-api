using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using proj_minimal_api.Domain.Enuns;

namespace proj_minimal_api.Domain.ModelViews
{
    public record AdministratorModelView
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string Perfil { get; set; } = default!;
    }
}