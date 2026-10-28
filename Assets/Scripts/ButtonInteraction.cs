using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonAction : MonoBehaviour
{
    public string sceneToLoad; // Name of the scene to load, leave empty for Exit Game button
    public bool isExitButton = false; // Set true for the exit button

    private void Update()
    {
        if (GameInput.Instance == null || !GameInput.Instance.ClickPressed || Camera.main == null)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(GameInput.Instance.PointerPosition);
        if (Physics.Raycast(ray, out var hit) && hit.transform == transform)
        {
            if (isExitButton)
            {
                ExitGame();
            }
            else
            {
                LoadScene();
            }
        }
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad); // Load the specified scene
        }
    }

    private void ExitGame()
    {
        Debug.Log("Exit Game");
        Application.Quit(); // Close the application
    }
}