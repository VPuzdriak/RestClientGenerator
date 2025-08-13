using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;

namespace RestClient.Analyzer.ClientBuilder;

internal sealed class HttpClientTemplateEngine
{
    private const string HttpClientInterfaceTemplate =
        """
        #nullable enable

        using System.Threading.Tasks;
        {{Usings}}

        namespace {{Namespace}} {
          public interface {{InterfaceName}} {
        {{Methods}}
          }
        }
        """;

    private const string HttpClientClassTemplate =
        """
        #nullable enable

        using System.Net.Http;
        using System.Text;
        using System.Text.Json;
        using System.Threading.Tasks;

        {{Usings}}

        namespace {{Namespace}} {
          public partial class {{ClassName}} : {{InterfaceName}} {
            private readonly HttpClient _httpClient;
            
            public {{ClassName}}(HttpClient httpClient) {
                _httpClient = httpClient;
            }

        {{Methods}}
          }
        }
        """;

    private const string HttpClientMethodContractTemplate =
        """
            {{ReturnType}} {{MethodName}}({{Parameters}});
        """;

    private const string NoResponseHttpClientMethodTemplate =
        """
            public async Task {{MethodName}}({{Parameters}})
            {
                var httpRequestMessage = new HttpRequestMessage({{MethodType}}, {{MethodUrl}});
                {{RequestContentInitializer}}
                
                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
                httpResponseMessage.EnsureSuccessStatusCode();
            }
        """;

    private const string HttpClientMethodTemplate =
        """
            public async {{ReturnType}} {{MethodName}}({{Parameters}})
            {
                var httpRequestMessage = new HttpRequestMessage({{MethodType}}, {{MethodUrl}});
                {{RequestContentInitializer}}
                
                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
                httpResponseMessage.EnsureSuccessStatusCode();
                
                var httpResponseBody = await httpResponseMessage.Content.ReadFromJsonAsync<{{ResponseType}}>();
                return httpResponseBody!;
            }
        """;

    public string GenerateHttpClientInterface(HttpClientMetadata httpClientMetadata)
    {
        var builder = HttpClientInterfaceTemplate
            .Replace("{{Usings}}", string.Join("\n", httpClientMetadata.Usings.Select(u => $"using {u};")))
            .Replace("{{Namespace}}", httpClientMetadata.ClientNamespace)
            .Replace("{{InterfaceName}}", httpClientMetadata.ClientInterfaceName)
            .Replace("{{Methods}}", string.Join("\n", httpClientMetadata.Methods.Select(GenerateMethodContract)));

        return builder;
    }

    public string GenerateHttpClientClass(HttpClientMetadata httpClientMetadata)
    {
        var builder = HttpClientClassTemplate
            .Replace("{{Usings}}", string.Join("\n", httpClientMetadata.Usings.Select(u => $"using {u};")))
            .Replace("{{Namespace}}", httpClientMetadata.ClientNamespace)
            .Replace("{{ClassName}}", httpClientMetadata.ClientClassName)
            .Replace("{{InterfaceName}}", httpClientMetadata.ClientInterfaceName)
            .Replace("{{Methods}}", string.Join("\n", httpClientMetadata.Methods.Select(GenerateMethodImplementation)));

        return builder;
    }

    private string GenerateMethodImplementation(HttpClientMethodMetadata methodMetadata)
    {
        if (methodMetadata.ReturnType == "void")
        {
            return GenerateMethodImplementationWithNoResponse(methodMetadata);
        }

        return GenerateMethodImplementationWithResponse(methodMetadata);
    }

    private string GenerateMethodImplementationWithResponse(HttpClientMethodMetadata methodMetadata)
    {
        var parameters = GetMethodParameters(methodMetadata);
        var methodType = GetMethodType(methodMetadata);
        var methodUrl = GetMethodUrl(methodMetadata);
        var requestContentInitializer = GetRequestContentInitializer(methodMetadata);

        var builder = new StringBuilder(HttpClientMethodTemplate)
            .Replace("{{ReturnType}}", $"Task<{methodMetadata.ReturnType}>")
            .Replace("{{MethodName}}", methodMetadata.Name)
            .Replace("{{Parameters}}", parameters)
            .Replace("{{MethodType}}", methodType)
            .Replace("{{MethodUrl}}", methodUrl)
            .Replace("{{RequestContentInitializer}}", requestContentInitializer)
            .Replace("{{ResponseType}}", methodMetadata.ReturnType);

        return builder.ToString();
    }

    private string GenerateMethodImplementationWithNoResponse(HttpClientMethodMetadata methodMetadata)
    {
        var parameters = GetMethodParameters(methodMetadata);
        var methodType = GetMethodType(methodMetadata);
        var methodUrl = GetMethodUrl(methodMetadata);
        var requestContentInitializer = GetRequestContentInitializer(methodMetadata);

        var builder = new StringBuilder(NoResponseHttpClientMethodTemplate)
            .Replace("{{ReturnType}}", "void")
            .Replace("{{MethodName}}", methodMetadata.Name)
            .Replace("{{Parameters}}", parameters)
            .Replace("{{MethodType}}", methodType)
            .Replace("{{MethodUrl}}", methodUrl)
            .Replace("{{RequestContentInitializer}}", requestContentInitializer);

        return builder.ToString();
    }

    private string GenerateMethodContract(HttpClientMethodMetadata methodMetadata)
    {
        var builder = new StringBuilder(HttpClientMethodContractTemplate)
            .Replace("{{ReturnType}}",
                methodMetadata.ReturnType == "void" ? "Task" : $"Task<{methodMetadata.ReturnType}>")
            .Replace("{{MethodName}}", methodMetadata.Name)
            .Replace("{{Parameters}}", string.Join(", ", methodMetadata.Parameters.Select(p => $"{p.Type} {p.Name}")));

        return builder.ToString();
    }

    private string GetMethodParameters(HttpClientMethodMetadata methodMetadata)
        => string.Join(", ", methodMetadata.Parameters.Select(p => $"{p.Type} {p.Name}"));

    private string GetMethodType(HttpClientMethodMetadata methodMetadata)
        => methodMetadata.MethodType.Method switch
        {
            "GET" => "HttpMethod.Get",
            "POST" => "HttpMethod.Post",
            "PUT" => "HttpMethod.Put",
            "PATCH" => "HttpMethod.Patch",
            "DELETE" => "HttpMethod.Delete",
            _ => throw new NotSupportedException($"HTTP method {methodMetadata.MethodType} is not supported.")
        };

    private string GetMethodUrl(HttpClientMethodMetadata methodMetadata)
    {
        var methodUrlBuilder = new StringBuilder($"$\"{methodMetadata.Route}\"");
        foreach (var parameter in methodMetadata.Parameters)
        {
            methodUrlBuilder.Replace($"{parameter.Name}:{parameter.Type}", parameter.Name);
        }

        return methodUrlBuilder.ToString();
    }

    private string GetRequestContentInitializer(HttpClientMethodMetadata methodMetadata)
    {
        if (methodMetadata.ContentParameter is null)
        {
            return string.Empty;
        }

        var contentType = "application/json";
        var contentInitializer =
            $"httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize({methodMetadata.ContentParameter.Name}), Encoding.UTF8, \"{contentType}\");";

        return contentInitializer;
    }
}