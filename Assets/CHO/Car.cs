using System;
using UnityEngine;

public class Car : MonoBehaviour
{
   private CarMovement _moveCompo;
   private CarInput _inputCompo;
   private CarRotation _rotateCompo;

   private void Awake()
   {
      _moveCompo = GetComponentInChildren<CarMovement>();
      _rotateCompo = GetComponentInChildren<CarRotation>();
      _inputCompo = GetComponent<CarInput>();
   }

   private void Update()
   {
      _moveCompo.SetMove(_inputCompo.MoveDir);
      //_rotateCompo.SetMove(_inputCompo.MoveDir);
   }
}
