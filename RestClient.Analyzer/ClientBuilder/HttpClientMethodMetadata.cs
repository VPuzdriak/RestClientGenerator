using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RestClient.Analyzer.ClientBuilder;

internal sealed class HttpClientMethodMetadata
{
    private readonly List<HttpClientMethodParameterMetadata> _parameters = [];
    public string ReturnTypeNamespace { get; set; }
    public string ReturnType { get; }
    public string Name { get; }
    public string Route { get; }
    public HttpMethod MethodType { get; }

    public IReadOnlyList<HttpClientMethodParameterMetadata> Parameters => _parameters;

    public HttpClientMethodParameterMetadata? ContentParameter =>
        _parameters.FirstOrDefault(p => !Route.Contains(p.Name));

    public HttpClientMethodMetadata(
        string returnTypeNamespace,
        string returnType,
        string name,
        string route,
        HttpMethod methodType)
    {
        ReturnTypeNamespace = returnTypeNamespace;
        ReturnType = returnType;
        Name = name;
        Route = route;
        MethodType = methodType;
    }

    public void AddParameter(HttpClientMethodParameterMetadata parameter)
    {
        _parameters.Add(parameter);
    }
}