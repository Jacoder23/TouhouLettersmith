using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
public class PlayDialogueBleepIntermittently : MonoBehaviour
{
    public SoundPlayer dialogueBleep;
    public int playEveryNCharacters = 3;
    int currentCount = 0;
    public void OnNewCharacterShown()
    {
        currentCount++;
        if (currentCount > playEveryNCharacters)
        {
            dialogueBleep.PlaySound();
            currentCount = 0;
        }
    } // typewriter effect
}
