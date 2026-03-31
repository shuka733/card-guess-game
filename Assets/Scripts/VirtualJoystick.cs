using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform Background;
    public RectTransform Knob;
    public float MaxRadius = 60f;

    public Vector2 Direction { get; private set; }

    private int activeTouchId = -1;

    public void OnPointerDown(PointerEventData e)
    {
        activeTouchId = e.pointerId;
        UpdateKnob(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (e.pointerId != activeTouchId) return;
        UpdateKnob(e);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (e.pointerId != activeTouchId) return;
        activeTouchId = -1;
        Direction = Vector2.zero;
        if (Knob) Knob.anchoredPosition = Vector2.zero;
    }

    void UpdateKnob(PointerEventData e)
    {
        if (Background == null || Knob == null) return;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Background, e.position, e.pressEventCamera, out local);
        local = Vector2.ClampMagnitude(local, MaxRadius);
        Knob.anchoredPosition = local;
        Direction = local / MaxRadius;
    }
}
