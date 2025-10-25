using UnityEngine;

public class MasterManager : MonoBehaviour
{
    private static MasterManager instance;
    public static MasterManager Instance => instance; // Singleton instance

    [Header("References")]
    public UIAudioManager uiAudioManager;
    public SettingsManager settingsManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
       
}
