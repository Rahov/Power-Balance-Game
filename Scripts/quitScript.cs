using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using UnityEngine.SceneManagement;

public class quitScript : MonoBehaviour
{
	public static bool gamePaused = false;
	private RectTransform pauseMenu;
	private RectTransform fog;

	void Awake()
	{
		pauseMenu = transform.Find("pauseMenu").GetComponent<RectTransform>();
		fog = transform.Find("fog").GetComponent<RectTransform>();

		transform.Find("pauseButton").GetComponent<Button_UI>().ClickFunc = () => 
		{ 
			if (!gamePaused)
			{
				Time.timeScale = 0f;
				pauseMenu.gameObject.SetActive(true);
				fog.gameObject.SetActive(true);
				gamePaused = true;
			} 
			else
			{
				Time.timeScale = 1f;
				pauseMenu.gameObject.SetActive(false);
				fog.gameObject.SetActive(false);			
				gamePaused = false;
			}
		};
		pauseMenu.transform.Find("resumeBtn").GetComponent<Button_UI>().ClickFunc = () => 
		{ 
			Time.timeScale = 1f;
			pauseMenu.gameObject.SetActive(false);
			fog.gameObject.SetActive(false);			
			gamePaused = false;	
		};
		transform.Find("quitButton").GetComponent<Button_UI>().ClickFunc = () => { Application.Quit(); };
		pauseMenu.transform.Find("quitBtn").GetComponent<Button_UI>().ClickFunc = () => { Application.Quit(); };
		pauseMenu.transform.Find("levelsBtn").GetComponent<Button_UI>().ClickFunc = () => { SceneManager.LoadScene(1); };
	}
}