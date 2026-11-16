using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        data[i].X += 1;
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

void AOSSpan(Vec3[] data)
{
    // ref iteration: no bounds checks, no repeated indexing
    foreach (ref var v in data.AsSpan())
    {
        v.X += 1;
    }
}

void SOASpan(Vec3Array data)
{
    // Span length is the loop bound, so the JIT can drop bounds checks
    var xs = data.Xs.AsSpan();
    for (int i = 0; i < xs.Length; i++)
    {
        xs[i] += 1;
    }
}

void SOASimdSpan(Vec3Array data)
{
    var xs = data.Xs;
    ref float start = ref MemoryMarshal.GetArrayDataReference(xs);
    nuint length = (nuint)xs.Length;
    nuint simdLength = (nuint)Vector<float>.Count;
    nuint i = 0;
    // Unchecked vector loads/stores in chunks of simdLength
    for (; i + simdLength <= length; i += simdLength)
    {
        (Vector.LoadUnsafe(ref start, i) + Vector<float>.One).StoreUnsafe(ref start, i);
    }
    // Remaining elements
    for (; i < length; i++)
    {
        Unsafe.Add(ref start, i) += 1;
    }
}

TimeTest("AOS Warmup", new Vec3[1000], AOS, 1000);
TimeTest("SOA Warmup", new Vec3Array(1000), SOA, 1000);
TimeTest("SOA SIMD Warmup", new Vec3Array(1000), SOASimd, 1000);
TimeTest("AOS Span Warmup", new Vec3[1000], AOSSpan, 1000);
TimeTest("SOA Span Warmup", new Vec3Array(1000), SOASpan, 1000);
TimeTest("SOA SIMD Span Warmup", new Vec3Array(1000), SOASimdSpan, 1000);
TimeTest("AOS Run", new Vec3[10000000], AOS, 1000);
TimeTest("AOS Span Run", new Vec3[10000000], AOSSpan, 1000);
TimeTest("SOA Run", new Vec3Array(10000000), SOA, 1000);
TimeTest("SOA Span Run", new Vec3Array(10000000), SOASpan, 1000);
TimeTest("SOA SIMD Run", new Vec3Array(10000000), SOASimd, 1000);
TimeTest("SOA SIMD Span Run", new Vec3Array(10000000), SOASimdSpan, 1000);

record struct Vec3(float X, float Y, float Z);

// Class instead of record struct: avoids copying the struct and generated equality/ToString members
sealed class Vec3Array(int length)
{
    public readonly float[] Xs = new float[length];
    public readonly float[] Ys = new float[length];
    public readonly float[] Zs = new float[length];
}