using UnityEditor;
[InitializeOnLoadAttribute]
public static class AutoRefresh
{
    static AutoRefresh()
    {
        EditorApplication.playModeStateChanged += PlayRefresh;
    }

    private static void PlayRefresh(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            AssetDatabase.Refresh();
        }
    }
}
