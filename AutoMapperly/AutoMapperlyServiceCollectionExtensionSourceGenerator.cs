using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace AutoMapperly
{
    [Generator]
    public class AutoMapperlyServiceCollectionExtensionSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var hasDIProvider = context.CompilationProvider
                .Select((compilation, _) => compilation.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.IServiceCollection") != null);

            var mapperProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax cds && cds.BaseList != null,
                    transform: static (ctx, _) =>
                    {
                        var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
                        var classSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

                        var result = classSymbol.Interfaces
                            .Where(i => i.Name == "IMap" && i.TypeArguments.Length == 2)
                            .Select(i => new MapMetadata
                            {
                                ClassName = classSymbol.Name,
                                InputTypeName = i.TypeArguments[0].Name,
                                OutputTypeName = i.TypeArguments[1].Name
                            })
                            .ToList();

                        return result;
                    })
                .Collect();

            var combinedProvider = hasDIProvider.Combine(mapperProvider);

            context.RegisterSourceOutput(combinedProvider, (spc, tuple) =>
            {
                var hasDI = tuple.Left;
                var mappers = tuple.Right;

                if (hasDI)
                {
                    var flattendMappers = mappers.SelectMany(c => c).ToList();

                    spc.AddSource("AutoMapperlyExtension.AutoMapperly.g.cs", $@"
using Microsoft.Extensions.DependencyInjection; 
namespace AutoMapperly.DI
{{
    public static class AutoMapperlyExtension
    {{
        public static IServiceCollection AddMappers(this IServiceCollection sc)
        {{
            {string.Join("\n", flattendMappers.Select(m => $"sc.AddScoped<IMap<{m.InputTypeName},{m.OutputTypeName}>, {m.ClassName}>();"))}

            return sc;
        }}
    }}
}}
");
                }
            });
        }
    }

    class MapMetadata
    {
        public string ClassName { get; set; }
        public string InputTypeName { get; set; }
        public string OutputTypeName { get; set; }
    }
}
