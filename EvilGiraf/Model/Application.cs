namespace EvilGiraf.Model;

public record Application(int Id, string Name, ApplicationType Type, string Link, string? Version);