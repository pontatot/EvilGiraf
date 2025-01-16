namespace EvilGiraf.Dto;

public record DeployGithubRequest(
    string Name,
    string Link,
    string? Version
);