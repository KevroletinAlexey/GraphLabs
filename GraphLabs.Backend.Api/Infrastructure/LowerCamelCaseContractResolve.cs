using Newtonsoft.Json.Serialization;

namespace WebApplication2.Infrastructure;

public class LowerCamelCaseContractResolver : DefaultContractResolver
{
    protected override string ResolvePropertyName(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;
        return $"{propertyName.Substring(0, 1).ToLower()}{propertyName.Substring(1)}";
    }
}