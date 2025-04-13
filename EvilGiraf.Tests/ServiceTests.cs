using System.Net;
using EvilGiraf.Interface;
using EvilGiraf.Model;
using EvilGiraf.Service.Kubernetes;
using FluentAssertions;
using k8s.Models;
using k8s;
using k8s.Autorest;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvilGiraf.Tests;

public class ServiceTests
{
    private IKubernetes Kubernetes { get; }
    private IServiceService ServiceService { get; }

    public ServiceTests()
    {
        Kubernetes = Substitute.For<IKubernetes>();
        
        var corev1OperationsSubstitute = Substitute.For<ICoreV1Operations>();
        
        var serviceResponse = new HttpOperationResponse<V1Service>();
        serviceResponse.Body = new V1Service();
        corev1OperationsSubstitute.CreateNamespacedServiceWithHttpMessagesAsync(Arg.Any<V1Service>(), Arg.Any<string>())
            .Returns(serviceResponse);
            
        var deleteResponse = new HttpOperationResponse<V1Service>{ Body = new V1Service()};
        corev1OperationsSubstitute.DeleteNamespacedServiceWithHttpMessagesAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(deleteResponse);
            
        var updateResponse = new HttpOperationResponse<V1Service>();
        updateResponse.Body = new V1Service();
        corev1OperationsSubstitute.ReplaceNamespacedServiceWithHttpMessagesAsync(Arg.Any<V1Service>(), Arg.Any<string>(), Arg.Any<string>(), null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(updateResponse);
            
        corev1OperationsSubstitute.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>())
            .Returns(new HttpOperationResponse<V1Namespace>());
            
        Kubernetes.CoreV1.Returns(corev1OperationsSubstitute);
        
        ServiceService = new ServiceService(Kubernetes);
    }
    
    [Fact]
    public async Task CreateService_Should_Return_Service()
    {
        var model = new ServiceModel(
            "service-test",
            "default",
            80
        );
        var result = await ServiceService.CreateService(model);
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Service>();
        await Kubernetes.CoreV1.Received().CreateNamespacedServiceWithHttpMessagesAsync(Arg.Any<V1Service>(), "default");
    }
    
