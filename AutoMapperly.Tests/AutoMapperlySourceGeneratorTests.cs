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

            var dto = new TestMapper().MapTestToTestDto(test);

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

        [Fact]
        public void AutoMapperlyAddsInstanceVersionsOfStaticMappers()
        {
            var provider = new ServiceCollection()
                .AddMappers()
                .BuildServiceProvider();

            var mapper = provider.GetRequiredService<IMapper<TestTwo, TestTwoDto>>();

            var test = new TestTwo("Vogon", 42);

            var dto = mapper.Map(test);

            Assert.Equal(test.Text, dto.Text);
            Assert.Equal(test.Value, dto.Value);
        }

        [Fact]
        public void AutoMapperlyAddsInstanceVersionsOfMappers()
        {
            var provider = new ServiceCollection()
                .AddMappers()
                .BuildServiceProvider();

            var mapper = provider.GetRequiredService<IMapper<TestThree, TestThreeDto>>();

            var test = new TestThree
            {
                Text = "Vogon",
                Value = 42
            };

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
        public partial TestDto MapTestToTestDto(Test test);
    }

    public record TestTwoDto(string Text, int Value);

    public record TestTwo(string Text, int Value);

    [Mapper]
    public static partial class TestTwoMapper
    {
        public static partial TestTwoDto MapTestTwoToTestDto(TestTwo test);
    }

    public class TestThree 
    {
        public required string Text { get; set; }
        public int Value { get; set; }
    }
    public record TestThreeDto
    {
        public required string Text { get; set; }
        public int Value { get; set; }
    }

    [Mapper]
    public static partial class TestThreeMapper
    {
        public static partial TestThreeDto MapTestThreeToTestThreeDto(this TestThree test);
    }
}
