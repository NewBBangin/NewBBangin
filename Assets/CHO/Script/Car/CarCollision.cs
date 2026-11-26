using System;
using Unity.VisualScripting;
using UnityEngine;

public class CarCollision : MonoBehaviour
{
   private void OnCollisionEnter(Collision collision)
   {
      DeleteOBJ(collision);
   }

   private void DeleteOBJ(Collision obj)
   {
      
   }
}
