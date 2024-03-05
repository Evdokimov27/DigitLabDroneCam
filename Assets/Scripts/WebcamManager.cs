using UnityEngine;
using OpenCvSharp;
using System.Collections.Generic;
using UnityEngine.Diagnostics;

public class WebcamManager : MonoBehaviour
{
    private List<WebCamTexture> webCamTextures = new List<WebCamTexture>();
    private List<Mat> webCamMats = new List<Mat>();
    public bool droidCam;
    public int NumberOfCameras;

    void Awake()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        List<WebCamDevice> desiredCameras = FindCameraByName(devices, "USB2.0 PC CAMERA");
        if (droidCam)
        {
            desiredCameras.AddRange(FindCameraByName(devices, "Web-camera KQ4M3FA1"));
            desiredCameras.AddRange(FindCameraByName(devices, "DroidCam Source 3"));
        }
        InitializeCameras(desiredCameras);

    }

    private void InitializeCameras(List<WebCamDevice> cameras)
    {
        foreach (WebCamDevice camera in cameras)
        {
            WebCamTexture webcamTexture = new WebCamTexture(camera.name);
            webCamTextures.Add(webcamTexture);
            webcamTexture.Play();

            webCamMats.Add(new Mat(webcamTexture.height, webcamTexture.width, MatType.CV_8UC3));
        }
    }

    public Mat GetWebCamMat(int index)
    {
        if (index >= 0 && index < webCamTextures.Count)
        {
            webCamMats[index] = UnityCV.TextureToMat(webCamTextures[index]);
            return webCamMats[index];
        }
        return null;
    }

    private List<WebCamDevice> FindCameraByName(WebCamDevice[] devices, string cameraName)
    {

        List<WebCamDevice> listCam = new List<WebCamDevice>();
        foreach (var device in devices)
        {
            if (device.name.Contains(cameraName))
            {
                listCam.Add(device);
            }
        }
        return listCam;
    }
}

