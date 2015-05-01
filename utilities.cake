#r "Library/NuGet/YamlDotNet.3.5.1/lib/net35/YamlDotNet.dll"

using YamlDotNet.Serialization;

public T GetBuildConfiguration<T>() where T : new()
{
    var workingDirectorySegments = GetContext().Environment.WorkingDirectory.Segments;
    var workingDirectoryName = workingDirectorySegments[workingDirectorySegments.Length - 1];

    var configFile = (new [] { "build.yml", String.Format("../{0}.build.yml", workingDirectoryName) })
        .FirstOrDefault(File.Exists);

    if (configFile == null)
    {
        return new T();
    }

    return new Deserializer(ignoreUnmatched: true).Deserialize<T>(new StreamReader(configFile));
}

public string GetSolution()
{
    var solutions = Directory.GetFiles(GetContext().Environment.WorkingDirectory.FullPath, "*.sln");

    if (solutions.Length == 1)
    {
        return solutions[0];
    }
    else
    {
        if (solutions.Length == 0)
        {
            throw new Exception("No solution found.");
        }
        else
        {
            throw new Exception("Multiple solutions found.");
        }
    }
}

public string Which(string executable)
{
    var seperator = new char[] { System.Environment.OSVersion.Platform == PlatformID.Unix ? ':' : ';' };

    var envPath = Environment.GetEnvironmentVariable("PATH");
    var envPathExt = Environment.GetEnvironmentVariable("PATHEXT");

    var paths = envPath == null ?
        new string[0] :
        envPath.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

    var pathExts = envPathExt == null ?
        new string[0] :
        envPathExt.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

    foreach (var path in paths)
    {
        var testPath = System.IO.Path.Combine(path, executable);

        if (File.Exists(testPath))
        {
            return testPath;
        }

        foreach (var pathExt in pathExts)
        {
            var testPathExt = System.IO.Path.Combine(path, executable) + pathExt;

            if (File.Exists(testPathExt))
            {
                return testPathExt;
            }
        }
    }

    return null;
}

public Version GetVersion()
{
    return new Version(File.ReadAllText("VERSION").Trim());
}
