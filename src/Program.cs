using System;
using System.Linq;
using System.IO;
using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace AlphaSecret
{
    class Program
    {

        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
            [Option("overwrite", Required = false, HelpText = "Overwrite exisiting files when restoring files.")]
            public bool Overwrite { get; set; }
            [Option('n', "no-restore", Required = false, HelpText = "Disabled restoring of suspicious files.")]
            public bool NoRestore { get; set; }
            [Option("stdin", Required = false, HelpText = "Read paths to process form stdin.")]
            public bool StdIn { get; set; }

            [Value(0)]
            public IEnumerable<string>? InputDirectoriesAndFiles { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                if (o.StdIn)
                {
                    if (o.Verbose)
                        Console.WriteLine("Reading paths from stdin");

                    for (var line = Console.ReadLine(); line != null; line = Console.ReadLine())
                        ProcessPath(o, line);
                }
                else if (o.InputDirectoriesAndFiles != null)
                {
                    foreach (var inputDirOrFile in o.InputDirectoriesAndFiles)
                        ProcessPath(o, inputDirOrFile);
                }
                else
                {
                    Console.WriteLine("No files to process passed.");
                }
            });
        }


        static void ProcessPath(Options options, string path)
        {
            try
            {
                var attrs = File.GetAttributes(path);
                if (attrs.HasFlag(FileAttributes.Directory))
                    ProcessDirectory(options, path);
                else
                    ProcessFile(options, path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing file or directory \"{path}\": {ex.Message}");
            }
        }

        static void ProcessDirectory(Options options, string directoryPath)
        {
            var candidatePaths = Directory.EnumerateFiles(directoryPath, "*.png", new EnumerationOptions()
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                MatchType = MatchType.Simple,
                RecurseSubdirectories = true,
            });

            if (options.Verbose)
                Console.WriteLine($"Enumerating files in directory: {directoryPath}");

            candidatePaths.AsParallel().ForAll(file => ProcessFile(options, file));
        }

        static void ProcessFile(Options options, string path)
        {
            try
            {
                using var img = Image.Load<Rgba32>(path);

                var isImageSuspicious = IsImageSuspicious(img);

                if (isImageSuspicious)
                {
                    Console.WriteLine(
                        options.Verbose
                            ? $"Image is suspicious: {path}"
                            : path
                    );

                    if (!options.NoRestore)
                    {
                        RestoreImageData(img);

                        var restoredPath = Path.ChangeExtension(path, Path.GetExtension(path) + "-restored");

                        // TODO: This is a race condition. ¯\_(ツ)_/¯
                        if (!options.Overwrite && File.Exists(restoredPath))
                            Console.Error.WriteLine($"Could not restore image to {restoredPath}, file already exists");

                        using var s = File.Create(restoredPath);
                        img.SaveAsPng(s);
                    }

                }
                else if (options.Verbose)
                    Console.WriteLine($"File seems ok: {path}");
            }
            catch (Exception ex) when (ex is ImageFormatException || ex is InvalidDataException)
            {
                Console.Error.WriteLine($"Could not check image, it seems to be a broken file \"{path}\": {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing file or directory \"{path}\": {ex.Message}");
            }
        }

        static int RestoreImageData(Image<Rgba32> img)
        {
            int restoredPixels = 0;
            for (int x = 0; x < img.Width; ++x)
            {
                for (int y = 0; y < img.Height; ++y)
                {
                    var pixel = img[x, y];
                    if (pixel.A == 0 && HasRgbData(pixel))
                    {
                        pixel.A = 255;
                        img[x, y] = pixel;
                        ++restoredPixels;
                    }
                }
            }
            return restoredPixels;
        }

        static bool IsImageSuspicious(Image<Rgba32> img)
        {
            for (int x = 0; x < img.Width; ++x)
            {
                for (int y = 0; y < img.Height; ++y)
                {
                    var pixel = img[x, y];
                    if (pixel.A == 0 && HasRgbData(pixel))
                        return true;
                }
            }
            return false;
        }

        static bool HasRgbData(Rgba32 pixel) => (pixel.R | pixel.G | pixel.G) != 0;
    }
}
