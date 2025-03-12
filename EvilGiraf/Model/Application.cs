using System.ComponentModel.DataAnnotations;

namespace EvilGiraf.Model;

public class Application
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }
    
    public ApplicationType Type { get; init; }
    
    [Required]
    [MaxLength(255)]
    public required string Link { get; init; }
    
    [MaxLength(255)]
    public string? Version { get; init; }
}