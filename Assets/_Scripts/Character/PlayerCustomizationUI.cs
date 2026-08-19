using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerCustomizationUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Image colorPreviewImage;
    [SerializeField] private TextMeshProUGUI coinValue;
    
    private string currentName = "JOHN";
    private Color currentColor = Color.white;
    private string hexColor;
    
    private string[] bannedWords;

    
    private void Awake()
    {
        // Load and cache banned words once at startup
        TextAsset bannedWordsFile = Resources.Load<TextAsset>("BannedNames");
        if (bannedWordsFile != null && !string.IsNullOrEmpty(bannedWordsFile.text))
        {
            bannedWords = bannedWordsFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            Debug.LogWarning("BannedNames check skipped (File not found or empty).");
            bannedWords = Array.Empty<string>();
        }

        LoadSaveData();
        
        
        // --- TEMPORARY HACK FOR TESTING ---
        // Load our inventory
        PlayerInventory testInv = LocalPlayerInventoryManager.LoadInventory();

        // Force equip Hat ID #1 (Make sure you set your Hat ScriptableObject's ItemID to 1!)
        testInv.EquippedItemIDs[0] = 3; 

        // Save it back to PlayerPrefs
        LocalPlayerInventoryManager.SaveInventory(testInv);
        // ----------------------------------

    }

    private void LoadSaveData()
    {
        currentName = PlayerPrefs.GetString(Utils.GetKey("PlayerName"), "John");
        nameInputField.text = currentName;
        
        string savedColor = PlayerPrefs.GetString(Utils.GetKey("PlayerColor"), "#FFFFFF");
        if (ColorUtility.TryParseHtmlString(savedColor, out Color color))
        {
            currentColor = color;
            if (colorPreviewImage != null) colorPreviewImage.color = currentColor;
        }

        if (coinValue != null)
            coinValue.text = LocalEconomyManager.GetCoins().ToString();
    }

    public void OnNameEndEdit(string newName)
    {
        string lowerName = newName.ToLower();
        bool isBanned = false;
        foreach (string word in bannedWords)
        {
            if (!string.IsNullOrWhiteSpace(word) && lowerName.Contains(word.ToLower()))
            {
                isBanned = true;
                break;
            }
        }
        if (isBanned)
        {
            newName = "BAD!";
            nameInputField.text = "BAD!";
        }
        currentName = newName;
    }
    
    public void OnColorSelected(Image buttonImage)
    {
        currentColor = buttonImage.color;
        
        if (colorPreviewImage != null) colorPreviewImage.color = currentColor;
        
        // Save ONLY the color here
        hexColor = "#" + ColorUtility.ToHtmlStringRGB(currentColor);
    }

    // Called when user closes the menu
    public void SaveCustomization()
    {
        PlayerPrefs.SetString(Utils.GetKey("PlayerName"), currentName);
        PlayerPrefs.SetString(Utils.GetKey("PlayerColor"), hexColor);
        PlayerPrefs.Save();
    }
}