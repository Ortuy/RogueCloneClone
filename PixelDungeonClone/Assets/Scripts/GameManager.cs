using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int seed;
    public int currentFloor = 1;
    public bool mapRevealed;
    public int guaranteedSpecialRoom = -1;
    public Item altarDrop;

    public float backgroundVolume, sfxVolume, volume;
    public bool postProcessingOn;

    [SerializeField]
    private Toggle postProcessingToggle;

    [SerializeField]
    private Camera playerCamera;
    private UnityEngine.Rendering.Universal.UniversalAdditionalCameraData mainCameraExtraData;

    [SerializeField]
    private Slider volumeSliderBG, volumeSliderSFX, volumeSliderMaster;
    [SerializeField]
    private AudioMixer mixer;

    [SerializeField]
    private Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResIndex = 0;
       
        if(!PlayerPrefs.HasKey("fullscreen"))
        {
            PlayerPrefs.SetInt("fullscreen", 1);
            PlayerPrefs.SetInt("resX", Screen.currentResolution.width);
            PlayerPrefs.SetInt("resY", Screen.currentResolution.width);
        }

        int resX = PlayerPrefs.GetInt("resX");
        int resY = PlayerPrefs.GetInt("resY");

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == resX
                && resolutions[i].height == resY)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();

        if(!PlayerPrefs.HasKey("volume"))
        {
            PlayerPrefs.SetFloat("volume", 1f);
            PlayerPrefs.SetFloat("volumeBG", 1f);
            PlayerPrefs.SetFloat("volumeSFX", 1f);
            PlayerPrefs.SetInt("postprocess", 1);
            PlayerPrefs.Save();
        }

        volume = PlayerPrefs.GetFloat("volume");
        volumeSliderMaster.value = volume;
        SetVolume(volume);
        backgroundVolume = PlayerPrefs.GetFloat("volumeBG");
        volumeSliderBG.value = backgroundVolume;
        SetBGVolume(backgroundVolume);
        sfxVolume = PlayerPrefs.GetFloat("volumeSFX");
        volumeSliderSFX.value = sfxVolume;
        SetSFXVolume(sfxVolume);

        postProcessingOn = PlayerPrefs.GetInt("postprocess") == 1 ? true : false;
        mainCameraExtraData = playerCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        mainCameraExtraData.renderPostProcessing = postProcessingOn;
        postProcessingToggle.isOn = postProcessingOn;
        postProcessingToggle.onValueChanged.AddListener(delegate { SetPostProcessing(postProcessingToggle); });

        bool fullscreenOn = PlayerPrefs.GetInt("fullscreen") == 1 ? true : false;
        Screen.fullScreen = fullscreenOn;
    }

    public void SetPostProcessing(Toggle toggle)
    {
        var value = toggle.isOn;
        postProcessingOn = value;
        mainCameraExtraData.renderPostProcessing = value;
        PlayerPrefs.SetInt("postprocess", value ? 1 : 0);
    }

    public void SetVolume(float value)
    {
        PlayerPrefs.SetFloat("volume", value);
        mixer.SetFloat("masterVol", Mathf.Log10(value) * 20);
    }

    public void SetBGVolume(float value)
    {
        PlayerPrefs.SetFloat("volumeBG", value);
        mixer.SetFloat("bgVol", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("volumeSFX", value);
        mixer.SetFloat("sfxVol", Mathf.Log10(value) * 20);
    }

    public void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
        PlayerPrefs.SetInt("fullscreen", value ? 1 : 0);
    }

    public void SetResolution (int value)
    {
        Resolution res = resolutions[value];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("resX", res.width);
        PlayerPrefs.SetInt("resY", res.height);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
