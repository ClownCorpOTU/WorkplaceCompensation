using UnityEngine;
using System;
using System.IO;

public class ScreenshotOnKey : MonoBehaviour
{
    [Header("Key to capture screenshot")]
    public KeyCode screenshotKey = KeyCode.F5;

    [Header("Folder name inside the game's data path")]
    public string folderName = "Screenshots";

    [Header("Image settings")]
    public string filePrefix = "screenshot_"; 
    public int superSize = 1; // 1 = normal res, 2 = double res, etc.

    void Update()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            TakeScreenshot();
        }
    }

    void TakeScreenshot()
    {
        // Create folder if missing
        string folderPath = Path.Combine(Application.persistentDataPath, folderName);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // Filename with timestamp
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{filePrefix}{timestamp}.png";

        string fullPath = Path.Combine(folderPath, fileName);

        // Take screenshot
        ScreenCapture.CaptureScreenshot(fullPath, superSize);

        Debug.Log($"📸 Screenshot saved to: {fullPath}");
        
        AudioManager.instance.Play("UI_Screenshot");
    }
}