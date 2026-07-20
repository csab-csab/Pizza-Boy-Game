using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    #region  clips
    [SerializeField] List<AudioClip> clipList = new List<AudioClip>();

    AudioSource source0;

    RadioManager radioManager;

    //Audio Listeners on the cars
    [SerializeField] List<AudioSource> carSources;
    private float original_car_source_vol;

    #endregion

    #region  Ambient Sound Variables
    [Header("Ambient Sound Sources")]
    [SerializeField] AudioSource windSource;
    [SerializeField] AudioSource birdsSource;
    [SerializeField] AudioSource cricketsSource;
    [Header("Ambient Sound Properties")]
    
    [SerializeField][Range(0f, 1f)] float LowerWindVolume;
    [Range(0.1f, 1f)] float TargetWindVolume;
    private float CurrentWindVolume;
    [SerializeField][Range(0.1f, 1f)] float UpperWindVolume;
    
    [Space(10)]
    
    [SerializeField][Range(0.1f, 1f)] float LowerWindPitch;
    [Range(0.1f, 1f)] float TargetWindPitch;
    private float CurrentWindPitch;
    [SerializeField][Range(0.1f, 2f)] float UpperWindPitch;

    [Space(10)]
    [SerializeField][Range(-0.7f, -0.1f)] float LeftPan;
    [Range(-1f, 1f)] float TargetWindPan;
    private float CurrentWindPan;
    [SerializeField][Range(0.1f, 0.7f)] float RightPan;

    [Space(10)]
    [SerializeField][Range(0f, 1f)] float LowerCricketsVolume;
    [Range(0.1f, 1f)] float TargetCricketsVolume;
    [SerializeField][Range(0.1f, 1f)] float UpperCricketsVolume;
    
    [SerializeField][Range(0.1f, 1f)] float LowerCricketsPitch;
    [Range(0.7f, 2f)] float TargetCricketsPitch;
    [SerializeField][Range(1f, 2f)] float UpperCricketsPitch;
   
    bool smoothVaryWindNoise = false;
    
    #endregion
   
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        
        SoundManager.instance = this;
    }

    private void Start()
    {
        source0 = GetComponent<AudioSource>();
        radioManager = GetComponentInChildren<RadioManager>();

        QuestManager.OnQuestCompleted += PlayQuestCompleteChime;
        GameManager.OnDeliveryCompleted += PlayQuestCompleteChime;

        InvokeRepeating("VaryWindNoiseProperties", 2f, 30f);
    }

    private void Update()
    {
        if(smoothVaryWindNoise)
        {
            SmoothVaryWindNoise();
        }
    }

    #region  General Audio Stuff

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

    public void ModifyMasterVolume(float volume)
    {
        AudioListener.volume = volume;
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
    #endregion

    #region  Ambient Sounds
    public void ToggleAmbientSounds(bool on)
    {
        if(windSource == null || cricketsSource == null)
        {
            Debug.LogError("One or more ambient audioSource is null. \n Set it in the sound manager inspector.");
        }

        switch(on)
        {
            case true:
                windSource.Play();
                cricketsSource.Play();
                break;
            case false:
                windSource.Stop();
                cricketsSource.Stop();
                break;
        }
        
    }

    private void VaryWindNoiseProperties()
    {
        if(windSource == null)
        {
            Debug.LogError("Wind source is null. Assign in inspector in sound manager.");
            return;
        }

        float newVolume;
        float minDiff = 0.07f;
        do
        {
            float randomVolume = UnityEngine.Random.Range(LowerWindVolume, UpperWindVolume);
            newVolume = randomVolume;
        }
        while(Mathf.Abs(newVolume-TargetWindVolume) < minDiff);
        
        TargetWindVolume = newVolume;
        

        float newPitch;
        do
        {
            float randomPitch = UnityEngine.Random.Range(LowerWindPitch, UpperWindPitch);
            newPitch = randomPitch;
        }
        while(Mathf.Abs(newPitch-TargetWindPitch) < minDiff);
        
        TargetWindPitch = newPitch;
        

        
        float randomPan = UnityEngine.Random.Range(LeftPan, RightPan);
        TargetWindPan = randomPan;
    
        smoothVaryWindNoise = true;
    
    }

    private void VaryCricketNoiseProperties()
    {
        if(cricketsSource == null)
        {
            Debug.LogError("Cricket source is null. Assign in inspector in sound manager.");
            return;
        }

        float newVolume;
        float minDiff = 0.07f;
        do
        {
            float randomVolume = UnityEngine.Random.Range(LowerCricketsVolume, UpperCricketsVolume);
            newVolume = randomVolume;
        }
        while(Mathf.Abs(newVolume-TargetCricketsVolume) < minDiff);
        
        TargetCricketsVolume = newVolume;
        

        float newPitch;
        do
        {
            float randomPitch = UnityEngine.Random.Range(LowerCricketsPitch, UpperCricketsPitch);
            newPitch = randomPitch;
        }
        while(Mathf.Abs(newPitch-TargetCricketsPitch) < minDiff);
        
        TargetCricketsPitch = newPitch; 

        cricketsSource.volume = TargetCricketsVolume;
        cricketsSource.pitch = TargetCricketsPitch;
    }

    private void SmoothVaryWindNoise()
    {
       CurrentWindVolume = Mathf.MoveTowards(CurrentWindVolume, TargetWindVolume, 0.2f * Time.deltaTime);
       CurrentWindPitch = Mathf.MoveTowards(CurrentWindPitch, TargetWindPitch, 0.1f * Time.deltaTime);
       CurrentWindPan = Mathf.MoveTowards(CurrentWindPan, TargetWindPan, 0.1f * Time.deltaTime); 

       windSource.volume = CurrentWindVolume;
       windSource.pitch = CurrentWindPitch;
       windSource.panStereo = CurrentWindPan;

       if(Mathf.Approximately(CurrentWindVolume, TargetWindVolume) 
       && Mathf.Approximately(CurrentWindPitch, TargetWindPitch)
       && Mathf.Approximately(CurrentWindPan, TargetWindPan))
        {
           smoothVaryWindNoise = false; 
        }
    }

    public void SwapTimeOfDayAmbienece(bool day)
    {
        if(day)
        {
            CancelInvoke("VaryCricketNoiseProperties");
            cricketsSource.Stop();
            birdsSource.Play();
        }
        else
        {
            birdsSource.Stop();
            cricketsSource.Play();
            InvokeRepeating("VaryCricketNoiseProperties", 2f, 5f);
        }
    }
    #endregion

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
        //don't play audio as car is off
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
