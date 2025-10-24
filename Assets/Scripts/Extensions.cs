using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class Extensions
{
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
