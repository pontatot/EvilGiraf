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
    public async Task Deploy_Async_ShouldCreateNewDeployment_WhenApplicationExists()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-app",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-app:latest",
            Version = "1.0.0",
            Port = 22
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
        var service = new HttpOperationResponse<V1Service>
        {
            Body = new V1Service()
        };
        
        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();
        
        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(
                application.Name, 
                application.Id.ToNamespace())
            .Returns(deployment);
        
        _kubernetes.CoreV1.ReadNamespacedServiceWithHttpMessagesAsync(
                application.Name, 
                application.Id.ToNamespace())
            .Returns(service);
        
        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>()).Returns(new HttpOperationResponse<V1Namespace>{ Body = new V1Namespace() });
        
        _kubernetes.AppsV1.ReplaceNamespacedDeploymentWithHttpMessagesAsync(
                Arg.Any<V1Deployment>(),
                application.Name, 
                application.Id.ToNamespace())
            .Returns(deployment);

        // Act
        var response = await Client.PostAsync($"/api/deploy/{application.Id}?isAsync=true", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
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
            Version = "1.0.0",
            Port = 22,
            DomainName = "test.com"
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
        var service = new HttpOperationResponse<V1Service>
        {
            Body = new V1Service()
        };
        
        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();
        
        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(
                application.Name, 
                application.Id.ToNamespace())
            .Returns(deployment);
        
        _kubernetes.CoreV1.ReadNamespacedServiceWithHttpMessagesAsync(
                application.Name, 
                application.Id.ToNamespace())
            .Returns(service);
        
        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>()).Returns(new HttpOperationResponse<V1Namespace>{ Body = new V1Namespace() });
        
        _kubernetes.AppsV1.ReplaceNamespacedDeploymentWithHttpMessagesAsync(
                Arg.Any<V1Deployment>(),
                application.Name, 
                application.Id.ToNamespace())
            .Returns(deployment);
        
        _kubernetes.NetworkingV1.ReadNamespacedIngressWithHttpMessagesAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new HttpOperationResponse<V1Ingress>{ Body = new V1Ingress() });

        // Act
        var response = await Client.PostAsync($"/api/deploy/{application.Id}?isAsync=false", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await _kubernetes.AppsV1.Received().ReadNamespacedDeploymentWithHttpMessagesAsync(
            application.Name,
            application.Id.ToNamespace());

        await _kubernetes.AppsV1.Received().ReplaceNamespacedDeploymentWithHttpMessagesAsync(
            Arg.Any<V1Deployment>(),
            application.Name,
            application.Id.ToNamespace());
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
            Version = "1.0.0",
            Port = 22,
            DomainName = "test.com"
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
        var service = new HttpOperationResponse<V1Service>
        {
            Body = new V1Service()
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
        
        _kubernetes.CoreV1.CreateNamespacedServiceWithHttpMessagesAsync(
                Arg.Any<V1Service>(), 
                application.Id.ToNamespace())
            .Returns(service);
        
        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>()).Returns(new HttpOperationResponse<V1Namespace>{ Body = new V1Namespace()});
        
        _kubernetes.NetworkingV1.CreateNamespacedIngressWithHttpMessagesAsync(Arg.Any<V1Ingress>(), application.Id.ToNamespace()).Returns(new HttpOperationResponse<V1Ingress>{ Body = new V1Ingress()});
        
        // Act
        var response = await Client.PostAsync($"/api/deploy/{application.Id}?isAsync=false", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await _kubernetes.AppsV1.Received().ReadNamespacedDeploymentWithHttpMessagesAsync(
            application.Name,
            application.Id.ToNamespace());
    }

    [Fact]
    public async Task Deploy_ShouldReturn404_WhenApplicationDoesNotExist()
    {
        // Act
        var response = await Client.PostAsync("/api/deploy/999", null);

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
            Version = "1.0.0",
            Port = 22
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
        var response = await Client.GetAsync($"/api/deploy/{application.Id}");

        // Assert
        response.Should().BeSuccessful();
        var deployResponse = await response.Content.ReadFromJsonAsync<DeployResponse>();
        deployResponse.Should().NotBeNull();
        // You can add more specific assertions about the status value if needed
    }

    [Fact]
    public async Task Status_ShouldReturn201_WhenApplicationExistsButNotDeployed()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-app",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-app:latest",
            Version = "1.0.0",
            Port = 22
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
        var response = await Client.GetAsync($"/api/deploy/{application.Id}");

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Status_ShouldReturn404_WhenApplicationDoesNotExist()
    {
        // Arrange
        const int nonExistentApplicationId = 999;

        // Act
        var response = await Client.GetAsync($"/api/deploy/{nonExistentApplicationId}");

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain($"Application {nonExistentApplicationId} not found");
    }

    [Fact]
    public async Task ListDeployments_ShouldReturnOkResult_WithEmptyList_WhenNoDeployments()
    {
        var applications = new List<Application>
        {
            new() { Name = "app1", Type = ApplicationType.Docker, Link = "docker.io/app1:latest", Version = "1.0.0", Port = 22 },
            new() { Name = "app2", Type = ApplicationType.Git, Link = "k8s.io/app2:latest", Version = "2.0.0", Port = null }
        };

        _dbContext.Applications.AddRange(applications);
        await _dbContext.SaveChangesAsync();
        
        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new HttpOperationResponse<V1Deployment>());
        // Act
        var response = await Client.GetAsync("/api/deploy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var deployResponses = await response.Content.ReadFromJsonAsync<List<DeployResponse>>();
        deployResponses.Should().NotBeNull();
        deployResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task ListDeployments_ShouldReturnOkResult_WithDeployments()
    {
        // Arrange
        var applications = new List<Application>
        {
            new() { Name = "app1", Type = ApplicationType.Docker, Link = "docker.io/app1:latest", Version = "1.0.0", Port = 22 },
            new() { Name = "app2", Type = ApplicationType.Git, Link = "k8s.io/app2:latest", Version = "2.0.0", Port = null }
        };

        _dbContext.Applications.AddRange(applications);
        await _dbContext.SaveChangesAsync();

        var deployment = new V1Deployment
        {
            Status = new V1DeploymentStatus { Replicas = 1 }
        };

        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new HttpOperationResponse<V1Deployment> { Body = deployment });

        // Act
        var response = await Client.GetAsync("/api/deploy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var deployResponses = await response.Content.ReadFromJsonAsync<List<DeployResponse>>();
        deployResponses.Should().NotBeNull();
        deployResponses.Should().HaveCountGreaterOrEqualTo(2);
        deployResponses!.Select(resp => resp.Status).Should().ContainEquivalentOf(deployment.Status);
    }
    
    [Fact]
    public async Task Undeploy_ShouldReturn204_WhenDeploymentIsDeleted()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-app",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-app:latest",
            Version = "1.0.0",
            Port = 22
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();

        _kubernetes.CoreV1.DeleteNamespaceWithHttpMessagesAsync(
            application.Id.ToNamespace())
            .Returns(new HttpOperationResponse<V1Status>());

        // Act
        var response = await Client.DeleteAsync($"/api/deploy/{application.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _kubernetes.CoreV1.Received(1).DeleteNamespaceWithHttpMessagesAsync(
            application.Id.ToNamespace());
    }

    [Fact]
    public async Task Undeploy_ShouldReturn404_WhenApplicationDoesNotExist()
    {
        // Act
        var response = await Client.DeleteAsync("/api/deploy/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Be("Application with ID 999 not found.");
    }
    
    [Fact]
    public async Task Deploy_ShouldCreateDeployment_WithGitApplication()
    {
        // Arrange
        var gitApp = new Application 
        {
            Name = "git-app", 
            Type = ApplicationType.Git, 
            Link = "https://github.com/example/repo.git", 
            Version = "1.0.0", 
            Port = null
        };
        
        _dbContext.Applications.Add(gitApp);
        await _dbContext.SaveChangesAsync();
        
        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(gitApp.Id.ToNamespace()).Returns(new HttpOperationResponse<V1Namespace>{ Body = new V1Namespace() });
        
        var successfulJob = new V1Job
        {
            Metadata = new V1ObjectMeta { Name = $"build-{gitApp.Name}" },
            Status = new V1JobStatus { Succeeded = 1, CompletionTime = DateTime.UtcNow }
        };
        
        _kubernetes.BatchV1.CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Job> { Body = successfulJob });
        
        _kubernetes.BatchV1.ReadNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Job> { Body = successfulJob });
        
        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(
            Arg.Any<string>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Deployment>());
        _kubernetes.AppsV1.CreateNamespacedDeploymentWithHttpMessagesAsync(
            Arg.Any<V1Deployment>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Deployment>{ Body = new V1Deployment() });
        
        _kubernetes.CoreV1.ReadNamespacedSecretWithHttpMessagesAsync(Arg.Any<string>(), gitApp.Id.ToNamespace()).Returns(new HttpOperationResponse<V1Secret>());
        // Act
        var response = await Client.PostAsync($"/api/deploy/{gitApp.Id}?isAsync=false", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await _kubernetes.BatchV1.Received(1).CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            gitApp.Id.ToNamespace(),
            null,
            Arg.Any<string>());
        
        await _kubernetes.AppsV1.Received(1).CreateNamespacedDeploymentWithHttpMessagesAsync(
            Arg.Any<V1Deployment>(),
            gitApp.Id.ToNamespace(),
            null,
            Arg.Any<string>());
    }
    
    [Fact]
    public async Task Deploy_WithTimeoutBuildGitApplication()
    {
        // Arrange
        var gitApp = new Application 
        {
            Name = "git-app", 
            Type = ApplicationType.Git, 
            Link = "https://github.com/example/repo.git", 
            Version = "1.0.0", 
            Port = null
        };
        
        _dbContext.Applications.Add(gitApp);
        await _dbContext.SaveChangesAsync();
        
        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(gitApp.Id.ToNamespace()).Returns(new HttpOperationResponse<V1Namespace>{ Body = new V1Namespace() });
        
        var successfulJob = new V1Job
        {
            Metadata = new V1ObjectMeta { Name = $"build-{gitApp.Name}" },
            Status = new V1JobStatus { Succeeded = 1, CompletionTime = DateTime.UtcNow }
        };
        
        _kubernetes.BatchV1.CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Job> { Body = successfulJob });
        
        _kubernetes.BatchV1.ReadNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Job> { Body = successfulJob });
        
        _kubernetes.BatchV1.DeleteNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Status>());
        
        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(
            Arg.Any<string>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Deployment>());
        _kubernetes.AppsV1.CreateNamespacedDeploymentWithHttpMessagesAsync(
            Arg.Any<V1Deployment>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Deployment>{ Body = new V1Deployment() });
        
        _kubernetes.CoreV1.ReadNamespacedSecretWithHttpMessagesAsync(Arg.Any<string>(), gitApp.Id.ToNamespace()).Returns(new HttpOperationResponse<V1Secret>());
        // Act
        var response = await Client.PostAsync($"/api/deploy/{gitApp.Id}?isAsync=false&timeoutSeconds=0", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await _kubernetes.BatchV1.Received(1).CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            gitApp.Id.ToNamespace());
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await _kubernetes.BatchV1.Received(1).DeleteNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(),
            gitApp.Id.ToNamespace());
        
        await _kubernetes.AppsV1.DidNotReceive().CreateNamespacedDeploymentWithHttpMessagesAsync(
            Arg.Any<V1Deployment>(),
            gitApp.Id.ToNamespace());
    }

    [Fact]
    public async Task Status_ShouldReturnCorrectStatus_ForGitApplication()
    {
        // Arrange
        var gitApp = new Application 
        {
            Name = "git-status-app", 
            Type = ApplicationType.Git, 
            Link = "https://github.com/example/repo.git", 
            Version = "1.0.0", 
            Port = 80
        };
        
        _dbContext.Applications.Add(gitApp);
        await _dbContext.SaveChangesAsync();
        
        var deployment = new V1Deployment
        {
            Metadata = new V1ObjectMeta { Name = gitApp.Name },
            Status = new V1DeploymentStatus { AvailableReplicas = 1, Replicas = 1 }
        };
        
        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(
            gitApp.Name,
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Deployment> { Body = deployment });
        
        // Act
        var response = await Client.GetAsync($"/api/deploy/{gitApp.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var deployStatus = await response.Content.ReadFromJsonAsync<DeployResponse>();
        deployStatus.Should().NotBeNull();
        deployStatus!.Status.Replicas.Should().Be(1);
        deployStatus.Status.AvailableReplicas.Should().Be(1);
    }

    [Fact]
    public async Task ListDeployments_ShouldIncludeGitApplications()
    {
        // Arrange
        var applications = new List<Application>
        {
            new() { Name = "docker-app", Type = ApplicationType.Docker, Link = "docker.io/app:latest", Version = "1.0.0", Port = 80 },
            new() { Name = "git-app", Type = ApplicationType.Git, Link = "https://github.com/example/repo.git", Version = "1.0.0", Port = 8080 }
        };

        _dbContext.Applications.AddRange(applications);
        await _dbContext.SaveChangesAsync();

        var deployment = new V1Deployment
        {
            Status = new V1DeploymentStatus { Replicas = 1, AvailableReplicas = 1 }
        };

        _kubernetes.AppsV1.ReadNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new HttpOperationResponse<V1Deployment> { Body = deployment });

        // Act
        var response = await Client.GetAsync("/api/deploy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var deployResponses = await response.Content.ReadFromJsonAsync<List<DeployResponse>>();
        deployResponses.Should().NotBeNull();
        deployResponses.Should().HaveCountGreaterThanOrEqualTo(2);
        deployResponses!.Select(resp => resp.Status).Should().ContainEquivalentOf(deployment.Status);
    }

    [Fact]
    public async Task Deploy_ShouldHandleFailedGitBuild()
    {
        // Arrange
        var gitApp = new Application 
        {
            Name = "git-fail-app", 
            Type = ApplicationType.Git, 
            Link = "https://github.com/example/broken-repo.git", 
            Version = "1.0.0", 
            Port = 80
        };
        
        _dbContext.Applications.Add(gitApp);
        await _dbContext.SaveChangesAsync();
        
        var failedJob = new V1Job
        {
            Metadata = new V1ObjectMeta { Name = $"build-{gitApp.Name}" },
            Status = new V1JobStatus { Failed = 1, Succeeded = 0 }
        };
        
        _kubernetes.BatchV1.CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Job> { Body = failedJob });
        
        _kubernetes.BatchV1.ReadNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(),
            gitApp.Id.ToNamespace()
        ).Returns(new HttpOperationResponse<V1Job> { Body = failedJob });
        
        // Act
        var response = await Client.PostAsync($"/api/deploy/{gitApp.Id}?isAsync=false", null);
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }
}