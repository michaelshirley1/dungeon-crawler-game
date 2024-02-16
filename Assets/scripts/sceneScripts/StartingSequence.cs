using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System;

public class StartingSequence : MonoBehaviour
{
    public GameObject wall;
    public bool startingSequenceStarted = false;
    public bool startingSequenceFinished = false;
    public CameraShake cameraShake;
    public AudioSource audioSourceLift;
    public AudioSource audioSourceEntry;
    public AudioSource audioSourceSqueak;
    private ProcGenHandler spawnStrategy;
    private float pressTime = 0;
    public GameObject prison;
    public GameObject blockingWall;
    public void pressButton()
    {
        if (startingSequenceStarted)
        {
            return;
        }
        // time since the button was pressed
        pressTime = Time.time;
        startingSequenceStarted = true;
        shakeScreens();
        loadSoundSequence(audioSourceLift, audioSourceEntry);
        prison.transform.position = Vector3.MoveTowards(prison.transform.position, new Vector3(0, -6.64f, 0), 0.1f);
        spawnStrategy = GameObject.FindGameObjectsWithTag("SpawnStart")[0].GetComponent<ProcGenHandler>();
        spawnStrategy.GenerateRooms();
    }

    void loadSoundSequence(AudioSource audioSourceLift, AudioSource audioSourceEntry)
    {
        audioSourceLift.Play();
        audioSourceSqueak.Play();
    }
    IEnumerator afterLoadSounds()
    {
        audioSourceLift.Play();
        yield return new WaitForSeconds(audioSourceLift.clip.length);
        audioSourceEntry.Play();
        blockingWall.transform.position = Vector3.MoveTowards( blockingWall.transform.position, new Vector3(0, 10, 0), 0.1f);
    }

    void Update()
    {
        if (pressTime != 0){
            if (Time.time - pressTime > 10 && !startingSequenceFinished)
            {
                audioSourceSqueak.Stop();
                startingSequenceFinished = true;
                shakeScreens();
                afterLoadSounds();
            }
        }
    }

    void shakeScreens(){
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Transform cameraContainer = player.transform.Find("Camera_Pivot");
            if (cameraContainer != null)
            {
                StartCoroutine(cameraShake.Shake(2f, 0.1f, cameraContainer));
            }
        }
    }
}



        