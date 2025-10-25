using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPortrait : MonoBehaviour
{
    [SerializeField]
    private Image CharacterImage;
    [SerializeField]
    private Image DropShadow;

    public void SetCharacterPortrait(Sprite sprite)
    {
        CharacterImage.sprite = sprite;
        DropShadow.sprite = sprite;
    }
}
