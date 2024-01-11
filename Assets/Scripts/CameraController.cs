using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using TMPro;
using System.Threading.Tasks;
using System.Linq;
using System;
using Unity.VisualScripting;
using static OpenCvSharp.Tracking.Tracker;
using System.Collections;
using UnityEngine.UIElements;

//* Created by Evdokimov # ObederTeam # DigitLab *//
[Serializable]
public class CameraController : MonoBehaviour
{
    [Header("Гонка")]
    public ResultRace[] result;
    [Header("Настройки")]
    public SettingsRace settings;
    public TMP_Text timer;
    [Header("Включить видео с DroidCam")] 
    public bool droidCam;
    [Header("Параметры")]
    public GameObject prefabDrone;
    public Transform gridLayout;
    public GameObject[] camObject;
    public TMP_Text[] cameraText;
    public float[] lastMarkerDetectionTime;
    public float markerClearTime;
    public float currentTime;
    public Mat[] mat;
    public Mat[] grayMat;
    //
    private List<WebCamTexture> webCamTexture;
    private Point2f[][] corners;
    private int[] ids;
    private Point2f[][] rejectedImgPoints;
    private DetectorParameters detectorParameters = DetectorParameters.Create();
    private Dictionary dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250);
    private bool spawned;
    private bool endStart = false;
    //
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
        mat = new Mat[desiredCamera.Count];
        grayMat= new Mat[desiredCamera.Count];
        lastMarkerDetectionTime = new float[desiredCamera.Count];

        if (desiredCamera.Count > 0)
        {
            for (int index = 0; index < desiredCamera.Count; index++)
            {
                webCamTexture.Add(new WebCamTexture(desiredCamera[index].name));
                webCamTexture[index].Play();
            }
            spawned = true;
        }
    }
    IEnumerator StartTimer()
    {
        if (!endStart)
        {
            for(; settings.timerStart > 0; settings.timerStart--)
            {
                timer.text = settings.timerStart.ToString();
                yield return new WaitForSeconds(1);
            }
            timer.text = "Полетел!";

            yield return new WaitForSeconds(1);

            endStart = true;
        }
        while (endStart)
        {
            currentTime += Time.deltaTime;
            Time.timeScale = 1;
            timer.text = currentTime.ToString().Substring(0,4) + " сек.";
            yield return new WaitForSeconds(0.01f);
        }
    }
    void UpdateImage(int index)
    {
        if (cameraText[index] != null)
        {
            webCamTexture[index].deviceName = "Дрон " + (index+1);
            cameraText[index].text = webCamTexture[index].deviceName;
            camObject[index].name = webCamTexture[index].deviceName;
        }
        cameraText[index] = camObject[index].GetComponentInChildren<TMP_Text>();
        mat[index] = UnityCV.TextureToMat(webCamTexture[index]);
        grayMat[index] = new Mat();
        if(endStart) CheckMarkAsync(index);
        // Update the RawImage texture with the modified Mat
        camObject[index].GetComponent<RawImage>().texture = UnityCV.MatToTexture(mat[index]);
        Resources.UnloadUnusedAssets();
    }

    void Update()
    {

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
    bool CheckPairs(int[] ids)
    {
        bool boolean = false;
        if (ids.Length >= settings.marker)
        {
            boolean = true;
        }
        return boolean;
    }
    public bool currentBool;
    void CheckMarkAsync(int index)
    {
        var res = result[index];
        Cv2.CvtColor(mat[index], grayMat[index], ColorConversionCodes.BGR2GRAY);
        CvAruco.DetectMarkers(grayMat[index], dictionary, out corners, out ids, detectorParameters, out rejectedImgPoints);
        CvAruco.DrawDetectedMarkers(mat[index], corners, ids);

        if (CheckPairs(ids))
        {
            lastMarkerDetectionTime[index] = currentTime;
            currentBool = true;
        }


        if (currentBool && currentTime - lastMarkerDetectionTime[index] < markerClearTime)
        {
            Debug.Log(webCamTexture[index].deviceName + " обнаружил метку");
            if (ids.Length < 1)
            {
                currentBool = false;
                Debug.Log(webCamTexture[index].deviceName + " прошел круг");
                res.allTime = currentTime;
                if (res.currentTime.Count < 1) res.currentTime.Add(currentTime);
                else
                {
                    var time = res.allTime - res.currentTime[res.currentTime.Count-1];
                    res.currentTime.Add(time);
                }
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
    [Header("Кол-во маркеров для отметки")]
    public int marker;
    [Header("Отсчет до старта (в сек.)")]
    public int timerStart;
    [Header("Тип заезда")]
    public TypeRace type;
    [Header("Кол-во кругов")]
    public int circles;
}
[Serializable]
public class ResultRace
{
    [Header("Время круга/трассы")]
    public List<float> currentTime = new List<float>();
    [Header("Общее время")]
    public float allTime;
}
