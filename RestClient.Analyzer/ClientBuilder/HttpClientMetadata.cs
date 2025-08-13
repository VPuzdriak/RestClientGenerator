using System;
using System.Collections.Generic;

namespace RestClient.Analyzer.ClientBuilder;

internal sealed class HttpClientMetadata
{
    private readonly HashSet<string> _usings = [];
    private readonly List<HttpClientMethodMetadata> _methods = [];

    public HttpClientMetadata(string controllerNamespace, string controllerName, string? controllerRoute)
    {
        ControllerNamespace = controllerNamespace;
        ControllerName = controllerName;
        ControllerRoute = controllerRoute;
        ClientNamespace = GetClientNamespace(controllerNamespace);
        ClientClassName = controllerName.Replace("Controller", "Client");
    }


    public string ControllerName { get; }
    public string ControllerNamespace { get; }
    public string? ControllerRoute { get; }
    public string ClientNamespace { get; }
    public string ClientClassName { get; }

    public string ClientFileName => $"{ClientClassName}.g.cs";
    public string ClientInterfaceName => $"I{ClientClassName}";
    public string ClientInterfaceFileName => $"{ClientInterfaceName}.g.cs";

    public IReadOnlyCollection<string> Usings => _usings;
    public IReadOnlyList<HttpClientMethodMetadata> Methods => _methods;

    public void AddMethods(IEnumerable<HttpClientMethodMetadata> methodMetadata)
    {
        foreach (var method in methodMetadata)
        {
            _usings.Add(method.ReturnTypeNamespace);

            foreach (var parameter in method.Parameters)
            {
                _usings.Add(parameter.TypeNamespace);
            }

            _methods.Add(method);
        }
    }

    private string GetClientNamespace(string controllerNamespace)
    {
        var controllersSegmentIndex = ControllerNamespace.LastIndexOf(".Controllers", StringComparison.Ordinal);
        var finalSegment = ".Clients";

        if (controllersSegmentIndex > -1)
        {
            return controllerNamespace.Substring(0, controllersSegmentIndex) + finalSegment;
        }

        return controllerNamespace + finalSegment;
    }
}

internal sealed class HttpClientMethodParameterMetadata
{
    public string Name { get; }
    public string Type { get; }
    public string TypeNamespace { get; }

    public HttpClientMethodParameterMetadata(string name, string type, string typeNamespace)
    {
        Name = name;
        Type = type;
        TypeNamespace = typeNamespace;
    }
}