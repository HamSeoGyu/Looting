using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class UnitInfoUI : MonoBehaviour
{
    public static UnitInfoUI Instance;

    [Header("Root")]
    public GameObject unitInfoPanel;
    public Canvas rootCanvas;

    [Header("Card Image")]
    public Image cardImage;
    public Sprite warriorCardSprite;
    public Sprite archerCardSprite;
    public Sprite mageCardSprite;
    public Sprite pyromancerCardSprite;
    public Sprite frostMageCardSprite;
    public Sprite swampShamanCardSprite;

    [Header("Texts")]
    public TextMeshProUGUI attackLabelText;
    public TextMeshProUGUI attackValueText;
    public TextMeshProUGUI upgradeCostLabelText;
    public TextMeshProUGUI upgradeCostValueText;

    [Header("Upgrade Button")]
    public Button upgradeButton;

    [Header("Follow Selected Unit")]
    public Vector3 worldOffset = new Vector3(0f, 3f, 0f);

    private UnitStats selectedUnit;

    private RectTransform panelRect;
    private RectTransform canvasRect;
    private RectTransform upgradeButtonRect;
    private Camera uiCamera;

    private bool blockOutsideCloseUntilMouseRelease = false;

    // 같은 프레임에 직접 클릭 감지 + Button OnClick 이 둘 다 들어와도
    // 강화가 2번 되지 않도록 막는 용도
    private int lastUpgradeFrame = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (unitInfoPanel != null)
            panelRect = unitInfoPanel.GetComponent<RectTransform>();

        if (upgradeButton != null)
            upgradeButtonRect = upgradeButton.GetComponent<RectTransform>();

        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null)
                rootCanvas = FindFirstObjectByType<Canvas>();
        }

        if (rootCanvas != null)
        {
            canvasRect = rootCanvas.GetComponent<RectTransform>();

            if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                uiCamera = null;
            else
                uiCamera = rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main;
        }
    }

    void Start()
    {
        HideUnit();
    }

    void LateUpdate()
    {
        if (selectedUnit != null)
        {
            if (!selectedUnit.gameObject.activeInHierarchy)
            {
                HideUnit();
                return;
            }

            RefreshUI();
            UpdatePanelPosition();
        }

        HandleMouseInput();
    }

    public void ShowUnit(UnitStats unit)
    {
        if (unit == null) return;

        // 같은 유닛 다시 선택이면 토글 닫기
        if (selectedUnit == unit && unitInfoPanel != null && unitInfoPanel.activeSelf)
        {
            HideUnit();
            return;
        }

        selectedUnit = unit;
        selectedUnit.InitializeIfNeeded();

        if (unitInfoPanel != null)
            unitInfoPanel.SetActive(true);

        RefreshUI();
        UpdatePanelPosition();

        // 유닛 클릭으로 창을 연 프레임의 같은 마우스 입력으로
        // 바로 닫히지 않게 막음
        blockOutsideCloseUntilMouseRelease = true;
    }

    public void HideUnit()
    {
        selectedUnit = null;

        if (unitInfoPanel != null)
            unitInfoPanel.SetActive(false);

        blockOutsideCloseUntilMouseRelease = false;
    }

    void RefreshUI()
    {
        if (selectedUnit == null) return;

        selectedUnit.InitializeIfNeeded();

        if (cardImage != null)
            cardImage.sprite = GetCardSprite(selectedUnit.unitType);

        if (attackLabelText != null)
            attackLabelText.text = "공격력:";

        if (attackValueText != null)
            attackValueText.text = selectedUnit.GetCurrentAttack().ToString();

        if (upgradeCostLabelText != null)
            upgradeCostLabelText.text = "강화비용:";

        if (upgradeCostValueText != null)
            upgradeCostValueText.text = selectedUnit.GetUpgradeCost().ToString();

        if (upgradeButton != null)
            upgradeButton.interactable = selectedUnit.CanUpgrade();
    }

    Sprite GetCardSprite(UnitStats.UnitType type)
    {
        switch (type)
        {
            case UnitStats.UnitType.Warrior:
                return warriorCardSprite;

            case UnitStats.UnitType.Archer:
                return archerCardSprite;

            case UnitStats.UnitType.Mage:
                return mageCardSprite;

            case UnitStats.UnitType.Pyromancer:
                return pyromancerCardSprite;

            case UnitStats.UnitType.FrostMage:
                return frostMageCardSprite;

            case UnitStats.UnitType.SwampShaman:
                return swampShamanCardSprite;
        }

        return null;
    }

    void UpdatePanelPosition()
    {
        if (selectedUnit == null) return;
        if (panelRect == null) return;
        if (canvasRect == null) return;
        if (Camera.main == null) return;

        Vector3 worldPos = selectedUnit.transform.position + worldOffset;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            uiCamera,
            out localPoint
        );

        panelRect.anchoredPosition = localPoint;
    }

    void HandleMouseInput()
    {
        if (unitInfoPanel == null || !unitInfoPanel.activeSelf)
            return;

        if (selectedUnit == null)
            return;

        if (blockOutsideCloseUntilMouseRelease)
        {
            if (!IsLeftMousePressed())
                blockOutsideCloseUntilMouseRelease = false;

            return;
        }

        if (!WasLeftMousePressedThisFrame())
            return;

        Vector2 mouseScreenPos = GetMouseScreenPosition();

        // 1. 강화 버튼 클릭이면 강화
        if (IsPointerInsideRect(upgradeButtonRect, mouseScreenPos))
        {
            TryUpgradeCurrentUnit();
            return;
        }

        // 2. 실제 보이는 UI 위를 눌렀으면 유지
        if (IsPointerOverVisibleUI(mouseScreenPos))
        {
            return;
        }

        // 3. 유닛 클릭 처리
        UnitStats clickedUnit = GetUnitUnderPointer();
        if (clickedUnit != null)
        {
            // 같은 유닛 다시 클릭 -> 닫기
            if (clickedUnit == selectedUnit)
            {
                HideUnit();
                return;
            }

            // 다른 유닛 클릭 -> 교체
            ShowUnit(clickedUnit);
            return;
        }

        // 4. 아무것도 아니면 닫기
        HideUnit();
    }

    bool IsPointerOverVisibleUI(Vector2 screenPos)
    {
        if (IsPointerInsideRect(upgradeButtonRect, screenPos))
            return true;

        if (cardImage != null)
        {
            RectTransform rect = cardImage.GetComponent<RectTransform>();
            if (IsPointerInsideRect(rect, screenPos))
                return true;
        }

        if (attackLabelText != null)
        {
            RectTransform rect = attackLabelText.GetComponent<RectTransform>();
            if (IsPointerInsideRect(rect, screenPos))
                return true;
        }

        if (attackValueText != null)
        {
            RectTransform rect = attackValueText.GetComponent<RectTransform>();
            if (IsPointerInsideRect(rect, screenPos))
                return true;
        }

        if (upgradeCostLabelText != null)
        {
            RectTransform rect = upgradeCostLabelText.GetComponent<RectTransform>();
            if (IsPointerInsideRect(rect, screenPos))
                return true;
        }

        if (upgradeCostValueText != null)
        {
            RectTransform rect = upgradeCostValueText.GetComponent<RectTransform>();
            if (IsPointerInsideRect(rect, screenPos))
                return true;
        }

        return false;
    }

    bool IsPointerInsideRect(RectTransform rect, Vector2 screenPos)
    {
        if (rect == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, uiCamera);
    }

    UnitStats GetUnitUnderPointer()
    {
        if (Camera.main == null) return null;

        Vector2 mouseScreen = GetMouseScreenPosition();
        Vector3 world = Camera.main.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, Mathf.Abs(Camera.main.transform.position.z))
        );
        world.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll(world);
        if (hits == null || hits.Length == 0) return null;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            UnitStats unit = hit.GetComponent<UnitStats>();
            if (unit == null) unit = hit.GetComponentInParent<UnitStats>();
            if (unit == null) unit = hit.GetComponentInChildren<UnitStats>();

            if (unit != null)
                return unit;
        }

        return null;
    }

    Vector2 GetMouseScreenPosition()
    {
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return Input.mousePosition;
    }

    bool WasLeftMousePressedThisFrame()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.wasPressedThisFrame;

        return Input.GetMouseButtonDown(0);
    }

    bool IsLeftMousePressed()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.isPressed;

        return Input.GetMouseButton(0);
    }

    void TryUpgradeCurrentUnit()
    {
        if (selectedUnit == null) return;

        // 같은 프레임 중복 강화 방지
        if (lastUpgradeFrame == Time.frameCount)
            return;

        lastUpgradeFrame = Time.frameCount;

        // 버튼이 비활성 상태면 강화 안 함
        if (upgradeButton != null && !upgradeButton.interactable)
            return;

        bool upgraded = selectedUnit.TryUpgrade();
        if (upgraded)
            RefreshUI();
    }

    // Inspector의 Button OnClick에 연결해도 되고, 비워둬도 됩니다.
    public void OnUpgradeButtonClicked()
    {
        TryUpgradeCurrentUnit();
    }
}