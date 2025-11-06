using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowIfPlayerPrefStringEquals : MonoBehaviour
{
    public CanvasGroup canvas;
    public string keyName;
    public string equals;

    void Start()
    {
        if (!PlayerPrefs.HasKey(keyName))
            return;

        if (PlayerPrefs.GetString(keyName) == equals)
        {
            canvas.alpha = 1;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
        }
    }
}
