using _1lab.Models;

namespace _1lab.Entities;

public class Pass
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Document { get; set; } = "";
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public PassStatus Status { get; set; }
}
