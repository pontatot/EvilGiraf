using EvilGiraf.Model;

namespace EvilGiraf.Dto;

public record ApplicationCreateDto(
    string Name,
    ApplicationType Type,
    string Link,
    string? Version,
    int? Port,
    string? DomainName,
    string[]? Variables
);

public record ApplicationResultDto(
    int Id,
    string Name,
    ApplicationType Type,
    string Link,
    string? Version,
    int? Port,
    string? DomainName,
    string[] Variables
);

public record ApplicationUpdateDto(
    string? Name,
    ApplicationType? Type,
    string? Link,
    string? Version,
    int? Port,
    string? DomainName,
    string[]? Variables
);
