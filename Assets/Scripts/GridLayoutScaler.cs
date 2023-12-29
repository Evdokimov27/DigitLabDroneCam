using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityEngine.UI;

public class GridLayoutScaler : MonoBehaviour
{
    public GridLayoutGroup gridLayoutGroup;
    public int childCountCurrent;
    public CameraController control;

    private void Start()
    {
        childCountCurrent = transform.childCount;
    }
    void Update()
    {
        AdjustGridLayout();
    }

    void AdjustGridLayout()
    {
        int childCount = transform.childCount;
        int itemsPerRow = 3;
        int rowCount = Mathf.CeilToInt((float)childCount / itemsPerRow);
        gridLayoutGroup.constraintCount = itemsPerRow;
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        gridLayoutGroup.constraintCount = rowCount;
        if (childCount < 5 && childCount > 0)
        {
            switch (childCount)
            {
                case 1:
                    {
                        gridLayoutGroup.cellSize = new Vector2(1366, 768); break;
                    }
                case 2:
                    {
                        gridLayoutGroup.cellSize = new Vector2(1024, 768); break;
                    }
                case 3:
                    {
                        gridLayoutGroup.cellSize = new Vector2(640, 480); break;
                    }
                case 4:
                    {
                        gridLayoutGroup.cellSize = new Vector2(640, 200) ; break;
                    }
            }
        }
        // Обновляем текущее количество детей
        childCountCurrent = childCount;
    }
}
