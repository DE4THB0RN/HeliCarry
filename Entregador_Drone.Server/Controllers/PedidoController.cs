using Entregador_Drone.Server.Modelos;
using Entregador_Drone.Server.Serviços;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Entregador_Drone.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly ILogger<PedidoController> _logger;
        private readonly AppDbContext _context;

        public PedidoController(ILogger<PedidoController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> Get()
        {
            var pedidos = await _context.Pedido.ToListAsync();
            if (pedidos == null || pedidos.Count == 0)
            {
                return NotFound("Nenhum pedido encontrado.");
            }
            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetById(int id)
        {
            var pedido = await _context.Pedido.FindAsync(id);
            if (pedido == null)
            {
                return NotFound($"Pedido com ID {id} não encontrado.");
            }
            return Ok(pedido);
        }

        [HttpPost]
        public async Task<ActionResult<Pedido>> Post([FromBody] PedidoDto pedidoDto)
        {
            // 1. Encontre o nó do cliente com base no ID recebido do frontend
            var localizacaoCliente = await _context.C_No.FindAsync(pedidoDto.LocalizacaoClienteId);

            // 2. Verifique se o nó existe
            if (localizacaoCliente == null)
            {
                return NotFound("Localização do cliente não encontrada.");
            }

            // 3. Crie a entidade Pedido a partir do DTO
            var novoPedido = new Pedido
            {
                LocalizacaoCliente = localizacaoCliente,
                LocalizacaoClienteId = localizacaoCliente.Id,
                Peso = pedidoDto.Peso,
                Prioridade = pedidoDto.Prioridade,
                Status = StatusPedido.Pendente,
                TempoCriacao = DateTime.Now
            };

            // 4. Salve a nova entidade no banco de dados
            _context.Pedido.Add(novoPedido);
            await _context.SaveChangesAsync();

            // 5. Retorne o pedido criado
            return CreatedAtAction(nameof(GetById), new { id = novoPedido.Id }, novoPedido);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pedido = await _context.Pedido.FindAsync(id);

            if (pedido == null)
            {
                return NotFound($"Pedido com ID {id} não encontrado.");
            }

            _context.Pedido.Remove(pedido);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
