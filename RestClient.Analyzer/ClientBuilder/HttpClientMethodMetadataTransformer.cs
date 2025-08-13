using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RestClient.Analyzer.ClientBuilder;

internal sealed class HttpClientMethodMetadataTransformer
{
    private static readonly List<string> HttpMethodAttributes =
    [
        "HttpGetAttribute",
        "HttpPostAttribute",
        "HttpPutAttribute",
        "HttpDeleteAttribute",
        "HttpPatchAttribute",
        "HttpHeadAttribute",
        "HttpOptionsAttribute"
    ];

    public HttpClientMethodMetadata? Transform(
        IMethodSymbol method,
        HttpClientMetadata httpClientMetadata,
        CancellationToken ct)
    {
        if (method.ReturnType is not INamedTypeSymbol actionMethodReturnType)
        {
            throw new ArgumentException("Method return type must be a named type symbol.", method.ReturnType.Name);
        }

        var (methodRoute, methodType) = GetMethodRoute(httpClientMetadata, method);
        if (methodRoute is null || methodType is null)
        {
            return null;
        }

        var returnType = UnwrapMethodReturnType(actionMethodReturnType);
        var returnTypeNamespace = returnType.ContainingNamespace.ToDisplayString();
        var returnTypeName = returnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        var methodMetadata =
            new HttpClientMethodMetadata(returnTypeNamespace, returnTypeName, method.Name, methodRoute, methodType);

        foreach (var parameter in method.Parameters)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            var parameterTypeNamespace = parameter.Type.ContainingNamespace.ToDisplayString();
            var parameterTypeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var parameterMetadata = new HttpClientMethodParameterMetadata(
                parameter.Name,
                parameterTypeName,
                parameterTypeNamespace);

            methodMetadata.AddParameter(parameterMetadata);
        }

        return methodMetadata;
    }

    private (string? route, HttpMethod? type) GetMethodRoute(
        HttpClientMetadata httpClientMetadata,
        IMethodSymbol method)
    {
        var methodAttributes = method.GetAttributes();
        var httpAttribute = methodAttributes.FirstOrDefault(a =>
            a.AttributeClass?.Name is not null && HttpMethodAttributes.Contains(a.AttributeClass.Name));

        if (httpAttribute is null)
        {
            return (null, null);
        }

        var httpMethod = httpAttribute.AttributeClass?.Name switch
        {
            "HttpGetAttribute" => HttpMethod.Get,
            "HttpPostAttribute" => HttpMethod.Post,
            "HttpPutAttribute" => HttpMethod.Put,
            "HttpDeleteAttribute" => HttpMethod.Delete,
            "HttpPatchAttribute" => new HttpMethod("PATCH"),
            "HttpHeadAttribute" => HttpMethod.Head,
            "HttpOptionsAttribute" => HttpMethod.Options,
            _ => null
        };

        if (httpAttribute.ConstructorArguments.FirstOrDefault().Value is not string routeArgument)
        {
            return (httpClientMetadata.ControllerRoute, httpMethod);
        }

        return (httpClientMetadata.ControllerRoute + "/" + routeArgument, httpMethod);
    }
    

    private INamedTypeSymbol UnwrapMethodReturnType(INamedTypeSymbol returnType)
    {
        var compilation = CSharpCompilation.Create(null);

        if (returnType is { IsGenericType: true, Name: "Task" or "ValueTask" or "ActionResult" })
        {
            if (returnType.TypeArguments[0] is INamedTypeSymbol innerType)
            {
                return UnwrapMethodReturnType(innerType);
            }

            throw new ArgumentException("Method return type must be a named type symbol.", returnType.Name);
        }

        if (returnType.Name is "ActionResult" or "IActionResult" or "Task" or "ValueTask")
        {
            return compilation.GetSpecialType(SpecialType.System_Void);
        }

        return returnType;
    }
}