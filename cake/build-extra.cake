#l "utilities.cake"

using YamlDotNet.Serialization;

public sealed class BuildConfig
{
    [YamlMember(Alias = "input_dir")]
    public string InputDir { get; set; }

    [YamlMember(Alias = "output_dir")]
    public string OutputDir { get; set; }

    [YamlMember(Alias = "conversions")]
    public ConvertConfig[] Conversions { get; set; }
}

public sealed class ConvertConfig
{
    [YamlMember(Alias = "input")]
    public string Input { get; set; }

    [YamlMember(Alias = "output")]
    public string Output { get; set; }

    [YamlMember(Alias = "background")]
    public string Background { get; set; }

    [YamlMember(Alias = "level_colors")]
    public string LevelColors { get; set; }

    [YamlMember(Alias = "resize")]
    public string Resize { get; set; }
}

public void Convert(BuildConfig buildConfig)
{
    foreach (var conversion in buildConfig.Conversions)
    {
        Console.WriteLine("Converting {0} -> {1}", conversion.Input, conversion.Output);

        if (!DirectoryExists(buildConfig.OutputDir))
            CreateDirectory(buildConfig.OutputDir);

        Convert(
            System.IO.Path.Combine(buildConfig.InputDir, conversion.Input),
            System.IO.Path.Combine(buildConfig.OutputDir, conversion.Output),
            background: conversion.Background,
            levelColors: conversion.LevelColors,
            resize: conversion.Resize
        );
    }
}

public void Convert(string inputFile, string outputFile,
    string background = null, string levelColors = null, string resize = null
)
{
    var arguments = new StringBuilder();

    if (background != null)
        arguments.AppendFormat(" -background {0}", background);

    arguments.AppendFormat(" {0}", inputFile);

    if (resize != null)
        arguments.AppendFormat(" -resize {0}", resize);

    if (levelColors != null)
        arguments.AppendFormat(" -channel RGB +level-colors {0}", levelColors);

    arguments.AppendFormat(" {0}", outputFile);

    var exitCode = StartProcess(Which("magick"), new ProcessSettings { Arguments = arguments.ToString() });

    if (exitCode != 0)
    {
        throw new Exception(String.Format("Error converting {0}", inputFile));
    }
}
