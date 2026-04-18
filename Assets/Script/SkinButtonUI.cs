using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkinButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private Image image;

    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = Vector3.one * 1.1f;
    private Vector3 selectedScale = Vector3.one * 1.2f;

    private Color normalColor;
    private Color hoverColor = Color.white;
    private Color selectedColor = Color.green;

    private bool isSelected = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        normalColor = image.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelected) return;

        transform.localScale = hoverScale;
        image.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected) return;

        transform.localScale = normalScale;
        image.color = normalColor;
    }

    // 🔥 GỌI TỪ SkinSelectionUI
    public void SetSelected(bool value)
    {
        isSelected = value;

        if (isSelected)
        {
            transform.localScale = selectedScale;
            image.color = selectedColor;
        }
        else
        {
            transform.localScale = normalScale;
            image.color = normalColor;
        }
    }
}