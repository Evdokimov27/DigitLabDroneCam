using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using OpenCvSharp.Flann;
using System.Linq;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Timeline;
using System.IO;
using System.Text;

public enum RaceStage
{
    NotStarted,
    Racing,
    Finished
}

[Serializable]
public class RaceManager : MonoBehaviour
{
    public TypeRace raceType;
    public TMP_Dropdown sprintOrCircules;
    public TMP_InputField markerCount;
    public TMP_InputField markerOnCircul;

    public int countdownTime = 3;
    public int maxWinner = 3;
    public TMP_Text timerText;
    public TMP_Text resultsText;
    public Result[] results;
    public WebcamManager webcamManager;
    public GameObject prefab;
    public Transform imagesParent;

    private DateTime startTime;
    private TimeSpan elapsedTime;
    private bool raceInProgress = false;
    private RaceStage currentStage = RaceStage.NotStarted;
    private Color gold = new Color(1, 0.843f, 0);
    private Color silver = new Color(0.753f, 0.753f, 0.753f, 1f);
    private Color bronze = new Color(0.804f, 0.498f, 0.196f, 1f);
    [SerializeField]
    private void Start()
    {
        if (webcamManager == null || prefab == null)
        {
            Debug.LogError("WebcamManager или prefab не заданы.");
            return;
        }

        results = new Result[webcamManager.NumberOfCameras];
        for (int cameraIndex = 0; cameraIndex < webcamManager.NumberOfCameras; cameraIndex++)
        {
            GameObject newImageObject = Instantiate(prefab, imagesParent);
            results[cameraIndex] = new Result
            {
                camObject = newImageObject,
                camName = newImageObject.transform.GetChild(0).GetComponent<TMP_InputField>(),
                camTime = newImageObject.transform.GetChild(1).GetComponent<TMP_Text>(),
                winNomber = newImageObject.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>()
            };
        }
    }
    public void ChangeDropdown()
    {
        if (sprintOrCircules.value == 0)
        {
            raceType = TypeRace.Circuit;
            markerOnCircul.gameObject.SetActive(true);
        }
        if (sprintOrCircules.value == 1)
        {
            raceType = TypeRace.Sprint;
            markerOnCircul.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (currentStage == RaceStage.Racing)
        {
            foreach (Result result in results)
            {
                if (int.TryParse(markerCount.text, out int number))
                    if (result.marker == number)
                    {
                        FinishRace();
                    }
            }
        }
        var dropdownlist = GameObject.Find("Dropdown List");
        if (dropdownlist != null) dropdownlist.transform.localScale = new Vector3(2, 2, 1);
        if (currentStage == RaceStage.Racing)
        {
            UpdateRaceTimer();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (currentStage)
            {
                case RaceStage.NotStarted:
                    StartCoroutine(StartRaceCountdown());
                    break;
                case RaceStage.Racing:
                    FinishRace();
                    break;
                case RaceStage.Finished:
                    RestartRace();
                    break;
            }
        }
    }
    public void Check(int cameraIndex)
    {

        if (elapsedTime.TotalSeconds > 0)
        {
            double currentTimeForCurrentLap = elapsedTime.TotalSeconds - results[cameraIndex].allTime;
            results[cameraIndex].resultTime.Add(currentTimeForCurrentLap);
            results[cameraIndex].allTime = results[cameraIndex].resultTime.Sum();
        }
    }
    private IEnumerator StartRaceCountdown()
    {
        RestartRace();
        int _countdownTime = countdownTime;
        while (_countdownTime > 0)
        {
            resultsText.text = "Старт через: " + _countdownTime;
            yield return new WaitForSeconds(1);
            _countdownTime--;
        }
        BeginRace();
    }
   
    private void BeginRace()
    {
        sprintOrCircules.interactable = false;
        markerCount.interactable = false;
        markerOnCircul.interactable = false;
        currentStage = RaceStage.Racing;
        startTime = DateTime.Now;
        raceInProgress = true;
        foreach(var result in results)
        {
            result.marker = 0;
        }
        timerText.text = "00:00:00";
        resultsText.text = $"Гонка идет!\nSpace для преждевременного окончания";

    }

    private void UpdateRaceTimer()
    {
        if (raceInProgress)
        {
            elapsedTime = DateTime.Now - startTime;
            timerText.text = elapsedTime.ToString(@"hh\:mm\:ss\.fff");
        }
    }
    
    private void FinishRace()
    {
        sprintOrCircules.interactable = true;
        markerCount.interactable = true;
        markerOnCircul.interactable = true;
        raceInProgress = false;
        currentStage = RaceStage.Finished;
        ShowResults();
    }

    private void ShowResults()
    {
        resultsText.text = "Гонка завершена! Время: " + elapsedTime.ToString(@"hh\:mm\:ss\.fff");
        DisplayTopRaceResults(maxWinner);
        SaveRaceResultsToGoogleSheets();
    }
    void RestartRace()
    {
        foreach (var result in results)
        {
            result.resultTime.Clear();
            result.circleTime.Clear();
            result.allTime = 0;
            result.marker = 0;
            if(raceType == TypeRace.Circuit)result.camTime.text = $"Пройдено кругов: {result.marker}/{markerCount.text}\nВремя кругов:";
            if(raceType == TypeRace.Sprint)result.camTime.text = $"Пройдено меток: {result.marker}/{markerCount.text}";
            result.camObject.SetActive(true);
            result.camObject.transform.GetChild(2).gameObject.SetActive(false);
        }
        currentStage = RaceStage.NotStarted;
        raceInProgress = false;
        timerText.text = "00:00:00";
        resultsText.text = $"Готовы к новому старту!\nSpace для начала";
        startTime = DateTime.MinValue;
        elapsedTime = TimeSpan.Zero;
        for (int i = 0; i < results.Length; i++)
        {
            results[i].resultTime.Clear();
            results[i].circleTime.Clear();
            results[i].allTime = 0;
        }
    }
    public void SaveRaceResultsToGoogleSheets()
    {
        StartCoroutine(SendRaceResults());
    }

    IEnumerator SendRaceResults()
    {
        RaceResultData raceData = new RaceResultData
        {
            participantResults = new List<ParticipantResult>()
        };

        var sortedResults = results.OrderBy(r => r.marker).ThenBy(r => r.allTime).ToArray();
        int.TryParse(markerCount.text, out int number);
        for (int i = 0; i < sortedResults.Length; i++)
        {
            ParticipantResult participantResult = null;
            if (raceType == TypeRace.Circuit)
            {
                participantResult = new ParticipantResult
                {
                    place = i + 1,
                    typeRace = "Круговая",
                    participantName = sortedResults[i].camName.text,
                    totalTime = sortedResults[i].allTime,
                    lapTimes = sortedResults[i].circleTime,
                    marker = sortedResults[i].marker,
                    maxMarker = number,
                };
            }
            if (raceType == TypeRace.Sprint)
            {
                participantResult = new ParticipantResult
                {
                    place = i + 1,
                    typeRace = "Спринт",
                    participantName = sortedResults[i].camName.text,
                    totalTime = sortedResults[i].allTime,
                    lapTimes = sortedResults[i].resultTime,
                    marker = sortedResults[i].marker,
                    maxMarker = number,
                };
            }
            raceData.participantResults.Add(participantResult);
        }


        

        string jsonData = JsonUtility.ToJson(raceData);
        Debug.Log(jsonData);
        UnityWebRequest www = UnityWebRequest.Post("https://script.google.com/macros/s/AKfycbwlTnbYKYG18bMYZHoGsirGdj9G5sqiZwBNHWNamR-InmkHMdkXC1ECYMMqkBdPjqE0vg/exec", "");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();
        if (www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Race results sent successfully " + www.result);
        }
    }







    private void DisplayTopRaceResults(int topPlaces)
    {
        if (results.Length == 0)
        {
            Debug.LogError("Нет результатов для отображения.");
            return;
        }

        // Создаем новый отсортированный список
        var sortedResults = results
            .OrderByDescending(r => r.marker) // Сначала сортируем по количеству меток
            .ThenBy(r => r.allTime)           // Затем сортируем по времени
            .ToArray();

        // Обрабатываем отсортированные результаты
        for (int i = 0; i < sortedResults.Length; i++)
        {
            if (i < topPlaces)
            {
                sortedResults[i].camObject.transform.GetChild(2).gameObject.SetActive(true);
                sortedResults[i].winNomber.text = $"Место {i + 1}\nОбщее время:\n{sortedResults[i].allTime:F2} сек";
                switch (i + 1)
                {
                    case 1:
                        sortedResults[i].winNomber.color = gold;
                        break;
                    case 2:
                        sortedResults[i].winNomber.color = silver;
                        break;
                    case 3:
                        sortedResults[i].winNomber.color = bronze;
                        break;
                }
            }
            else
            {
                sortedResults[i].camObject.SetActive(false);
            }
        }
    }



    [Serializable]
    public class Result
    {
        public List<double> resultTime = new List<double>();
        public List<double> circleTime = new List<double>();
        public double allTime = 0;
        public int marker;
        public GameObject camObject;
        public TMP_InputField camName;
        public TMP_Text camTime;
        public TMP_Text winNomber;


        public string GetTimeSummary()
        {

            string timeSummary = "";
            for (int i = 0; i < circleTime.Count; i++)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(circleTime[i]);
                string formattedTime = timeSpan.ToString(@"mm\:ss\.fff");
                timeSummary += $"Круг {(i + 1)}: {formattedTime}\n";
            }
            return timeSummary;
        }
    }
    [System.Serializable]
    public class RaceResultData
    {
        public List<ParticipantResult> participantResults;
    }

    [Serializable]
    public class ParticipantResult
    {
        public int place;
        public string typeRace;
        public string participantName;
        public double totalTime;
        public List<double> lapTimes;
        public int marker;
        public int maxMarker;
    }

}

public enum TypeRace
{
    Sprint,
    Circuit
};