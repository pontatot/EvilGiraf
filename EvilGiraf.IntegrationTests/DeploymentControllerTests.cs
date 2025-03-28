using System.Net;
using System.Net.Http.Json;
using EvilGiraf.Dto;
using EvilGiraf.Model;
using EvilGiraf.Service;
using FluentAssertions;
using k8s;
using k8s.Autorest;
using k8s.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvilGiraf.IntegrationTests;

public class DeploymentControllerTests : AuthenticatedTestBase
{
    private readonly DatabaseService _dbContext;
    private readonly IKubernetes _kubernetes;

    public DeploymentControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _dbContext = GetDatabaseService();
        _kubernetes = GetKubernetesClient();
    }

    [Fact]
    public async Task Deploy_ShouldCreateNewDeployment_WhenApplicationExists()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-app",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-app:latest",
            Version = "1.0.0"
        };
        var deployment = new HttpOperationResponse<V1Deployment>
        {
            Body = new V1Deployment
            {
                Status = new V1DeploymentStatus
                {
                    Replicas = 1,
                    ReadyReplicas = 1
                }
            }
        };
        
        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();
        
        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(
                application.Name, 
                application.Id.ToNamespace())
            .Returns(deployment);
        
        _kubernetes.AppsV1.ReplaceNamespacedDeploymentWithHttpMessagesAsync(
                Arg.Any<V1Deployment>(),
                application.Name, 
                application.Id.ToNamespace())
            .Returns(deployment);

        // Act
        var response = await Client.PostAsync($"/deploy/{application.Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await _kubernetes.AppsV1.Received().ReadNamespacedDeploymentWithHttpMessagesAsync(
            Arg.Is<string>(name => name == application.Name),
            Arg.Is<string>(ns => ns == application.Id.ToNamespace()));

        await _kubernetes.AppsV1.Received().ReplaceNamespacedDeploymentWithHttpMessagesAsync(
            Arg.Any<V1Deployment>(),
            Arg.Is<string>(name => name == application.Name),
            Arg.Is<string>(ns => ns == application.Id.ToNamespace()));
    }

    [Fact]
    public async Task Deploy_ShouldCreateNewDeployment_WhenApplicationNotExists()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-app",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-app:latest",
            Version = "1.0.0"
        };

        var deployment = new HttpOperationResponse<V1Deployment>
        {
            Body = new V1Deployment
            {
                Status = new V1DeploymentStatus
                {
                    Replicas = 1,
                    ReadyReplicas = 1
                }
            }
        };
        
        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };
        
        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();
        
        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(
                application.Name, 
                application.Id.ToNamespace())
            .Throws(httpException);
        
        _kubernetes.AppsV1.CreateNamespacedDeploymentWithHttpMessagesAsync(
                Arg.Any<V1Deployment>(),
                application.Id.ToNamespace())
            .Returns(deployment);
        
        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>()).Returns(new HttpOperationResponse<V1Namespace>());
        
        // Act
        var response = await Client.PostAsync($"/deploy/{application.Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await _kubernetes.AppsV1.Received().ReadNamespacedDeploymentWithHttpMessagesAsync(
            Arg.Is<string>(name => name == application.Name),
            Arg.Is<string>(ns => ns == application.Id.ToNamespace()));
    }

    [Fact]
    public async Task Deploy_ShouldReturn404_WhenApplicationDoesNotExist()
    {
        // Act
        var response = await Client.PostAsync("/deploy/999", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.ReadAsStringAsync().Result.Should().Be("Application 999 not found");
    }

    [Fact]
    public async Task Status_ShouldReturnDeploymentStatus_WhenApplicationExistsAndIsDeployed()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-app",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-app:latest",
            Version = "1.0.0"
        };

        var deployment = new HttpOperationResponse<V1Deployment>
        {
            Body = new V1Deployment
            {
                Status = new V1DeploymentStatus
                {
                    Replicas = 1,
                    ReadyReplicas = 1
                }
            }
        };
        
        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();
        
        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(
                application.Name, 
                application.Id.ToNamespace())
            .Returns(deployment);

        // Act
        var response = await Client.GetAsync($"/deploy/{application.Id}");

        // Assert
        response.Should().BeSuccessful();
        var deployResponse = await response.Content.ReadFromJsonAsync<DeployResponse>();
        deployResponse.Should().NotBeNull();
        // You can add more specific assertions about the status value if needed
    }

    [Fact]
    public async Task Status_ShouldReturn404_WhenApplicationExistsButNotDeployed()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-app",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-app:latest",
            Version = "1.0.0"
        };
        
        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };
        
        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();
        
        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(
                application.Name, 
                application.Id.ToNamespace())
            .Throws(httpException);

        // Act
        var response = await Client.GetAsync($"/deploy/{application.Id}");

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain($"Application {application.Id} is not deployed");
    }

    [Fact]
    public async Task Status_ShouldReturn404_WhenApplicationDoesNotExist()
    {
        // Arrange
        const int nonExistentApplicationId = 999;

        // Act
        var response = await Client.GetAsync($"/deploy/{nonExistentApplicationId}");

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain($"Application {nonExistentApplicationId} not found");
    }
}