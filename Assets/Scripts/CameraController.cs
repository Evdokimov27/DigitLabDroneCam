using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using TMPro;
using System.Linq;
using OpenCvSharp.Demo;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using OpenCvSharp.Flann;

//* Created by Evdokimov # ObederTeam # DigitLab *//
[Serializable]
public class CameraController : MonoBehaviour
{
    public List<ResultRaces> result = new List<ResultRaces>();
    public SettingsRace settings;
    public TMP_Text timer;
    public TMP_Text errorText;
    public bool droidCam;
    public GameObject prefabDrone;
    public Transform gridLayout;
    public float markerClearTime;
    public Dictionary<int, bool> markerDetected;
    public bool[] markerDetectedVisible;
    public bool checkTrue;
    public RaceStage raceStage = RaceStage.NotStarted;
    public GameObject[] camObject;
    public TMP_Text[] camName;
    public TMP_Text[] camTime;

    private List<WebCamTexture> webCamTexture;
    private Point2f[][] corners;
    private int[] ids;
    private Point2f[][] rejectedImgPoints;
    private DetectorParameters detectorParameters = DetectorParameters.Create();
    private Dictionary dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250);
    private bool spawned;
    private bool endStart = false;

    private List<Dictionary<int, double>> lastMarkerDetectionTimes = new List<Dictionary<int, double>>();
    private Mat[] mat;
    private Mat[] grayMat;
    private bool isCoroutineRunning = false;
    private bool resultsShown = false;

    public DateTime startTime;
    public TimeSpan elapsedTime;
    public double currentElapsedTime;

    void Start()
    {
        settings.circles = 0;
        WebCamDevice[] devices = WebCamTexture.devices;
        List<WebCamDevice> desiredCamera = new List<WebCamDevice>();
        foreach (WebCamDevice device in FindCameraByName(devices, "USB2.0 PC CAMERA"))
        {
            desiredCamera.Add(device);
        }
        if (droidCam && FindCameraByName(devices, "DroidCam Source 3").Count > 0)
        {
            desiredCamera.Add(FindCameraByName(devices, "DroidCam Source 3")[0]);
        }
        

        webCamTexture = new List<WebCamTexture>();
        camObject = new GameObject[desiredCamera.Count];
        camName = new TMP_Text[desiredCamera.Count];
        camTime = new TMP_Text[desiredCamera.Count];
        markerDetected = new Dictionary<int, bool>();
        mat = new Mat[desiredCamera.Count];
        grayMat = new Mat[desiredCamera.Count];
        Dictionary<int, double> lastMarkerDetectionTime = new Dictionary<int, double>();

        for (int i = 0; i < settings.marker; i++)
        {

            markerDetected.Add(i, false);
            lastMarkerDetectionTime.Add(i, 0d);

        }
        if (desiredCamera.Count > 0)
        {
            for (int index = 0; index < desiredCamera.Count; index++)
            {
                webCamTexture.Add(new WebCamTexture(desiredCamera[index].name));
                webCamTexture[index].Play();

                lastMarkerDetectionTimes.Add(lastMarkerDetectionTime);
            }

            for (int index = 0; index < webCamTexture.Count; index++)
            {
                if (camObject[index] == null)
                {
                    camObject[index] = Instantiate(prefabDrone);
                    camObject[index].transform.SetParent(gridLayout);
                    camObject[index].gameObject.transform.localScale = new Vector3(1, 1, 1);
                    result.Add(camObject[index].GetComponent<Drone>().result);
                    camTime[index] = camObject[index].transform.GetChild(1).GetComponent<TMP_Text>();
                    spawned = true;
                }

                if (camObject[index] != null)
                {
                    UpdateImage(index);
                }
            }
            markerDetectedVisible = new bool[markerDetected.Keys.Count];

        }
    }

    IEnumerator StartCountdown()
    {
        var timerVar = settings.timerStart;
        if (!endStart)
        {
            for (; timerVar > 0; timerVar--)
            {
                timer.text = timerVar.ToString();
                yield return new WaitForSeconds(1);
            }
            timer.text = "Полетел!";
            yield return new WaitForSeconds(1);
            endStart = true;
            startTime = DateTime.Now;
            raceStage = RaceStage.Countdown;
        }
        raceStage = RaceStage.Racing;
        while (endStart)
        {
            elapsedTime = DateTime.Now - startTime;
            currentElapsedTime = elapsedTime.TotalSeconds;
            timer.text = elapsedTime.ToString(@"hh\:mm\:ss\:fff");
            if (raceStage == RaceStage.Finished)
            {
                break;
            }
            yield return null;
        }
    }

    void UpdateImage(int index)
    {
            if (camObject[index] != null)
            {
                webCamTexture[index].deviceName = "Дрон " + (index + 1);
                camObject[index].name = webCamTexture[index].deviceName;
            }

            camName[index] = camObject[index].GetComponentInChildren<TMP_Text>();
            mat[index] = UnityCV.TextureToMat(webCamTexture[index]);
            grayMat[index] = new Mat();
            if (endStart) CheckMarkAsync(index);

            camObject[index].GetComponent<RawImage>().texture = UnityCV.MatToTexture(mat[index]);
            Resources.UnloadUnusedAssets();
        
    }

    void Update()
    {

        if (Input.GetKeyUp(KeyCode.Space))
        {
            switch (raceStage)
            {
                case RaceStage.NotStarted:
                    StartCoroutine(StartCountdown());
                    break;
                case RaceStage.Racing:
                    ShowResults();
                    break;
                case RaceStage.Finished:
                    RestartRace();
                    break;
            }
        }

        void ShowResults()
        {
            if (!resultsShown)
            {
                // ... (код для вывода результатов)
                resultsShown = true;
                raceStage = RaceStage.Finished;
            }
        }

        void RestartRace()
        {
            endStart = false;
            startTime = DateTime.MinValue;
            elapsedTime = TimeSpan.Zero;
            for (int i = 0; i < result.Count; i++)
            {
                result[i].currentTime.Clear();
                result[i].allTime = 0;
            }
            for (int i = 0; i < webCamTexture.Count; i++)
            {
                Destroy(camObject[i]);
                camObject[i] = null;
            }
            spawned = false;
            isCoroutineRunning = false;
            resultsShown = false;
            raceStage = RaceStage.NotStarted;
        }

        for (int index = 0; index < webCamTexture.Count; index++)
        {
            if (camObject[index] == null)
            {
                camObject[index] = Instantiate(prefabDrone);
                camObject[index].transform.SetParent(gridLayout);
                camObject[index].gameObject.transform.localScale = new Vector3(1, 1, 1);
                result.Add(camObject[index].GetComponent<Drone>().result);
                camTime[index] = camObject[index].transform.GetChild(1).GetComponent<TMP_Text>();
                spawned = true;
            }

            if (camObject[index] != null)
            {
                UpdateImage(index);
            }
        }
    }

    void CheckMarkAsync(int index)
    {

            Cv2.CvtColor(mat[index], grayMat[index], ColorConversionCodes.BGR2GRAY);
            CvAruco.DetectMarkers(grayMat[index], dictionary, out corners, out ids, detectorParameters, out rejectedImgPoints);
            CvAruco.DrawDetectedMarkers(mat[index], corners, ids);

            if (ids.Length > 0)
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    int markerId = ids[i];
                    markerDetected[markerId] = true;
                    lastMarkerDetectionTimes[index][markerId] = elapsedTime.TotalSeconds;
                }
            }
            for (int i = 0; i < markerDetected.Keys.Count; i++)
            {
                if (elapsedTime.TotalSeconds - lastMarkerDetectionTimes[index][i] >= markerClearTime)
                {
                    markerDetected[i] = false;
                }
            }
            bool allValuesTrue = markerDetected.Values.All(x => x);
            if (allValuesTrue)
            {
                StartCheckMarkStatusAfterDelay(index);
            }
    }

    void StartCheckMarkStatusAfterDelay(int index)
    {
       
            if (!isCoroutineRunning)
            {
                StartCoroutine(CheckMarkStatusAfterDelay(index));
            }
    }

    IEnumerator CheckMarkStatusAfterDelay(int index)
    {
        isCoroutineRunning = true;

        yield return new WaitForSeconds(0.2f);

        bool allMarksStillDetected = true;

        for (int i = 0; i < markerDetected.Keys.Count; i++)
        {
            allMarksStillDetected = allMarksStillDetected && markerDetected[i];
        }

        if (allMarksStillDetected)
        {
            yield return new WaitForSeconds(markerClearTime);

            for (int i = 0; i < markerDetected.Keys.Count; i++)
            {
                allMarksStillDetected = allMarksStillDetected && !markerDetected[i];
            }

            if (allMarksStillDetected)
            {
                allMarksStillDetected = false;
                if (result[index].currentTime.Count > 0)
                {
                    double currentTimeForCurrentLap = elapsedTime.TotalSeconds - result[index].allTime;
                    result[index].currentTime.Add(currentTimeForCurrentLap);
                    result[index].allTime = result[index].currentTime.Sum();
                }
                else
                {
                    result[index].currentTime.Add(elapsedTime.TotalSeconds);
                    result[index].allTime = result[index].currentTime.Sum();
                }
                camTime[index].text = ("Время кругов:\n " + result[index].GetTimeSummary());
            }
        }
        isCoroutineRunning = false;
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

[Serializable]
public enum TypeRace
{
    Circle,
    Drag,
    Timeout
};

[Serializable]
public enum RaceStage
{
    NotStarted,
    Countdown,
    Racing,
    Finished
}

[Serializable]
public class SettingsRace
{
    public int marker;
    public int timerStart;
    public TypeRace type;
    public int circles;
}


