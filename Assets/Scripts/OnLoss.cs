using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;

public class OnLoss : MonoBehaviour
{
    public CanvasGroup parentCanvas;
    public CanvasGroup showOnLoss;
    public AudioSource music;

    public void Lose()
    {
        Debug.Log("Lose");
        AudioManager.StopAllMusic();
        // need to manually stop the game music now since its not jsam
        music.Stop();
        Invoke("Loss", 3.0f); // unhardcode and match depending on what kind of hit kogasa is doing
    }

    void Loss()
    {
        AudioManager.PlaySound(LibrarySounds.Lose);

        showOnLoss.alpha = 1;
        showOnLoss.blocksRaycasts = true;
        showOnLoss.interactable = true;

        parentCanvas.alpha = 1;
        parentCanvas.blocksRaycasts = true;
        parentCanvas.interactable = true;
    }
}
