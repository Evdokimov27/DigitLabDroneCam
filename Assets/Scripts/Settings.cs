using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Settings : MonoBehaviour
{
    [SerializeField] GameObject settingsDelay;
	// Start is called before the first frame update
	public void OnActive()
	{
		settingsDelay.SetActive(!settingsDelay.activeSelf);
	}
	public void Exit()
	{
		Application.Quit();
	}
}
