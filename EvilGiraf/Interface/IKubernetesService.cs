using EvilGiraf.Model;

namespace EvilGiraf.Interface;

public interface IKubernetesService
{
    public Task Deploy(Application app);
    public Task Undeploy(Application app);
}