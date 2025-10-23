using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Cursor : MonoBehaviour
{
    public List<Tile> wordInProgress;
    // Start is called before the first frame update
    void Start()
    {
        wordInProgress = new List<Tile>();
    }

    public void AddTile(Tile tile)
    {
        wordInProgress.Add(tile);
        Debug.Log(string.Join(' ', wordInProgress.Select(x => x.value).ToArray()));
    }

    public void RemoveTile(Tile tile)
    {
        wordInProgress.Remove(tile);
        Debug.Log(string.Join(' ', wordInProgress.Select(x => x.value).ToArray()));
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
}
