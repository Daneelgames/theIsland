using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoopsLoadingManager : MonoBehaviour
{
    public static LoopsLoadingManager instance;
    public GameObject loadingTrigger;
    public Transform positionForPlayerTeleport;
    public int currentLoopScene = 0;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }

    public void LoadNextLoop()
    {
        // UNLOAD OLD SCENE IF ANY
        Debug.Log(currentLoopScene);
        if (currentLoopScene > 0)
        {
            SceneManager.UnloadSceneAsync(currentLoopScene);
        }
        
        // LOAD NEW ONE
        if (SceneManager.sceneCountInBuildSettings > currentLoopScene + 1)
            currentLoopScene++;
        
        SceneManager.LoadSceneAsync(currentLoopScene, LoadSceneMode.Additive);

        PlayerMovement.instance.controller.enabled = false;
        PlayerMovement.instance.teleport = true;
        PlayerMovement.instance.transform.position = positionForPlayerTeleport.position;
        PlayerMovement.instance.TeleportPlayerHead();
        PlayerMovement.instance.teleport = false;
        PlayerMovement.instance.controller.enabled = true;
    }
}
