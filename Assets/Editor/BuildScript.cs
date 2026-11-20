using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildScript
{
    public static void BuildStandaloneWindows()
    {
        string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
        string outputPath = "Builds/Windows/Juego.exe";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = outputPath;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build finalizado correctamente con un tamano de " + summary.totalSize + " bytes.");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Error durante la compilacion del ejecutable.");
        }
    }
}
