using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionSetter : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Text fullScreenText;

    private List<Resolution> resolutions;

    void Start()
    {
        resolutions = new List<Resolution>(Screen.resolutions);
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        List<string> options = new List<string>();
        HashSet<string> seenResolutions = new HashSet<string>();

        foreach (var resolution in resolutions)
        {
            float aspectRatio = (float)resolution.width / resolution.height;
            // Проверяем, соответствует ли разрешение соотношению сторон 16:9
            if (Mathf.Approximately(aspectRatio, 16f / 9f))
            {
                string option = resolution.width + " x " + resolution.height;
                // Проверяем, не было ли такого разрешения ранее
                if (!seenResolutions.Contains(option))
                {
                    options.Add(option);
                    seenResolutions.Add(option);

                    // Проверяем, является ли это текущим разрешением экрана
                    if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = options.Count - 1;
                    }
                }
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int resolutionIndex)
    {
        string[] dimensions = resolutionDropdown.options[resolutionIndex].text.Split('x');
        int width = int.Parse(dimensions[0].Trim());
        int height = int.Parse(dimensions[1].Trim());
        Screen.SetResolution(width, height, Screen.fullScreen);
    }
    bool IsFullScreen()
    {
        return Screen.fullScreen;
    }
    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        if(!IsFullScreen()) fullScreenText.text = "Полноэкранный режим: вкл.";
        if(IsFullScreen()) fullScreenText.text = "Полноэкранный режим: выкл.";
        Debug.Log(IsFullScreen());
    }
}
