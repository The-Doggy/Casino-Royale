using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitchButton : MonoBehaviour
{
    // Basically only using this because we can't call SceneManager.LoadSceneAsync() directly from the editor
    public void LoadScene(string scene)
    {
        SceneManager.LoadSceneAsync(scene);
    }

    public void Quit()
    {
        // Debug function to allow me to gain chips for testing, only works in the editor
#if UNITY_EDITOR
        PlayerManager.Instance.AddChips(10);
#endif
        Application.Quit();
    }
}
