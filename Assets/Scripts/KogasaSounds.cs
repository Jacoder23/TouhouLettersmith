using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
public class KogasaSounds : MonoBehaviour
{
    public void PlayRatchetSound()
    {
        AudioManager.PlaySound(LibrarySounds.PrepareSwing);
    }

    public void PlayWhooshSound()
    {
        AudioManager.PlaySound(LibrarySounds.Whoosh);
    }

    public void PlayImpactSound()
    {
        AudioManager.PlaySound(LibrarySounds.Impact);
    }
}
