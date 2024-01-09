using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using System.Linq;

public class CameraController : MonoBehaviour
{
    private WebCamTexture[] webCamTexture;
    private Point2f[][] corners;
    private int[] ids;
    private Point2f[][] rejectedImgPoints;
    private DetectorParameters detectorParameters = DetectorParameters.Create();
    private Dictionary dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250);
    private bool spawned;

    public GameObject prefabDrone;
    public Transform gridLayout;
    public Texture[] tempMaterial;
    public GameObject[] camObject;
    public Texture2D[] texture;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        WebCamDevice[] desiredCamera = FindCameraByName(devices, "USB2.0 PC CAMERA").ToArray();
        webCamTexture = new WebCamTexture[desiredCamera.Length];
        texture = new Texture2D[desiredCamera.Length];
        camObject = new GameObject[desiredCamera.Length];
        tempMaterial = new Texture[desiredCamera.Length];
        camObject[0] = this.gameObject;

        if (desiredCamera.Length > 0)
        {
            for (int index = 0; index < desiredCamera.Length; index++)
            {
                webCamTexture[index] = new WebCamTexture(desiredCamera[index].name);
                webCamTexture[index].Play();
                texture[index] = new Texture2D(webCamTexture[index].width, webCamTexture[index].height);
            }
            StartCoroutine(UpdateImageRoutine(0));

            // Новый код: Устанавливаем имя камеры в компонент Text на prefabDrone
            Text cameraText = camObject[0].GetComponentInChildren<Text>();
            if (cameraText != null)
            {
                cameraText.text = webCamTexture[0].deviceName;
            }

            spawned = true;
        }
    }

    IEnumerator UpdateImageRoutine(int index)
    {
        while (true)
        {
            // CameraController
            Mat mat = Unity.TextureToMat(webCamTexture[index]);
            Mat grayMat = new Mat();
            Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);
            CvAruco.DetectMarkers(grayMat, dictionary, out corners, out ids, detectorParameters, out rejectedImgPoints);
            CvAruco.DrawDetectedMarkers(mat, corners, ids);
            texture[index] = Unity.MatToTexture(mat);
            camObject[index].GetComponent<RawImage>().texture = texture[index];
            mat.Dispose();
            grayMat.Dispose();
            // MarkerDetector
            yield return null;
        }
    }

    private List<WebCamDevice> FindCameraByName(WebCamDevice[] devices, string cameraName)
    {
        List<WebCamDevice> listCam = new List<WebCamDevice>();
        foreach (var device in devices)
        {
            if (device.name.Contains(cameraName))
            {
                Debug.Log(device.name);
                listCam.Add(device);
            }
        }

        return listCam;
    }

    void Update()
    {
        for (int index = 0; index < webCamTexture.Length; index++)
        {
            if (camObject[index] != null)
            {
                StartCoroutine(UpdateImageRoutine(index));
                OnDestroys();
            }
            else
            {
                // Создаем новый уникальный объект prefabDrone для каждой камеры
                camObject[index] = Instantiate(prefabDrone);
                camObject[index].transform.SetParent(gridLayout);
                camObject[index].gameObject.transform.localScale = new Vector3(1, 1, 1);
                spawned = true;  // Перенесено в блок else, чтобы устанавливаться только при создании нового объекта
            }
        }
    }

    private void OnDestroys()
    {
        // Освобождаем ресурсы при уничтожении объекта
        foreach (var tex in texture)
        {
            Destroy(tex);
        }
        foreach (var mat in tempMaterial)
        {
            Destroy(mat);
        }
    }
}
