using DocumentIntelligence.MiniRag.Search;
using FluentAssertions;

namespace DocumentIntelligence.MiniRag.Tests.Search;

public class CosineSimilarityTests
{
    [Fact]
    public void Calculate_Should_Return_One_For_Identical_Vectors()
    {
        double result = CosineSimilarity.Calculate([1f, 2f, 3f], [1f, 2f, 3f]);

        result.Should().BeApproximately(1.0, precision: 1e-6);
    }

    [Fact]
    public void Calculate_Should_Return_Zero_For_Orthogonal_Vectors()
    {
        double result = CosineSimilarity.Calculate([1f, 0f], [0f, 1f]);

        result.Should().BeApproximately(0.0, precision: 1e-6);
    }

    [Fact]
    public void Calculate_Should_Return_Zero_When_Vector_Has_Zero_Magnitude()
    {
        double result = CosineSimilarity.Calculate([0f, 0f], [1f, 2f]);

        result.Should().Be(0);
    }

    [Fact]
    public void Calculate_Should_Throw_When_Vector_Lengths_Differ()
    {
        Action act = () => CosineSimilarity.Calculate([1f, 2f], [1f]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Calculate_Should_Throw_When_Vector_Is_Empty()
    {
        Action act = () => CosineSimilarity.Calculate([], []);

        act.Should().Throw<ArgumentException>();
    }
}
