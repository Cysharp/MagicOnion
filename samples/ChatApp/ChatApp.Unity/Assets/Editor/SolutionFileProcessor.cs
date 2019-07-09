using System;
using UnityEditor;

public class SolutionFileProcessor : AssetPostprocessor
{
    const string ProjectTypeGuidCsharp = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";
    const string ProjectTypeGuidConsoleApp = "9A19103F-16F7-4668-BE54-9A1E7A4F7556";

    private static string OnGeneratedSlnSolution(string path, string content)
    {
        // Automatically include ProjectFiles into SolutionFile

        var newContent = AddProject(
            content: content,
            ProjectTypeGuidConsoleApp,
            projectGuid: "053476FC-B8B2-4A14-AED2-3733DFD5DFC3",
            projectName: "ChatApp.Server",
            projectPath: "..\\ChatApp.Server\\ChatApp.Server.csproj");


        return newContent;
    }

    private static string AddProject(string content, string projectTypeGuid, string projectGuid, string projectName, string projectPath)
    {
        var add = $"Project(\"{projectTypeGuid}\") = \"{projectName}\", \"{projectPath}\", \"{projectGuid}\",\"{Environment.NewLine}EndProject";

        var newContent = content.Replace($"EndProject{Environment.NewLine}Global",
            $"EndProject{Environment.NewLine}{add}{Environment.NewLine}Global");

        return newContent;
    }
}
