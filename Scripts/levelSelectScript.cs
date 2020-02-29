using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using CodeMonkey.Utils;
using UnityEngine;


public class levelSelectScript : demandGraph
{
    void Awake()
    {  
        transform.Find("level1Btn").GetComponent<Button_UI>().ClickFunc = () => {SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);};
        transform.Find("level2Btn").GetComponent<Button_UI>().ClickFunc = () => {SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);};
        transform.Find("level3Btn").GetComponent<Button_UI>().ClickFunc = () => {SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 3);};
        transform.Find("level4Btn").GetComponent<Button_UI>().ClickFunc = () => {SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 4);};
        transform.Find("level5Btn").GetComponent<Button_UI>().ClickFunc = () => {SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 5);};
        transform.Find("level6Btn").GetComponent<Button_UI>().ClickFunc = () => {SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 6);};
        transform.Find("level7Btn").GetComponent<Button_UI>().ClickFunc = () => {SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 7);};
        transform.Find("level8Btn").GetComponent<Button_UI>().ClickFunc = () => {SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 8);};
    }
}