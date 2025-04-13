using System.Net;
using EvilGiraf.Service.Kubernetes;
using FluentAssertions;
using k8s;
using k8s.Autorest;
using k8s.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvilGiraf.Tests;

public class NamespaceTests
{
    private readonly IKubernetes _kubernetes;
    private readonly NamespaceService _namespaceService;

    public NamespaceTests()
    {
        _kubernetes = Substitute.For<IKubernetes>();
        var corev1OperationsSubstitute = Substitute.For<ICoreV1Operations>();
        _kubernetes.CoreV1.Returns(corev1OperationsSubstitute);
        _namespaceService = new NamespaceService(_kubernetes);
    }

    [Fact]
    public async Task CreateNamespace_Should_Return_Namespace()
    {
        // Arrange
        var namespaceName = "test-namespace";
        var namespaceResponse = new HttpOperationResponse<V1Namespace>
        {
            Body = new V1Namespace
            {
                Metadata = new V1ObjectMeta
                {
                    Name = namespaceName
                }
            }
        };

        _kubernetes.CoreV1.CreateNamespaceWithHttpMessagesAsync(
            Arg.Is<V1Namespace>(ns => ns.Metadata.Name == namespaceName))
            .Returns(Task.FromResult(namespaceResponse));

        // Act
        var result = await _namespaceService.CreateNamespace(namespaceName);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Name.Should().Be(namespaceName);
        await _kubernetes.CoreV1.Received(1).CreateNamespaceWithHttpMessagesAsync(
            Arg.Is<V1Namespace>(ns => ns.Metadata.Name == namespaceName));
    }

    [Fact]
    public async Task ReadNamespace_Should_Return_Namespace_When_Exists()
    {
        // Arrange
        var namespaceName = "test-namespace";
        var namespaceResponse = new HttpOperationResponse<V1Namespace>
        {
            Body = new V1Namespace
            {
                Metadata = new V1ObjectMeta
                {
                    Name = namespaceName
                }
            }
        };

        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(
            namespaceName)
            .Returns(Task.FromResult(namespaceResponse));

        // Act
        var result = await _namespaceService.ReadNamespace(namespaceName);

        // Assert
        result.Should().NotBeNull();
        result!.Metadata.Name.Should().Be(namespaceName);
        await _kubernetes.CoreV1.Received(1).ReadNamespaceWithHttpMessagesAsync(
            namespaceName);
    }

    [Fact]
    public async Task ReadNamespace_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var namespaceName = "non-existent-namespace";
        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(
            namespaceName)
            .Throws(httpException);

        // Act
        var result = await _namespaceService.ReadNamespace(namespaceName);

        // Assert
        result.Should().BeNull();
        await _kubernetes.CoreV1.Received(1).ReadNamespaceWithHttpMessagesAsync(
            namespaceName);
    }

    [Fact]
    public async Task DeleteNamespace_Should_Return_Status_When_Exists()
    {
        // Arrange
        var namespaceName = "test-namespace";
        var deleteResponse = new HttpOperationResponse<V1Status>
        {
            Body = new V1Status
            {
                Status = "Success"
            }
        };

        _kubernetes.CoreV1.DeleteNamespaceWithHttpMessagesAsync(
            namespaceName)
            .Returns(Task.FromResult(deleteResponse));

        // Act
        var result = await _namespaceService.DeleteNamespace(namespaceName);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Success");
        await _kubernetes.CoreV1.Received(1).DeleteNamespaceWithHttpMessagesAsync(
            namespaceName);
    }

    [Fact]
    public async Task DeleteNamespace_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var namespaceName = "non-existent-namespace";
        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.CoreV1.DeleteNamespaceWithHttpMessagesAsync(
            namespaceName)
            .Throws(httpException);

        // Act
        var result = await _namespaceService.DeleteNamespace(namespaceName);

        // Assert
        result.Should().BeNull();
        await _kubernetes.CoreV1.Received(1).DeleteNamespaceWithHttpMessagesAsync(
            namespaceName);
    }

    [Fact]
    public async Task CreateIfNotExistsNamespace_Should_Return_Existing_Namespace()
    {
        // Arrange
        var namespaceName = "test-namespace";
        var existingNamespace = new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = namespaceName
            }
        };
        var namespaceResponse = new HttpOperationResponse<V1Namespace>
        {
            Body = existingNamespace
        };

        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(
            namespaceName)
            .Returns(Task.FromResult(namespaceResponse));

        // Act
        var result = await _namespaceService.CreateIfNotExistsNamespace(namespaceName);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Name.Should().Be(namespaceName);
        await _kubernetes.CoreV1.Received(1).ReadNamespaceWithHttpMessagesAsync(
            namespaceName);
        await _kubernetes.CoreV1.DidNotReceive().CreateNamespaceWithHttpMessagesAsync(
            Arg.Any<V1Namespace>());
    }

    [Fact]
    public async Task CreateIfNotExistsNamespace_Should_Create_And_Return_New_Namespace()
    {
        // Arrange
        var namespaceName = "test-namespace";
        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(
            namespaceName)
            .Throws(httpException);

        var newNamespace = new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = namespaceName
            }
        };
        var createResponse = new HttpOperationResponse<V1Namespace>
        {
            Body = newNamespace
        };

        _kubernetes.CoreV1.CreateNamespaceWithHttpMessagesAsync(
            Arg.Is<V1Namespace>(ns => ns.Metadata.Name == namespaceName))
            .Returns(Task.FromResult(createResponse));

        // Act
        var result = await _namespaceService.CreateIfNotExistsNamespace(namespaceName);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Name.Should().Be(namespaceName);
        await _kubernetes.CoreV1.Received(1).ReadNamespaceWithHttpMessagesAsync(
            namespaceName);
        await _kubernetes.CoreV1.Received(1).CreateNamespaceWithHttpMessagesAsync(
            Arg.Is<V1Namespace>(ns => ns.Metadata.Name == namespaceName));
    }
}
