using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AutoMapperly
{
    [Generator]
    public class AutoMapperlySourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var mapperInfosProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                    transform: static (ctx, _) => Transform(ctx))
                .Where(c => c is not null)
                .Collect();

            var hasDIProvider = context.CompilationProvider
                .Select((compilation, _) => compilation.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.IServiceCollection") != null);

            var comboProvider = mapperInfosProvider.Combine(hasDIProvider);

            // Generate source code for each mapper info and add it to the compilation
            context.RegisterSourceOutput(comboProvider, (spc, providers) =>
            {
                (ImmutableArray<List<MapperInfo>> mapperInfos, bool hasDI) = providers;

                if (mapperInfos == null)
                {
                    return;
                }

                foreach (var innerMapperInfo in mapperInfos)
                foreach (var mapperInfo in innerMapperInfo)
                {
                    var source = $@"
namespace {mapperInfo.NamespaceName}
{{
    public partial class {mapperInfo.ClassName} : IMap<{mapperInfo.InputTypeName},{mapperInfo.OutputTypeName}>
    {{
        public {mapperInfo.OutputTypeName} Map({mapperInfo.InputTypeName} input)
        {{
            return {mapperInfo.MethodName}(input);
        }}
    }}
}}";
                    spc.AddSource($"{mapperInfo.ClassName}_{mapperInfo.MethodName}_AutoMapperly.g.cs", source);
                }

                //Service Collection
                if (hasDI)
                {

                    spc.AddSource("IMapper.AutoMapperly.g.cs", $@"
using Microsoft.Extensions.DependencyInjection; 
namespace AutoMapperly;
public interface IMapper<TIn,TOut>
{{
    TOut Map(TIn input);
}}

public class Mapper<TIn,TOut> : IMapper<TIn,TOut>
{{
    private readonly IServiceProvider _sp;

    public Mapper(IServiceProvider sp)
    {{
        _sp = sp;
    }}

    public TOut Map(TIn input)
    {{
        var mapper = _sp.GetRequiredService<IMap<TIn,TOut>>();
        return mapper.Map(input);
    }}
}}
");

                    var flattendMapperInfo = mapperInfos.SelectMany(m => m).ToList();

                    spc.AddSource("AutoMapperlyExtension.AutoMapperly.g.cs", $@"
using Microsoft.Extensions.DependencyInjection;

namespace AutoMapperly.DI
{{
    public static class AutoMapperlyExtension
    {{
        public static IServiceCollection AddMappers(this IServiceCollection sc)
        {{
            sc.AddScoped(typeof(IMapper<,>), typeof(Mapper<,>));
            {string.Join("\n", flattendMapperInfo
                    .Select(m => $"sc.AddScoped<IMap<{m.InputTypeName},{m.OutputTypeName}>, {m.NamespaceName}.{m.ClassName}>();"))}

            return sc;
        }}
    }}
}}
");


                }
            });
            
        }

        private static List<MapperInfo> Transform(GeneratorSyntaxContext ctx)
        {
            if (!IsMapperlyClass(ctx))
            {
                return null;
            }

            var classDeclaration = ctx.Node as ClassDeclarationSyntax;
            var classSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

            if (classSymbol.IsStatic)
            {
                // Skip static classes for instance mapper generation
                // There will be a seperate implementation for static classes
                return null;
            }

            var namespaceName = classSymbol.ContainingNamespace?.ToDisplayString();

            var methods = classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)));

            var methodSymbols = classSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.IsPartialDefinition);

            var mapperInfo = methodSymbols.Select(m =>
            new MapperInfo {
                NamespaceName = namespaceName,
                ClassName = classSymbol.Name,
                MethodName = m.Name,
                InputTypeName = m.Parameters.FirstOrDefault()?.Type.ToDisplayString(),
                OutputTypeName = m.ReturnType.ToDisplayString()
                }
            ).ToList();

            return mapperInfo;
        }

        private static bool IsMapperlyClass(GeneratorSyntaxContext ctx)
        {
            var classSymbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node as ClassDeclarationSyntax);

            return classSymbol?
                    .GetAttributes()
                    .Any(a => a.AttributeClass?.Name == nameof(MapperAttribute))
                    ?? false;
        }

        public class MapperInfo
        {
            public string NamespaceName { get; set; }
            public string ClassName { get; set; }
            public string MethodName { get; set; }
            public string InputTypeName { get; set; }
            public string OutputTypeName { get; set; }
        }
    }
}
