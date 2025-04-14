using System.Net;
using EvilGiraf.Model.Kubernetes;
using EvilGiraf.Service.Kubernetes;
using FluentAssertions;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvilGiraf.Tests;

public class IngressTests
{
    private readonly IKubernetes _kubernetes;
    private readonly IngressService _ingressService;

    public IngressTests()
    {
        _kubernetes = Substitute.For<IKubernetes>();
        _ingressService = new IngressService(_kubernetes, Substitute.For<IConfiguration>());
    }

    [Fact]
    public async Task CreateIngress_Should_Return_Ingress()
    {
        // Arrange
        var model = new IngressModel(
            "test-ingress",
            "default",
            "test-host",
            80,
            "/"
        );

        var ingressResponse = new HttpOperationResponse<V1Ingress>
        {
            Body = new V1Ingress
            {
                Metadata = new V1ObjectMeta
                {
                    Name = model.Name,
                    NamespaceProperty = model.Namespace
                }
            }
        };

        _kubernetes.NetworkingV1.CreateNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Namespace
        ).Returns(ingressResponse);
        
        
        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>())
            .Returns(new HttpOperationResponse<V1Namespace> { Body = new V1Namespace() });

        // Act
        var result = await _ingressService.CreateIngress(model);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Name.Should().Be(model.Name);
        result.Metadata.NamespaceProperty.Should().Be(model.Namespace);
        await _kubernetes.NetworkingV1.Received(1).CreateNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Namespace
        );
    }

    [Fact]
    public async Task CreateIngress_WhenNamespaceNotExists_ShouldCreateNamespaceAndReturnIngress()
    {
        // Arrange
        var model = new IngressModel(
            "test-ingress",
            "default",
            "test-host",
            80,
            "/"
        );

        var namespaceException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(
            model.Namespace
        ).Throws(namespaceException);

        var namespaceResponse = new HttpOperationResponse<V1Namespace>();
        _kubernetes.CoreV1.CreateNamespaceWithHttpMessagesAsync(
            Arg.Any<V1Namespace>()
        ).Returns(namespaceResponse);

        var ingressResponse = new HttpOperationResponse<V1Ingress>
        {
            Body = new V1Ingress
            {
                Metadata = new V1ObjectMeta
                {
                    Name = model.Name,
                    NamespaceProperty = model.Namespace
                }
            }
        };

        _kubernetes.NetworkingV1.CreateNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Namespace
        ).Returns(ingressResponse);

        // Act
        var result = await _ingressService.CreateIngress(model);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Name.Should().Be(model.Name);
        result.Metadata.NamespaceProperty.Should().Be(model.Namespace);
        await _kubernetes.NetworkingV1.Received(1).CreateNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Namespace
        );
    }

    [Fact]
    public async Task ReadIngress_Should_Return_Ingress()
    {
        // Arrange
        var name = "test-ingress";
        var @namespace = "default";

        var ingressResponse = new HttpOperationResponse<V1Ingress>
        {
            Body = new V1Ingress
            {
                Metadata = new V1ObjectMeta
                {
                    Name = name,
                    NamespaceProperty = @namespace
                }
            }
        };

        _kubernetes.NetworkingV1.ReadNamespacedIngressWithHttpMessagesAsync(
            name,
            @namespace
        ).Returns(ingressResponse);

        // Act
        var result = await _ingressService.ReadIngress(name, @namespace);

        // Assert
        result.Should().NotBeNull();
        result!.Metadata.Name.Should().Be(name);
        result.Metadata.NamespaceProperty.Should().Be(@namespace);
        await _kubernetes.NetworkingV1.Received(1).ReadNamespacedIngressWithHttpMessagesAsync(
            name,
            @namespace
        );
    }

    [Fact]
    public async Task ReadIngress_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var name = "non-existent-ingress";
        var @namespace = "default";

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.NetworkingV1.ReadNamespacedIngressWithHttpMessagesAsync(
            name,
            @namespace
        ).Throws(httpException);

        // Act
        var result = await _ingressService.ReadIngress(name, @namespace);

        // Assert
        result.Should().BeNull();
        await _kubernetes.NetworkingV1.Received(1).ReadNamespacedIngressWithHttpMessagesAsync(
            name,
            @namespace
        );
    }

    [Fact]
    public async Task UpdateIngress_Should_Return_Ingress()
    {
        // Arrange
        var model = new IngressModel(
            "test-ingress",
            "default",
            "test-host",
            80,
            "/"
        );

        var ingressResponse = new HttpOperationResponse<V1Ingress>
        {
            Body = new V1Ingress
            {
                Metadata = new V1ObjectMeta
                {
                    Name = model.Name,
                    NamespaceProperty = model.Namespace
                }
            }
        };

        _kubernetes.NetworkingV1.ReplaceNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Name,
            model.Namespace
        ).Returns(ingressResponse);

        // Act
        var result = await _ingressService.UpdateIngress(model);

        // Assert
        result.Should().NotBeNull();
        result!.Metadata.Name.Should().Be(model.Name);
        result.Metadata.NamespaceProperty.Should().Be(model.Namespace);
        await _kubernetes.NetworkingV1.Received(1).ReplaceNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Name,
            model.Namespace
        );
    }

    [Fact]
    public async Task UpdateIngress_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var model = new IngressModel(
            "non-existent-ingress",
            "default",
            "test-host",
            80,
            "/"
        );

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.NetworkingV1.ReplaceNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Name,
            model.Namespace
        ).Throws(httpException);

        // Act
        var result = await _ingressService.UpdateIngress(model);

        // Assert
        result.Should().BeNull();
        await _kubernetes.NetworkingV1.Received(1).ReplaceNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Name,
            model.Namespace
        );
    }

    [Fact]
    public async Task DeleteIngress_Should_Return_Status()
    {
        // Arrange
        var name = "test-ingress";
        var @namespace = "default";

        var statusResponse = new HttpOperationResponse<V1Status>
        {
            Body = new V1Status()
        };

        _kubernetes.NetworkingV1.DeleteNamespacedIngressWithHttpMessagesAsync(
            name,
            @namespace
        ).Returns(statusResponse);

        // Act
        var result = await _ingressService.DeleteIngress(name, @namespace);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Status>();
        await _kubernetes.NetworkingV1.Received(1).DeleteNamespacedIngressWithHttpMessagesAsync(
            name,
            @namespace
        );
    }

    [Fact]
    public async Task DeleteIngress_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var name = "non-existent-ingress";
        var @namespace = "default";

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.NetworkingV1.DeleteNamespacedIngressWithHttpMessagesAsync(
            name,
            @namespace
        ).Throws(httpException);

        // Act
        var result = await _ingressService.DeleteIngress(name, @namespace);

        // Assert
        result.Should().BeNull();
        await _kubernetes.NetworkingV1.Received(1).DeleteNamespacedIngressWithHttpMessagesAsync(
            name,
            @namespace
        );
    }

    [Fact]
    public async Task CreateOrReplaceIngress_WhenIngressExists_Should_Replace_ExistingIngress()
    {
        // Arrange
        var model = new IngressModel(
            "test-ingress",
            "default",
            "test-host",
            80,
            "/"
        );

        var existingIngress = new V1Ingress
        {
            Metadata = new V1ObjectMeta
            {
                Name = model.Name,
                NamespaceProperty = model.Namespace
            }
        };

        _kubernetes.NetworkingV1.ReadNamespacedIngressWithHttpMessagesAsync(
            model.Name,
            model.Namespace
        ).Returns(new HttpOperationResponse<V1Ingress> { Body = existingIngress });

        _kubernetes.NetworkingV1.ReplaceNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Name,
            model.Namespace
        ).Returns(new HttpOperationResponse<V1Ingress> { Body = existingIngress });

        // Act
        var result = await _ingressService.CreateOrReplaceIngress(model);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(existingIngress);
        await _kubernetes.NetworkingV1.DidNotReceive().CreateNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Namespace
        );
    }

    [Fact]
    public async Task CreateOrReplaceIngress_WhenIngressNotExists_Should_Create_And_Return_NewIngress()
    {
        // Arrange
        var model = new IngressModel(
            "test-ingress",
            "default",
            "test-host",
            80,
            "/"
        );

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.NetworkingV1.ReadNamespacedIngressWithHttpMessagesAsync(
            model.Name,
            model.Namespace
        ).Throws(httpException);

        var newIngress = new V1Ingress
        {
            Metadata = new V1ObjectMeta
            {
                Name = model.Name,
                NamespaceProperty = model.Namespace
            }
        };

        _kubernetes.NetworkingV1.CreateNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Namespace
        ).Returns(new HttpOperationResponse<V1Ingress> { Body = newIngress });
        
        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>())
            .Returns(new HttpOperationResponse<V1Namespace> { Body = new V1Namespace() });
        
        // Act
        var result = await _ingressService.CreateOrReplaceIngress(model);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(newIngress);
        await _kubernetes.NetworkingV1.Received(1).CreateNamespacedIngressWithHttpMessagesAsync(
            Arg.Any<V1Ingress>(),
            model.Namespace
        );
    }
} 