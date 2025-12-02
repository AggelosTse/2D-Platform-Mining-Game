using UnityEngine;
using UnityEngine.EventSystems;

public class buttonShrinkZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Vector3 originalScale;
    Vector3 targetScale;

    public float zoom = 1.2f;
    public float speed = 10f;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale; // ✅ FIX: start at normal size
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData e) => targetScale = originalScale * zoom;
    public void OnPointerExit(PointerEventData e) => targetScale = originalScale;
}
