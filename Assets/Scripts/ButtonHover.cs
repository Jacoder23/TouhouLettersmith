using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverRotationAngle = 2f;
    [SerializeField] private Vector2 hoverOffset = new Vector2(5f, 2f);
    [SerializeField] private float hoverDuration = 0.3f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;

    private RectTransform buttonRect;
    private Vector2 originalPosition;
    private Vector3 originalRotation;

    void Start()
    {
        buttonRect = GetComponent<RectTransform>();
        originalPosition = buttonRect.anchoredPosition;
        originalRotation = buttonRect.eulerAngles;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.cancel(gameObject);

        LeanTween.rotate(gameObject, originalRotation + Vector3.forward * hoverRotationAngle, hoverDuration).setEase(easeType);
        LeanTween.move(buttonRect, originalPosition + hoverOffset, hoverDuration).setEase(easeType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.cancel(gameObject);
        //Return to original position
        LeanTween.rotate(gameObject, originalRotation, hoverDuration).setEase(easeType);
        LeanTween.move(buttonRect, originalPosition, hoverDuration).setEase(easeType);
    }
}
