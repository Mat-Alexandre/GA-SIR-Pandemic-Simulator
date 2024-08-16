using static System.Formats.Asn1.AsnWriter;

namespace GA_SIR;

public class Utils
{
    /// <summary>
    /// Read a given file and set its values to an iterator.
    /// </summary>
    /// <param name="filename">Path of the file. It must be a Nx3 set of lines, where N is number of lines.</param>
    /// <param name="separator">The separator of each component in the line. E.g.: ","</param>
    /// <returns>A tuple of each allocated array.</returns>
    public static (uint[], uint[], uint[]) ReadFile(string filename, string separator = ",")
    {
        FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        List<uint> sus = new(), inf = new(), rec = new();

        using (StreamReader sr = new StreamReader(fs))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line != null)
                {
                    var splittedLine = line.Split(separator);
                    if (splittedLine.Length > 0)
                    {
                        sus.Add(uint.Parse(splittedLine[0]));
                        inf.Add(uint.Parse(splittedLine[1]));
                        rec.Add(uint.Parse(splittedLine[2]));
                    }
                }
            }
        }
        return (sus.ToArray(), inf.ToArray(), rec.ToArray());
    }

    /// <summary>
    /// Interpolate the <paramref name="value"/> between two integers.
    /// </summary>
    /// <param name="value">The value to interpolate.</param>
    /// <param name="min1">The actual minimum possible value.</param>
    /// <param name="max1">The actual maximum possible value.</param>
    /// <param name="min2">The intended minimum value.</param>
    /// <param name="max2">The intended maximum value.</param>
    /// <returns>The representation of <paramref name="value"/> between <paramref name="min2"/> and <paramref name="max2"/></returns>
    public static float Interpolate(int value, int min1, int max1, int min2 = 0, int max2 = 1)
    {
        if (value > max1)
            throw new ArgumentOutOfRangeException("Value must be max1 parameter.");

        float slope = max2 - min2;
        slope /= (float)max1 - (float)min1;
        return min2 + slope * ((float)value - (float)min1);
    }

    /// <summary>
    /// Converts a binary array to its respective decimal value
    /// </summary>
    /// <param name="array">Array-like object that implements IEnumerable. It must be a binary array.</param
    /// <returns>Integer representation of <paramref name="array"/></returns>
    public static int BinaryArrayToDecimal(IEnumerable<byte> array)
    {
        return Convert.ToInt32(String.Join("", array), 2);
    }

    // TODO: Documentate <param name="random">
    /// <summary>
    /// Get random samples of a collection.
    /// </summary>
    /// <param name="source">The source to get the sample. Must implement IEnumerable.</param>
    /// <param name="sampleSize">The size of the sample. Must be less than source length.</param>
    /// <returns>The sample of <paramref name="source"/> with size <paramref name="sampleSize"/></returns>
    /// <exception cref="InvalidOperationException">When the only constraint is not fulfilled.</exception>
    public static IEnumerable<int> GetRandomElements(IEnumerable<int> source, int sampleSize, Random random)
    {
        var length = source.Count();
        var enumerator = source.GetEnumerator();

        if (length < sampleSize)
        {
            throw new InvalidOperationException("The source length is less than count.");
        }

        while (sampleSize > 0)
        {
            const int bias = 1;
            var next = random.Next((length / bias) - sampleSize - bias) + 1;
            length -= next;

            while (next > 0)
            {
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException("What, we starved out?");
                }

                --next;
            }
            yield return enumerator.Current;
            --sampleSize;
        }
    }

    // TODO: Move this method to a math utils function
    public static float MeanSquaredError(IEnumerable<uint> yTrue, IEnumerable<uint> yPred)
    {
        var diff = new List<uint>();
        for (var i = 0; i < yTrue.Count(); i++)
        {
            var aux = yTrue.ElementAt(i) - yPred.ElementAt(i);
            diff.Add(aux * aux);
        }

        float sum = 0;
        diff.ForEach(x => sum += x);
        return sum / diff.Count;
    }

    public static void PrintInitialConfig(string fileName, int popSize, int generations, int bitSize, int days, float reprodutionRate, float crossoverRate, float mutationRate, uint totalNodes)
    {
        Console.WriteLine("============================= GENETIC ALGORITHM ============================");
        Console.WriteLine("=================================== S.I.R ==================================");
        Console.WriteLine($"\nOpened File? {fileName}");

        Console.WriteLine($"Simulation parameters------------------------------------------------------");
        Console.WriteLine($"Population size: {popSize}");
        Console.WriteLine($"Number of generations: {generations}");
        Console.WriteLine($"Chromossome bits: {bitSize}");
        Console.WriteLine($"Simulated days: {days}");
        Console.WriteLine($"Reprodution rate: {reprodutionRate}");
        Console.WriteLine($"Crossover rate: {crossoverRate}");
        Console.WriteLine($"Mutation rate: {mutationRate}");
        Console.WriteLine($"Graph total nodes: {totalNodes}");
        Console.WriteLine($"---------------------------------------------------------------------------");

        Console.WriteLine("============================================================================");
    }
}