using System;
using System.IO;
using GoldMeridian.PaintLabel.IO;

namespace GoldMeridian.PaintLabel.CLI;

internal static class Program
{
    public static void Main(string[] args)
    {
        var binaries = Directory.GetFiles(args[0], "*.fxc");
        foreach (var path in binaries)
        {
            using var fs = File.OpenRead(path);
            using var r = new BinaryReader(fs);
            var effect = new EffectReader(r).ReadEffect();
            Console.WriteLine(effect.HasErrors);
        }
    }
}
