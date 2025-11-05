using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSControl : MonoBehaviour
{
  public ParticleSystem dustParticleSystem;

  public void PlayParticleSystem(){
    dustParticleSystem.Play();
  }
  
}
