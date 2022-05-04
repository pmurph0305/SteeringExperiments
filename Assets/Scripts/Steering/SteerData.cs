using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SteerData
{
  [BitMask(typeof(SteeringOperations))]
  public SteeringOperations steeringFlags;

  public bool Should(SteeringOperations op)
  {
    return ((steeringFlags & op) == op);
  }


  [Header("Seek")]
  [Tooltip("Uses pursuit, estimating where the player will be when seeking.")]
  public bool PursuitSeek;
  public float MaxSeekForce = 5f;
  public float VelOffsetMultiplier = 0.5f;

  [Header("Flee")]
  public float MaxFleeForce = 5f;

  [Header("Arrival")]
  public float MaxArrivalForce = 5f;

  [Header("Wander")]
  public float WanderCircleDistance = 1f;
  [Tooltip("Controls the force of the wander.")]
  public float WanderCircleRadius = 1f;



  [Header("Charge")]
  [Tooltip("Minimum dot product result between currently velocity and forward to add additional charging force.")]
  public float MinDotProduct = 0.9f;
  public float MaxChargeForce = 5f;

  [Header("Avoidance")]
  public float MaxAvoidanceForce = 5f;
  public LayerMask collisionMask;
  public Vector3 castOffset = Vector3.up;
  public float sphereRadius = 0.5f;
  public float avoidDistance = 5f;

}

[System.Flags]
public enum SteeringOperations
{
  None = 0,
  Seek = 1 << 0,
  Arrival = 1 << 1,
  Avoidance = 1 << 2,
  Wander = 1 << 3,
  Flee = 1 << 4,
  Charge = 1 << 5,
}