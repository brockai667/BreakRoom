using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// Zvýrazní kartu pri prejdení myšou (zväčšenie + jasnejší rámik).
public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Graphic border;
    public Color normal = Color.gray;
    public Color highlight = Color.white;

    public void OnPointerEnter(PointerEventData e)
    {
        transform.localScale = Vector3.one * 1.06f;
        if (border != null) border.color = highlight;
    }

    public void OnPointerExit(PointerEventData e)
    {
        transform.localScale = Vector3.one;
        if (border != null) border.color = normal;
    }
}
