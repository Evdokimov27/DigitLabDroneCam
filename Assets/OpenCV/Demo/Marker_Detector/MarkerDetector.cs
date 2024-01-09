namespace OpenCvSharp.Demo {

    using UnityEngine;
    using System.Collections;
    using UnityEngine.UI;
    using Aruco;

    public class MarkerDetector : MonoBehaviour {

        private WebCamTexture webCamTexture;
        private Point2f[][] corners;
        private int[] ids;
        private Point2f[][] rejectedImgPoints;

        void Start () {
            // Создаем параметры по умолчанию для детекции
            DetectorParameters detectorParameters = DetectorParameters.Create();

            // Словарь содержит набор всех доступных маркеров
            Dictionary dictionary = CvAruco.GetPredefinedDictionary (PredefinedDictionaryName.Dict6X6_250);

            // Создаем WebCamTexture
            webCamTexture = new WebCamTexture();
            webCamTexture.Play();

            // Ждем, пока WebCamTexture будет готов
            while (!webCamTexture.isPlaying) { }

            RawImage rawImage = gameObject.GetComponent<RawImage>();

            // Запускаем бесконечный цикл обновления изображения
            StartCoroutine(UpdateImageRoutine(rawImage, detectorParameters, dictionary));
        }

        // Бесконечный цикл для обновления изображения
        IEnumerator UpdateImageRoutine(RawImage rawImage, DetectorParameters detectorParameters, Dictionary dictionary) {
            while (true) {
                // Создаем Mat из текущего кадра WebCamTexture
                Mat mat = Unity.TextureToMat(webCamTexture);

                // Конвертируем изображение в оттенки серого
                Mat grayMat = new Mat();
                Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);

                // Детектируем и рисуем маркеры
                CvAruco.DetectMarkers(grayMat, dictionary, out corners, out ids, detectorParameters, out rejectedImgPoints);
                CvAruco.DrawDetectedMarkers(mat, corners, ids);

                // Создаем Unity текстуру с обнаруженными маркерами
                Texture2D outputTexture = Unity.MatToTexture(mat);

                // Устанавливаем текстуру, чтобы увидеть результат
                rawImage.texture = outputTexture;

                // Освобождаем ресурсы Mat
                mat.Dispose();
                grayMat.Dispose();

                // Ждем один кадр перед следующим обновлением
                yield return null;
            }
        }
    }
}
