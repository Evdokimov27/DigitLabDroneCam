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

//* Created by Evdokimov # ObederTeam # DigitLab *//
[Serializable]
public class CameraController : MonoBehaviour
{
    public ResultRace[] result;
    public SettingsRace settings;
    public TMP_Text timer;
    public bool droidCam;
    public GameObject prefabDrone;
    public Transform gridLayout;
    public float markerClearTime;
    public Dictionary<int, bool> markerDetected;
    public bool[] markerDetectedVisible;
    public bool checkTrue;

    private List<WebCamTexture> webCamTexture;
    private Point2f[][] corners;
    private int[] ids;
    private Point2f[][] rejectedImgPoints;
    private DetectorParameters detectorParameters = DetectorParameters.Create();
    private Dictionary dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250);
    private bool spawned;
    private bool endStart = false;
    private GameObject[] camObject;
    private TMP_Text[] cameraText;
    private List<Dictionary<int, double>> lastMarkerDetectionTimes = new List<Dictionary<int, double>>();
    private Mat[] mat;
    private Mat[] grayMat;
    private bool isCoroutineRunning = false;

    public DateTime startTime;
    public TimeSpan elapsedTime;

    void Start()
    {
        settings.circles = 0;
        WebCamDevice[] devices = WebCamTexture.devices;
        List<WebCamDevice> desiredCamera = FindCameraByName(devices, "USB2.0 PC CAMERA");

        if (droidCam && FindCameraByName(devices, "DroidCam Source 3").Count > 0)
        {
            desiredCamera.Add(FindCameraByName(devices, "DroidCam Source 3")[0]);
        }

        result = new ResultRace[desiredCamera.Count];
        webCamTexture = new List<WebCamTexture>();
        camObject = new GameObject[desiredCamera.Count];
        cameraText = new TMP_Text[desiredCamera.Count];
        markerDetected = new Dictionary<int, bool>();
        mat = new Mat[desiredCamera.Count];
        grayMat = new Mat[desiredCamera.Count];
        if (desiredCamera.Count > 0)
        {
            for (int index = 0; index < desiredCamera.Count; index++)
            {
                Dictionary<int, double> lastMarkerDetectionTime = new Dictionary<int, double>();
                webCamTexture.Add(new WebCamTexture(desiredCamera[index].name));
                webCamTexture[index].Play();
                for (int i = 0; i < settings.marker; i++)
                {
                    markerDetected.Add(i, false);
                    lastMarkerDetectionTime.Add(i, 0d);
                }
                lastMarkerDetectionTimes.Add(lastMarkerDetectionTime);
            }
            spawned = true;
            markerDetectedVisible = new bool[markerDetected.Keys.Count];
        }

    }

    IEnumerator StartTimer()
    {
        if (!endStart)
        {
            for (; settings.timerStart > 0; settings.timerStart--)
            {
                timer.text = settings.timerStart.ToString();
                yield return new WaitForSeconds(1);
            }
            timer.text = "Полетел!";
            yield return new WaitForSeconds(1);
            endStart = true;
            startTime = DateTime.Now;
        }

        while (endStart)
        {
            elapsedTime = DateTime.Now - startTime;
            timer.text = elapsedTime.ToString(@"hh\:mm\:ss\:fff");
            yield return null;
        }
    }

    void UpdateImage(int index)
    {
        if (cameraText[index] != null)
        {
            webCamTexture[index].deviceName = "Дрон " + (index + 1);
            cameraText[index].text = webCamTexture[index].deviceName;
            camObject[index].name = webCamTexture[index].deviceName;
        }

        cameraText[index] = camObject[index].GetComponentInChildren<TMP_Text>();
        mat[index] = UnityCV.TextureToMat(webCamTexture[index]);
        grayMat[index] = new Mat();
        if (endStart) CheckMarkAsync(index);

        camObject[index].GetComponent<RawImage>().texture = UnityCV.MatToTexture(mat[index]);
        Resources.UnloadUnusedAssets();
    }

    void Update()
    {
        for(int i = 0; i < markerDetected.Count; i++)
        {
            markerDetectedVisible[i] = markerDetected[i];
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            StartCoroutine(StartTimer());
        }

        for (int index = 0; index < webCamTexture.Count; index++)
        {
            if (camObject[index] == null)
            {
                camObject[index] = Instantiate(prefabDrone);
                camObject[index].transform.SetParent(gridLayout);
                camObject[index].gameObject.transform.localScale = new Vector3(1, 1, 1);
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
        var res = result[index];
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
                Debug.Log("круг пройден");
                result[index].currentTime.Add(elapsedTime.TotalSeconds);
                result[index].allTime = +result[index].currentTime[result[index].currentTime.Count-1];
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
    Круг,
    Драг,
    Время
};

[Serializable]
public class SettingsRace
{
    public int marker;
    public int timerStart;
    public TypeRace type;
    public int circles;
}

[Serializable]
public class ResultRace
{
    public List<double> currentTime = new List<double>();
    public double allTime;
}
