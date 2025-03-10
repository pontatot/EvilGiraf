using System.ComponentModel.DataAnnotations;

namespace EvilGiraf.Model;

public class Application
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public ApplicationType Type { get; set; }
    public string Link { get; set; }
    public string? Version { get; set; }
}