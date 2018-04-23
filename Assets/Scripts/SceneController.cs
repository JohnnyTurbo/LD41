using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

    public GameObject loadingScreen;

    void Start()
    {
        loadingScreen.SetActive(false);
    }

	public void OnButtonBegin()
    {
        loadingScreen.SetActive(true);
        SceneManager.LoadScene(1);
    }

    public void OnButtonMainMenu()
    {
        loadingScreen.SetActive(true);
        SceneManager.LoadScene(0);
    }
}
