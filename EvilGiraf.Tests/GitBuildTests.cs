using EvilGiraf.Model;
using EvilGiraf.Model.Kubernetes;
using EvilGiraf.Service;
using FluentAssertions;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvilGiraf.Tests;

public class GitBuildTests
{
    private readonly IKubernetes _kubernetes;
    private readonly GitBuildService _service;
    private const string RegistryUrl = "registry.example.com";
    private const string RegistryUsername = "test";
    private const string RegistryPassword = "1234";

    public GitBuildTests()
    {
        _kubernetes = Substitute.For<IKubernetes>();
        var configuration = Substitute.For<IConfiguration>();
        
        configuration["DockerRegistry:Url"].Returns(RegistryUrl);
        
        _service = new GitBuildService(_kubernetes, configuration);
    }

    [Fact]
    public async Task BuildAndPushFromGitAsync_SuccessfulBuild_ReturnsImageUrl()
    {
        // Arrange
        var app = new Application
        {
            Id = 1,
            Name = "test-app",
            Version = "1.0",
            Link = "github.com/test/repo",
            Port = 8080
        };

        var expectedImageUrl = $"{RegistryUrl}/evilgiraf-{app.Id}:{app.Version}";
        var ns = app.Id.ToNamespace();

        var createdJob = new V1Job();
        _kubernetes.BatchV1.CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(), 
            ns)
            .Returns(Task.FromResult(new HttpOperationResponse<V1Job> { Body = createdJob }));

