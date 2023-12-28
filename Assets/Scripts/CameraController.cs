using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public WebCamTexture[] webCamTexture;
    public Texture2D[] textures;
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        webCamTexture = new WebCamTexture[devices.Length];
        textures = new Texture2D[devices.Length];
        WebCamDevice[] desiredCamera = FindCameraByName(devices, "USB2.0 PC CAMERA").ToArray();
        if (desiredCamera.Length > 0)
        {
            for (int index = 0; index < desiredCamera.Length; index++)
            {
                Debug.Log(desiredCamera[0].name);

                webCamTexture[index] = new WebCamTexture(desiredCamera[index].name);
                webCamTexture[index].Play();
            }
        }
    }

    private List<WebCamDevice> FindCameraByName(WebCamDevice[] devices, string cameraName)
    {
        List<WebCamDevice> listCam = new List<WebCamDevice>();
        foreach (var device in devices)
        {
            if (device.name.Contains(cameraName))
            {
                Debug.Log(device.name);
                listCam.Add(device);
            }
        }

        return listCam;
    }

    void Update()
    {
        // Обновление текстуры с изображением с камеры
        if (webCamTexture.Length > 0)
        {
            textures[0] = new Texture2D(webCamTexture[0].width, webCamTexture[0].height);
            textures[0].SetPixels(webCamTexture[0].GetPixels());
            textures[0].Apply();
        }
    }
}
