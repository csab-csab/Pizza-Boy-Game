using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] List<AudioClip> clipList = new List<AudioClip>();

    AudioSource source0;

    RadioManager radioManager;

    //Audio Listeners on the cars
    [SerializeField] List<AudioSource> carSources;
    private float original_car_source_vol;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        
        SoundManager.instance = this;
        AudioListener.volume = 0.5f;
    }

    private void Start()
    {
        source0 = GetComponent<AudioSource>();
        radioManager = GetComponentInChildren<RadioManager>();

        QuestManager.OnQuestCompleted += PlayQuestCompleteChime;
    }

    /// <summary>
    /// Sets up the specified audio clip/// </summary>
    /// <param name="source"></param>
    /// <param name="clip"></param>
    /// <param name="loop"></param>
    public void SetAudioClip(AudioSource source, AudioClip clip, bool loop) 
    {
        source.playOnAwake = false;
        source.loop = loop;
        source.clip = clip;
    }

    public void PlayAudioSource(AudioSource source) 
    {
        if (!source.isPlaying)
        {
            source.Play();
        }
        else 
        {
            //Debug.LogWarning("This source is already Playing audio");
            return;
        }
    }

    public void StopAudioSource(AudioSource source) 
    {
        if (source.isPlaying)
        {
            source.Stop();
        }
        else
        {
            Debug.LogWarning("This source is already Playing audio");
            return;
        }

    }
    
    public void ModifyVolume(AudioSource audioSource, float volume)
    {
        audioSource.volume = volume;
    }

    public void ModifyPitch(AudioSource audioSource, float pitch) 
    {
        if (audioSource == null) 
        {
            Debug.LogError("AudioSource Doesnt Exsit!");
            return; 
        }
        audioSource.pitch = pitch;
    }


    #region Special

   /// <summary>
   /// This is a special function which plays the car startup sound and once its over plays the car running sound
   /// </summary>
   /// <param name="audioSource"></param>
   /// <returns></returns>
    public IEnumerator PlayCarStartSound(AudioSource audioSource, AudioClip carRunning) 
    {
        audioSource.loop = false;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(audioSource.clip.length);
        audioSource.Stop();
        audioSource.loop = true;
        audioSource.clip = carRunning;
        PlayAudioSource(audioSource);
    }

    public IEnumerator PlayCarStopSound(AudioSource audioSource, AudioClip carRunning) 
    {
        audioSource.loop = false;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(audioSource.clip.length);
        audioSource.Stop();
        audioSource.loop = true;
        audioSource.clip = carRunning;
        //dont play audio as car is off
    }

    public void MuteAllAudio() 
    {
        AudioListener.pause = true;
    }

    public void UnMuteAllAudio() 
    {
        AudioListener.pause = false;
    }

    public void ToggleMuteAudioForPause(bool mute) 
    { 
        ToggleMuteCarSounds(mute);
        MuteRadio(mute);
        
    }


    private void PlayQuestCompleteChime() 
    { 
        //I want this to overwrite other audio so only checking if null 
        if(source0 == null) 
        {
            Debug.LogError("AudioSource 0 is null.");
            return;
        }
        
        source0.clip = clipList[0];

        source0.Play();

        StartCoroutine(TempMuteRadio(source0.clip.length));
    }

    public void PlayCountdownAudio() 
    {
        if (source0 == null || source0.isPlaying)
        {
            Debug.LogError("AudioSource 0 is either null or is already playing audio!");
            return;
        }

        source0.clip = clipList[1];

        source0.Play();

        StartCoroutine(TempMuteRadio(source0.clip.length));
    }
    /// <summary>
    /// Mute Radio for a specified lenght of time. For example, while a sound effect plays.
    /// </summary>
    /// <param name="lengthToMute"></param>
    /// <returns></returns>
    IEnumerator TempMuteRadio(float lengthToMute) 
    { 
        if(radioManager == null) 
        {
            Debug.LogError("RadioManager is null.");
            yield return null;
        }

        radioManager.MuteRadio(true);

        yield return new WaitForSecondsRealtime(lengthToMute);

        radioManager.MuteRadio(false);
    }

    /// <summary>
    /// Reset before adding resets the carSources list found in the sound manager script.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="resetBeforeAdding"></param>
    public void GetCarAudioSources(AudioSource source, bool resetBeforeAdding = false) 
    {
        
        if (resetBeforeAdding) 
        { 
          carSources.Clear();
        }
        
        carSources.Add(source);
    }


    private void ToggleMuteCarSounds(bool mute) 
    {
        if (mute)
        {
            foreach (AudioSource _source in carSources)
            {
              _source.Pause();
            }
        }
        else
        {
            foreach (AudioSource _source in carSources)
            {
              _source.UnPause();
            }
        }
    }

    private void MuteRadio(bool mute) 
    { 
        radioManager.MuteRadio(mute);
    }
    #endregion
}
