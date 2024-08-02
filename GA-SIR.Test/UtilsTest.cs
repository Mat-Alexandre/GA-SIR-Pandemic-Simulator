namespace GA_SIR.Test;

public class UtilsTest
{
    [Theory]
    [InlineData("file")]
    public void ReadFile_ShouldThrowException_WhenFileNotFound(string fileName)
    {
        Assert.Throws<FileNotFoundException>(() => Utils.ReadFile(fileName, ","));
    }

    [Fact]
    public void ReadFile_ShouldThrowException_WhenFileNameIsNotGiven()
    {
        Assert.Throws<ArgumentException>(() => Utils.ReadFile("", ","));
    }

    [Theory]
    [InlineData(0, 100, 101)]
    public void Interpolate_ShouldThrowException_WhenValueIsOutOfRAnge(int min, int max, int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Utils.Interpolate(value, min, max));
    }

    [Theory]
    [InlineData(new byte[] { 0, 0, 0, 0 }, 0)]
    [InlineData(new byte[] { 1, 0, 0, 0 }, 8)]
    [InlineData(new byte[] { 1, 1, 1, 1 }, 15)]
    public void BinaryArrayToDecimal_ShouldConvert(byte[] array, int expected)
    {
        var actual = Utils.BinaryArrayToDecimal(array);
        Assert.True(actual == expected);
    }

    [Theory]
    [InlineData(new byte[] { 2, 0, 0, 1 })]
    public void BinaryArrayToDecimal_ShouldThrowException_WhenArrayContainsNonBinaryNumbers(byte[] array)
    {
        Assert.Throws<FormatException>(() => Utils.BinaryArrayToDecimal(array));
    }

    [Theory]
    [InlineData(new int[] { 1, 2, 3, 4, 5 }, 2, new int[] { 1, 2 })]
    public void GetRandomElements_ShouldReturnARandomSample(int[] source, int sampleSize, int[] expected)
    {
        var actual = Utils.GetRandomElements(source, sampleSize, new Random(12345));
        Assert.Equal(expected, actual);
        Assert.True(actual.Count() == 2);
    }

    [Theory]
    [InlineData(
        new uint[] { 100, 200, 300 },
        new uint[] { 100, 200, 301 },
        (float)1 / 3
    )]
    public void MeanSquaredError_ShouldEvaluate(uint[] yTrue, uint[] yPred, float expected)
    {
        var actual = Utils.MeanSquaredError(yTrue, yPred);
        Assert.Equal(expected, actual);
    }
}