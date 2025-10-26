using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScrambleTiles : MonoBehaviour
{
    public TileManager tileManager;
    public Cursor cursor;

    public void ScrambleAllTiles()
    {
        tileManager.ScrambleAllTiles();
        tileManager.DeselectAllTiles();
        cursor.ClearBoard();
    }
}
