using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerButton : MonoBehaviour
{
    [SerializeField] private AudioSource clickSound;

    public delegate void ButtonClickHandler();
    public event ButtonClickHandler ButtonClicked;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            ButtonClicked.Invoke();
            clickSound?.Play();
        }
    }
}
