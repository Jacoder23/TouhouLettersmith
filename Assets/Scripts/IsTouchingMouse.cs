using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
public class IsTouchingMouse : MonoBehaviour
{
    public bool IsTouching = false;

    public void SetIsTouching(bool touching)
    {
        IsTouching = touching;
    }
}
