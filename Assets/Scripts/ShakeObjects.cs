using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeObjects : MonoBehaviour
{
    public List<Transform> objectsToShake;
    public float magnitude; // of each shake
    public float length; // of each shake
    public int quantityOfShakes;

    public float GetLengthOfAnimation()
    {
        return (quantityOfShakes + 1) * length;
    }
    public void Shake()
    {
        foreach (Transform t in objectsToShake)
        {
            var initialPosition = t.localPosition;
            for (int i = 0; i < quantityOfShakes; i++)
            {
                    LeanTween.moveLocalX(t.gameObject, initialPosition.x + magnitude * Random.Range(-1f, 1f), length).setEaseShake().setDelay(i * length);
                    LeanTween.moveLocalY(t.gameObject, initialPosition.y + magnitude * Random.Range(-1f, 1f), length).setEaseShake().setDelay(i * length);
            }
            LeanTween.moveLocalX(t.gameObject, initialPosition.x, length).setEaseShake().setDelay(quantityOfShakes * (length + 1));
            LeanTween.moveLocalY(t.gameObject, initialPosition.y, length).setEaseShake().setDelay(quantityOfShakes * (length + 1));
        }
    }
}
