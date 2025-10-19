using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class MenuController : MonoBehaviour
{
   [Header("UI References")]
   public GameObject mainMenuUI;
   public GameObject optionMenuUI;
   public string SceneToLoad;

   public void Awake(){
        mainMenuUI.SetActive(true);
        optionMenuUI.SetActive(false);
        DontDestroyOnLoad(gameObject);
   }


   public void MainMenuPlay()
   {
       SceneManager.LoadScene(SceneToLoad);
   }

   public void MainMenuQuit()
   {
       Application.Quit();
   }

}


