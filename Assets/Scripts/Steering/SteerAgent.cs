using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SteerAgent
{
  public Transform transform;
  public Vector3 position = Vector3.zero;
  public Vector3 velocity = Vector3.zero;

  public SteerAgent(Transform t)
  {
    transform = t;
    position = t.position;
    velocity = Vector3.zero;
  }
}
