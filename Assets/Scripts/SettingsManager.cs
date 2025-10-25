using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{

    public static SettingsManager Instance;
    public float MouseSensitivity { get; private set; } = 30f;
    public bool InvertY { get; private set; } = false;
    //public Slider mouseSensitivitySlider;
    //public Toggle invertYToggle;
    public float ScreenWidth { get; private set; } = 1920;
    public float ScreenHeight { get; private set; } = 1080;
    

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }

        
    }
    /*
    private void Start()
    {
        if(mouseSensitivitySlider != null && invertYToggle != null){
            InitialiseSliders();
        }
    }
    */

    /*
    public void InitialiseSliders(){
        mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 30f);
        invertYToggle.onValueChanged.AddListener(SetInvertY);
        invertYToggle.isOn = PlayerPrefs.GetInt("InvertY", 0) == 1;

        SetMouseSensitivity(mouseSensitivitySlider.value);
        SetInvertY(invertYToggle.isOn);
        SetScreenResolution(ScreenWidth, ScreenHeight);

    }
    
    private void OnDestroy()
    {
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.RemoveListener(SetMouseSensitivity);
        }
    }
    */

 

    public void SetMouseSensitivity(float sensitivity)
    {
        MouseSensitivity = sensitivity;
        Debug.Log($"Mouse Sensitivity set to: {MouseSensitivity}");
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
        

    }

    public void SetInvertY(bool invert)
    {
        InvertY = invert;
        Debug.Log($"Invert Y set to: {InvertY}");
        PlayerPrefs.SetInt("InvertY", invert ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetScreenResolution(float width, float height)
    {
        ScreenWidth = width;
        ScreenHeight = height;
        Debug.Log($"Screen Resolution set to: {ScreenWidth}x{ScreenHeight}");
        Screen.SetResolution((int)width, (int)height, Screen.fullScreen);
        PlayerPrefs.SetFloat("ScreenWidth", width);
        PlayerPrefs.SetFloat("ScreenHeight", height);

        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 30f);
        InvertY = PlayerPrefs.GetInt("InvertY", 0) == 1;
        ScreenWidth = PlayerPrefs.GetFloat("ScreenWidth", 1920);
        ScreenHeight = PlayerPrefs.GetFloat("ScreenHeight", 1080);

        Debug.Log("Settings Loaded:");
        Debug.Log($"Mouse Sensitivity: {MouseSensitivity}");
        Debug.Log($"Invert Y: {InvertY}");
        Debug.Log($"Screen Resolution: {ScreenWidth}x{ScreenHeight}");
    }
    /*
    public void SetMouseSensitivityFromSlider(float value)
    {
        if (mouseSensitivitySlider != null)
        {
            SetMouseSensitivity(mouseSensitivitySlider.value);
        }
    }
    */




    
}
