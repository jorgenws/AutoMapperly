using Microsoft.Extensions.DependencyInjection;
using Riok.Mapperly.Abstractions;
using AutoMapperly.DI;

namespace AutoMapperly.Tests
{
    public class AutoMapperlySourceGeneratorTests
    {
        [Fact]
        public void MapperlyWorksAsIntended()
        {
            var test = new Test("Vogon", 42);

            var dto = new TestMapper().MapTestDto(test);

            Assert.Equal(test.Text, dto.Text);
            Assert.Equal(test.Value, dto.Value);
        }

        [Fact]
        public void AutoMapperlyInterfaceAddedToMapperlyClass()
        {
            IMap<Test, TestDto> mapper = new TestMapper();

            var test = new Test("Vogon", 42);

            var dto = mapper.Map(test);

            Assert.Equal(test.Text, dto.Text);
            Assert.Equal(test.Value, dto.Value);
        }

        [Fact]
        public void AutoMapperlySetsUpMappersAndUsesIMapperToPickTheAppropriateMapper()
        {
            var provider = new ServiceCollection()
                .AddMappers()
                .BuildServiceProvider();

            var mapper = provider.GetRequiredService<IMapper<Test, TestDto>>();

            var test = new Test("Vogon", 42);

            var dto = mapper.Map(test);

            Assert.Equal(test.Text, dto.Text);
            Assert.Equal(test.Value, dto.Value);
        }
    }

    public record TestDto(string Text, int Value);

    public record Test(string Text, int Value);

    [Mapper]
    public partial class TestMapper
    {
        public partial TestDto MapTestDto(Test test);
    }
}
