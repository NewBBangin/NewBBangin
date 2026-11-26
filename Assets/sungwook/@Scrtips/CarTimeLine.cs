using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class CarTimeLine : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector director;

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            PlayTimeLine();
        }
    }

    public void PlayTimeLine()
    {
        director.Play();
    }
}
