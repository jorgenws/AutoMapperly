using Riok.Mapperly.Abstractions;

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

            IMapper<Test, TestDto> mapper = new TestMapper();

            var dto = mapper.Map(test);

            Assert.Equal(test.Text, dto.Text);
            Assert.Equal(test.Value, dto.Value);
        }
    }

    public interface Map2<TIn, TOut>
    {
        TOut Map(TIn input);
    }

    public record TestDto(string Text, int Value);

    public record Test(string Text, int Value);

    [Mapper]
    public partial class TestMapper
    {
        public partial TestDto MapTestDto(Test test);
    }
}
