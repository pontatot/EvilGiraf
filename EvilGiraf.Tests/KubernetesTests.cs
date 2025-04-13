using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Interface;
using EvilGiraf.Model;
using EvilGiraf.Service;
using FluentAssertions;
using k8s.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvilGiraf.Tests;

public class KubernetesTests
{
    private readonly IDeploymentService _deploymentService;
    private readonly IGitBuildService _gitBuildService;
    private readonly KubernetesService _kubernetesService;

    public KubernetesTests()
    {
        _deploymentService = Substitute.For<IDeploymentService>();
        _gitBuildService = Substitute.For<IGitBuildService>();
        _kubernetesService = new KubernetesService(_deploymentService, Substitute.For<INamespaceService>(), Substitute.For<IServiceService>(), _gitBuildService);
    }

    private static Application CreateTestApplication() => new()
    {
        Id = 1,
        Name = "test-app",
        Type = ApplicationType.Docker,
        Link = "docker.io/test:latest",
        Version = "1.0.0",
        Ports = [22]
    };

    [Fact]
    public async Task Deploy_ShouldCreateNewDeployment_WhenDeploymentDoesNotExist()
    {
        // Arrange
        var app = CreateTestApplication();
        _deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace())
            .Returns((V1Deployment?)null);
        
        _deploymentService.CreateDeployment(Arg.Any<DeploymentModel>())
            .Returns(new V1Deployment());

        // Act
        await _kubernetesService.Deploy(app);

        // Assert
        await _deploymentService.Received(1)
            .ReadDeployment(app.Name, app.Id.ToNamespace());
        
        await _deploymentService.Received(1)
            .CreateDeployment(Arg.Is<DeploymentModel>(d => 
                d.Name == app.Name && 
                d.Namespace == app.Id.ToNamespace() &&
                d.Image == app.Link &&
                d.Replicas == 1));
        
        await _deploymentService.DidNotReceive()
            .UpdateDeployment(Arg.Any<DeploymentModel>());
    }

    [Fact]
    public async Task Deploy_ShouldUpdateDeployment_WhenDeploymentExists()
    {
        // Arrange
        var app = CreateTestApplication();
        _deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace())
            .Returns(new V1Deployment());
        
        _deploymentService.UpdateDeployment(Arg.Any<DeploymentModel>())
            .Returns(new V1Deployment());

        // Act
        await _kubernetesService.Deploy(app);

        // Assert
        await _deploymentService.Received(1)
            .ReadDeployment(app.Name, app.Id.ToNamespace());
        
        await _deploymentService.DidNotReceive()
            .CreateDeployment(Arg.Any<DeploymentModel>());
        
        await _deploymentService.Received(1)
            .UpdateDeployment(Arg.Is<DeploymentModel>(d => 
                d.Name == app.Name && 
                d.Namespace == app.Id.ToNamespace() &&
                d.Image == app.Link &&
                d.Replicas == 1));
    }

    [Fact]
    public async Task Deploy_ShouldHandleDeploymentServiceErrors()
    {
        // Arrange
        var app = CreateTestApplication();
        _deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace())
            .ThrowsAsync(new Exception("Deployment service error"));

        // Act
        var act = () => _kubernetesService.Deploy(app);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Deployment service error");
        
        await _deploymentService.DidNotReceive()
            .CreateDeployment(Arg.Any<DeploymentModel>());
        
        await _deploymentService.DidNotReceive()
            .UpdateDeployment(Arg.Any<DeploymentModel>());
    }
}