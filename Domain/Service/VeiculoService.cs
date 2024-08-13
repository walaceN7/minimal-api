using minimal_api.Domain.Entity;
using minimal_api.Domain.Interface;
using minimal_api.Infraestrutura.Db;

namespace minimal_api.Domain.Service
{
    public class VeiculoService : IVeiculosService
    {
        private readonly DBContexto _contexto;
        public VeiculoService(DBContexto contexto)
        {
            _contexto = contexto;
        }

        public void Apagar(Veiculo veiculo)
        {
            _contexto.Remove(veiculo);
            _contexto.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _contexto.Update(veiculo);
            _contexto.SaveChanges();
        }

        public Veiculo? BuscaPorId(int id)
        {
            return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Incluir(Veiculo veiculo)
        {
            _contexto.Veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _contexto.Veiculos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(v => v.Nome.ToLower().Contains(nome));
            }

            int itensPorPagina = 10;

            if (pagina.HasValue)
            {
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }

            return query.ToList();
        }
    }
}
