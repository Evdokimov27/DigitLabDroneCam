using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;

public class ColorFilter : MonoBehaviour
{
    public Color targetColor = Color.red;
    public float tolerance = 0.1f;
    public Texture2D texture;
    public Image test;

    private void Update()
    {
        texture = this.gameObject.GetComponent<CameraController>().texture[0];

        // Производим цветовую фильтрацию
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (ColorWithinTolerance(pixels[i], targetColor, tolerance))
            {
                // Если цвет близок к целевому цвету, меняем его на другой цвет (например, черный)
                pixels[i] = Color.black;
            }
        }

        // Применяем измененные пиксели к текстуре
        texture.SetPixels(pixels);
        texture.Apply();

        // Обновляем текстуру объекта
        test.material.mainTexture = texture;
    }

    private bool ColorWithinTolerance(Color a, Color b, float tolerance)
    {
        float deltaR = Mathf.Abs(a.r - b.r);
        float deltaG = Mathf.Abs(a.g - b.g);
        float deltaB = Mathf.Abs(a.b - b.b);

        return deltaR < tolerance && deltaG < tolerance && deltaB < tolerance;
    }
}
