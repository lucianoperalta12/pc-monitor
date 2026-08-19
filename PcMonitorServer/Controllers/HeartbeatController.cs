using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcMonitorServer.Data;
using PcMonitorServer.Models;

namespace PcMonitorServer.Controllers;

[ApiController]
[Route("api")]
public class HeartbeatController(AppDbContext db) : ControllerBase
{
    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MachineId))
            return BadRequest("machineId es requerido.");

        var now = DateTime.UtcNow;

        // Upsert machine
        var machine = await db.Machines
            .FirstOrDefaultAsync(m => m.MachineId == request.MachineId);

        if (machine is null)
        {
            machine = new Machine
            {
                MachineId = request.MachineId,
                Name = request.MachineId,
                LastSeenAt = now
            };
            db.Machines.Add(machine);
        }
        else
        {
            machine.LastSeenAt = now;
        }

        // Crear sesión si no hay una abierta
        var openSession = await db.Sessions
            .FirstOrDefaultAsync(s => s.MachineId == request.MachineId && s.EndedAt == null);

        if (openSession is null)
        {
            db.Sessions.Add(new Session
            {
                MachineId = request.MachineId,
                StartedAt = now
            });
        }

        await db.SaveChangesAsync();
        return Ok();
    }
}

public record HeartbeatRequest(string MachineId);
