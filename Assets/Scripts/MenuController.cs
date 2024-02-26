using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private UIController ui;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        ui.EnterMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Fire1") != 0)
        {
            ui.CreateGameState();

            SceneManager.LoadScene("HVHS");
            SceneManager.UnloadSceneAsync("Menu");
        }
    }
}
