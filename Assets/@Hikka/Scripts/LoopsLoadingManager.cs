using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoopsLoadingManager : MonoBehaviour
{
    public static LoopsLoadingManager instance;
    public List<GameObject> loadingTriggers;
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
    
    public void ActivateAllLoadingTriggers()
    {
        for (int i = 0; i < loadingTriggers.Count; i++)
        {
            loadingTriggers[i].SetActive(true);   
        }
    }

    public void LoadNextLoop(Vector3 playerOffset)
    {
        // UNLOAD OLD SCENE IF ANY
        if (currentLoopScene > 0)
        {
            SceneManager.UnloadSceneAsync(currentLoopScene);
        }
        
        // LOAD NEW ONE
        if (SceneManager.sceneCountInBuildSettings > currentLoopScene + 1)
            currentLoopScene++;
        
        SceneManager.LoadSceneAsync(currentLoopScene, LoadSceneMode.Additive);
        
        
        ProceduralPlantsManager.instance.NewDay();
        if (PlayerMovement.instance.rb)
            PlayerMovement.instance.rb.isKinematic = true;
        else
            PlayerMovement.instance.controller.enabled = false;
        PlayerMovement.instance.teleport = true;
        //PlayerMovement.instance.transform.position = new Vector3(PlayerMovement.instance.transform.position.x, positionForPlayerTeleport.position.y, PlayerMovement.instance.transform.position.z);
        PlayerMovement.instance.transform.position = positionForPlayerTeleport.position + playerOffset;
        PlayerMovement.instance.TeleportPlayerHead();
        PlayerMovement.instance.teleport = false;
        if (PlayerMovement.instance.rb)
            PlayerMovement.instance.rb.isKinematic = false;
        else
            PlayerMovement.instance.controller.enabled = true;
    }
}
