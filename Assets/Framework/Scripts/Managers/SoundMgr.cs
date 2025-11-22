using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMgr : UnitySingleton<SoundMgr>
{
    private const int  MAX_SOUND = 8;//同时播放8个音效
    private const string MusicMuteKey = "isMusicMute";
    private const string SoundMuteKey = "isSoundMute";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundVolumeKey = "SoundVolume";
    
    private List<AudioSource> sounds;
    private int currIndex;
    private AudioSource musicSource;
    
    private int isMusicMute = 0;
    private int isSoundMute = 0;
    
    public void Init()
    {
        this.sounds = new List<AudioSource>();
        for (int i = 0; i < MAX_SOUND; i++)
        {
            sounds.Add(this.gameObject.AddComponent<AudioSource>());
        }
        
        this.musicSource = this.gameObject.AddComponent<AudioSource>();
        this.currIndex = 0;


        this.isMusicMute = 0;
        this.isSoundMute = 0;
        if (PlayerPrefs.HasKey(MusicMuteKey))
        {
            this.isMusicMute = PlayerPrefs.GetInt(MusicMuteKey);
        }

        if (PlayerPrefs.HasKey(SoundMuteKey))
        {
            this.isSoundMute = PlayerPrefs.GetInt(SoundMuteKey);
        }
        
        float musicVolume = 1.0f;
        float soundVolume = 1.0f;
        if (PlayerPrefs.HasKey(MusicVolumeKey))
        {
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey);
        }
        this.SetMusicVolume(musicVolume);

        if (PlayerPrefs.HasKey(SoundVolumeKey))
        {
            soundVolume = PlayerPrefs.GetFloat(SoundVolumeKey);
        }
        this.SetSoundVolume(soundVolume);
    }

    public void PlayMusic(string musicName, bool loop = true)
    {
        AudioClip clip = ResMgr.Instance.LoadAssetSync<AudioClip>(musicName);
        if(clip == null) return;
        
        this.musicSource.clip = clip;
        this.musicSource.loop = loop;
        if (this.isMusicMute != 0) return;
        
        this.musicSource.Play();
    }

    public void StopMusic()
    {
        this.musicSource.Stop();
    }
    
    public int PlaySound(string soundName, bool loop = false)
    {
        if (this.isSoundMute != 0)
        {
            return -1;
        }
        
        AudioClip clip = ResMgr.Instance.LoadAssetSync<AudioClip>(soundName);
        if (clip == null)
        {
            return -1;
        }
        int soundId = this.currIndex;
        AudioSource audioSource = this.sounds[this.currIndex++];
        
        this.currIndex = (this.currIndex % MAX_SOUND);
        audioSource.clip = clip;
        audioSource.loop = loop;

        if (this.isSoundMute != 0)
        {
            return soundId;
        }
        audioSource.Play();
        
        return soundId;
    }
    
    public int PlayOneShot(string soundName, bool loop = false)
    {
        if(this.isSoundMute != 0) return -1;
        
        AudioClip clip = ResMgr.Instance.LoadAssetSync<AudioClip>(soundName);
        if (clip == null)
        {
            return -1;
        }
        
        int soundId = this.currIndex;
        AudioSource audioSource = this.sounds[this.currIndex++];
        
        this.currIndex = (this.currIndex % MAX_SOUND);
        audioSource.clip = clip;
        audioSource.loop = loop;

        if (this.isSoundMute != 0)
        {
            return soundId;
        }
        audioSource.PlayOneShot(clip);
        
        return soundId;
    }

    public void StopSound(int soundId)
    {
        if (soundId < 0 || soundId >= MAX_SOUND) return;
        this.sounds[soundId].Stop();
    }

    public void StopAllSounds()
    {
        for (int i = 0; i < MAX_SOUND; i++)
        {
            this.sounds[i].Stop();
        }
    }
    
    public void SetMusicMute(bool isMute)
    {
        bool isMusicMute = this.isMusicMute != 0;
        if (isMusicMute == isMute)
        {
            return;
        }
        this.isMusicMute = isMute ? 1 : 0;
        PlayerPrefs.SetInt(MusicMuteKey, this.isMusicMute);

        this.musicSource.mute = isMute;
    }

    public void SetSoundMute(bool isMute)
    {
        bool isSoundMute = this.isSoundMute != 0; 
        if (isSoundMute == isMute) 
        {
            return;
        }
        
        this.isSoundMute = isMute ? 1 : 0;
        PlayerPrefs.SetInt(SoundMuteKey, this.isSoundMute);
        
        //音效都是短暂的，所有就不管它。
        for (int i = 0; i < MAX_SOUND; i++)
        {
            this.sounds[i].mute = isMute;
        }
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        this.musicSource.volume = volume;
        
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }
    
    public void SetSoundVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        for (int i = 0; i < MAX_SOUND; i++)
        {
            this.sounds[i].volume = volume;
        }
        
        PlayerPrefs.SetFloat(SoundVolumeKey, volume);
    }
}
