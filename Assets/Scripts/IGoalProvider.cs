using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGoalProvider
{
  Transform GetTransform();
  Vector3 GetPosition();
  Vector3 GetVelocity();
}
