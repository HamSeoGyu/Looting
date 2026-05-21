using UnityEngine;
using UnityEngine.InputSystem;

public class UnitPointerController : MonoBehaviour
{
    [Header("Click / Hold")]
    public float longPressTime = 0.2f;

    private UnitDrag unitDrag;
    private UnitStats unitStats;
    private Camera mainCam;

    private bool pointerDown = false;
    private bool dragStarted = false;
    private float pointerDownTime = 0f;

    void Awake()
    {
        unitDrag = GetComponent<UnitDrag>();
        unitStats = GetComponent<UnitStats>();
        mainCam = Camera.main;
    }

    void Update()
    {
        if (!pointerDown) return;
        if (Mouse.current == null) return;
        if (mainCam == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = ScreenToWorld(mouseScreen);

        if (!dragStarted && Time.unscaledTime - pointerDownTime >= longPressTime)
        {
            dragStarted = true;
            Debug.Log(gameObject.name + " 길게 눌러서 드래그 시작");

            if (UnitInfoUI.Instance != null)
            {
                UnitInfoUI.Instance.HideUnit();
            }

            if (unitDrag != null)
            {
                unitDrag.BeginDrag(mouseWorld);
            }
        }

        if (dragStarted && unitDrag != null)
        {
            unitDrag.DragTo(mouseWorld);
        }
    }

    void OnMouseDown()
    {
        Debug.Log(gameObject.name + " OnMouseDown 들어옴");

        pointerDown = true;
        dragStarted = false;
        pointerDownTime = Time.unscaledTime;
    }

    void OnMouseUp()
    {
        Debug.Log(gameObject.name + " OnMouseUp 들어옴");

        if (!pointerDown) return;

        if (dragStarted)
        {
            if (unitDrag != null)
            {
                unitDrag.EndDrag();
            }
        }
        else
        {
            Debug.Log(gameObject.name + " 짧게 클릭됨 -> 강화창 열기 시도");

            if (unitStats != null && UnitInfoUI.Instance != null)
            {
                UnitInfoUI.Instance.ShowUnit(unitStats);
            }
            else
            {
                Debug.LogWarning(gameObject.name + " unitStats 또는 UnitInfoUI.Instance가 null입니다.");
            }
        }

        pointerDown = false;
        dragStarted = false;
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 world = mainCam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCam.transform.position.z))
        );
        world.z = 0f;
        return world;
    }
}