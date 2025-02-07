using k8s;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace EvilGiraf.IntegrationTests;

public class SubstituteWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var kubeDescriptor = new ServiceDescriptor(typeof(IKubernetes), Substitute.For<IKubernetes>());
            services.Replace(kubeDescriptor);
        });
    }
}