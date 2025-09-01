using Entregador_Drone.Server.Modelos;
using Entregador_Drone.Server.Serviços;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Entregador_Drone.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntregaController : ControllerBase
    {
        private readonly ILogger<EntregaController> _logger;
        private readonly AppDbContext _context;

        public EntregaController(ILogger<EntregaController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Entrega>>> Get()
        {
            var entregas = await _context.Entrega.ToListAsync();
            if (entregas == null || entregas.Count == 0)
            {
                return NotFound("Nenhuma entrega encontrada.");
            }
            return Ok(entregas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Entrega>> GetById(int id)
        {
            var entrega = await _context.Entrega.FindAsync(id);
            if (entrega == null)
            {
                return NotFound($"Entrega com ID {id} não encontrada.");
            }
            return Ok(entrega);
        }

        [HttpPost]
        public async Task<ActionResult<Entrega>> Post([FromBody] Entrega entrega)
        {
            _context.Entrega.Add(entrega);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = entrega.Id }, entrega);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Entrega entrega)
        {
            if (id != entrega.Id)
            {
                return BadRequest("ID da entrega não corresponde ao ID fornecido na URL.");
            }

            _context.Entry(entrega).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Entrega.AnyAsync(b => b.Id == id))
                {
                    return NotFound($"Entrega com ID {id} não encontrada.");
                }

                throw;

            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Entrega.FindAsync(id);

            if (cliente == null)
            {
                return NotFound($"Entrega com ID {id} não encontrada.");
            }

            _context.Entrega.Remove(cliente);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
