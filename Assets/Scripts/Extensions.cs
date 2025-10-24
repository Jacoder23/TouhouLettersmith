using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
public static class Extensions
{

    public static TilePosition GetTilePositionFromIndex(int index, int gridSize)
    {
        var tilePosition = new TilePosition();

        tilePosition.y = Mathf.FloorToInt((float)index / (float)gridSize);
        tilePosition.x = index - tilePosition.y * gridSize;

        return tilePosition;
    }
    public static int GetIndexFromTilePosition(int x, int y, int gridSize)
    {
        if (y < 0)
            y = 0;

        return x + y * gridSize;
    }

    // https://stackoverflow.com/a/450250
    public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
    {
        var item = list[oldIndex];

        list.RemoveAt(oldIndex);

        if (newIndex > oldIndex) newIndex--;
        // the actual index could have shifted due to the removal

        list.Insert(newIndex, item);
    }
    public static void Move<T>(this List<T> list, T item, int newIndex)
    {
        if (item != null)
        {
            var oldIndex = list.IndexOf(item);
            if (oldIndex > -1)
            {
                list.RemoveAt(oldIndex);

                if (newIndex > oldIndex) newIndex--;
                // the actual index could have shifted due to the removal

                list.Insert(newIndex, item);
            }
        }

    }

    // https://stackoverflow.com/a/6219488
    private static readonly Regex sWhitespace = new Regex(@"\s+");
    public static string ReplaceWhitespace(string input, string replacement)
    {
        return sWhitespace.Replace(input, replacement);
    }

    // TODO: figure out one with a combined QU
    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string RandomLetterOfTheAlphabet()
    {
        int random = Random.Range(0, 25);
        return alphabet.Substring(random, 1);
    }

    public static void DebugLog2DJaggedArray(string[][] array)
    {
        string output = "";
        for (int i = 0; i < array.Length; i++)
        {
            string[] row = array[i];
            output += string.Join(' ', row) + "\n";
        }
        Debug.Log(output);
    }

    //https://stackoverflow.com/q/4670720
    public static string[][] Copy2DJaggedArray(this string[][] source)
    {
        string[][] destination = new string[source.Length][];
        // For each Row
        for (int y = 0; y < source.Length; y++)
        {
            // Initialize Array
            destination[y] = new string[source[y].Length];
            // For each Column
            for (int x = 0; x < destination[y].Length; x++)
            {
                destination[y][x] = source[y][x];
            }
        }
        return destination;
    }

    public static bool ValidTileDestination(TilePosition origin, TilePosition destination, int gridSize)
    {
        // TODO: allow for yukari umbrella and okina door upgrades

        // bitboards? nah just figure it out

        if (origin == null)
            return true;

        if (origin.x == -1 || origin.y == -1)
            return true;

        if (origin.x == 0 && (destination.x != 0 && destination.x != 1))
            return false;

        if (origin.x == gridSize && (destination.x != gridSize && destination.x != gridSize - 1))
            return false;

        if (origin.y == 0 && (destination.y != 0 && destination.y != 1))
            return false;

        if (origin.y == gridSize && (destination.y != gridSize && destination.y != gridSize - 1))
            return false;

        if (Mathf.Abs(origin.x - destination.x) > 1)
            return false;

        if (Mathf.Abs(origin.y - destination.y) > 1)
            return false;

        // otherwise
        return true;
    }
}
