using UnityEngine;
using TMPro;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    [Header("Gold")]
    public int startGold = 200;
    public int currentGold;

    [Header("UI")]
    public TextMeshProUGUI goldText;

    public int CurrentGold => currentGold;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        currentGold = startGold;
        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        currentGold += amount;
        UpdateGoldUI();
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;

        if (currentGold < amount)
        {
            return false;
        }

        currentGold -= amount;
        UpdateGoldUI();
        return true;
    }

    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    public void UpdateGoldText()
    {
        UpdateGoldUI();
    }

    void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = currentGold.ToString();
        }
    }
}