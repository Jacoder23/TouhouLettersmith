using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI.Extensions;

public class Cursor : MonoBehaviour
{
    public TextMeshProUGUI validityIndicator;
    public WordVerification verifier;
    public TileManager tileManager;
    public UILineTextureRenderer line;
    [Header("Attributes")]
    public List<Tile> wordInProgress;
    public TilePosition cursorPosition;
    public List<string> letterInProgress;
    // Start is called before the first frame update
    void Start()
    {
        wordInProgress = new List<Tile>();
        letterInProgress = new List<string>();
    }

    WordValidity ValidateWord()
    {
        return verifier.ValidWord(string.Join("", wordInProgress.Select(x => x.value).ToArray()));
    }

    string CurrentWord()
    {
        return string.Join("", wordInProgress.Select(x => x.value).ToArray());
    }

    void LateUpdate()
    {
        if (wordInProgress.Count == 0)
            validityIndicator.text = "";
    }

    void UpdateCursorPosition()
    {
        if (wordInProgress.Count > 0)
            cursorPosition = wordInProgress.LastOrDefault().position;
        else
            cursorPosition = new TilePosition();

        //Debug.Log(cursorPosition.x + ", " + cursorPosition.y);
    }

    public void SubmitWord()
    {
        if(ValidateWord() != WordValidity.Invalid)
        {
            letterInProgress.Add(CurrentWord());
            wordInProgress.Clear();
            tileManager.RemoveSelectedTiles();
            UpdateCursorPosition();
            UpdateLineRenderer();
        }
    }

    void UpdateLineRenderer()
    {
        if (wordInProgress.Count < 2)
            line.Points = new Vector2[] { Vector2.zero, Vector2.zero };
        else
            line.Points = wordInProgress.Select(x => (Vector2)x.transform.localPosition).ToArray();
    }

    public void AddTile(Tile tile)
    {
        wordInProgress.Add(tile);
        UpdateCursorPosition();
        UpdateLineRenderer();
        UpdateIndicator();
        //Debug.Log(string.Join(' ', wordInProgress.Select(x => x.value).ToArray()));
    }

    public void RemoveTile(Tile tile)
    {
        wordInProgress.Remove(tile);
        UpdateCursorPosition();
        UpdateLineRenderer();
        UpdateIndicator();
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

    void UpdateIndicator()
    {
        validityIndicator.text = CurrentWord();
        var validity = ValidateWord();

        if (validity == WordValidity.Invalid)
        {
            validityIndicator.color = Color.gray;
        }
        else if (validity == WordValidity.Valid)
        {
            validityIndicator.color = Color.white;
        }
        else if (validity == WordValidity.Bonus)
        {
            validityIndicator.color = Color.white;
            validityIndicator.text = "<rainb>" + CurrentWord() + "</rainb>";
        }
    }
}
