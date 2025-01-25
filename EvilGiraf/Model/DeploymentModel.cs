namespace EvilGiraf.Model;

public record DeploymentModel(string Name, string Namespace, int Replicas, string Image, int[] Ports);