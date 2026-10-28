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
    }

    [MenuItem("Tools/Close Broken Editor Windows")]
    public static void CloseFallbacks()
    {
        EditorWindow[] windows;
        try
        {
            windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
        }
        catch
        {
            return;
        }

        var closed = 0;

        foreach (var window in windows)
        {
            if (!window)
            {
                continue;
            }

            string typeName;
            string title;
            try
            {
                typeName = window.GetType().Name;
                title = window.titleContent != null ? window.titleContent.text : string.Empty;
            }
            catch
            {
                continue;
            }

            if (typeName != "FallbackEditorWindow" && title != "Failed to load")
            {
                continue;
            }

            try
            {
                window.Close();
                closed++;
            }
            catch
            {
                // Fallback windows can already be half-destroyed; ignore close failures.
            }
        }

        if (closed > 0)
        {
            Debug.Log($"Closed {closed} broken editor window(s).");
        }
    }
}
