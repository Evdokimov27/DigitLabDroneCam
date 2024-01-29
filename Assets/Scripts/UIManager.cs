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

    private void AdjustGridLayout(int numberOfCameras)
    {
        switch (numberOfCameras)
        {
            case 1:
                {
                    gridLayoutGroup.cellSize = new Vector2(800, 800);
                    break;
                }
            case 2:
                {
                    gridLayoutGroup.cellSize = new Vector2(800, 800);
                    break;

                }
            case 3:
                {
                    gridLayoutGroup.cellSize = new Vector2(600, 600);
                    break;
                }
            case 4:
                {
                    gridLayoutGroup.cellSize = new Vector2(425, 425);
                    break;
                }
            case 5:
                {
                    gridLayoutGroup.cellSize = new Vector2(350, 350);
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
