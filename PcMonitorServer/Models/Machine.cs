using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcMonitorServer.Models;

[Table("machines", Schema = "pc_monitor")]
public class Machine
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("machine_id")]
    [MaxLength(100)]
    public string MachineId { get; set; } = string.Empty;

    [Column("name")]
    [MaxLength(200)]
    public string? Name { get; set; }

    [Column("last_seen_at")]
    public DateTime LastSeenAt { get; set; }
}
