using EvilGiraf.Interface;
using EvilGiraf.Interface.Kubernetes;
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
    public IKubernetes Kubernetes { get; }
    public IServiceService ServiceService { get; }

    public ServiceTests()
    {
        Kubernetes = Substitute.For<IKubernetes>();

        ServiceService = new ServiceService(Kubernetes);

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

        Kubernetes.AppsV1.Returns(appv1OperationsSubstitute);
        
        var corev1OperationsSubstitute = Substitute.For<ICoreV1Operations>();
        corev1OperationsSubstitute.ReadNamespaceWithHttpMessagesAsync(Arg.Any<string>()).Returns(new HttpOperationResponse<V1Namespace>());
        
        Kubernetes.CoreV1.Returns(corev1OperationsSubstitute);
        
        ServiceService = new ServiceService(Kubernetes);
    }
    
}