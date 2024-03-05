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
    List<KeyCode> keys = new List<KeyCode>() {
    KeyCode.F1,
    KeyCode.F2,
    KeyCode.F3,
    KeyCode.F4,
    KeyCode.F5,
    KeyCode.F6,
    KeyCode.F7,
    KeyCode.F8,
};
    void Start()
    {
        correctImg = new Image[webcamManager.NumberOfCameras];
        markerDetected = new SerializableDictionary[webcamManager.NumberOfCameras];
        idsCurrent = new int[markerCount];

        for (int i = 0; i < webcamManager.NumberOfCameras; i++)
        {
            markerDetected[i] = new SerializableDictionary();
            correctImg[i] = raceManager.results[i].camObject.transform.GetChild(5).GetComponent<Image>();

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
        try


        {
            if (raceManager.raceInProgress && raceManager?.results[0]?.Finished == false && raceManager.results.Length > 0 && Input.GetKeyDown(keys[0]))
            {
                correctImg[0].color = Color.green;


                raceManager.Check(0);
                if (raceManager.raceType == TypeRace.Circuit)
                {
                    if (int.TryParse(raceManager.markerOnCircul.text, out int markerOnCircul))
                    {
                        if (raceManager.results[0].resultTime.Count % markerOnCircul == 0)
                        {
                            raceManager.results[0].marker++;
                            raceManager.results[0].circleTime.Add(raceManager.results[0].allTime);
                            raceManager.results[0].raceText.text = $"Пройдено кругов: {raceManager.results[0].marker}/{raceManager.markerCount.text}\n";
                            raceManager.results[0].camTime.text = raceManager.results[0].GetTimeSummary();
                        }
                    }
                }
                if (raceManager.raceType == TypeRace.Sprint)
                {
                    raceManager.results[0].marker++;
                    raceManager.results[0].raceText.text = $"Пройдено меток: {raceManager.results[0].marker}/{raceManager.markerCount.text}";
                }
                correctImg[0].enabled = false;
            }
            if (raceManager.raceInProgress && raceManager?.results[1]?.Finished == false && raceManager.results.Length > 1 && raceManager.results[1] != null && Input.GetKeyDown(keys[1]))
            {
                correctImg[1].color = Color.green;


                raceManager.Check(1);
                if (raceManager.raceType == TypeRace.Circuit)
                {
                    if (int.TryParse(raceManager.markerOnCircul.text, out int markerOnCircul))
                    {
                        if (raceManager.results[1].resultTime.Count % markerOnCircul == 0)
                        {
                            raceManager.results[1].marker++;
                            raceManager.results[1].circleTime.Add(raceManager.results[1].allTime);
                            raceManager.results[1].raceText.text = $"Пройдено кругов: {raceManager.results[1].marker}/{raceManager.markerCount.text}\n";
                            raceManager.results[1].camTime.text = raceManager.results[1].GetTimeSummary();
                        }
                    }
                }
                if (raceManager.raceType == TypeRace.Sprint)
                {
                    raceManager.results[1].marker++;
                    raceManager.results[1].raceText.text = $"Пройдено меток: {raceManager.results[1].marker}/{raceManager.markerCount.text}";
                }
                correctImg[1].enabled = false;
            }
            if (raceManager.raceInProgress && raceManager?.results[2]?.Finished == false && raceManager.results.Length > 2 && Input.GetKeyDown(keys[2]))
            {
                correctImg[2].color = Color.green;


                raceManager.Check(2);
                if (raceManager.raceType == TypeRace.Circuit)
                {
                    if (int.TryParse(raceManager.markerOnCircul.text, out int markerOnCircul))
                    {
                        if (raceManager.results[2].resultTime.Count % markerOnCircul == 0)
                        {
                            raceManager.results[2].marker++;
                            raceManager.results[2].circleTime.Add(raceManager.results[2].allTime);
                            raceManager.results[2].raceText.text = $"Пройдено кругов: {raceManager.results[2].marker}/{raceManager.markerCount.text}\n";
                            raceManager.results[2].camTime.text = raceManager.results[2].GetTimeSummary();
                        }
                    }
                }
                if (raceManager.raceType == TypeRace.Sprint)
                {
                    raceManager.results[2].marker++;
                    raceManager.results[2].raceText.text = $"Пройдено меток: {raceManager.results[2].marker}/{raceManager.markerCount.text}";
                }
                correctImg[2].enabled = false;
            }
            if (raceManager.raceInProgress && raceManager?.results[3]?.Finished == false && raceManager.results.Length > 3 && Input.GetKeyDown(keys[3]))
            {
                correctImg[3].color = Color.green;


                raceManager.Check(3);
                if (raceManager.raceType == TypeRace.Circuit)
                {
                    if (int.TryParse(raceManager.markerOnCircul.text, out int markerOnCircul))
                    {
                        if (raceManager.results[3].resultTime.Count % markerOnCircul == 0)
                        {
                            raceManager.results[3].marker++;
                            raceManager.results[3].circleTime.Add(raceManager.results[3].allTime);
                            raceManager.results[3].raceText.text = $"Пройдено кругов: {raceManager.results[3].marker}/{raceManager.markerCount.text}\n";
                            raceManager.results[3].camTime.text = raceManager.results[3].GetTimeSummary();
                        }
                    }
                }
                if (raceManager.raceType == TypeRace.Sprint)
                {
                    raceManager.results[3].marker++;
                    raceManager.results[3].raceText.text = $"Пройдено меток: {raceManager.results[3].marker}/{raceManager.markerCount.text}";
                }
                correctImg[3].enabled = false;
            }
            if (raceManager.raceInProgress && raceManager?.results[4]?.Finished == false && raceManager.results.Length > 4 && Input.GetKeyDown(keys[4]))
            {
                correctImg[4].color = Color.green;


                raceManager.Check(4);
                if (raceManager.raceType == TypeRace.Circuit)
                {
                    if (int.TryParse(raceManager.markerOnCircul.text, out int markerOnCircul))
                    {
                        if (raceManager.results[4].resultTime.Count % markerOnCircul == 0)
                        {
                            raceManager.results[4].marker++;
                            raceManager.results[4].circleTime.Add(raceManager.results[4].allTime);
                            raceManager.results[4].raceText.text = $"Пройдено кругов: {raceManager.results[4].marker}/{raceManager.markerCount.text}\n";
                            raceManager.results[4].camTime.text = raceManager.results[4].GetTimeSummary();
                        }
                    }
                }
                if (raceManager.raceType == TypeRace.Sprint)
                {
                    raceManager.results[4].marker++;
                    raceManager.results[4].raceText.text = $"Пройдено меток: {raceManager.results[4].marker}/{raceManager.markerCount.text}";
                }
                correctImg[4].enabled = false;
            }
            if (raceManager.raceInProgress && raceManager?.results[5]?.Finished == false && raceManager.results.Length > 5 && Input.GetKeyDown(keys[5]))
            {
                correctImg[5].color = Color.green;


                raceManager.Check(5);
                if (raceManager.raceType == TypeRace.Circuit)
                {
                    if (int.TryParse(raceManager.markerOnCircul.text, out int markerOnCircul))
                    {
                        if (raceManager.results[5].resultTime.Count % markerOnCircul == 0)
                        {
                            raceManager.results[5].marker++;
                            raceManager.results[5].circleTime.Add(raceManager.results[5].allTime);
                            raceManager.results[5].raceText.text = $"Пройдено кругов: {raceManager.results[5].marker}/{raceManager.markerCount.text}\n";
                            raceManager.results[5].camTime.text = raceManager.results[5].GetTimeSummary();
                        }
                    }
                }
                if (raceManager.raceType == TypeRace.Sprint)
                {
                    raceManager.results[5].marker++;
                    raceManager.results[5].raceText.text = $"Пройдено меток: {raceManager.results[5].marker}/{raceManager.markerCount.text}";
                }
                correctImg[5].enabled = false;
            }
            if (raceManager.raceInProgress && raceManager?.results[6]?.Finished == false && raceManager.results.Length > 6 && Input.GetKeyDown(keys[6]))
            {
                correctImg[6].color = Color.green;


                raceManager.Check(6);
                if (raceManager.raceType == TypeRace.Circuit)
                {
                    if (int.TryParse(raceManager.markerOnCircul.text, out int markerOnCircul))
                    {
                        if (raceManager.results[6].resultTime.Count % markerOnCircul == 0)
                        {
                            raceManager.results[6].marker++;
                            raceManager.results[6].circleTime.Add(raceManager.results[6].allTime);
                            raceManager.results[6].raceText.text = $"Пройдено кругов: {raceManager.results[6].marker}/{raceManager.markerCount.text}\n";
                            raceManager.results[6].camTime.text = raceManager.results[6].GetTimeSummary();
                        }
                    }
                }
                if (raceManager.raceType == TypeRace.Sprint)
                {
                    raceManager.results[6].marker++;
                    raceManager.results[6].raceText.text = $"Пройдено меток: {raceManager.results[6].marker}/{raceManager.markerCount.text}";
                }
                correctImg[6].enabled = false;
            }
            if (raceManager.raceInProgress && raceManager?.results[7]?.Finished == false && raceManager.results.Length > 7 && Input.GetKeyDown(keys[7]))
            {
                correctImg[7].color = Color.green;


                raceManager.Check(7);
                if (raceManager.raceType == TypeRace.Circuit)
                {
                    if (int.TryParse(raceManager.markerOnCircul.text, out int markerOnCircul))
                    {
                        if (raceManager.results[7].resultTime.Count % markerOnCircul == 0)
                        {
                            raceManager.results[7].marker++;
                            raceManager.results[7].circleTime.Add(raceManager.results[7].allTime);
                            raceManager.results[7].raceText.text = $"Пройдено кругов: {raceManager.results[7].marker}/{raceManager.markerCount.text}\n";
                            raceManager.results[7].camTime.text = raceManager.results[7].GetTimeSummary();
                        }
                    }
                }
                if (raceManager.raceType == TypeRace.Sprint)
                {
                    raceManager.results[7].marker++;
                    raceManager.results[7].raceText.text = $"Пройдено меток: {raceManager.results[7].marker}/{raceManager.markerCount.text}";
                }
                correctImg[7].enabled = false;
            }
        }
        catch(Exception e)
		{

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
                dronesImages[droneIndex].markerImages[markerIndex] = raceManager.results[droneIndex].camObject.transform.GetChild(6).GetChild(markerIndex).GetComponent<Image>();
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

                    raceManager.Check(cameraIndex);
                    if (raceManager.raceType == TypeRace.Circuit)
                    {
                        if (int.TryParse(raceManager.markerOnCircul.text, out int markerOnCircul))
                        {
                            if (raceManager.results[cameraIndex].resultTime.Count % markerOnCircul == 0)
                            {
                                raceManager.results[cameraIndex].marker++;
                                raceManager.results[cameraIndex].circleTime.Add(raceManager.results[cameraIndex].allTime);
                                raceManager.results[cameraIndex].raceText.text = $"Пройдено кругов: {raceManager.results[cameraIndex].marker}/{raceManager.markerCount.text}\n";
                                raceManager.results[cameraIndex].camTime.text = raceManager.results[cameraIndex].GetTimeSummary();
                            }
                        }
                    }
                    if (raceManager.raceType == TypeRace.Sprint)
                    {
                        raceManager.results[cameraIndex].marker++;
                        raceManager.results[cameraIndex].raceText.text = $"Пройдено меток: {raceManager.results[cameraIndex].marker}/{raceManager.markerCount.text}";
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
