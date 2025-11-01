using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimatedImage : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float frameDuration = 0.3f;

    private Image imageComponent;
    private int currentIndex = 0;
    private Coroutine animationCoroutine;

    void Start()
    {
        imageComponent = GetComponent<Image>();
        if (sprites != null && sprites.Length > 0)
            animationCoroutine = StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        while (true)
        {
            imageComponent.sprite = sprites[currentIndex];
            yield return new WaitForSeconds(frameDuration);
            currentIndex = (currentIndex + 1) % sprites.Length;
        }
    }

    void OnValidate()
    {
        frameDuration = Mathf.Max(0.01f, frameDuration);
    }

    void OnDisable()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
    }
}