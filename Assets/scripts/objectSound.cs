using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectSound : MonoBehaviour
{
    bool fastEnough = false;

    public enum materialType{
        wood,
        metal,
        plastic,
        glass,
        food,
        radio
    }

    public materialType soundType;
    AudioSource audioSource;

    void Update()
    {
        if (gameObject.GetComponent<Rigidbody>().velocity.magnitude > 2)
        {
            fastEnough = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (fastEnough)
        {
            AudioClip clip = Resources.Load<AudioClip>("MaterialSounds/" + soundType.ToString() + UnityEngine.Random.Range(1, 3));
            AudioSource.PlayClipAtPoint(clip, transform.position, 0.4f);
            fastEnough = false;
        }
    }
}
