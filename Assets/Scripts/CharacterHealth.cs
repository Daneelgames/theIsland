using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterHealth : MonoBehaviour
{
    public CharacterController characterController;

    public int health = 30;

    public float fallDamageSafeHeightStep = 20;
    public int fallDamageStep = 5;
    private Vector3 lastGroundPosition;
    private Vector3 currentGroundPosition;

    private Coroutine fallCoroutine;
    
    private void Update()
    {
        if (characterController)
            GetFall();
    }

    void OnGUI()
    {
        
        GUI.color = Color.green;
        GUI.Box(new Rect(Screen.width / 2 - Screen.width / 16, Screen.height * 0.95f, Screen.width / 8, Screen.height / 20), "HEALTH: " + health + "/30");
    }

    void GetFall()
    {
        if (fallCoroutine == null && characterController.isGrounded == false)
            fallCoroutine = StartCoroutine(Fall());
    }

    IEnumerator Fall()
    {
        lastGroundPosition = transform.position;
        while (characterController.isGrounded == false)
        {
            yield return null;
        }

        currentGroundPosition = transform.position;

        float fallHeight = lastGroundPosition.y - currentGroundPosition.y;

        print("Fall Height is " + fallHeight);
        for (int i = 1; i < 10; i++)
        {
            if (fallHeight > fallDamageSafeHeightStep * i && fallHeight < fallDamageSafeHeightStep * (i+1))
            {
                TakeDamage(fallDamageStep * i);
                break;
            }
        }
        fallCoroutine = null;
    }

    void TakeDamage(int dmg)
    {
        health -= dmg;

        if (health <= 0)
            StartCoroutine(Death());
    }

    IEnumerator Death()
    {
        if (characterController)
        {
            characterController.enabled = false;
            yield return new WaitForSeconds(3);

            SceneManager.LoadScene(0);
        }
    }
}
