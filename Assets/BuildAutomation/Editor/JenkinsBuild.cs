using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor.Build.Reporting;

public class JenkinsBuild
{
    public static void PerformBuild()
    {
        string[] args = Environment.GetCommandLineArgs();

        // Get platform argument
        string target = args.FirstOrDefault(arg => arg.StartsWith("platform="))?.Split('=')[1];
        if (string.IsNullOrEmpty(target))
        {
            Debug.LogError("No platform specified. Use -executeMethod JenkinsBuild.PerformBuild platform=Windows");
            return;
        }

        BuildTarget buildTarget = GetBuildTarget(target);
        string[] scenes = GetScenesForPlatform(target);
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogError("No scenes specified for this platform.");
            return;
        }

        string buildPath = GetBuildPath(target);

        Debug.Log($"🛠️ Starting build for {target} to {buildPath}");

        BuildReport report = BuildPipeline.BuildPlayer(
            scenes,
            buildPath,
            buildTarget,
            BuildOptions.None
        );

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"✅ Build succeeded: {report.summary.totalSize} bytes at {buildPath}");
        }
        else
        {
            Debug.LogError("❌ Build failed!");
        }
    }

    private static BuildTarget GetBuildTarget(string target)
    {
        switch (target.ToLower())
        {
            case "windows": return BuildTarget.StandaloneWindows64;
            case "android": return BuildTarget.Android;
            case "ios": return BuildTarget.iOS;
            default:
                throw new ArgumentException($"Unknown build target: {target}");
        }
    }

    private static string[] GetScenesForPlatform(string target)
    {
        // Optionally: load specific scenes per platform here
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    private static string GetBuildPath(string target)
    {
        string productName = Application.productName;

        switch (target.ToLower())
        {
            case "windows":
                return $"Builds/Windows/{productName}.exe";
            case "android":
                return $"Builds/Android/{productName}.apk";
            case "ios":
                return "Builds/iOS"; // Xcode project folder
            default:
                throw new ArgumentException($"Unknown target: {target}");
        }
    }
}
