using Microsoft.CodeAnalysis;

namespace AutoMapperly
{
    [Generator]
    public class AutoMapperlyInterfaceSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("IMap.AutoMapperly.g.cs", @"
namespace AutoMapperly
{
    public interface IMap<TInput, TOutput>
    {
        TOutput Map(TInput input);
    }
}"
                );
            });
        }
    }
}
