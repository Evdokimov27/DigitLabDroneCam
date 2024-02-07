using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Transform imagesParent;
    public GridLayoutGroup gridLayoutGroup;
    public RaceManager raceManager;

    public void CreateCameraImages(int numberOfCameras)
    {
        AdjustGridLayout(numberOfCameras); 
    }
    private void Start()
    {
        foreach (RaceManager.Result r in raceManager.results)
        {
            r.panelImg = r.camObject.transform.GetChild(3).GetComponent<RectTransform>();
            r.imgResult = r.camObject.transform.GetChild(4).GetComponent<Image>();
        }
    }
    private void AdjustGridLayout(int numberOfCameras)
    {
        
        switch (numberOfCameras)
        {
            case 1:
                {
                    gridLayoutGroup.cellSize = new Vector2(800, 800);
                    foreach (RaceManager.Result r in raceManager.results)
                    {
                        r.panelImg.transform.localScale = new Vector3(0.5f, 0.5f);

                        r.imgResult.transform.localScale = new Vector2(1,1);
                    }
                    break;
                }
            case 2:
                {
                    gridLayoutGroup.cellSize = new Vector2(800, 800);
                    foreach (RaceManager.Result r in raceManager.results)
                    {
                        r.panelImg.transform.localScale = new Vector3(0.5f, 0.5f);

                        r.imgResult.transform.localScale = new Vector2(1, 1);
                    }
                    break;

                }
            case 3:
                {
                    gridLayoutGroup.cellSize = new Vector2(600, 600);
                    foreach (RaceManager.Result r in raceManager.results)
                    {
                        r.panelImg.transform.localScale = new Vector3(0.4f, 0.4f);

                        r.imgResult.transform.localScale = new Vector2(0.8f, 0.8f);
                    }
                    break;
                }
            case 4:
                {
                    gridLayoutGroup.cellSize = new Vector2(425, 425);
                    foreach (RaceManager.Result r in raceManager.results)
                    {
                        r.panelImg.transform.localScale = new Vector3(0.3f, 0.3f);

                        r.imgResult.transform.localScale = new Vector2(0.6f, 0.6f);
                    }
                    break;
                }
            case >4:
                {
                    gridLayoutGroup.cellSize = new Vector2(350, 350);
                    foreach (RaceManager.Result r in raceManager.results)
                    {
                        r.panelImg.transform.localScale = new Vector3(0.25f, 0.25f);

                        r.imgResult.transform.localScale = new Vector2(0.5f, 0.5f);
                    }
                    break;
                }
        }
    }

    public void UpdateCameraImage(int cameraIndex, Texture cameraFrame)
    {
        if (cameraIndex >= 0 && cameraIndex < imagesParent.childCount)
        {
            var cameraImage = imagesParent.GetChild(cameraIndex).GetComponent<RawImage>();
            if (cameraImage != null)
            {
                cameraImage.texture = cameraFrame;
            }
        }
    }

  
}
