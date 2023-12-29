using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public WebCamTexture[] webCamTexture;
    public Texture2D[] texture;
    public GameObject[] camObject;
    public GameObject prefabDrone;
    public Transform gridLayout;
    public bool spawned;
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        WebCamDevice[] desiredCamera = FindCameraByName(devices, "USB2.0 PC CAMERA").ToArray();
        webCamTexture = new WebCamTexture[desiredCamera.Length];
        texture = new Texture2D[desiredCamera.Length];
        camObject = new GameObject[desiredCamera.Length];
        camObject[0] = this.gameObject;
        if (desiredCamera.Length > 0)
        {
            for (int index = 0; index < desiredCamera.Length; index++)
            {
                webCamTexture[index] = new WebCamTexture(desiredCamera[index].name);
                webCamTexture[index].Play();
                texture[index] = new Texture2D(webCamTexture[index].width, webCamTexture[index].height);

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
                Debug.Log(device.name);
                listCam.Add(device);
            }
        }

        return listCam;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        for (int index = 0; index < webCamTexture.Length; index++)
        {
            if (camObject[index] != null)
            {
                UpdateTexture(index);
                camObject[index].GetComponent<Image>().material.mainTexture = texture[index];
            }
            else
            {
                camObject[index] = Instantiate(prefabDrone);
                camObject[index].transform.SetParent(gridLayout);
                camObject[index].gameObject.transform.localScale = new Vector3(1, 1, 1);
            }
            spawned = true;
        }
    }
    private void UpdateTexture(int index)
    {
        Color32[] colors = webCamTexture[index].GetPixels32();
        texture[index].SetPixels32(colors);
        texture[index].Apply();
    }
}
