using EvilGiraf.Model;

namespace EvilGiraf.Dto;

public record ApplicationCreateDto(
    string Name,
    ApplicationType Type,
    string Link,
    string? Version,
    int[]? Ports
);

public record ApplicationResultDto(
    int Id,
    string Name,
    ApplicationType Type,
    string Link,
    string? Version,
    int[] Ports
);

public record ApplicationUpdateDto(
    string? Name,
    ApplicationType? Type,
    string? Link,
    string? Version,
    int[]? Ports
);
