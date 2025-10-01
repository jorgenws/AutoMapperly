using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapperly
{
    [Generator]
    public class AutoMapperlyInterfaceSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("IMapper.g.cs", @"
namespace AutoMapperly
{
    public interface IMapper<TInput, TOutput>
    {
        TOutput Map(TInput input);
    }
}"
                );
            });
        }
    }

    [Generator]
    public class AutoMapperlySourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var mapperInfos = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                    transform: static (ctx, _) => Transform(ctx))
                .Where(c => c is not null);

            // Generate source code for each mapper info and add it to the compilation
            context.RegisterSourceOutput(mapperInfos, (spc, mapperInfoList) =>
            {
                if (mapperInfoList == null || mapperInfoList.Count == 0)
                    return;

                foreach (var mapperInfo in mapperInfoList)
                {
                    var source = $@"
            namespace {mapperInfo.NamespaceName}
            {{
            public partial class {mapperInfo.ClassName} : IMapper<{mapperInfo.InputTypeName},{mapperInfo.OutputTypeName}>
            {{
                public {mapperInfo.OutputTypeName} Map({mapperInfo.InputTypeName} input)
            {{
                    return {mapperInfo.MethodName}(input);
            }}
            }}
            }}";
                    spc.AddSource($"{mapperInfo.ClassName}_{mapperInfo.MethodName}_Mapperly.g.cs", source);
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
