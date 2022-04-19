using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BoidsParameters
{
  public float toCenterFactor = 0.01f;
  public float DistanceCheck = 1;
  public float MatchVelocityFactor = 0.125f;
  public float AvoidanceFactor = 0.05f;

  public float AwarenessRange = 20f;
  public float VelocityLimit = 10f;
  public Vector3 Goal = Vector3.zero;
  public float GoalFactor = 0.02f;
  public float BoundaryFactor = 10;
  public Vector3 Boundaries = Vector3.one * 10;
}
