using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GoldMeridian.PaintLabel.IO;

namespace GoldMeridian.PaintLabel.CLI;

internal static class Program
{
    public static void Main(string[] args)
    {
        var binaries = Directory.GetFiles(args[0], "*.fxc", SearchOption.AllDirectories);

        var files = new Dictionary<string, byte[]>();
        foreach (var path in binaries)
        {
            files[path] = File.ReadAllBytes(path);
        }
        
        var sw = Stopwatch.StartNew();

        const int times = 2500;
        for (var i = 0; i < times; i++)
        {
            foreach (var path in binaries)
            {
                var bytes = files[path];
                using var ms = new MemoryStream(bytes);
                using var r = new BinaryReader(ms);
                _ = EffectReader.ReadEffect(r);
            }   
        }

        sw.Stop();

        Console.WriteLine($"Parsed {binaries.Length} * {times} ({binaries.Length * times}) files in {sw.Elapsed:g}");
    }
}
