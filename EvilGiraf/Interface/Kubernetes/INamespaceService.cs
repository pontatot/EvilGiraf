using k8s.Models;

namespace EvilGiraf.Interface.Kubernetes;

public interface INamespaceService
{
    public Task<V1Namespace> CreateNamespace(string name);

    public Task<V1Namespace?> ReadNamespace(string name);

    public Task<V1Namespace?> CreateIfNotExistsNamespace(string name);
}  