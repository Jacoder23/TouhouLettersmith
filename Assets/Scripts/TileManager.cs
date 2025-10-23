using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KaimiraGames;
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
    WeightedList<string> weightedRandomAlphabet;
    void Start()
    {
        InitWeightedAlphabet();

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
                tile.tileManager = this;
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

    // just faffed about with the wikipedia letter frequency list
    // TODO: have a version that inserts actual words into the board state and that also gives a random word of X length when asked for so when we need to drop in a new bunch of letters we have some options
    public string WeightedRandomLetterOfTheAlphabet()
    {
        return weightedRandomAlphabet.Next();
    }

    void InitWeightedAlphabet()
    {
        List<WeightedListItem<string>> weightedAlphabet = new()
        {
            new WeightedListItem<string>("A", 10),
            new WeightedListItem<string>("B", 5),
            new WeightedListItem<string>("C", 5),
            new WeightedListItem<string>("D", 6),
            new WeightedListItem<string>("E", 10),
            new WeightedListItem<string>("F", 5),
            new WeightedListItem<string>("G", 5),
            new WeightedListItem<string>("H", 7),
            new WeightedListItem<string>("I", 10),
            new WeightedListItem<string>("J", 3),
            new WeightedListItem<string>("K", 3),
            new WeightedListItem<string>("L", 5),
            new WeightedListItem<string>("M", 5),
            new WeightedListItem<string>("N", 5),
            new WeightedListItem<string>("O", 10),
            new WeightedListItem<string>("P", 7),
            new WeightedListItem<string>("Q", 2),
            new WeightedListItem<string>("R", 7),
            new WeightedListItem<string>("S", 7),
            new WeightedListItem<string>("T", 9),
            new WeightedListItem<string>("U", 10),
            new WeightedListItem<string>("V", 2),
            new WeightedListItem<string>("W", 5),
            new WeightedListItem<string>("X", 2),
            new WeightedListItem<string>("Y", 3),
            new WeightedListItem<string>("Z", 2),
        };
        weightedRandomAlphabet = new(weightedAlphabet);
    }
}
