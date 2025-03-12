using System.ComponentModel.DataAnnotations;

namespace EvilGiraf.Model;

public class Application
{
    [Key]
    public int Id { get; init; }
    
    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }
    
    public ApplicationType Type { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Link { get; set; }
    
    [MaxLength(255)]
    public string? Version { get; set; }
}