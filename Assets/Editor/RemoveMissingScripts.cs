using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class RemoveMissingScripts : Editor
{
    [MenuItem("GameObject/Remove Missing Scripts")]
    public static void Remove()
    {
        var objs = Resources.FindObjectsOfTypeAll<GameObject>();
        int count = objs.Sum(GameObjectUtility.RemoveMonoBehavioursWithMissingScript);
        Debug.Log($"Removed {count} missing scripts");
    }
}
