using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ToggleFont : MonoBehaviour
{
    // WARNING: Only really works if all the text uses the same font, otherwise mucks it up, this is a hasty solution made for a jam and it shows
    // but it does work

    public TMP_FontAsset defaultFont;
    public TMP_FontAsset alternateFont;
    bool useAlternateFont = false;

    void Start()
    {
        if (!PlayerPrefs.HasKey("useAlternateFont"))
            PlayerPrefs.SetInt("useAlternateFont", 0);

        UpdateFonts();
    }

    public void ToggleCurrentFont()
    {
        if (PlayerPrefs.GetInt("useAlternateFont") == 0)
            PlayerPrefs.SetInt("useAlternateFont", 1);
        else
            PlayerPrefs.SetInt("useAlternateFont", 0);
        UpdateFonts();
    }

    void UpdateFonts()
    {
        if (PlayerPrefs.GetInt("useAlternateFont") == 1)
        {
            foreach (var text in FindObjectsOfType<TextMeshProUGUI>())
            {
                if(text.font == defaultFont) // to try and avoid messing with custom materials on buttons
                    text.font = alternateFont;
            }
        }
        else
        {
            if (PlayerPrefs.GetInt("useAlternateFont") == 0)
            {
                foreach (var text in FindObjectsOfType<TextMeshProUGUI>())
                {
                    if (text.font == alternateFont)
                        text.font = defaultFont;
                }
            }
        }
    }

}
