using EvilGiraf.Model;

namespace EvilGiraf.Dto;

public record ApplicationDto(
    string Name,
    ApplicationType Type,
    string Link,
    string? Version
);
