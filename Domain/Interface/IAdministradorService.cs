using minimal_api.Domain.DTO;
using minimal_api.Domain.Entity;

namespace minimal_api.Domain.Interface
{
    public interface IAdministradorService
    {
        Administrador? Login(LoginDTO loginDTO);
        Administrador Incluir(Administrador administrador);
        Administrador? BuscarPorId(int id);
        List<Administrador> Todos(int? pagina);
    }
}
