using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#region List of lists
    //this looks complicated but it essentially creates a list of a list;
    //one list is used for the list of radios and the other
    //the list of tracks found on each radio

    //it works by creating a class with a list of audioclips and then creating another class
    //which creates a list of the previous tracklist class; creating a list of track lists.

    [System.Serializable]
    public class TrackList
    {
        public List<AudioClip> Tracks;
    }


    [System.Serializable]
    public class Radios
    {
        public List<TrackList> radios;
    }
#endregion

public class RadioManager : MonoBehaviour
{
    //-1 = radio off
    [SerializeField]private int currentRadio = -1; 

    //-1 so all tracks have an equal chance of playing on first switch
    private int currentTrack = -1;

    //Make sure to set these in the editor
    [SerializeField] private AudioSource RadioSource;

    [SerializeField] private AudioSource RadioStatic;

    public Radios Radio = new Radios();

    [SerializeField] bool isMuted;

    private void Start()
    {
        currentRadio = -1;
    }

    public void SwitchRadio()
    {
        RadioSource.Stop();
        RadioStatic.Play();

        currentRadio++;
        currentTrack = -1;

        //if the number of radios is reached, loop around and turn radio off
        if (currentRadio > Radio.radios.Count - 1)
        {
            currentRadio = -1;
            CanvasController.instance.UpdateRadioStationUi(currentRadio, "Radio Off");
            return;
        }

        ChooseTrack(currentTrack);
    }

    private void ChooseTrack(int CurrentTrack) 
    {
       int trackNum = Random.Range(0, Radio.radios[currentRadio].Tracks.Count);

       //if track is not the same as the previous one, play it
       if(trackNum != CurrentTrack) 
       {
         currentTrack =  trackNum;
         PlayRadio();
       }
       else 
       {
         ChooseTrack(currentTrack);
       }
    }


    private void PlayRadio() 
    {
        RadioSource.clip = Radio.radios[currentRadio].Tracks[currentTrack];
        RadioSource.Play();
        CanvasController.instance.UpdateRadioStationUi(currentRadio, Radio.radios[currentRadio].Tracks[currentTrack].name);
    }

    public void MuteRadio(bool muted) 
    {
        isMuted = muted;
        RadioSource.mute = muted;
    }


    private void Update()
    {
        if (Input.GetKeyDown("r") && GameManager.instance.gameState == GameManager.GameState.Playing)
        {
            SwitchRadio();
        }

        //radio isn't playing music and is not off
        if (!RadioSource.isPlaying && currentRadio != -1)
        {
            ChooseTrack(currentTrack);
        }
    }
}
