#l "cake/build-extra.cake"

public sealed class Globals
{
    public string Target { get; set; }
    public bool Release { get; set; }
    public BuildConfig BuildConfig { get; set; }
    public string OutputDir { get; set; }
    public string OutputBuildDir { get; set; }
    public string OutputPackageDir { get; set; }
}

private Globals GetGlobals()
{
    var globals = new Globals();
    globals.Target = Argument<string>("target", "Package");
    globals.Release = Argument<bool>("release", false);
    globals.BuildConfig = GetBuildConfiguration<BuildConfig>();
    globals.OutputDir = System.IO.Path.Combine(".build/out");
    globals.OutputBuildDir = System.IO.Path.Combine(globals.OutputDir, "build");
    globals.OutputPackageDir = System.IO.Path.Combine(globals.OutputDir, "package");

    return globals;
}

Task("CleanBuild")
    .Does(() =>
{
    var globals = GetGlobals();
    CleanDirectories(new DirectoryPath[] { globals.OutputBuildDir });
});

Task("CleanPackage")
    .Does(() =>
{
    var globals = GetGlobals();
    CleanDirectories(new DirectoryPath[] { globals.OutputPackageDir });
});

Task("BuildVersionInfo")
    .Does(() =>
{
    var globals = GetGlobals();

    SemVer buildVersion;

    var changeLog = GetChangeLog();
    var version = changeLog.LatestVersion;
    var rev = GetGitRevision(useShort: true);

    if (rev != null && !globals.Release)
    {
        if (version.Build == null)
        {
            buildVersion = new SemVer(version.Major, version.Minor, version.Patch, version.Pre, rev);
        }
        else
        {
            throw new Exception("VERSION already contains build metadata");
        }
    }
    else
    {
        buildVersion = version;
    }

    System.IO.File.WriteAllText(System.IO.Path.Combine(globals.OutputDir, "VERSION"), buildVersion);
    System.IO.File.WriteAllText(System.IO.Path.Combine(globals.OutputDir, "PRELEASE"), (buildVersion.Pre != null).ToString().ToLower());
    System.IO.File.WriteAllText(System.IO.Path.Combine(globals.OutputDir, "CHANGELOG"), changeLog.LatestChanges);
});

Task("Build")
    .IsDependentOn("CleanBuild")
    .IsDependentOn("BuildVersionInfo")
    .Does(() =>
{
    var globals = GetGlobals();

    Convert(globals.BuildConfig);
    CopyFile("LICENSE.md", System.IO.Path.Combine(globals.OutputBuildDir, "LICENSE.md"));
});

Task("NugetPackage")
    .IsDependentOn("CleanPackage")
    .IsDependentOn("Build")
    .Does(() =>
{
    var globals = GetGlobals();

    NuGetPack("Apokee.Artwork.nuspec", new NuGetPackSettings {
        Version = GetVersion().ToString(),
        OutputDirectory = globals.OutputPackageDir,
    });
});

Task("ZipPackage")
    .IsDependentOn("CleanPackage")
    .IsDependentOn("Build")
    .Does(() =>
{
    var globals = GetGlobals();

    var packageFile = System.IO.Path.Combine(globals.OutputPackageDir, String.Format("Apokee.Artwork-{0}.zip", GetBuildVersion()));

    Zip(globals.OutputBuildDir, packageFile);
});

Task("Package")
    .IsDependentOn("ZipPackage")
    .IsDependentOn("NugetPackage")
    .Does(() =>
{
});

RunTarget(GetGlobals().Target);

private SemVer GetBuildVersion()
{
    var globals = GetGlobals();
    return new SemVer(System.IO.File.ReadAllText(System.IO.Path.Combine(globals.OutputDir, "VERSION")));
}
