using System;
using System.IO;
using System.Reflection;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

public static class RegenerateIdeProjectFiles
{
    [MenuItem("Tools/Regenerate IDE Project Files")]
    public static void Sync()
    {
        DeleteGeneratedProjectFiles();

        // Cursor.app is not matched by Unity's VS Code discovery (.*Code.*.app$),
        // so CodeEditor.CurrentEditor.SyncAll() is often a no-op. Force SDK-style
        // generation through the Visual Studio IDE package instead.
        if (!TryForceVisualStudioSdkGeneration())
        {
            CodeEditor.CurrentEditor.SyncAll();
        }

        AssetDatabase.Refresh();
        Debug.Log("IDE project files regenerated for Unity " + Application.unityVersion);
    }

    private static bool TryForceVisualStudioSdkGeneration()
    {
        try
        {
            var vsAssembly = typeof(VisualStudioEditor).Assembly;
            var styleType = vsAssembly.GetType("Microsoft.Unity.VisualStudio.Editor.GeneratorStyle");
            var factoryType = vsAssembly.GetType("Microsoft.Unity.VisualStudio.Editor.GeneratorFactory");
            if (styleType == null || factoryType == null)
            {
                return false;
            }

            var sdkStyle = Enum.Parse(styleType, "SDK");
            var getInstance = factoryType.GetMethod(
                "GetInstance",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { styleType },
                null);

            var generator = getInstance?.Invoke(null, new object[] { sdkStyle });
            var sync = generator?.GetType().GetMethod("Sync", BindingFlags.Public | BindingFlags.Instance);
            if (sync == null)
            {
                return false;
            }

            sync.Invoke(generator, null);
            Debug.Log("Forced SDK-style C# project generation (Cursor-compatible).");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Forced project generation failed: " + ex.Message);
            return false;
        }
    }

    private static void DeleteGeneratedProjectFiles()
    {
        var root = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(root))
        {
            return;
        }

        foreach (var path in Directory.GetFiles(root, "*.csproj"))
        {
            File.Delete(path);
        }

        foreach (var path in Directory.GetFiles(root, "*.sln"))
        {
            File.Delete(path);
        }

        foreach (var path in Directory.GetFiles(root, "*.slnx"))
        {
            File.Delete(path);
        }
    }
}
