using System.Net;
using EvilGiraf.Interface;
using EvilGiraf.Model;
using EvilGiraf.Service;
using FluentAssertions;
using k8s;
using k8s.Autorest;
using k8s.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

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

        var deleteResponse = new HttpOperationResponse<V1Status>();
        deleteResponse.Body = new V1Status();
        appv1OperationsSubstitute.DeleteNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(deleteResponse);

        var updateResponse = new HttpOperationResponse<V1Deployment>();
        updateResponse.Body = new V1Deployment();
        appv1OperationsSubstitute.ReplaceNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<V1Deployment>(), Arg.Any<string>(), Arg.Any<string>(), null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(updateResponse);

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
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Deployment>();
        await Client.AppsV1.Received().CreateNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<V1Deployment>(), "default");
    }
    
    [Fact]
    public async Task ReadDeployment_Should_Return_Deployment()
    {
        var deploymentName = "deployment-test";
        var @namespace = "default";

        var deploymentResponse = new HttpOperationResponse<V1Deployment>
        {
            Body = new V1Deployment
            {
                Metadata = new V1ObjectMeta
                {
                    Name = deploymentName,
                    NamespaceProperty = @namespace
                }
            }
        };

        Client.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(deploymentName, @namespace, null, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(deploymentResponse));

        var result = await DeploymentService.ReadDeployment(deploymentName, @namespace);

        result.Metadata.Name.Should().Be(deploymentName);
        result.Metadata.NamespaceProperty.Should().Be(@namespace);
        
        await Client.AppsV1.Received().ReadNamespacedDeploymentWithHttpMessagesAsync(deploymentName, @namespace, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadDeployment_Should_Throw_KeyNotFoundException_When_NotFound()
    {
        var deploymentName = "non-existent-deployment";
        var @namespace = "default";

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        Client.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(deploymentName, @namespace, null, null, Arg.Any<CancellationToken>())
            .Throws(httpException);

        await DeploymentService.Invoking(x => x.ReadDeployment(deploymentName, @namespace))
            .Should().ThrowAsync<KeyNotFoundException>();

        await Client.AppsV1.Received().ReadNamespacedDeploymentWithHttpMessagesAsync(deploymentName, @namespace, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteDeployment_Should_Return_Status()
    {
        var result = await DeploymentService.DeleteDeployment("deployment-test", "default");
        result.Should().NotBeNull()
            .And.BeOfType<V1Status>();
        await Client.AppsV1.Received().DeleteNamespacedDeploymentWithHttpMessagesAsync("deployment-test", "default");
    }

    [Fact]
    public async Task DeleteDeployment_Should_Throw_Exception()
    {
        Client.AppsV1.DeleteNamespacedDeploymentWithHttpMessagesAsync("deployment-test", "default")
            .Returns<Task<HttpOperationResponse<V1Status>>>(x => throw new Exception("Error"));
        Func<Task> act = async () => await DeploymentService.DeleteDeployment("deployment-test", "default");
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task UpdateDeployment_Should_Return_Deployment()
    {
        var model = new DeploymentModel(
            "deployment-test",
            "default",
            1,
            "nginx",
            [80]
        );
        var result = await DeploymentService.UpdateDeployment(model);
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Deployment>();
        await Client.AppsV1.Received().ReplaceNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<V1Deployment>(), "deployment-test", Arg.Any<string>(), null, null, null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDeployment_Should_Throw_Exception()
    {
        var model = new DeploymentModel(
            "deployment-test",
            "default",
            1,
            "nginx",
            [80]
        );
        Client.AppsV1.ReplaceNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<V1Deployment>(), "deployment-test", Arg.Any<string>(), null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns<Task<HttpOperationResponse<V1Deployment>>>(x => throw new Exception("Error"));
        Func<Task> act = async () => await DeploymentService.UpdateDeployment(model);
        await act.Should().ThrowAsync<Exception>();
    }
}