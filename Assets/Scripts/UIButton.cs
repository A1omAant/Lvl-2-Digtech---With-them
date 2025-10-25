using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{

    private Slider slider;
    
    private float lastSliderValue;
    private float lastSoundTime = 0f;
    [SerializeField] private float soundCooldown = 0.1f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIAudioManager.Instance?.PlaySFX("UIHover");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIAudioManager.Instance?.PlaySFX("UIClick");
    }

    private void Awake()
    {
        slider = GetComponent<Slider>();
        if(slider == null ) {
        slider = GetComponentInChildren<Slider>();
        }
        
        
        if (slider != null)
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            Debug.Log("Listener added to slider.");
            lastSliderValue = slider.value;
        }
        
    }
    void Start(){
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

     private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            Debug.Log("Listener removed from slider.");
        }
    }

    public void OnSliderValueChanged(float newValue)
    {
        if (Time.time - lastSoundTime < soundCooldown)
        return;

        lastSoundTime = Time.time;

        


        UIAudioManager.Instance?.PlaySliderMove();

       
    }

    

   
}