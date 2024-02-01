using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    public WebcamManager webcamManager;
    public RaceManager raceManager;
    public UIManager uiManager;
    public MarkerDetectionManager markerDetectionManager;



    void Update()
    {
        uiManager.CreateCameraImages(webcamManager.NumberOfCameras);

        for (int i = 0; i < webcamManager.NumberOfCameras; i++)
        {
            Mat frame = webcamManager.GetWebCamMat(i);
            if (frame != null)
            {
                Texture cameraFrame = UnityCV.MatToTexture(frame);
                uiManager.UpdateCameraImage(i, cameraFrame);
                markerDetectionManager.ProcessFrame(i, frame);
                Resources.UnloadUnusedAssets();
            }
        }
    }
}
