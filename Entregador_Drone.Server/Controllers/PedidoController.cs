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
        public async Task<ActionResult<Pedido>> Post([FromBody] Pedido pedido)
        {
            _context.Pedido.Add(pedido);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = pedido.Id }, pedido);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Pedido pedido)
        {
            if (id != pedido.Id)
            {
                return BadRequest("ID do pedido não corresponde ao ID fornecido na URL.");
            }

            _context.Entry(pedido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Pedido.AnyAsync(b => b.Id == id))
                {
                    return NotFound($"Pedido com ID {id} não encontrado.");
                }

                throw;

            }

            return NoContent();
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
