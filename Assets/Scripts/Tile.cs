using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public string value;
    public TileManager tileManager;
    // store tile value on its own along with bools for if its on fire, etc.
    void Start()
    {
        // placeholder
        SetTileValue(tileManager.WeightedRandomLetterOfTheAlphabet());
    }

    public void SetTileValue(string text)
    {
        value = text;
        textMeshPro.text = value;
    }
}