    [Fact]
    public async Task CreateService_with_namespace_not_exist_Should_Return_Service()
    {
        var response = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };
        Kubernetes.CoreV1.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>()).Throws(response);
        Kubernetes.CoreV1.CreateNamespaceWithHttpMessagesAsync(Arg.Any<V1Namespace>()).Returns(new HttpOperationResponse<V1Namespace>());
        
        var model = new ServiceModel(
            "service-test",
            "default",
            80
        );
        var result = await ServiceService.CreateService(model);
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Service>();
        await Kubernetes.CoreV1.Received().CreateNamespacedServiceWithHttpMessagesAsync(Arg.Any<V1Service>(), "default");
    }
    
    [Fact]
    public async Task ReadService_Should_Return_Service()
    {
        var serviceName = "service-test";
        var @namespace = "default";

        var serviceResponse = new HttpOperationResponse<V1Service>
        {
            Body = new V1Service
            {
                Metadata = new V1ObjectMeta
                {
                    Name = serviceName,
                    NamespaceProperty = @namespace
                }
            }
        };

        Kubernetes.CoreV1.ReadNamespacedServiceWithHttpMessagesAsync(serviceName, @namespace, null, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(serviceResponse));

        var result = await ServiceService.ReadService(serviceName, @namespace);
        
        result.Should().NotBeNull();
        result!.Metadata.Name.Should().Be(serviceName);
        result.Metadata.NamespaceProperty.Should().Be(@namespace);
        
        await Kubernetes.CoreV1.Received().ReadNamespacedServiceWithHttpMessagesAsync(serviceName, @namespace, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadService_Should_Return_null_When_NotFound()
    {
        var serviceName = "non-existent-service";
        var @namespace = "default";

        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };

        Kubernetes.CoreV1.ReadNamespacedServiceWithHttpMessagesAsync(serviceName, @namespace, null, null, Arg.Any<CancellationToken>())
            .Throws(httpException);

        var result = await ServiceService.ReadService(serviceName, @namespace);
        
        result.Should().BeNull();

        await Kubernetes.CoreV1.Received().ReadNamespacedServiceWithHttpMessagesAsync(serviceName, @namespace, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteService_Should_Return_Service()
    {
        var result = await ServiceService.DeleteService("service-test", "default");
        result.Should().NotBeNull()
            .And.BeOfType<V1Service>();
        await Kubernetes.CoreV1.Received().DeleteNamespacedServiceWithHttpMessagesAsync("service-test", "default");
    }

    [Fact]
    public async Task DeleteService_Should_Return_Null()
    {
        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };
        Kubernetes.CoreV1.DeleteNamespacedServiceWithHttpMessagesAsync("service-test", "default")
            .Throws(httpException);
        var result = await ServiceService.DeleteService("service-test", "default");
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateService_Should_Return_Service()
    {
        var model = new ServiceModel(
            "service-test",
            "default",
            80
        );
        var result = await ServiceService.UpdateService(model);
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Service>();
        await Kubernetes.CoreV1.Received().ReplaceNamespacedServiceWithHttpMessagesAsync(Arg.Any<V1Service>(), "service-test", "default", null, null, null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateService_Should_Return_Null()
    {
        var model = new ServiceModel(
            "service-test",
            "default",
            80
        );
        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };
        Kubernetes.CoreV1.ReplaceNamespacedServiceWithHttpMessagesAsync(Arg.Any<V1Service>(), "service-test", "default", null, null, null, null, null, Arg.Any<CancellationToken>())
            .Throws(httpException);
        var result = await ServiceService.UpdateService(model);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateIfNotExistsService_Should_Return_Existing_Service()
    {
        var model = new ServiceModel(
            "service-test",
            "default",
            80
        );
        
        var existingService = new V1Service
        {
            Metadata = new V1ObjectMeta
            {
                Name = model.Name,
                NamespaceProperty = model.Namespace
            }
        };
        
        Kubernetes.CoreV1.ReadNamespacedServiceWithHttpMessagesAsync(model.Name, model.Namespace, null, null, Arg.Any<CancellationToken>())
            .Returns(new HttpOperationResponse<V1Service> { Body = existingService });

        var result = await ServiceService.CreateIfNotExistsService(model);
        
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Service>();
        result.Metadata.Name.Should().Be(model.Name);
        result.Metadata.NamespaceProperty.Should().Be(model.Namespace);
        
        await Kubernetes.CoreV1.DidNotReceive().CreateNamespacedServiceWithHttpMessagesAsync(Arg.Any<V1Service>(), Arg.Any<string>());
    }

    [Fact]
    public async Task CreateIfNotExistsService_Should_Create_New_Service()
    {
        var model = new ServiceModel(
            "service-test",
            "default",
            80
        );
        
        var httpException = new HttpOperationException
        {
            Response = new HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
        };
        
        Kubernetes.CoreV1.ReadNamespacedServiceWithHttpMessagesAsync(model.Name, model.Namespace, null, null, Arg.Any<CancellationToken>())
            .Throws(httpException);
            
        var newService = new V1Service
        {
            Metadata = new V1ObjectMeta
            {
                Name = model.Name,
                NamespaceProperty = model.Namespace
            }
        };
        
        Kubernetes.CoreV1.CreateNamespacedServiceWithHttpMessagesAsync(Arg.Any<V1Service>(), Arg.Any<string>())
            .Returns(new HttpOperationResponse<V1Service> { Body = newService });

        var result = await ServiceService.CreateIfNotExistsService(model);
        
        result.Should().NotBeNull();
        result.Should().BeOfType<V1Service>();
        result.Metadata.Name.Should().Be(model.Name);
        result.Metadata.NamespaceProperty.Should().Be(model.Namespace);
        
        await Kubernetes.CoreV1.Received().CreateNamespacedServiceWithHttpMessagesAsync(Arg.Any<V1Service>(), model.Namespace);
    }
}