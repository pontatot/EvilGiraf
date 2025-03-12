using k8s.Models;

namespace EvilGiraf.Dto;

public record DeployResponse(
    V1DeploymentStatus Status
);