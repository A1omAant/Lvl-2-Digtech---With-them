using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MasterBinder : MonoBehaviour
{
    public static MasterBinder Instance;
    [Header("References Settings Manager")]
   public Slider MouseSensitivitySlider;
   //public Toggle InvertYToggle;
   [Header("References Audio Manager")]
   public Slider MasterVolumeSlider;
   public Slider MusicVolumeSlider;
   public Slider SFXVolumeSlider;
   public Button RestoreDefaultVolumesButton;

   private void Start(){
        BindSettingsUI();
        //BindAudioUI();
   }

   public void BindSettingsUI(){
        if(SettingsManager.Instance == null) return;


            if(MouseSensitivitySlider != null ){
                AddUISoundTrigger(MouseSensitivitySlider);
                float mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 30f);
                MouseSensitivitySlider.SetValueWithoutNotify(mouseSensitivity);

                MouseSensitivitySlider.onValueChanged.RemoveAllListeners();
                MouseSensitivitySlider.onValueChanged.AddListener(value =>
                {
                    SettingsManager.Instance.SetMouseSensitivity(value);
                    MouseSensitivitySlider.GetComponent<UIButton>().OnSliderValueChanged(MouseSensitivitySlider.value);
                    //UIAudioManager.Instance?.PlaySliderMove();
                });

                

            }
            
            //float invertY = PlayerPrefs.GetInt("InvertY", 0);
            //InvertYToggle.isOn = invertY == 1;
            //InvertYToggle.onValueChanged.AddListener(value =>
            //{
               // SettingsManager.Instance.SetInvertY(value);
                //UIAudioManager.Instance?.PlaySFX("UIToggleChange");
            //});
            
            SettingsManager.Instance.SetMouseSensitivity(MouseSensitivitySlider.value);
            //SettingsManager.Instance.SetInvertY(InvertYToggle.isOn);
        
   }

   public void AddUISoundTrigger(Slider slider){
        var trigger = slider.gameObject.GetComponent<UIButton>();
        if(trigger == null){
            slider.gameObject.AddComponent<UIButton>();
        }
   }

}
