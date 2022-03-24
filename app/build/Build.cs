using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Octopus;
using Nuke.Common.Tools.Kubernetes;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
/*
    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath TestsDirectory => RootDirectory / "tests";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
*/ //            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
/*        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });
*/
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Parameter("Nuspec Filename")] readonly string NuspecFile = "deploy.nuspec"; //default
    [Parameter("App build version")] readonly string BuildVersion; //Must specify
    [Parameter("Project Description")] readonly string ProjectDescription = "LinuxHydraPartitionController Nuget Package built on " + DateTime.UtcNow.ToString("MM-dd-yyyy"); //default
    [Parameter("Project Author")] readonly string ProjectAuthor = "Pharmacy OneSource"; //default
    [Parameter("Project Copyright")] readonly string ProjectCopyright = "Copyright 2022 Pharmacy OneSource"; //default
    [Parameter("Project VCS Url")] readonly string ProjectUrl = "https://dev-gitlab.dev.rph.int/rollout/linux-hydra-partition-controller"; //default
    [Parameter("NuGet repository server Url")] readonly string NugetApiUrl = "https://nuget01.dev.rph.int/nuget/Teamcity/"; //default
    [Parameter("Nuget repository server ApiKey")] readonly string NugetApiKey;
    [Parameter("Publishes .NET runtime with app")] readonly Boolean SelfContained = true;

    [Solution] readonly Solution Solution;
    AbsolutePath SourceDirectory => RootDirectory / "";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
                //SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                //TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetPublish(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(BuildVersion)
                .SetFileVersion(BuildVersion)
                .SetVersion(BuildVersion)
                .SetInformationalVersion(BuildVersion)
                .SetCopyright(ProjectCopyright)
                .SetSelfContained(SelfContained)
                .SetRuntime(SelfContained ? "win-x64" : null)
                .SetOutput(ArtifactsDirectory / "output"));
        });

    Target Pack => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            NuGetTasks.NuGetPack(s => s
                .SetConfiguration(Configuration)
                .SetTargetPath(RootDirectory / NuspecFile)
                .SetIncludeReferencedProjects(true)
                .SetVersion(BuildVersion)
                .SetProperty("description", ProjectDescription)
                .SetProperty("copyright", ProjectCopyright)
                .SetProperty("authors", ProjectAuthor)
                .SetProperty("projectUrl", ProjectUrl)
                .SetOutputDirectory(ArtifactsDirectory / "nuget"));
        });

    [Obsolete]
    Target Push => _ => _
        .DependsOn(Pack)
        .Requires(() => NugetApiUrl)
        .Requires(() => NugetApiKey)
        //.Requires(() => Configuration.Equals(Configuration.Release))
        .Executes(() =>
        {
            GlobFiles(ArtifactsDirectory / "nuget" / "*.nupkg")
                .NotEmpty()
                .Where(x => !x.EndsWith("symbols.nupkg"))
                .ForEach(x =>
                {
                    NuGetTasks.NuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSource(NugetApiUrl)
                        .SetApiKey(NugetApiKey)
                    );
                });
        });
}