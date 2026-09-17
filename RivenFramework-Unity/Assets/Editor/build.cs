using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    public static void BuildLinux()
    {
        Build(BuildTarget.StandaloneLinux64, BuildTargetGroup.Standalone);
    }

    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone);
    }

    private static void Build(BuildTarget target, BuildTargetGroup targetGroup)
    {
        string[] args = Environment.GetCommandLineArgs();
        string outputPath = GetArgValue(args, "-outputPath");
        bool devBuild = args.Contains("-devBuild");

        if (string.IsNullOrEmpty(outputPath))
        {
            Debug.LogError("[BuildScript] -outputPath argument is missing");
            EditorApplication.Exit(1);
            return;
        }

        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[BuildScript] No scenes enabled in Build Settings");
            EditorApplication.Exit(1);
            return;
        }

        BuildOptions options = BuildOptions.None;

        if (devBuild)
        {
            options |= BuildOptions.Development;
            options |= BuildOptions.ConnectWithProfiler;
            options |= BuildOptions.EnableDeepProfilingSupport;
            Debug.Log("[BuildScript] Development build enabled");
        }

        Debug.Log($"[BuildScript] Building {target} → {outputPath}");

        BuildReport report = BuildPipeline.BuildPlayer(
            new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                targetGroup = targetGroup,
                options = options,
            }
        );

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] Build succeeded: {outputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[BuildScript] Build FAILED: {report.summary.result}");
            EditorApplication.Exit(1);
        }
    }

    private static string GetArgValue(string[] args, string flag)
    {
        int idx = Array.IndexOf(args, flag);
        if (idx >= 0 && idx + 1 < args.Length)
            return args[idx + 1];
        return null;
    }
}
