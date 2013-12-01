using UnityEngine;
using System.Collections;

public class Music : MonoBehaviour
{
    public AudioClip _sound;

    public void Start()
    {
        audio.loop = true;
        audio.clip = _sound;
        audio.Play();
    }
}