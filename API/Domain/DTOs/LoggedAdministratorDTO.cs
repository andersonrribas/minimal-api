using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proj_minimal_api.Domain.DTOs
{
    public record LoggedAdministratorDTO
    {
        public string Email { get; set; } = default!;
        public string Profile { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}