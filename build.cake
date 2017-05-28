#l "cake/build-extra.cake"

var target = Argument<string>("target", "Package");
var release = Argument<bool>("release", false);

var buildConfig = GetBuildConfiguration<BuildConfig>();

var outputDir = System.IO.Path.Combine(".build/out");
var outputBuildDir = System.IO.Path.Combine(outputDir, "build");
var outputPackageDir = System.IO.Path.Combine(outputDir, "package");

Task("CleanBuild")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { outputBuildDir });
});

Task("CleanPackage")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { outputPackageDir });
});

Task("BuildVersionInfo")
    .Does(() =>
{
    SemVer buildVersion;

    var changeLog = GetChangeLog();
    var version = changeLog.LatestVersion;
    var rev = GetGitRevision(useShort: true);

    if (rev != null && !release)
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

    System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, "VERSION"), buildVersion);
    System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, "PRELEASE"), (buildVersion.Pre != null).ToString().ToLower());
    System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, "CHANGELOG"), changeLog.LatestChanges);
});

Task("Build")
    .IsDependentOn("CleanBuild")
    .IsDependentOn("BuildVersionInfo")
    .Does(() =>
{
    Convert(buildConfig);
    CopyFile("LICENSE.md", System.IO.Path.Combine(outputBuildDir, "LICENSE.md"));
});

Task("NugetPackage")
    .IsDependentOn("CleanPackage")
    .IsDependentOn("Build")
    .Does(() =>
{
    NuGetPack("Apokee.Artwork.nuspec", new NuGetPackSettings {
        Version = GetVersion().ToString(),
        OutputDirectory = outputPackageDir,
    });
});

Task("ZipPackage")
    .IsDependentOn("CleanPackage")
    .IsDependentOn("Build")
    .Does(() =>
{
    var packageFile = System.IO.Path.Combine(outputPackageDir, String.Format("Apokee.Artwork-{0}.zip", GetBuildVersion()));

    Zip(outputBuildDir, packageFile);
});

Task("Package")
    .IsDependentOn("ZipPackage")
    .IsDependentOn("NugetPackage")
    .Does(() =>
{
});

RunTarget(target);

private SemVer GetBuildVersion()
{
    return new SemVer(System.IO.File.ReadAllText(System.IO.Path.Combine(outputDir, "VERSION")));
}
