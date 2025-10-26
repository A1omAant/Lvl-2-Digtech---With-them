using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Collections;


public class PauseMenuController : MonoBehaviour
{
   public GameObject pauseMenuUI;
   public GameObject otherUI;
   public GameObject noteUI;
    public GameObject WinConditionUI;
    public GameObject deathScreenUI;
   public bool paused = false;
   public bool isInNote = false;
    private bool noteInputLocked = false;
    public bool UnableToUnpause = false; // Set to true to prevent unpausing (e.g., during death or win sequence) or if in pause menu deeper menus
   
   void Awake(){
  
        
   }

   void Update()
   {
       handlePauseInput();
       //handleNoteInput();
   }

    private void handlePauseInput()
    {
         if (Input.GetKeyDown(KeyCode.Escape))
         {
            if (isInNote && !noteInputLocked)
            {
                ExitNote();
            }
            else if (paused)
            {
                if (UnableToUnpause == false)
                { // nesting this to prevent reopening pause menu when it shouldn't be possible
                    Resume();
                }
            }
            else
            {
                Pause();
            }

            if (!paused && !isInNote)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    private void handleNoteInput()
    {

         if (Input.GetKeyDown(KeyCode.Escape))
         {
            if (isInNote){
              ExitNote();
              Cursor.lockState = CursorLockMode.Locked;
              Cursor.visible = false;
            }
         }
    }

    public void SetUnpauseable(bool value){
        UnableToUnpause = value;
    }

   public void Resume()
   {
       pauseMenuUI.SetActive(false);
       otherUI.SetActive(true);
         noteUI.SetActive(false);
       
       Time.timeScale = 1f;
       paused = false;
       Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false;
       isInNote = false;
   }
   public void Pause()
   {
       pauseMenuUI.SetActive(true);
       otherUI.SetActive(false);
       noteUI.SetActive(false);
       Debug.Log("PAUSED");
       Time.timeScale = 0f;
       paused = true;
       Cursor.lockState = CursorLockMode.None;
       Cursor.visible = true;
         isInNote = false;
   }
   public void LoadMainMenu()
   {
       Time.timeScale = 1f;
       SceneManager.LoadScene("Main Menu");
   }

   public void Note(){

         pauseMenuUI.SetActive(false);
         otherUI.SetActive(false);
         noteUI.SetActive(true);
         Time.timeScale = 0f;

         Cursor.lockState = CursorLockMode.None;
         Cursor.visible = true;
            isInNote = true;
            noteInputLocked = true;
            StartCoroutine(UnlockNoteInputNextFrame());
   }
   public void ExitNote(){
         pauseMenuUI.SetActive(false);
            otherUI.SetActive(true);
         noteUI.SetActive(false);
         Time.timeScale = 1f;
         
         Cursor.lockState = CursorLockMode.Locked;
         Cursor.visible = false;
        isInNote = false;
   }

   public void QuitGame()
   {
       Application.Quit();
   }

   public void WinGame(){
       Time.timeScale = 0f;
       paused = true;
       
       StartCoroutine(WinSequence());
   }

    private System.Collections.IEnumerator WinSequence()
    {
      Image blackScreen = WinConditionUI.GetComponent<Image>();
      TMP_Text winText = WinConditionUI.GetComponentInChildren<TMP_Text>();
      Button MainMenuButton = WinConditionUI.GetComponentInChildren<Button>();
      WinConditionUI.SetActive(true);
      MainMenuButton.gameObject.SetActive(false);
      blackScreen.color = new Color(0, 0, 0, 0);
      winText.color = new Color(winText.color.r, winText.color.g, winText.color.b, 0);

      yield return StartCoroutine(FadeInImage(blackScreen, 4f));

      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;

      yield return new WaitForSecondsRealtime(1f);
      MainMenuButton.gameObject.SetActive(true);
      yield return StartCoroutine(FadeInTMP(winText, 2f));
      MainMenuButton.interactable = true;

   }

    private System.Collections.IEnumerator FadeInImage(Image image, float duration)
    {
        
        float currentTime = 0f;
        while (currentTime < duration)
        {
            float alpha = Mathf.Clamp01(currentTime / duration);
            image.color = new Color(0, 0, 0, alpha);
            currentTime += Time.unscaledDeltaTime; // use unscaled delta time for pause menu
            yield return null;
        }
        image.color = new Color(0, 0, 0, 1); // ensure it's fully opaque at the end
    }
    private System.Collections.IEnumerator FadeInTMP(TMP_Text buttonText, float duration)
    {
        float currentTime = 0f;
        Color originalColor = buttonText.color;

        while (currentTime < duration)
        {
            float alpha = Mathf.Clamp01(currentTime / duration);
            buttonText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            currentTime += Time.unscaledDeltaTime; // use unscaled delta time for pause menu
            yield return null;
        }
        buttonText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1); // ensure it's fully opaque at the end
    }

    public void Die(){
        paused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Player Death Sequence Initiated");
        deathScreenUI.SetActive(true);
        otherUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        noteUI.SetActive(false);

        Image blackScreen = deathScreenUI.GetComponent<Image>();
        StartCoroutine(FadeInImage(blackScreen, 5f));

        GameplayAudio.Instance?.StopAllSounds();
        GameplayAudio.Instance?.PlayGameplaySFX("Player Death");
        GameplayAudio.Instance?.PlayGameplaySFX("PlayerDeath");
        Time.timeScale = 0f;



    }

    public void RestartLevel(){
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator UnlockNoteInputNextFrame()
{
    yield return null; // wait one frame
    noteInputLocked = false;
}


}
