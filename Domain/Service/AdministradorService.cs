using minimal_api.Domain.DTO;
using minimal_api.Domain.Entity;
using minimal_api.Domain.Interface;
using minimal_api.Infraestrutura.Db;

namespace minimal_api.Domain.Service
{
    public class AdministradorService : IAdministradorService
    {
        private readonly DBContexto _contexto;
        public AdministradorService(DBContexto db)
        {
            _contexto = db;
        }
        public Administrador? Login(LoginDTO loginDTO)
        {            
            return _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        }
    }
}
