using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RestClient.Analyzer.ClientBuilder;

[Generator]
public sealed class HttpClientGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
                "Microsoft.AspNetCore.Mvc.ApiControllerAttribute",
                Predicate,
                Transform);

        context.RegisterSourceOutput(provider, Generate);
    }

    private void Generate(SourceProductionContext ctx, HttpClientMetadata? httpClientMetadata)
    {
        if (httpClientMetadata is null)
        {
            return;
        }

        var templateEngine = new HttpClientTemplateEngine();

        var interfaceSource = templateEngine.GenerateHttpClientInterface(httpClientMetadata);
        ctx.AddSource(httpClientMetadata.ClientInterfaceFileName, SourceText.From(interfaceSource, Encoding.UTF8));

        var classSource = templateEngine.GenerateHttpClientClass(httpClientMetadata);
        ctx.AddSource(httpClientMetadata.ClientFileName, SourceText.From(classSource, Encoding.UTF8));
    }

    private HttpClientMetadata? Transform(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
        => new HttpClientMetadataTransformer().Transform(ctx, ct);

    private bool Predicate(SyntaxNode node, CancellationToken ct) => true;
}