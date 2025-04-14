using k8s.Models;

namespace EvilGiraf.Model.Kubernetes;

public record ConfigMapModel(string Name, string Namespace, List<string> Data);

public static class ConfigMapModelExtensions
{
    public static V1ConfigMap ToConfigMap(this ConfigMapModel model)
    {
        return new V1ConfigMap
        {
            Metadata = new V1ObjectMeta
            {
                Name = model.Name
            },
            Data = model.Data.Count > 0 ? model.Data.Select(w =>
            {
                var words = w.Split('=', 2);
                return new KeyValuePair<string,string>(words[0], words[1]);
            }).ToDictionary() : null
        };
    }
}