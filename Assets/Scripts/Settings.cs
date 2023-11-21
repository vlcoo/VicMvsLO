using System.Text.RegularExpressions;
using NSMB.Utils;
using UnityEngine;
using UnityEngine.Audio;

using Photon.Pun;
using UnityEngine.Serialization;

public class Settings : Singleton<Settings> {
    public AudioMixer mixer;

    private float _volumeMaster, _volumeMusic, _volumeSFX;
    public float VolumeMaster {
        get => _volumeMaster;
        set {
            _volumeMaster = Mathf.Clamp01(value);
            ApplyVolumeSettings();
        }
    }
    public float VolumeSFX {
        get => _volumeSFX;
        set {
            _volumeSFX = Mathf.Clamp01(value);
            ApplyVolumeSettings();
        }
    }
    public float VolumeMusic {
        get => _volumeMusic;
        set {
            _volumeMusic = Mathf.Clamp01(value);
            ApplyVolumeSettings();
        }
    }

    private bool _rumbleController;
    public bool rumbleController
    {
        get => _rumbleController;
        set
        {
            _rumbleController = value;
            GlobalController.Instance.rumbler.rumbleEnabled = value;
        }
    }

    public string nickname;
    public int character, skin;
    public bool ndsResolution = false, fireballFromSprint = true, vsync = false, fourByThreeRatio = false;
    public bool scoreboardAlways = false, filter = true, reduceUIAnims = true, onScreenControlsAlways;

    public void Awake() {
        if (!InstanceCheck())
            return;

        Instance = this;
        LoadSettingsFromPreferences();
        ApplyVolumeSettings();
    }

    public void LoadSettingsFromPreferences() {
        nickname = PlayerPrefs.GetString("Nickname");
        if (string.IsNullOrEmpty(nickname))
            nickname = "Player" + Random.Range(1000, 10000);

        VolumeSFX = PlayerPrefs.GetFloat("volumeSFX", 0.5f);
        VolumeMusic = PlayerPrefs.GetFloat("volumeMusic", 0.25f);
        VolumeMaster = PlayerPrefs.GetFloat("volumeMaster", 1);
        ndsResolution = PlayerPrefs.GetInt("NDSResolution", 0) == 1;
        fireballFromSprint = PlayerPrefs.GetInt("FireballFromSprint", 1) == 1 && Utils.GetDeviceType() != Utils.DeviceType.MOBILE;
        rumbleController = PlayerPrefs.GetInt("RumbleController", 1) == 1;
        onScreenControlsAlways = PlayerPrefs.GetInt("ForceOnScreenControls", 0) == 1 || Utils.GetDeviceType() == Utils.DeviceType.MOBILE;
        vsync = PlayerPrefs.GetInt("VSync", 0) == 1;
        fourByThreeRatio = PlayerPrefs.GetInt("NDS4by3", 0) == 1;
        scoreboardAlways = PlayerPrefs.GetInt("ScoreboardAlwaysVisible", 1) == 1;
        reduceUIAnims = PlayerPrefs.GetInt("ReduceUIAnims", 0) == 1;
        filter = PlayerPrefs.GetInt("ChatFilter", 1) == 1;
        character = PlayerPrefs.GetInt("Character", 0);
        skin = PlayerPrefs.GetInt("Skin", 0);
    }
    public void SaveSettingsToPreferences() {
        PlayerPrefs.SetString("Nickname", Regex.Replace(PhotonNetwork.NickName, "\\(\\d*\\)", ""));
        PlayerPrefs.SetFloat("volumeSFX", VolumeSFX);
        PlayerPrefs.SetFloat("volumeMusic", VolumeMusic);
        PlayerPrefs.SetFloat("volumeMaster", VolumeMaster);
        PlayerPrefs.SetInt("NDSResolution", ndsResolution ? 1 : 0);
        PlayerPrefs.SetInt("FireballFromSprint", fireballFromSprint ? 1 : 0);
        PlayerPrefs.SetInt("RumbleController", rumbleController ? 1 : 0);
        PlayerPrefs.SetInt("ForceOnScreenControls", onScreenControlsAlways ? 1 : 0);
        PlayerPrefs.SetInt("VSync", vsync ? 1 : 0);
        PlayerPrefs.SetInt("NDS4by3", fourByThreeRatio ? 1 : 0);
        PlayerPrefs.SetInt("ScoreboardAlwaysVisible", scoreboardAlways ? 1 : 0);
        PlayerPrefs.SetInt("ReduceUIAnims", reduceUIAnims ? 1 : 0);
        PlayerPrefs.SetInt("ChatFilter", filter ? 1 : 0);
        PlayerPrefs.SetInt("Character", character);
        PlayerPrefs.SetInt("Skin", skin);
        PlayerPrefs.Save();
    }

    void ApplyVolumeSettings() {
        mixer.SetFloat("MusicVolume", Mathf.Log10(VolumeMusic) * 20);
        mixer.SetFloat("SoundVolume", Mathf.Log10(VolumeSFX) * 20);
        mixer.SetFloat("MasterVolume", Mathf.Log10(VolumeMaster) * 20);
    }
}