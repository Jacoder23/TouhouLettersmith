using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public Tile tilePrefab;
    public Transform grid;

    [Header("Settings")]
    public int gridSize;
    public float spaceBetweenTiles;

    List<Tile> instantiatedTiles;
    string[][] currentBoardState; // encode in each string extra data like what about the other things
    // format: X010101 with each 0/1 showing if it's a special tile type or not, theoretically allows combinations of tile types but that's out of scope
    
    string[][] blankBoardState;
    void Start()
    {
        instantiatedTiles = new List<Tile>();

        blankBoardState = new string[gridSize][];

        for (int i = 0; i < blankBoardState.Length; i++)
        {
            blankBoardState[i] = new string[gridSize];
            for(int j = 0; j < blankBoardState[i].Length; j++)
            {
                blankBoardState[i][j] = "A";
            }
        }

        PopulateTileGrid();

        currentBoardState = GetBoardStateFromTiles();

        SetTilesToBoardState(currentBoardState);
    }

    void PopulateTileGrid()
    {
        Vector2 offset = new Vector2((gridSize - 1) * spaceBetweenTiles / 2f, (gridSize + 1) * spaceBetweenTiles / 2f);
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                var tile = Instantiate(tilePrefab);
                tile.transform.parent = grid;
                tile.transform.localPosition = new Vector2(j * spaceBetweenTiles,((float)gridSize - i) * spaceBetweenTiles) - offset;
                instantiatedTiles.Add(tile);
            }
        }
    }

    string[][] GetBoardStateFromTiles()
    {
        string[][] boardState = Extensions.Copy2DJaggedArray(blankBoardState);

        for (int i = 0; i < instantiatedTiles.Count; i++)
        {
            // [row][num in row]
            int currentRow = Mathf.FloorToInt((float)i / (float)gridSize);
            int currentNinRow = i - currentRow * gridSize;
            boardState[currentRow][currentNinRow] = instantiatedTiles[i].value;
        }

        return boardState;
    }

    void SetTilesToBoardState(string[][] boardState)
    {
        Extensions.DebugLog2DJaggedArray(boardState);
        for(int i = 0; i < boardState.Length; i++)
        {
            var rowState = boardState[i];
            for (int j = 0; j < rowState.Length; j++)
            {
                var tileState = rowState[j];
                instantiatedTiles[i * gridSize + j].SetTileValue(tileState);
            }
        }
    }
}
