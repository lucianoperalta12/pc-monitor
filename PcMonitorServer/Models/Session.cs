using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcMonitorServer.Models;

[Table("sessions", Schema = "pc_monitor")]
public class Session
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("machine_id")]
    [MaxLength(100)]
    public string MachineId { get; set; } = string.Empty;

    [Column("started_at")]
    public DateTime StartedAt { get; set; }

    [Column("ended_at")]
    public DateTime? EndedAt { get; set; }

    [NotMapped]
    public TimeSpan? Duration => EndedAt.HasValue ? EndedAt.Value - StartedAt : null;
}
