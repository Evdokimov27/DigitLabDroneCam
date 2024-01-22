using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using OpenCvSharp.Flann;
using System.Linq;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public enum RaceStage
{
    NotStarted,
    Racing,
    Finished
}

[Serializable]
public class RaceManager : MonoBehaviour
{
    int countdownTime = 3;
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
                camName = newImageObject.transform.GetChild(0).GetComponent<TMP_Text>(),
                camTime = newImageObject.transform.GetChild(1).GetComponent<TMP_Text>()
            };
        }
    }
    IEnumerator WaitForInitialization(int cameraIndex, GameObject newImageObject)
    {
        yield return new WaitUntil(() => results[cameraIndex] != null);
    }
    void Update()
    {
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
                    ShowResults();
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
        currentStage = RaceStage.Racing;
        startTime = DateTime.Now;
        raceInProgress = true;
        timerText.text = "00:00:00";
        resultsText.text = "";
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
        raceInProgress = false;
        currentStage = RaceStage.Finished;
        ShowResults();
    }

    private void ShowResults()
    {
        currentStage = RaceStage.Finished;
        resultsText.text = "Гонка завершена! Время: " + elapsedTime.ToString(@"hh\:mm\:ss\.fff");
    }
    void RestartRace()
    {
        foreach (var result in results)
        {
            result.resultTime.Clear();
            result.allTime = 0;
            result.camTime.text = "Время кругов:";
        }
        currentStage = RaceStage.NotStarted;
        raceInProgress = false;
        timerText.text = "00:00:00";
        resultsText.text = "Готовы к новому старту!";
        startTime = DateTime.MinValue;
        elapsedTime = TimeSpan.Zero;
        for (int i = 0; i < results.Length; i++)
        {
            results[i].resultTime.Clear();
            results[i].allTime = 0;
        }
    }


    [Serializable]
    public class Result
    {
        public List<double> resultTime = new List<double>();
        public double allTime = 0;
        public GameObject camObject;
        public TMP_Text camName;
        public TMP_Text camTime;


        public string GetTimeSummary()
        {

            string timeSummary = "";
            for (int i = 0; i < resultTime.Count; i++)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(resultTime[i]);
                string formattedTime = timeSpan.ToString(@"mm\:ss\.fff");
                timeSummary += $"Круг {(i + 1)}: {formattedTime}\n";
            }
            return timeSummary;
        }
    }
}
