using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundWave : MonoBehaviour
{
    private bool isApply = false;

    private void OnEnable()
    {
        isApply = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && isApply == false)
        {
            PlayerController.instance.moveSpeed *= 0.5f;
            isApply = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && isApply == true)
        {
            PlayerController.instance.moveSpeed *= 2f;
            isApply = false;
        }
    }
}
