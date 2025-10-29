using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public TileManager tileManager;
    public CanvasGroup itemCanvas;
    public LevelDatabase levelData;
    // TODO: add other items later

    void Start()
    {
        if(levelData.GetCurrentLevel().snowball)
        {
            itemCanvas.alpha = 1f;
            itemCanvas.interactable = true;
            itemCanvas.blocksRaycasts = true;
        }
        else
        {
            itemCanvas.alpha = 0f;
            itemCanvas.interactable = false;
            itemCanvas.blocksRaycasts = false;
        }
    }
    public void Snowball()
    {
        // Cirno's snowball: Turn all the fire tiles on the board into normal tiles
        foreach(var tile in tileManager.instantiatedTiles)
        {
            if(tile.type == TileType.Fire)
            {
                tile.ChangeTileType(TileType.Normal);
            }
            // used up
            itemCanvas.alpha = 0f;
            itemCanvas.interactable = false;
            itemCanvas.blocksRaycasts = false;
        }
    }
}
