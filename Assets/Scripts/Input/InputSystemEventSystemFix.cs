using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Replaces legacy StandaloneInputModule (uses UnityEngine.Input) with Input System UI module.
/// </summary>
public static class InputSystemEventSystemFix
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        FixAll();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FixAll();
    }

    private static void FixAll()
    {
        var systems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var system in systems)
        {
            Fix(system);
        }
    }

    private static void Fix(EventSystem system)
    {
        if (!system)
        {
            return;
        }

        var legacy = system.GetComponent<StandaloneInputModule>();
        if (legacy)
        {
            Object.Destroy(legacy);
        }

        var inputModule = system.GetComponent<InputSystemUIInputModule>();
        if (!inputModule)
        {
            inputModule = system.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (inputModule.actionsAsset == null)
        {
            inputModule.AssignDefaultActions();
        }
    }
}
