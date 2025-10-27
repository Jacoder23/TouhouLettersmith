using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
public class MainMenuMusic : MonoBehaviour
{
    public MusicPlayer music;
    bool played = false;
    public static MainMenuMusic Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        // play on start but only the first time when we load the main menu then not in other scenes, just keep playback
        if (!played)
        {
            music.Play();
            played = true;
        }

        DontDestroyOnLoad(gameObject);
    }
}
