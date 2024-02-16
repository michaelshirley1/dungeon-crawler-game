using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class playRadioMusic : MonoBehaviour
{
    private bool fastEnough = false;
    public void Run(GameObject obj)
    {
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        audioSource.Play();
    }

    public void UpdateFunc(GameObject obj)
    {
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        if (audioSource.isPlaying){
            if (obj.GetComponent<Rigidbody>().velocity.magnitude > 5)
            {
                fastEnough = true;
            }
        }
    }

    public void OnCollisionEnterFunc(GameObject obj)
    {
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        if (fastEnough && audioSource.isPlaying)
        {
            audioSource.Stop();
            fastEnough = false;
        }
    }
}
