using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CodeMonkey.Utils;

public class menuScript : MonoBehaviour
{
    private RectTransform menuContainer;
    private GameObject Level_Selection;

    void Awake()
    {
	    transform.Find("levelSelectBtn").GetComponent<Button_UI>().ClickFunc = () => 
        { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex +1); };
    }
}