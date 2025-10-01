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
}