        var pendingJob = new V1Job
        {
            Status = new V1JobStatus()
        };
        var completedJob = new V1Job
        {
            Status = new V1JobStatus { Succeeded = 1, CompletionTime = DateTime.UtcNow  }
        };
        _kubernetes.BatchV1.ReadNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(),
            ns)
            .Returns(new HttpOperationResponse<V1Job> { Body = pendingJob }, new HttpOperationResponse<V1Job> { Body = completedJob });

        // Act
        var result = await _service.BuildAndPushFromGitAsync(app);

        // Assert
        result.Should().Be(expectedImageUrl);
        await _kubernetes.BatchV1.Received(1).CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            ns);
    }

    [Fact]
    public async Task BuildAndPushFromGitAsync_Timeout_DeletesJob()
    {
        // Arrange
        var app = new Application
        {
            Id = 1,
            Name = "test-app",
            Version = "1.0",
            Link = "github.com/test/repo",
            Port = 8080
        };
        var ns = app.Id.ToNamespace();

        var createdJob = new V1Job();
        _kubernetes.BatchV1.CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(), 
            ns)
            .Returns(Task.FromResult(new HttpOperationResponse<V1Job> { Body = createdJob }));
        _kubernetes.BatchV1.DeleteNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(), 
            ns)
            .Returns(new HttpOperationResponse<V1Status>());

        // Act
        var result = await _service.BuildAndPushFromGitAsync(app, -1);

        // Assert
        result.Should().BeNull();
        await _kubernetes.BatchV1.Received(1).CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            ns);
        await _kubernetes.BatchV1.Received(1).DeleteNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(),
            ns);
    }

    [Fact]
    public async Task BuildAndPushFromGitAsync_FailedBuild_ReturnsNull()
    {
        // Arrange
        var app = new Application
        {
            Id = 2,
            Name = "test-app",
            Version = "1.0",
            Link = "github.com/test/repo",
            Port = 8080
        };
        var ns = app.Id.ToNamespace();

        var createdJob = new V1Job();
        _kubernetes.BatchV1.CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            ns)
            .Returns(Task.FromResult(new HttpOperationResponse<V1Job> { Body = createdJob }));

        var failedJob = new V1Job
        {
            Status = new V1JobStatus { Failed = 1 }
        };
        _kubernetes.BatchV1.ReadNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(),
            ns)
            .Returns(Task.FromResult(new HttpOperationResponse<V1Job>{ Body = failedJob }));

        // Act
        var result = await _service.BuildAndPushFromGitAsync(app);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task BuildAndPushFromGitAsync_RegistryCredentials_CreatesJobWithSecret()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        config["DockerRegistry:Url"].Returns(RegistryUrl);
        config["DockerRegistry:Username"].Returns(RegistryUsername);
        config["DockerRegistry:Password"].Returns(RegistryPassword);
        var service = new GitBuildService(_kubernetes, config);

        var app = new Application
        {
            Id = 3,
            Name = "test-app",
            Version = "1.0",
            Link = "github.com/test/repo",
            Port = 8080
        };

        var createdJob = new V1Job();
        _kubernetes.BatchV1.CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            app.Id.ToNamespace())
            .Returns(Task.FromResult(new HttpOperationResponse<V1Job> { Body = createdJob }));

        var completedJob = new V1Job
        {
            Status = new V1JobStatus { Succeeded = 1, CompletionTime = DateTime.UtcNow  }
        };
        _kubernetes.BatchV1.ReadNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(),
            app.Id.ToNamespace())
            .Returns(Task.FromResult(new HttpOperationResponse<V1Job> { Body = completedJob }));
        
        _kubernetes.CoreV1.ReadNamespacedSecretWithHttpMessagesAsync("docker-registry-credentials", app.Id.ToNamespace())
            .Returns(new HttpOperationResponse<V1Secret>());
        
        // Act
        var result = await service.BuildAndPushFromGitAsync(app);

        // Assert
        result.Should().Be($"{RegistryUrl}/evilgiraf-{app.Id}:{app.Version}");
        await _kubernetes.BatchV1.Received(1).CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            app.Id.ToNamespace());
    }

    [Fact]
    public async Task BuildAndPushFromGitAsync_RegistryCredentials_CreatesSecret()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        config["DockerRegistry:Url"].Returns(RegistryUrl);
        config["DockerRegistry:Username"].Returns(RegistryUsername);
        config["DockerRegistry:Password"].Returns(RegistryPassword);
        var service = new GitBuildService(_kubernetes, config);

        var app = new Application
        {
            Id = 3,
            Name = "test-app",
            Version = "1.0",
            Link = "github.com/test/repo",
            Port = 8080
        };

        var createdJob = new V1Job();
        _kubernetes.BatchV1.CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            app.Id.ToNamespace())
            .Returns(Task.FromResult(new HttpOperationResponse<V1Job> { Body = createdJob }));

        var completedJob = new V1Job
        {
            Status = new V1JobStatus { Succeeded = 1, CompletionTime = DateTime.UtcNow  }
        };
        _kubernetes.BatchV1.ReadNamespacedJobWithHttpMessagesAsync(
            Arg.Any<string>(),
            app.Id.ToNamespace())
            .Returns(Task.FromResult(new HttpOperationResponse<V1Job> { Body = completedJob }));
        
        _kubernetes.CoreV1.ReadNamespacedSecretWithHttpMessagesAsync("docker-registry-credentials", app.Id.ToNamespace())
            .Throws(new HttpOperationException());
        _kubernetes.CoreV1.CreateNamespacedSecretWithHttpMessagesAsync(Arg.Any<V1Secret>(), app.Id.ToNamespace())
            .Returns(new HttpOperationResponse<V1Secret>());
        
        // Act
        var result = await service.BuildAndPushFromGitAsync(app);

        // Assert
        result.Should().Be($"{RegistryUrl}/evilgiraf-{app.Id}:{app.Version}");
        await _kubernetes.BatchV1.Received(1).CreateNamespacedJobWithHttpMessagesAsync(
            Arg.Any<V1Job>(),
            app.Id.ToNamespace());
    }

    [Fact]
    public void BuildAndPushFromGitAsync_WithoutRegistry()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        config["DockerRegistry:Url"].Returns((string?)null);
        var action = () => new GitBuildService(_kubernetes, config);
        action.Should().Throw<ConfigurationException>().WithMessage("missing configuration DockerRegistry:Url");
    }
}
