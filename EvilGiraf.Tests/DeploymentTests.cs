using EvilGiraf.Interface;
using EvilGiraf.Model;
using EvilGiraf.Service;
using FluentAssertions;
using k8s;
using k8s.Autorest;
using k8s.Models;
using NSubstitute;

namespace EvilGiraf.Tests;

public class DeploymentTests
{
    public IDeploymentService DeploymentService { get; }
    public IKubernetes Client { get; }

    public DeploymentTests()
    {
        Client = Substitute.For<IKubernetes>();
        var appv1OperationsSubstitute = Substitute.For<IAppsV1Operations>();
        var deploymentResponse = new HttpOperationResponse<V1Deployment>();
        deploymentResponse.Body = new V1Deployment();
        appv1OperationsSubstitute.CreateNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<V1Deployment>(), Arg.Any<string>())
            .Returns(deploymentResponse);
        Client.AppsV1.Returns(appv1OperationsSubstitute);
        
        DeploymentService = new DeploymentService(Client);
    }
    
    [Fact]
    public async Task CreateDeployment_Should_Return_Deployment()
    {
        var model = new DeploymentModel(
            "deployment-test",
            "default",
            1,
            "nginx",
            [80]
        );
        var result = await DeploymentService.CreateDeployment(model);
        result.Should().NotBeNull()
            .And.BeOfType<V1Deployment>();
        await Client.AppsV1.Received().CreateNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<V1Deployment>(), "default");
    }
}