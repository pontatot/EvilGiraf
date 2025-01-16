namespace EvilGiraf.Dto;

public record DeployDockerRequest(
    string Name,
    string Link,
    string? Secret,
    string? Version
);