using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcMonitorServer.Data;

namespace PcMonitorServer.Controllers;

[ApiController]
[Route("api/pc")]
public class PcController(AppDbContext db, IConfiguration configuration) : ControllerBase
{
    private readonly int _offlineThresholdSeconds =
        configuration.GetValue<int>("Monitor:OfflineThresholdSeconds", 90);

    // GET /api/pc/status?machineId=PC-CASA
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus([FromQuery] string machineId = "PC-CASA")
    {
        var now = DateTime.UtcNow;
        var machine = await db.Machines
            .FirstOrDefaultAsync(m => m.MachineId == machineId);

        if (machine is null)
            return NotFound(new { error = $"Machine '{machineId}' no registrada." });

        var isOnline = (now - machine.LastSeenAt).TotalSeconds < _offlineThresholdSeconds;

        // Si está offline, cerrar sesión abierta con ended_at = last_seen_at
        if (!isOnline)
        {
            var openSession = await db.Sessions
                .FirstOrDefaultAsync(s => s.MachineId == machineId && s.EndedAt == null);

            if (openSession is not null)
            {
                openSession.EndedAt = machine.LastSeenAt;
                await db.SaveChangesAsync();
            }
        }

        // Sesión actual (abierta) o última cerrada
        var currentSession = await db.Sessions
            .Where(s => s.MachineId == machineId && s.EndedAt == null)
            .FirstOrDefaultAsync();

        var lastSession = await db.Sessions
            .Where(s => s.MachineId == machineId && s.EndedAt != null)
            .OrderByDescending(s => s.EndedAt)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            status = isOnline ? "ONLINE" : "OFFLINE",
            lastContact = machine.LastSeenAt,
            currentSession = currentSession is null ? null : new
            {
                startedAt = currentSession.StartedAt,
                endedAt = currentSession.EndedAt,
                duration = (now - currentSession.StartedAt).ToString(@"h\h\ m\m")
            },
            lastSession = lastSession is null ? null : new
            {
                startedAt = lastSession.StartedAt,
                endedAt = lastSession.EndedAt,
                duration = lastSession.Duration.HasValue
                    ? FormatDuration(lastSession.Duration.Value)
                    : null
            }
        });
    }

    // GET /api/pc/sessions?machineId=PC-CASA&limit=50
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(
        [FromQuery] string machineId = "PC-CASA",
        [FromQuery] int limit = 50)
    {
        var sessions = await db.Sessions
            .Where(s => s.MachineId == machineId && s.EndedAt != null)
            .OrderByDescending(s => s.StartedAt)
            .Take(Math.Min(limit, 200))
            .Select(s => new
            {
                s.Id,
                s.StartedAt,
                s.EndedAt,
                duration = FormatDuration(s.EndedAt!.Value - s.StartedAt)
            })
            .ToListAsync();

        return Ok(sessions);
    }

    private static string FormatDuration(TimeSpan ts)
    {
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        return $"{ts.Minutes}m {ts.Seconds}s";
    }
}
