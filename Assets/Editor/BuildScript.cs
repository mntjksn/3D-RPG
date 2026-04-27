using UnityEditor;

public class BuildScript
{
    public static void PerformBuild()
    {
        BuildPipeline.BuildPlayer(
            new[] { "Assets/00 Scenes/Main.unity" },
            "Build/Build.exe",
            BuildTarget.StandaloneWindows64,
            BuildOptions.None
        );
    }
}