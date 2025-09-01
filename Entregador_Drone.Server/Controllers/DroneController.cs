using Entregador_Drone.Server.Modelos;
using Entregador_Drone.Server.Serviços;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Entregador_Drone.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DroneController : ControllerBase
    {
        private readonly ILogger<DroneController> _logger;
        private readonly AppDbContext _context;

        public DroneController(ILogger<DroneController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Drone>>> Get()
        {
            var drones = await _context.Drone.ToListAsync();
            if (drones == null || drones.Count == 0)
            {
                return NotFound("Nenhuma entrega encontrada.");
            }
            return Ok(drones);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Drone>> GetById(int id)
        {
            var drone = await _context.Drone.FindAsync(id);
            if (drone == null)
            {
                return NotFound($"Drone com ID {id} não encontrado.");
            }
            return Ok(drone);
        }

        [HttpPost]
        public async Task<ActionResult<Drone>> Post([FromBody] DroneDto droneDto)
        {
            var baseNode = await _context.C_No.FirstOrDefaultAsync(n => n.IsBase);

            if (baseNode == null)
            {
                // Se não houver uma base cadastrada, retorna um erro 404 ou 500.
                // O 404 (Not Found) é uma opção válida, pois o recurso (a base) não existe.
                return NotFound("Nenhuma base de drone foi encontrada para registrar o novo drone.");
            }

            // Cria uma nova instância do Drone a partir do DTO
            var novoDrone = new Drone
            {
                CapacidadeMaximaKg = droneDto.CapacidadeMaxKg,
                AutonomiaKm = droneDto.AutonomiaKm,
                BateriaAtual = droneDto.BateriaAtual,
                ConsumoPorKm = droneDto.ConsumoPorKm,
                ConsumoPorSegundo = droneDto.ConsumoPorSegundo,
                // Atribui a base de operações encontrada como a localização inicial
                LocalizacaoAtual = baseNode,
                Status = "Idle"
            };

            _context.Drone.Add(novoDrone);
            await _context.SaveChangesAsync();

            // Retorna o novo drone criado com a localização base
            return CreatedAtAction(nameof(Get), new { id = novoDrone.Id }, novoDrone);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Drone drone)
        {
            if (id != drone.Id)
            {
                return BadRequest("ID do drone não corresponde ao ID fornecido na URL.");
            }

            _context.Entry(drone).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Drone.AnyAsync(b => b.Id == id))
                {
                    return NotFound($"Drone com ID {id} não encontrado.");
                }

                throw;

            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var drone = await _context.Drone.FindAsync(id);

            if (drone == null)
            {
                return NotFound($"Drone com ID {id} não encontrado.");
            }

            _context.Drone.Remove(drone);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
