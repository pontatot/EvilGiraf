using System.Net;
using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Model;
using EvilGiraf.Service.Kubernetes;
using FluentAssertions;
using k8s;
using k8s.Autorest;
using k8s.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvilGiraf.Tests;

public class DeploymentTests
{
    private IDeploymentService DeploymentService { get; }
    private IKubernetes Client { get; }

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
        
        var corev1OperationsSubstitute = Substitute.For<ICoreV1Operations>();
        corev1OperationsSubstitute.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>()).Returns(new HttpOperationResponse<V1Namespace>());
        
        Client.CoreV1.Returns(corev1OperationsSubstitute);
        
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
            80
        );
        var result = await DeploymentService.CreateDeployment(model);
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Deployment>();
        await Client.AppsV1.Received().CreateNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<V1Deployment>(), "default");
    }
    
    [Fact]
    public async Task CreateDeployment_with_namespace_not_exist_Should_Return_Deployment()
    {
        var response = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };
        Client.CoreV1.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>()).Throws(response);
        Client.CoreV1.CreateNamespaceWithHttpMessagesAsync(Arg.Any<V1Namespace>()).Returns(new HttpOperationResponse<V1Namespace>());
        
        var model = new DeploymentModel(
            "deployment-test",
            "default",
            1,
            "nginx",
            80
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
        
        result.Should().NotBeNull();
        result!.Metadata.Name.Should().Be(deploymentName);
        result.Metadata.NamespaceProperty.Should().Be(@namespace);
        
        await Client.AppsV1.Received().ReadNamespacedDeploymentWithHttpMessagesAsync(deploymentName, @namespace, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadDeployment_Should_Return_null_When_NotFound()
    {
        var deploymentName = "non-existent-deployment";
        var @namespace = "default";

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        Client.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(deploymentName, @namespace, null, null, Arg.Any<CancellationToken>())
            .Throws(httpException);

        var result = await DeploymentService.ReadDeployment(deploymentName, @namespace);
        
        result.Should().BeNull();

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
    public async Task DeleteDeployment_Should_Return_Null()
    {
        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };
        Client.AppsV1.DeleteNamespacedDeploymentWithHttpMessagesAsync("deployment-test", "default")
            .Throws(httpException);
        var result =  await DeploymentService.DeleteDeployment("deployment-test", "default");
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateDeployment_Should_Return_Deployment()
    {
        var model = new DeploymentModel(
            "deployment-test",
            "default",
            1,
            "nginx",
            80
        );
        var result = await DeploymentService.UpdateDeployment(model);
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Deployment>();
        await Client.AppsV1.Received().ReplaceNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<V1Deployment>(), "deployment-test", Arg.Any<string>(), null, null, null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDeployment_Should_Return_Null()
    {
        var model = new DeploymentModel(
            "deployment-test",
            "default",
            1,
            "nginx",
            80
        );
        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };
        Client.AppsV1.ReplaceNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<V1Deployment>(), "deployment-test",
                Arg.Any<string>(), null, null, null, null, null, Arg.Any<CancellationToken>())
            .Throws(httpException);
        var result = await DeploymentService.UpdateDeployment(model);
        result.Should().BeNull();
    }
}