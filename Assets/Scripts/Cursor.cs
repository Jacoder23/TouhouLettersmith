using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Cursor : MonoBehaviour
{
    public TextMeshProUGUI validityIndicator;
    public WordVerification verifier;
    [Header("Attributes")]
    public List<Tile> wordInProgress;
    public TilePosition cursorPosition;
    // Start is called before the first frame update
    void Start()
    {
        wordInProgress = new List<Tile>();
    }

    void LateUpdate()
    {
        if (wordInProgress.Count == 0)
            validityIndicator.text = "";
        else
            validityIndicator.text = verifier.ValidWord(string.Join("", wordInProgress.Select(x => x.value).ToArray())).ToString();
    }

    void UpdateCursorPosition()
    {
        if (wordInProgress.Count > 0)
            cursorPosition = wordInProgress.LastOrDefault().position;
        else
            cursorPosition = new TilePosition();

        //Debug.Log(cursorPosition.x + ", " + cursorPosition.y);
    }

    public void AddTile(Tile tile)
    {
        wordInProgress.Add(tile);
        UpdateCursorPosition();
        //Debug.Log(string.Join(' ', wordInProgress.Select(x => x.value).ToArray()));
    }

    public void RemoveTile(Tile tile)
    {
        wordInProgress.Remove(tile);
        UpdateCursorPosition();
        //Debug.Log(string.Join(' ', wordInProgress.Select(x => x.value).ToArray()));
    }

    public bool ToggleTile(Tile tile)
    {
        if (!wordInProgress.Contains(tile))
        {
            AddTile(tile);
            return true;
        }
        else
        {
            RemoveTile(tile);
            return false;
        }
    }
    // tile islands: when you use the same logic for finding valid tiles to turn on AND off and so can create isolated but selected tiles by traveling backwards without following the order of the tiles as they were originally selected
    public bool ToggleTileWithoutIslands(Tile tile)
    {
        if (!wordInProgress.Contains(tile))
        {
            AddTile(tile);
            return true;
        }
        else
        {
            if (wordInProgress.LastOrDefault() == tile)
            {
                RemoveTile(tile);
                return false;
            }
            return true;
        }
    }
}
