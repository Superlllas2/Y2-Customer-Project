using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

public static class RegenerateIdeProjectFiles
{
    private const string SessionKey = "Y2.RegeneratedIdeProjectFiles";

    [InitializeOnLoadMethod]
    private static void AutoRegenerateOnce()
    {
        if (SessionState.GetBool(SessionKey, false))
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            Sync();
        };
    }

    [MenuItem("Tools/Regenerate IDE Project Files")]
    public static void Sync()
    {
        CodeEditor.CurrentEditor.SyncAll();
        Debug.Log("IDE project files regenerated for Unity " + Application.unityVersion);
    }
}
