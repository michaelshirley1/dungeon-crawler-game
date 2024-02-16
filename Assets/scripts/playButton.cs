using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playButton : MonoBehaviour
{
    public GameObject startButton;
    public GameObject startText;
    public GameObject settingsButton;
    public GameObject settingsText;
    public GameObject exitButton;
    public GameObject exitText;
    public GameObject hostButton;
    public GameObject hostText;
    public GameObject joinButton;
    public GameObject joinText;
    public GameObject backButton;
    public GameObject backText;

    public void Start()
    {
        joinButton.SetActive(false);
        joinText.SetActive(false);
        hostButton.SetActive(false);
        hostText.SetActive(false);
        backButton.SetActive(false);
    }

    public void hostButt()
    {
        NetworkManager.Singleton.OnServerStarted+=HandleServerStarted;
        NetworkManager.Singleton.StartHost();
    }

    private void HandleServerStarted() {
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }

    public void joinButt()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void startButt()
    {
        startButton.SetActive(false);
        startText.SetActive(false);
        settingsButton.SetActive(false);
        settingsText.SetActive(false);
        exitButton.SetActive(false);
        exitText.SetActive(false);
        backButton.SetActive(true);
        hostButton.SetActive(true);
        hostText.SetActive(true);
        joinButton.SetActive(true);
        joinText.SetActive(true);
    }

    public void settingsButt()
    {

    }

    public void exitButt()
    {
        Application.Quit();
    }

    public void loadMain(){
        startButton.SetActive(true);
        startText.SetActive(true);
        settingsButton.SetActive(true);
        settingsText.SetActive(true);
        exitButton.SetActive(true);
        exitText.SetActive(true);
        backButton.SetActive(false);
        hostButton.SetActive(false);
        hostText.SetActive(false);
        joinButton.SetActive(false);
        joinText.SetActive(false);
    }

    public void hoverItem(){
        AudioClip clip = Resources.Load<AudioClip>("MaterialSounds/menuselect");
        AudioSource.PlayClipAtPoint(clip, new Vector3(32.36f, 5.318566f, -1.851f), 0.7f);
    }
}
