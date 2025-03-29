using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;

public class BuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    // List of source folders to copy JSON files from
    private readonly List<string> sourceFolders = new List<string>
    {
        "Assets/Data/Enemies/Levels",
        "Assets/Resources/EnemySO",
        "Assets/Resources/BossSO"
    };

    private readonly string targetRootPath = Path.Combine(Application.streamingAssetsPath, "JsonData");

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("==== Copying JSON files to StreamingAssets ====");

        // Ensure the root StreamingAssets folder exists
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        foreach (string sourcePath in sourceFolders)
        {
            string folderName = Path.GetFileName(sourcePath); // Extract folder name
            string targetPath = Path.Combine(targetRootPath, folderName);

            if (!Directory.Exists(sourcePath))
            {
                Debug.LogWarning($"[SKIPPED] Source folder not found: {sourcePath}");
                continue;
            }

            // Ensure the target directory exists
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            string[] jsonFiles = Directory.GetFiles(sourcePath, "*.json");

            if (jsonFiles.Length == 0)
            {
                Debug.LogWarning($"[EMPTY] No JSON files in: {sourcePath}");
                continue;
            }

            foreach (string file in jsonFiles)
            {
                string fileName = Path.GetFileName(file);
                string targetFilePath = Path.Combine(targetPath, fileName);

                File.Copy(file, targetFilePath, true); // Overwrite existing files
                Debug.Log($"[COPIED] {file} → {targetFilePath}");
            }
        }

        Debug.Log("==== JSON Copying Completed Successfully ====");
    }
}
