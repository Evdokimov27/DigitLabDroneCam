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

//* Created by Evdokimov # ObederTeam # DigitLab *//

public class CameraController : MonoBehaviour
{
    private List<WebCamTexture> webCamTexture;
    private Point2f[][] corners;
    private int[] ids;
    private Point2f[][] rejectedImgPoints;
    private DetectorParameters detectorParameters = DetectorParameters.Create();
    private Dictionary dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250);
    private bool spawned;

    public GameObject prefabDrone;
    public Transform gridLayout;
    public GameObject[] camObject;
    public TMP_Text[] cameraText;
    public float[] lastMarkerDetectionTime;
    public float markerClearTime;
    public float currentTime;
    public int count;
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        List<WebCamDevice> desiredCamera = FindCameraByName(devices, "USB2.0 PC CAMERA");
        if (FindCameraByName(devices, "DroidCam Source 3").Count > 0)
        {
            desiredCamera.Add(FindCameraByName(devices, "DroidCam Source 3")[0]);
        }
        webCamTexture = new List<WebCamTexture>();
        camObject = new GameObject[desiredCamera.Count];
        cameraText = new TMP_Text[desiredCamera.Count];
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

    async void UpdateImage(int index)
    {
        cameraText[index] = camObject[index].GetComponentInChildren<TMP_Text>();
        if (cameraText[index] != null)
        {
            cameraText[index].text = webCamTexture[index].deviceName;
        }
        Mat mat = UnityCV.TextureToMat(webCamTexture[index]);
        Mat grayMat = new Mat();
        await Task.Run(() => CheckMarkAsync(mat, grayMat, index));
        // Update the RawImage texture with the modified Mat
        camObject[index].GetComponent<RawImage>().texture = UnityCV.MatToTexture(mat);
        Resources.UnloadUnusedAssets();

    }

    void Update()
    {
        currentTime = Time.time;
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
    static bool CheckPairs(int[] ids)
    {
        bool boolean = false;
        if (ids.Length > 0)
        {
            var groupedIds = ids.GroupBy(x => x);
            boolean = groupedIds.All(group => group.Count() % 2 == 0);
        }
        return boolean;
    }
    public bool currentBool;
    async Task CheckMarkAsync(Mat mat, Mat grayMat, int index)
    {

        await Task.Run(() =>
        {
            Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);
            CvAruco.DetectMarkers(grayMat, dictionary, out corners, out ids, detectorParameters, out rejectedImgPoints);
            CvAruco.DrawDetectedMarkers(mat, corners, ids);

            if (CheckPairs(ids))
            {
                lastMarkerDetectionTime[index] = currentTime;
                currentBool = true;
            }

            if (currentBool)
            {
                if (currentTime - lastMarkerDetectionTime[index] < markerClearTime)
                {
                    Debug.Log("Дрон обнаружил метку");
                }
            }


        });
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
