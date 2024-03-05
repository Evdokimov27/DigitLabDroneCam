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
            r.panelImg = r.camObject.transform.GetChild(6).GetComponent<RectTransform>();
            r.imgResult = r.camObject.transform.GetChild(5).GetComponent<Image>();
        }
    }
   
    private void AdjustGridLayout(int numberOfCameras)
    {
        switch (numberOfCameras)
        {
            case 1:
                {
                    gridLayoutGroup.cellSize = new Vector2(800, 600);

                    foreach (RaceManager.Result r in raceManager.results)
                    {
                        r.camObject.transform.localScale = new Vector2(1f, 1f);
                        gridLayoutGroup.constraintCount = 1;

                    }
                    break;
                }
            case 2:
                {
                    gridLayoutGroup.cellSize = new Vector2(800, 600);

                    foreach (RaceManager.Result r in raceManager.results)
                    {
                        r.camObject.transform.localScale = new Vector2(0.7f, 0.7f);
                        gridLayoutGroup.constraintCount = 2;

                    }
                    break;

                }
            case 3:
                {
                    gridLayoutGroup.cellSize = new Vector2(575, 600);

                    foreach (RaceManager.Result r in raceManager.results)
                    {
                        r.camObject.transform.localScale = new Vector2(0.55f, 0.55f);
                        gridLayoutGroup.constraintCount = 3;

                    }
                    break;
                }
            case 4:
                {
                    gridLayoutGroup.cellSize = new Vector2(425, 325);

                    foreach (RaceManager.Result r in raceManager.results)
                    {
                        r.camObject.transform.localScale = new Vector2(0.35f, 0.35f);
                        gridLayoutGroup.constraintCount = 4;
                    }
                    break;
                }
            case >4:
                {
                    gridLayoutGroup.cellSize = new Vector2(425, 315);

                    foreach (RaceManager.Result r in raceManager.results)
                    {

                        r.camObject.transform.localScale = new Vector2(0.35f, 0.35f);
                        gridLayoutGroup.constraintCount = 4;

                    }
                    break;
                }
        }
    }

    public void UpdateCameraImage(int cameraIndex, Texture cameraFrame)
    {
        if (cameraIndex >= 0 && cameraIndex < imagesParent.childCount)
        {
            var cameraImage = imagesParent.GetChild(cameraIndex).GetChild(1).GetComponent<RawImage>();
            if (cameraImage != null)
            {
                cameraImage.texture = cameraFrame;
            }
        }
    }

  
}
