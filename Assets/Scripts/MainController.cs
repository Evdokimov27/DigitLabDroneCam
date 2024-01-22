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
    private Dictionary<int, bool>[] markerDetected;

    void Start()
    {
        markerDetected = new Dictionary<int, bool>[webcamManager.NumberOfCameras];
    }


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
                markerDetectionManager.ProcessFrame(i, frame, markerDetectionManager.markerDetected[i].dictionary);

                if (raceManager.results[i].resultTime.Count > 0)
                {
                    raceManager.results[i].camTime.text = raceManager.results[i].GetTimeSummary();
                }
                Resources.UnloadUnusedAssets();
            }
        }
    }
}
