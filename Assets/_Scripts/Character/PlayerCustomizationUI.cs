using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerCustomizationUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Image colorPreviewImage;
    
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
    }

    private void Start()
    {
        // Load saved data when the menu opens
        currentName = PlayerPrefs.GetString("PlayerName", "JOHN");
        nameInputField.text = currentName;
        
        string savedColor = PlayerPrefs.GetString("PlayerColor", "#FFFFFF");
        if (ColorUtility.TryParseHtmlString(savedColor, out Color color))
        {
            currentColor = color;
            if (colorPreviewImage != null) colorPreviewImage.color = currentColor;
        }
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

    public void SaveCustomization()
    {
        PlayerPrefs.SetString("PlayerName", currentName);
        PlayerPrefs.SetString("PlayerColor", hexColor);
        PlayerPrefs.Save();
    }
}