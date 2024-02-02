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
using Unity.Mathematics;

public class MarkerDetectionManager : MonoBehaviour
{
    private DetectorParameters detectorParameters;
    private Dictionary dictionary;
    public int[] idsCurrent;
    private bool isCoroutineRunning = false;
    private Dictionary<int, Dictionary<int, double>> lastMarkerDetectionTimes = new Dictionary<int, Dictionary<int, double>>();
    public float markerClearTime = 0;
    public float delay = 0;
    public Image[] correctImg;
    int result = 0;
    public UIManager uiManager;
    public WebcamManager webcamManager;
    public SerializableDictionary[] markerDetected;
    public int markerCount;
    public RaceManager raceManager;
    public TMP_InputField markerCleadDelay;
    public DroneImages[] dronesImages;
    public TMP_InputField checkDelay;
    private int[][] requiredMarkerSets = new int[][]
    {
    new int[] {0, 3, 5},
    new int[] {1, 4, 6},
    new int[] {2, 5, 7},
    new int[] {3, 0, 6},
    new int[] {4, 1, 7},
    new int[] {5, 2, 0},
    new int[] {6, 3, 1},
    new int[] {7, 4, 2},
    };
    void Start()
    {
        correctImg = new Image[webcamManager.NumberOfCameras];
        markerDetected = new SerializableDictionary[webcamManager.NumberOfCameras];
        idsCurrent = new int[markerCount];

        for (int i = 0; i < webcamManager.NumberOfCameras; i++)
        {
            markerDetected[i] = new SerializableDictionary();
            correctImg[i] = raceManager.results[i].camObject.transform.GetChild(4).GetComponent<Image>();

        }
        detectorParameters = DetectorParameters.Create();
        dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_100);
        InitializeDronesImages();
    }

    public void ProcessFrame(int cameraIndex, Mat frame)
    {
        if (frame.Channels() == 3 || frame.Channels() == 4)
        {
            Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
            CvAruco.DetectMarkers(frame, dictionary, out Point2f[][] corners, out int[] ids, detectorParameters, out _);
            CvAruco.DrawDetectedMarkers(frame, corners, ids);

            // Устанавливаем текстуру, чтобы увидеть результат
            UpdateMarkerStatus(cameraIndex, ids);
        }
    }
    private void Update()
    {
        if (float.TryParse(markerCleadDelay.text, out float number))
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
        if (!isCoroutineRunning && ids.Length > 0)
        {
            StartCoroutine(CheckMarkStatusAfterDelay(cameraIndex, markerDetectedDict));
        }
        else if (!isCoroutineRunning) correctImg[cameraIndex].enabled = false;
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
                dronesImages[droneIndex].markerImages[markerIndex] = raceManager.results[droneIndex].camObject.transform.GetChild(3).GetChild(markerIndex).GetComponent<Image>();
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
        if (float.TryParse(checkDelay.text, out float number))
        {
            delay = number / 1000;
            isCoroutineRunning = true;
            bool allRequiredMarkersDetected = false;

            foreach (var markerSet in requiredMarkerSets)
            {
                allRequiredMarkersDetected = markerSet.All(markerId => markerDetected.ContainsKey(markerId) && markerDetected[markerId]);
                if (allRequiredMarkersDetected)
                {
                    break;
                }
            }

            if (allRequiredMarkersDetected)
            {
                correctImg[cameraIndex].enabled = true;
                correctImg[cameraIndex].color = Color.yellow;
                yield return new WaitForSeconds(delay);


                bool zeroMarkersVisible = markerDetected.Values.All(x => !x);

                if (zeroMarkersVisible)
                {
                    correctImg[cameraIndex].color = Color.green;
                    Debug.Log(cameraIndex + " Все маркеры обнаружены и очищены");

                    raceManager.Check(cameraIndex);
                    if (raceManager.raceType == TypeRace.Circuit)
                    {
                        if (int.TryParse(raceManager.markerOnCircul.text, out int markerOnCircul))
                        {
                            if (raceManager.results[cameraIndex].resultTime.Count % markerOnCircul == 0)
                            {
                                raceManager.results[cameraIndex].marker++;
                                raceManager.results[cameraIndex].circleTime.Add(raceManager.results[cameraIndex].allTime);
                                raceManager.results[cameraIndex].camTime.text = $"Пройдено кругов: {raceManager.results[cameraIndex].marker}/{raceManager.markerCount.text}\nВремя кругов:\n";
                                raceManager.results[cameraIndex].camTime.text += raceManager.results[cameraIndex].GetTimeSummary();
                            }
                        }
                    }
                    if (raceManager.raceType == TypeRace.Sprint)
                    {
                        raceManager.results[cameraIndex].marker++;
                        raceManager.results[cameraIndex].camTime.text = $"Пройдено меток: {raceManager.results[cameraIndex].marker}/{raceManager.markerCount.text}";
                    }
                    yield return new WaitForSeconds(delay / 2);
                    correctImg[cameraIndex].enabled = false;
                }
                else correctImg[cameraIndex].enabled = false;

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
