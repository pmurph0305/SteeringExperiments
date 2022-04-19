using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BoidAgent
{
  public Vector3 Velocity;
  public Vector3 Position;
  public BoidAgent(Vector3 position, Vector3 velocity)
  {
    Position = position;
    Velocity = velocity;
  }


  public Vector3 centerOfMassVelocity;
  public Vector3 avoidVelocity;
  public Vector3 matchVelocity;
  public Vector3 toGoalVelocity;
  public Vector3 boundsVelocity;
}