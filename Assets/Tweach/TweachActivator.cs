using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweachActivator : MonoBehaviour
{
    [TextArea] public string description = "This is just an example script of how to activate Tweach";

    public KeyCode activationKey = KeyCode.Escape;
    public GameObject tweachPrefab;

    GameObject tweachInstance;

    void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            if (tweachInstance == null)
            {
                PauseGame();

                tweachInstance = Instantiate(tweachPrefab);

                tweachInstance.GetComponent<Tweach.TweachMain>().runOnExit = ResumeGame; //Tell Tweach to run ResumeGame() in this script when it exits.
            }
            else
            {
                Destroy(tweachInstance);
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }
}
