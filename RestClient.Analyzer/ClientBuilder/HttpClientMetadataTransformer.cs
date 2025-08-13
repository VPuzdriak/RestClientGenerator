using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace RestClient.Analyzer.ClientBuilder;

internal sealed class HttpClientMetadataTransformer
{
    public HttpClientMetadata? Transform(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol controllerDeclaration)
        {
            return null;
        }

        var controllerNamespace = controllerDeclaration.ContainingNamespace.ToDisplayString();
        var controllerName = controllerDeclaration.Name;
        var controllerRoute = controllerDeclaration.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == "RouteAttribute")?
            .ConstructorArguments[0].Value as string ?? string.Empty;

        var clientMetadata = new HttpClientMetadata(controllerNamespace, controllerName, controllerRoute);
        var methods = TransformMethods(clientMetadata, controllerDeclaration, ct);
        clientMetadata.AddMethods(methods);

        return clientMetadata;
    }

    private List<HttpClientMethodMetadata> TransformMethods(
        HttpClientMetadata httpClientMetadata,
        INamedTypeSymbol controllerDeclaration,
        CancellationToken ct)
    {
        var methods = new List<HttpClientMethodMetadata>();

        foreach (var method in controllerDeclaration.GetMembers().OfType<IMethodSymbol>())
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            var methodTransformer = new HttpClientMethodMetadataTransformer();
            var methodMetadata = methodTransformer.Transform(method, httpClientMetadata, ct);

            if (methodMetadata is not null)
            {
                methods.Add(methodMetadata);
            }
        }

        return methods;
    }
}