using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Linq;
public class PuppetData : SerializedMonoBehaviour
{
    public Dictionary<string, Sprite> expressions;
    public Dictionary<string, Sprite> backs;

    [Space]

    public float baseScale = 1f;
    public Vector2 spriteOffset;

    [Space]

    [ValueDropdown("GetExpressionStrings")]
    public string currentExpression;

    [ValueDropdown("GetBackStrings")]
    public string currentBack;

    public Vector2 currentLocation;

    [Space]

    public Image back;
    public Image expression;
    public void UpdateSprite(float lerpTime)
    {
        transform.localScale = new Vector2(baseScale, baseScale);

        expression.sprite = expressions[currentExpression];

        if (backs.Count > 0)
            back.sprite = backs[currentBack];
        else
            back.color = new Color32(0, 0, 0, 0);

        Debug.Log(currentLocation + ", " + spriteOffset);

        if (lerpTime != 0)
            LeanTween.moveLocal(gameObject, currentLocation + spriteOffset, lerpTime);
        else
            transform.localPosition = currentLocation + spriteOffset;
    }

    IEnumerable<string> GetExpressionStrings()
    {
        return expressions.Select(x => x.Key);
    }

    IEnumerable<string> GetBackStrings()
    {
        return backs.Select(x => x.Key);
    }
}
