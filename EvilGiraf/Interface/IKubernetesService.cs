using EvilGiraf.Model;

namespace EvilGiraf.Interface;

public interface IKubernetesService
{
    public Task Deploy(Application app, int timeoutSeconds = 600);
    public Task Undeploy(Application app);
}