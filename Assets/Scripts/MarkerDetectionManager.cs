using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.XR.ARFoundation;
using TMPro;
using System;
using UnityEngine.UI;

public class MarkerDetectionManager : MonoBehaviour
{
    private DetectorParameters detectorParameters;
    private Dictionary dictionary;
    private bool isCoroutineRunning = false;
    private Dictionary<int, Dictionary<int, double>> lastMarkerDetectionTimes = new Dictionary<int, Dictionary<int, double>>();
    public float markerClearTime;
    int result = 0;
    public UIManager uiManager;
    public WebcamManager webcamManager;
    public SerializableDictionary[] markerDetected;
    public int markerCount;
    public RaceManager raceManager = new RaceManager();
    public TMP_InputField textClear;
    public DroneImages[] dronesImages;
    public TMP_InputField textSpeed;
    void Start()
    {
        markerDetected = new SerializableDictionary[webcamManager.NumberOfCameras];
        for (int i = 0; i < webcamManager.NumberOfCameras; i++)
        {
            markerDetected[i] = new SerializableDictionary();
        }
        detectorParameters = DetectorParameters.Create();
        dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250);
        InitializeDronesImages();
    }

    public void ProcessFrame(int cameraIndex, Mat frame, Dictionary<int, bool> markerDetected)
    {
        if (frame.Channels() == 3 || frame.Channels() == 4)
        {
            Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
            CvAruco.DetectMarkers(frame, dictionary, out Point2f[][] corners, out int[] ids, detectorParameters, out _);
            CvAruco.DrawDetectedMarkers(frame, corners, ids);
            UpdateMarkerStatus(cameraIndex, ids);
        }
    }
    private void Update()
    {
        if (float.TryParse(textClear.text, out float number))
        {
            markerClearTime = number / 1000;
        }
    }
    private void UpdateMarkerStatus(int cameraIndex, int[] ids)
    {
        Dictionary<int, bool> markerDetectedDict = markerDetected[cameraIndex].dictionary;

        if (!lastMarkerDetectionTimes.ContainsKey(cameraIndex))
        {
            lastMarkerDetectionTimes[cameraIndex] = new Dictionary<int, double>();
        }

        foreach (int id in ids)
        {
            if (!markerDetectedDict.ContainsKey(id))
            {
                markerDetectedDict[id] = false;
            }
            if (!lastMarkerDetectionTimes[cameraIndex].ContainsKey(id))
            {
                lastMarkerDetectionTimes[cameraIndex][id] = 0;
            }

            markerDetectedDict[id] = true;
            lastMarkerDetectionTimes[cameraIndex][id] = Time.time;
        }

        foreach (var key in new List<int>(markerDetectedDict.Keys))
        {
            if (!lastMarkerDetectionTimes[cameraIndex].ContainsKey(key))
            {
                lastMarkerDetectionTimes[cameraIndex][key] = 0;
            }

            if (Time.time - lastMarkerDetectionTimes[cameraIndex][key] >= markerClearTime)
            {
                markerDetectedDict[key] = false;
            }
        }
        if (!isCoroutineRunning && markerDetectedDict.Values.All(x => x) && markerDetected[cameraIndex].dictionary.Keys.Count > markerCount-1)
        {
            StartCoroutine(CheckMarkStatusAfterDelay(cameraIndex, markerDetectedDict));
        }
        UpdateDroneImages(cameraIndex, markerDetected[cameraIndex].dictionary);

    }


    private void InitializeDronesImages()
    {
        dronesImages = new DroneImages[webcamManager.NumberOfCameras];
        for (int droneIndex = 0; droneIndex < webcamManager.NumberOfCameras; droneIndex++)
        {
            dronesImages[droneIndex] = new DroneImages
            {
                markerImages = new Image[markerCount]
            };

            for (int markerIndex = 0; markerIndex < markerCount; markerIndex++)
            {
                dronesImages[droneIndex].markerImages[markerIndex] = raceManager.results[droneIndex].camObject.transform.GetChild(2).GetChild(markerIndex).GetComponent<Image>();
            }
        }
    }
    private void UpdateDroneImages(int droneIndex, Dictionary<int, bool> markerDetectedDict)
    {
        for (int markerIndex = 0; markerIndex < dronesImages[droneIndex].markerImages.Length; markerIndex++)
        {
            bool isMarkerDetected = markerDetectedDict.ContainsKey(markerIndex) && markerDetectedDict[markerIndex];
            dronesImages[droneIndex].markerImages[markerIndex].color = isMarkerDetected ? Color.green : Color.red;
        }
    }

    IEnumerator CheckMarkStatusAfterDelay(int cameraIndex, Dictionary<int, bool> markerDetected)
    {
        if (float.TryParse(textSpeed.text, out float number))
        {
            isCoroutineRunning = true;
            yield return new WaitForSeconds(number / 1000);
            bool allMarksStillDetected = markerDetected.Values.All(x => x);
            if (allMarksStillDetected)
            {
                yield return new WaitForSeconds(markerClearTime);
                allMarksStillDetected = markerDetected.Values.All(x => !x);
                if (allMarksStillDetected)
                {
                    Debug.Log(cameraIndex + " Все маркеры обнаружены и очищены");
                    raceManager.Check(cameraIndex);
                }
            }
        }
        isCoroutineRunning = false;
    }
}
[Serializable]
public class DroneImages
{
    public Image[] markerImages;
}
