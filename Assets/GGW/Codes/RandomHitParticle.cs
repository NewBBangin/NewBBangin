using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GGW.Codes
{
  public class RandomHitParticle : MonoBehaviour
  { 
    [SerializeField] private GameObject[] Feels;

    public void PlayingParticles()
    {
          Feels[Random.Range(0, Feels.Length)].GetComponent<MMF_Player>().PlayFeedbacks();
    }
  
  }
}
