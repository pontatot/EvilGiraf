using EvilGiraf.Model;

namespace EvilGiraf.Interface;

public interface IGitBuildService
{
    Task<string?> BuildAndPushFromGitAsync(Application app);
}