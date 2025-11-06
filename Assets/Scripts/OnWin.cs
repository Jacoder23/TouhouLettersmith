using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;
using JSAM;
using Sirenix.OdinInspector;

public class OnWin : MonoBehaviour
{
    public CanvasGroup parentCanvas;
    public CanvasGroup showOnWin;
    public TextMeshProUGUI pointsText;
    public Cursor cursor;
    public TurnCounter turnCounter;
    public AudioSource music;

    [Button]
    public void Win()
    {
        PlayerPrefs.SetString("NextScene", "VNScene");
        PlayerPrefs.SetString("AtLeastOneLevelCompleted", "true");
        Debug.Log("Win");
        AudioManager.StopAllMusic();
        // need to manually stop the game music now since its not jsam
        music.Stop();
        Invoke("Victory", 3.0f); // unhardcode and match to if its a kogasa big win or not
    }

    void Victory()
    {
        AudioManager.PlaySound(LibrarySounds.Fanfare);

        showOnWin.alpha = 1;
        showOnWin.blocksRaycasts = true;
        showOnWin.interactable = true;

        parentCanvas.alpha = 1;
        parentCanvas.blocksRaycasts = true;
        parentCanvas.interactable = true;

        int wordsSmithed = cursor.letterInProgress.Count();
        int tilesUsed = string.Join(string.Empty, cursor.letterInProgress).Length;
        string longestWord = cursor.letterInProgress.OrderByDescending(x => x.Length).First();
        int longestWordScore = cursor.letterInProgress.OrderByDescending(x => x.Length).First().Length * 10;
        string shortestWord = cursor.letterInProgress.OrderByDescending(x => x.Length).Last();
        int shortestWordScore = cursor.letterInProgress.OrderByDescending(x => x.Length).Last().Length * 10;
        int turnsLeft = turnCounter.maxTurns - turnCounter.turns;
        if (turnsLeft == 0)
            turnsLeft = 1;

        int score = (wordsSmithed + tilesUsed + longestWordScore + shortestWordScore) * turnsLeft;

        // hell
        pointsText.text = " \n<size=\"60%\"> \n<align=\"center\"><b> </b></align></size> \n<size=\"40%\"> \n+" + wordsSmithed + " \n \n+" + tilesUsed + " \n \n" + longestWord + " \n+" + longestWordScore + " \n \n" + shortestWord + " \n+" + shortestWordScore + " \n \nx" + turnsLeft + " \n</size><size=\"60%\"> \n<align=\"center\"><b> </b></align></size> \n" + score;
    }
}
