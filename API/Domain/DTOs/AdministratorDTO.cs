using proj_minimal_api.Domain.Enum;

namespace MinimalApi.DTOs;

public record AdministratorDTO
{
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = default!;
    public PerfilEnum? Perfil { get; set; } = default!;
}