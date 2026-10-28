using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity 6 layouts can still reference removed windows (Asset Store, Version Control).
/// Those become FallbackEditorWindow titled "Failed to load" and spam errors on maximize/play.
/// </summary>
[InitializeOnLoad]
public static class CloseBrokenEditorWindows
{
    static CloseBrokenEditorWindows()
    {
        EditorApplication.delayCall += CloseFallbacks;
        EditorApplication.playModeStateChanged += _ => EditorApplication.delayCall += CloseFallbacks;
    }

    [MenuItem("Tools/Close Broken Editor Windows")]
    public static void CloseFallbacks()
    {
        var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
        var closed = 0;

        foreach (var window in windows)
        {
            if (!window)
            {
                continue;
            }

            var typeName = window.GetType().Name;
            var title = window.titleContent != null ? window.titleContent.text : string.Empty;

            if (typeName == "FallbackEditorWindow" || title == "Failed to load")
            {
                window.Close();
                closed++;
            }
        }

        if (closed > 0)
        {
            Debug.Log($"Closed {closed} broken editor window(s).");
        }
    }
}
