using System;
using System.Diagnostics;
using System.IO;
using GoldMeridian.PaintLabel.IO;

namespace GoldMeridian.PaintLabel.CLI;

internal static class Program
{
    public static void Main(string[] args)
    {
        var binaries = Directory.GetFiles(args[0], "*.fxc", SearchOption.AllDirectories);

        var sw = Stopwatch.StartNew();
        foreach (var path in binaries)
        {
            using var fs = File.OpenRead(path);
            using var r = new BinaryReader(fs);
            _ = EffectReader.ReadEffect(r);
        }

        sw.Stop();

        Console.WriteLine($"Parsed {binaries.Length} files in {sw.Elapsed:g}");
    }
}
