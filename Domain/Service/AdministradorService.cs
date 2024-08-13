using minimal_api.Domain.DTO;
using minimal_api.Domain.Entity;
using minimal_api.Domain.Interface;
using minimal_api.Domain.ModelViews;
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

        public Administrador Incluir(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();

            return administrador;
        }

        public Administrador? BuscarPorId(int id)
        {
            return _contexto.Administradores.Where(a => a.ID == id).FirstOrDefault();
        }

        public List<Administrador> Todos(int? pagina)
        {
            var query = _contexto.Administradores.AsQueryable();

            int itensPorPagina = 10;

            if (pagina.HasValue)
            {
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }

            return query.ToList();
        }        
    }
}
