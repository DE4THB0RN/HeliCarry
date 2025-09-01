using Entregador_Drone.Server.Serviços;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Entregador_Drone.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CidadeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CidadeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("estado")]
        public IActionResult GetEstadoCidade()
        {
            try
            {
                // 🔹 Limitar quantidade de nós retornados para não sobrecarregar o front
                var nos = _context.C_No.ToList();

                // 🔹 Drones agora possuem LocalizacaoAtual (referência a um No)
                var drones = _context.Drone
                    .Include(d => d.LocalizacaoAtual)
                    .ToList();

                // 🔹 Carregar pedidos com a localização do cliente
                var pedidos = _context.Pedido
                    .Include(p => p.LocalizacaoCliente)
                    .ToList();

                return Ok(new
                {
                    nos,
                    drones,
                    pedidos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar estado da cidade: {ex.Message}");
            }
        }
        
    }
}
