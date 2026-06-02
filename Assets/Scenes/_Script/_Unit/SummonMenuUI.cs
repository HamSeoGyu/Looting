using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SummonMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject unitButtonPanel;
    public Button summonButton;
    public Canvas rootCanvas;

    private RectTransform unitButtonPanelRect;
    private Camera uiCamera;

    // 메뉴를 방금 열었을 때 같은 클릭으로 바로 닫히는 것 방지
    private bool blockOutsideCloseUntilMouseRelease = false;

    void Awake()
    {
        if (unitButtonPanel != null)
        {
            unitButtonPanelRect = unitButtonPanel.GetComponent<RectTransform>();
        }

        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null)
            {
                rootCanvas = FindFirstObjectByType<Canvas>();
            }
        }

        if (rootCanvas != null)
        {
            if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                uiCamera = null;
            else
                uiCamera = rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main;
        }
    }

    void Start()
    {
        if (unitButtonPanel != null)
        {
            unitButtonPanel.SetActive(false);
        }
    }

    void Update()
    {
        HandleOutsideClick();
    }

    public void ToggleUnitMenu()
    {
        if (unitButtonPanel == null) return;

        bool next = !unitButtonPanel.activeSelf;
        unitButtonPanel.SetActive(next);

        // 열었을 때만, 마우스를 뗄 때까지 바깥 클릭 판정 잠시 막기
        if (next)
        {
            blockOutsideCloseUntilMouseRelease = true;
        }
    }

    public void ShowUnitMenu()
    {
        if (unitButtonPanel == null) return;

        unitButtonPanel.SetActive(true);
        blockOutsideCloseUntilMouseRelease = true;
    }

    public void HideUnitMenu()
    {
        if (unitButtonPanel == null) return;

        unitButtonPanel.SetActive(false);
        blockOutsideCloseUntilMouseRelease = false;
    }

    void HandleOutsideClick()
    {
        if (unitButtonPanel == null) return;
        if (!unitButtonPanel.activeSelf) return;
        if (Mouse.current == null) return;

        // 방금 메뉴를 열었다면, 마우스를 완전히 뗄 때까지 닫기 금지
        if (blockOutsideCloseUntilMouseRelease)
        {
            if (!Mouse.current.leftButton.isPressed)
            {
                blockOutsideCloseUntilMouseRelease = false;
            }
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        // 소환 버튼 또는 직업 버튼 패널 위 클릭이면 닫지 않음
        if (IsPointerOverSummonUI())
            return;

        // 그 외 클릭이면 닫기
        HideUnitMenu();
    }

    bool IsPointerOverSummonUI()
    {
        if (EventSystem.current == null || Mouse.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == null) continue;

            // 소환 버튼 자신 또는 자식
            if (summonButton != null)
            {
                Transform summonTransform = summonButton.transform;
                if (result.gameObject.transform == summonTransform || result.gameObject.transform.IsChildOf(summonTransform))
                {
                    return true;
                }
            }

            // 직업 버튼 패널 자신 또는 자식
            if (unitButtonPanel != null)
            {
                Transform panelTransform = unitButtonPanel.transform;
                if (result.gameObject.transform == panelTransform || result.gameObject.transform.IsChildOf(panelTransform))
                {
                    return true;
                }
            }
        }

        return false;
    }
}