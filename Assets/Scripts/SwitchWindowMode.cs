using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SwitchWindowMode : MonoBehaviour
{
    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
    public void EnterFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
    public void LeaveFullscreen()
    {
        Screen.fullScreen = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EnterFullscreen();
        }
        else if (Input.GetKeyDown(KeyCode.F11))
        {
            LeaveFullscreen();
        }
    }
}
