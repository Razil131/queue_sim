using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] Button exitButton;
    bool callLoad = false;

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
        callLoad = false; 
    }

    public void LoadSave()
    {
        SceneManager.LoadScene("MainScene");
        callLoad = true;
    }

     void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainScene")
        {

            if (callLoad)
            {
            LoadSaveFromController();
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;

        }
    }

    void LoadSaveFromController()
    {
        GameObject obj = GameObject.Find("SimulationControllerObject");
        if (obj != null)
        {
            SimulationController comp = obj.GetComponent<SimulationController>();
            if (comp != null)
            {
                comp.LoadSimulation();
            }
        }
    }
}
