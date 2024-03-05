using UnityEngine;
using System.IO;
using System.Collections.Generic;
using TMPro;

public class RaceResultsManager : MonoBehaviour
{
    [System.Serializable]
    public class RaceResult
    {
        public string name;
        public List<double> circleTime;
        public double allTime;
    }

    [System.Serializable]
    public class RaceData
    {
        public List<RaceResult> raceResults = new List<RaceResult>();
    }

    public TMP_InputField fileNameInput;
    private RaceData currentRaceData = new RaceData();

    public void AddResult(string name, List<double> circle, double time)
    {
        var existingResult = currentRaceData.raceResults.Find(result => result.name == name);
        if (existingResult != null)
        {
            existingResult.circleTime = circle;
            existingResult.allTime = time;
        }
        else
        {
            currentRaceData.raceResults.Add(new RaceResult { name = name, circleTime = circle, allTime = time });
        }
    }


    public void SaveResults()
    {
        string json = JsonUtility.ToJson(currentRaceData, true);
        string path = Path.Combine(Application.persistentDataPath, fileNameInput.text + ".json");
        File.WriteAllText(path, json);
        Debug.Log($"Results saved to {path}");
    }

    // Вызовите этот метод для очистки данных перед новым днем гонок
    public void ResetData()
    {
        currentRaceData = new RaceData();
    }
}
