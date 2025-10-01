using Microsoft.Extensions.DependencyInjection;
using Riok.Mapperly.Abstractions;
using AutoMapperly.DI;

namespace AutoMapperly.Tests
{
    public class AutoMapperlySourceGeneratorTests
    {
        [Fact]
        public void Test1()
        {
            var test = new Test("Vogon", 42);

            var dto = new TestMapper().MapTestDto(test);

            Assert.Equal(test.Text, dto.Text);
            Assert.Equal(test.Value, dto.Value);
        }

        [Fact]
        public void Test2()
        {
            var test = new Test("Vogon", 42);

            IMap<Test, TestDto> mapper = new TestMapper();

            var dto = mapper.Map(test);

            Assert.Equal(test.Text, dto.Text);
            Assert.Equal(test.Value, dto.Value);
        }

        [Fact]
        public void Test3()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddMappers();
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
