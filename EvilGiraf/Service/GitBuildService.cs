using System.Text;
using System.Text.Json;
using EvilGiraf.Interface;
using EvilGiraf.Model;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace EvilGiraf.Service;

public class GitBuildService(
    IKubernetes kubernetes,
    IConfiguration configuration) : IGitBuildService
{
    private readonly string _registryUrl = configuration["DockerRegistry:Url"] ?? throw new ConfigurationException("missing configuration DockerRegistry:Url");
    private readonly string? _registryUsername = configuration["DockerRegistry:Username"];
    private readonly string? _registryPassword = configuration["DockerRegistry:Password"];

    public async Task<string?> BuildAndPushFromGitAsync(Application app, int timeoutSeconds = 600)
    {
        var jobName = $"docker-build-{app.Id}-{Guid.NewGuid()}";
        var secret = await EnsureRegistrySecretExistsAsync(app.Id.ToNamespace());
        
        var job = CreateBuildJob(jobName, app, secret);
        await kubernetes.BatchV1.CreateNamespacedJobAsync(job, app.Id.ToNamespace());
        var completedJob = await WaitForJobCompletionAsync(jobName, app.Id.ToNamespace(), timeoutSeconds);

        return IsJobSuccessful(completedJob) ? $"{_registryUrl}/evilgiraf-{app.Id}:{app.Version ?? "latest"}" : null;
    }

    private V1Job CreateBuildJob(string jobName, Application app, bool secret)
    {
        var fullImageName = $"{_registryUrl}/evilgiraf-{app.Id}:{app.Version ?? "latest"}";
        var fullGitUrl = "git://" + app.Link + (string.IsNullOrEmpty(app.Version) ? "" : $"#{app.Version}");
        
        return new V1Job
        {
            Metadata = new V1ObjectMeta
            {
                Name = jobName,
                NamespaceProperty = app.Id.ToNamespace()
            },
            Spec = new V1JobSpec
            {
                BackoffLimit = 0,
                Template = new V1PodTemplateSpec
                {
                    Spec = new V1PodSpec
                    {
                        RestartPolicy = "Never",
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "kaniko",
                                Image = "gcr.io/kaniko-project/executor:latest",
                                Args =
                                [
                                    "--context=" + fullGitUrl,
                                    // "--context-sub-path=EvilGiraf",
                                    "--destination=" + fullImageName,
                                    "--dockerfile=Dockerfile"
                                ],
                                VolumeMounts = secret ?
                                [
                                    new V1VolumeMount
                                    {
                                        Name = "docker-config",
                                        MountPath = "/kaniko/.docker"
                                    }
                                ] :
                                Array.Empty<V1VolumeMount>()
                            }
                        ],
                        Volumes = secret ?
                        [
                            new V1Volume
                            {
                                Name = "docker-config",
                                Secret = new V1SecretVolumeSource
                                {
                                    SecretName = "docker-registry-credentials",
                                    Items =
                                    [
                                        new V1KeyToPath
                                        {
                                            Key = ".dockerconfigjson",
                                            Path = "config.json"
                                        }
                                    ]
                                }
                            }
                        ] :
                        Array.Empty<V1Volume>()
                    }
                }
            }
        };
    }

    private async Task<bool> EnsureRegistrySecretExistsAsync(string @namespace)
    {
        if (string.IsNullOrEmpty(_registryUsername) || string.IsNullOrEmpty(_registryPassword))
            return false;
        try
        {
            await kubernetes.CoreV1.ReadNamespacedSecretAsync("docker-registry-credentials", @namespace);
        }
        catch (HttpOperationException)
        {
            var auth = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_registryUsername}:{_registryPassword}"));
            
            var dockerConfig = new
            {
                auths = new Dictionary<string, object>
                {
                    {
                        _registryUrl, new
                        {
                            username = _registryUsername,
                            password = _registryPassword,
                            auth
                        }
                    }
                }
            };
            
            var dockerConfigJson = JsonSerializer.Serialize(dockerConfig);
            var dockerConfigJsonBytes = Encoding.UTF8.GetBytes(dockerConfigJson);
            
            var secret = new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = "docker-registry-credentials",
                    NamespaceProperty = @namespace
                },
                Type = "kubernetes.io/dockerconfigjson",
                Data = new Dictionary<string, byte[]>
                {
                    { ".dockerconfigjson", dockerConfigJsonBytes }
                }
            };
            
            await kubernetes.CoreV1.CreateNamespacedSecretAsync(secret, @namespace);
        }
        return true;
    }

    private async Task<V1Job?> WaitForJobCompletionAsync(string jobName, string @namespace, int timeoutSeconds = 600)
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddSeconds(timeoutSeconds);
        
        while (DateTime.UtcNow < endTime)
        {
            var job = await kubernetes.BatchV1.ReadNamespacedJobAsync(jobName, @namespace);
            
            if (job.Status.CompletionTime.HasValue || 
                job.Status.Failed is > 0)
            {
                return job;
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        await kubernetes.BatchV1.DeleteNamespacedJobAsync(jobName, @namespace);
        return null;
    }

    private static bool IsJobSuccessful(V1Job? job)
    {
        return job?.Status.Succeeded is > 0;
    }
}