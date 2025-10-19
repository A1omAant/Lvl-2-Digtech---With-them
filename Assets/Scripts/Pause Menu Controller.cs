using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class PauseMenuController : MonoBehaviour
{
   public GameObject pauseMenuUI;
   public GameObject otherUI;
   public bool paused = false;

   void Update()
   {
       if (Input.GetKeyDown(KeyCode.Escape))
       {
           if (paused)
           {
               if(pauseMenuUI.activeSelf){
                   Resume();
               }

           }
           else
           {
               Pause();
           }
       }
   }

   public void Resume()
   {
       pauseMenuUI.SetActive(false);
       otherUI.SetActive(true);
       Time.timeScale = 1f;
       paused = false;
       Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false;
   }
   public void Pause()
   {
       pauseMenuUI.SetActive(true);
       otherUI.SetActive(false);
       Debug.Log("PAUSED");
       Time.timeScale = 0f;
       paused = true;
       Cursor.lockState = CursorLockMode.None;
       Cursor.visible = true;
   }
   public void LoadMainMenu()
   {
       Time.timeScale = 1f;
       SceneManager.LoadScene("Main Menu");
   }

   public void QuitGame()
   {
       Application.Quit();
   }


}
