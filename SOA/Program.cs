using System.Diagnostics;
using System.Numerics;

void TimeTest<T>(string name, T data, Action<T> action, int times)
{
    var stopwatch = Stopwatch.StartNew();
    for (int i = 0; i < times; i++)
    {
        action(data);
    }
    stopwatch.Stop();
    Console.WriteLine($"{name} Time taken: {stopwatch.ElapsedMilliseconds / (double)times} ms");
}

void AOS(Vec3[] data)
{  
    for (int i = 0; i < data.Length; i++)
    {
        data[i].X +=1;
    }
}


void SOA(Vec3Array data)
{
    var xs = data.Xs;
    for (int i = 0; i < data.Xs.Length; i++)
    {
        xs[i] += 1;
    }
}

void SOASimd(Vec3Array data)
{
    var xs = data.Xs;
    var length = xs.Length;
    int i = 0;
    int simdLength = Vector<float>.Count;
    // Process in chunks of simdLength
    for (; i <= length - simdLength; i += simdLength)
    {
        var vector = new Vector<float>(xs, i);
        vector += Vector<float>.One;
        vector.CopyTo(xs, i);
    }
    // Process remaining elements
    for (; i < length; i++)
    {
        xs[i] += 1;
    }
}

TimeTest("AOS Warmup", new Vec3[1000], AOS, 1000);
TimeTest("SOA Warmup", new Vec3Array(1000), SOA, 1000);
TimeTest("SOA SIMD Warmup", new Vec3Array(1000), SOASimd, 1000);
TimeTest("AOS Run", new Vec3[10000000], AOS, 1000);
TimeTest("SOA Run", new Vec3Array(10000000), SOA, 1000);
TimeTest("SOA SIMD Run", new Vec3Array(10000000), SOASimd, 1000);

record struct Vec3(float X, float Y, float Z);
record struct Vec3Array(int Length) {
    public float[] Xs { get; init; } = new float[Length];
    public float[] Ys { get; init; } = new float[Length];
    public float[] Zs { get; init; } = new float[Length];
};