using pawKitLib;

namespace pawKitLib.Tests;

public class ExampleTests
{
    [Fact]
    public void Example_Test_Should_Pass()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        Assert.True(actual);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 5, 10)]
    [InlineData(-1, 1, 0)]
    public void Add_Numbers_Should_Return_Correct_Sum(int a, int b, int expected)
    {
        // Arrange & Act
        var result = a + b;

        // Assert
        Assert.Equal(expected, result);
    }
}