namespace EvilGiraf.Dto;

public record DeleteDeploymentRequest(
    string Name,
    string Namespace
);