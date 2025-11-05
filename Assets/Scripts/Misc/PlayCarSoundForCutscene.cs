using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCarSoundForCutscene : MonoBehaviour
{
    AudioSource CarSoundSource;
    
    // Start is called before the first frame update
    void Start()
    {
       CarSoundSource = this.GetComponent<AudioSource>(); 
       CarSoundSource.Play();
       CarSoundSource.mute = true;
    }

  
}
