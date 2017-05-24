// tools
#addin nuget:?package=Cake.Paket
#tool nuget:?package=Paket

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var buildDir = Directory("./bin") + Directory(configuration);
var packageDir = Directory("./packages");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./**/obj");
    CleanDirectories("./**/bin");
    CleanDirectory(buildDir);
    CleanDirectory(packageDir);
});

Task("Paket-Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    PaketRestore();
});

Task("Build")
    .IsDependentOn("Paket-Restore")
    .Does(() =>
{
    MSBuild("GoogleCloudChaosMonkey.sln", settings =>
        settings.SetConfiguration(configuration));
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS AND EXECUTION
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");
RunTarget(target);
