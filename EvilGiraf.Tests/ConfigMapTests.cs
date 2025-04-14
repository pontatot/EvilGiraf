using System.Net;
using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Model.Kubernetes;
using EvilGiraf.Service.Kubernetes;
using FluentAssertions;
using k8s;
using k8s.Autorest;
using k8s.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvilGiraf.Tests;

public class ConfigMapTests
{
    private readonly IKubernetes _kubernetes;
    private readonly IConfigMapService _configMapService;

    public ConfigMapTests()
    {
        _kubernetes = Substitute.For<IKubernetes>();
        _configMapService = new ConfigMapService(_kubernetes);
    }

    [Fact]
    public async Task CreateConfigMap_Should_Return_ConfigMap()
    {
        // Arrange
        var model = new ConfigMapModel(
            "test-configmap",
            "default",
            ["KEY=value", "KEY2=value2"]
        );

        var configmapResponse = new HttpOperationResponse<V1ConfigMap>
        {
            Body = new V1ConfigMap
            {
                Metadata = new V1ObjectMeta
                {
                    Name = model.Name,
                    NamespaceProperty = model.Namespace
                }
            }
        };

        _kubernetes.CoreV1.CreateNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Namespace
        ).Returns(configmapResponse);
        
        
        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>())
            .Returns(new HttpOperationResponse<V1Namespace> { Body = new V1Namespace() });

        // Act
        var result = await _configMapService.CreateConfigMap(model);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Name.Should().Be(model.Name);
        result.Metadata.NamespaceProperty.Should().Be(model.Namespace);
        await _kubernetes.CoreV1.Received(1).CreateNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Namespace
        );
    }

    [Fact]
    public async Task CreateConfigMap_WhenNamespaceNotExists_ShouldCreateNamespaceAndReturnConfigMap()
    {
        // Arrange
        var model = new ConfigMapModel(
            "test-configmap",
            "default",
            ["KEY=value", "KEY2=value2"]
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

        var configmapResponse = new HttpOperationResponse<V1ConfigMap>
        {
            Body = new V1ConfigMap
            {
                Metadata = new V1ObjectMeta
                {
                    Name = model.Name,
                    NamespaceProperty = model.Namespace
                }
            }
        };

        _kubernetes.CoreV1.CreateNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Namespace
        ).Returns(configmapResponse);

        // Act
        var result = await _configMapService.CreateConfigMap(model);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Name.Should().Be(model.Name);
        result.Metadata.NamespaceProperty.Should().Be(model.Namespace);
        await _kubernetes.CoreV1.Received(1).CreateNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Namespace
        );
    }

    [Fact]
    public async Task ReadConfigMap_Should_Return_ConfigMap()
    {
        // Arrange
        var name = "test-configmap";
        var @namespace = "default";

        var configmapResponse = new HttpOperationResponse<V1ConfigMap>
        {
            Body = new V1ConfigMap
            {
                Metadata = new V1ObjectMeta
                {
                    Name = name,
                    NamespaceProperty = @namespace
                }
            }
        };

        _kubernetes.CoreV1.ReadNamespacedConfigMapWithHttpMessagesAsync(
            name,
            @namespace
        ).Returns(configmapResponse);

        // Act
        var result = await _configMapService.ReadConfigMap(name, @namespace);

        // Assert
        result.Should().NotBeNull();
        result!.Metadata.Name.Should().Be(name);
        result.Metadata.NamespaceProperty.Should().Be(@namespace);
        await _kubernetes.CoreV1.Received(1).ReadNamespacedConfigMapWithHttpMessagesAsync(
            name,
            @namespace
        );
    }

    [Fact]
    public async Task ReadConfigMap_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var name = "non-existent-configmap";
        var @namespace = "default";

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.CoreV1.ReadNamespacedConfigMapWithHttpMessagesAsync(
            name,
            @namespace
        ).Throws(httpException);

        // Act
        var result = await _configMapService.ReadConfigMap(name, @namespace);

        // Assert
        result.Should().BeNull();
        await _kubernetes.CoreV1.Received(1).ReadNamespacedConfigMapWithHttpMessagesAsync(
            name,
            @namespace
        );
    }

    [Fact]
    public async Task UpdateConfigMap_Should_Return_ConfigMap()
    {
        // Arrange
        var model = new ConfigMapModel(
            "test-configmap",
            "default",
            ["KEY=value", "KEY2=value2"]
        );

        var configmapResponse = new HttpOperationResponse<V1ConfigMap>
        {
            Body = new V1ConfigMap
            {
                Metadata = new V1ObjectMeta
                {
                    Name = model.Name,
                    NamespaceProperty = model.Namespace
                }
            }
        };

        _kubernetes.CoreV1.ReplaceNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Name,
            model.Namespace
        ).Returns(configmapResponse);

        // Act
        var result = await _configMapService.UpdateConfigMap(model);

        // Assert
        result.Should().NotBeNull();
        result!.Metadata.Name.Should().Be(model.Name);
        result.Metadata.NamespaceProperty.Should().Be(model.Namespace);
        await _kubernetes.CoreV1.Received(1).ReplaceNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Name,
            model.Namespace
        );
    }

    [Fact]
    public async Task UpdateConfigMap_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var model = new ConfigMapModel(
            "non-existent-configmap",
            "default",
            ["KEY=value", "KEY2=value2"]
        );

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.CoreV1.ReplaceNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Name,
            model.Namespace
        ).Throws(httpException);

        // Act
        var result = await _configMapService.UpdateConfigMap(model);

        // Assert
        result.Should().BeNull();
        await _kubernetes.CoreV1.Received(1).ReplaceNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Name,
            model.Namespace
        );
    }

    [Fact]
    public async Task DeleteConfigMap_Should_Return_Status()
    {
        // Arrange
        var name = "test-configmap";
        var @namespace = "default";

        var statusResponse = new HttpOperationResponse<V1Status>
        {
            Body = new V1Status()
        };

        _kubernetes.CoreV1.DeleteNamespacedConfigMapWithHttpMessagesAsync(
            name,
            @namespace
        ).Returns(statusResponse);

        // Act
        var result = await _configMapService.DeleteConfigMap(name, @namespace);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Status>();
        await _kubernetes.CoreV1.Received(1).DeleteNamespacedConfigMapWithHttpMessagesAsync(
            name,
            @namespace
        );
    }

    [Fact]
    public async Task DeleteConfigMap_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var name = "non-existent-configmap";
        var @namespace = "default";

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.CoreV1.DeleteNamespacedConfigMapWithHttpMessagesAsync(
            name,
            @namespace
        ).Throws(httpException);

        // Act
        var result = await _configMapService.DeleteConfigMap(name, @namespace);

        // Assert
        result.Should().BeNull();
        await _kubernetes.CoreV1.Received(1).DeleteNamespacedConfigMapWithHttpMessagesAsync(
            name,
            @namespace
        );
    }

    [Fact]
    public async Task CreateOrReplaceConfigMap_WhenConfigMapExists_Should_Update_ExistingConfigMap()
    {
        // Arrange
        var model = new ConfigMapModel(
            "test-configmap",
            "default",
            ["KEY=value", "KEY2=value2"]
        );
        var updatedModel = new ConfigMapModel(
            "test-configmap",
            "default",
            ["KEY=value", "KEY2=value2"]
        );

        var configMap = new V1ConfigMap
        {
            Metadata = new V1ObjectMeta
            {
                Name = model.Name,
                NamespaceProperty = model.Namespace
            }
        };

        var updatedConfigMap = new V1ConfigMap
        {
            Metadata = new V1ObjectMeta
            {
                Name = updatedModel.Name,
                NamespaceProperty = updatedModel.Namespace
            }
        };

        _kubernetes.CoreV1.ReadNamespacedConfigMapWithHttpMessagesAsync(
            model.Name,
            model.Namespace
        ).Returns(new HttpOperationResponse<V1ConfigMap> { Body = configMap });

        _kubernetes.CoreV1.ReplaceNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Name,
            model.Namespace
        ).Returns(new HttpOperationResponse<V1ConfigMap> { Body = updatedConfigMap });

        // Act
        var result = await _configMapService.CreateOrReplaceConfigMap(updatedModel);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(updatedConfigMap);
        await _kubernetes.CoreV1.DidNotReceive().CreateNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Namespace
        );
    }

    [Fact]
    public async Task CreateOrReplaceConfigMap_WhenConfigMapNotExists_Should_Create_And_Return_NewConfigMap()
    {
        // Arrange
        var model = new ConfigMapModel(
            "test-configmap",
            "default",
            ["KEY=value", "KEY2=value2"]
        );

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        _kubernetes.CoreV1.ReadNamespacedConfigMapWithHttpMessagesAsync(
            model.Name,
            model.Namespace
        ).Throws(httpException);

        var newConfigMap = new V1ConfigMap
        {
            Metadata = new V1ObjectMeta
            {
                Name = model.Name,
                NamespaceProperty = model.Namespace
            }
        };

        _kubernetes.CoreV1.CreateNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Namespace
        ).Returns(new HttpOperationResponse<V1ConfigMap> { Body = newConfigMap });
        
        _kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>())
            .Returns(new HttpOperationResponse<V1Namespace> { Body = new V1Namespace() });
        
        // Act
        var result = await _configMapService.CreateOrReplaceConfigMap(model);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(newConfigMap);
        await _kubernetes.CoreV1.Received(1).CreateNamespacedConfigMapWithHttpMessagesAsync(
            Arg.Any<V1ConfigMap>(),
            model.Namespace
        );
    }
} 