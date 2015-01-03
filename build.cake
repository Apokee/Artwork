#l "build-extra.cake"

var target = Argument<string>("target", "Package");

var buildConfig = GetBuildConfiguration<BuildConfig>();
var version = GetVersion();

var outputDir = System.IO.Path.Combine("Output");
var outputBuildDir = System.IO.Path.Combine(outputDir, "Build");
var outputPackageDir = System.IO.Path.Combine(outputDir, "Package");

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

Task("Build")
    .IsDependentOn("CleanBuild")
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
        Version = version.ToString(),
        OutputDirectory = outputPackageDir,
    });
});

Task("ZipPackage")
    .IsDependentOn("CleanPackage")
    .IsDependentOn("Build")
    .Does(() =>
{
    var packageFile = System.IO.Path.Combine(outputPackageDir, String.Format("Apokee.Artwork-{0}.zip", version));

    Zip(outputBuildDir, packageFile);
});

Task("Package")
    .IsDependentOn("ZipPackage")
    .IsDependentOn("NugetPackage")
    .Does(() =>
{
});

RunTarget(target);
