using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class BoidsSimulation
{
  private static BoidsSimulation _instance = new BoidsSimulation();

  public static BoidsSimulation Instance { get { return _instance; } }

  BoidsParameters bp;
  public void SetParameters(BoidsParameters p)
  {
    bp = p;
  }

  public void UpdateBoids(float timeStep)
  {
    Vector3 v1, v2, v3, v4, v5 = v4 = v3 = v2 = v1 = Vector3.zero;
    Vector3 centerOfMass = CalculateCenterOfMass();
    Vector3 averageVelocity = CalculateAverageVelocity();
    int i = 0;
    foreach (var b in Agents)
    {
      inRange = AgentsInRange(b);
      if (i == 0)
      {
        foreach (var a in inRange)
        {
          Debug.DrawLine(b.Position, a.Position, Color.red);
        }
      }
      i++;
      b.centerOfMassVelocity = VelocityToPerceivedCenterOfMass(b, centerOfMass);
      b.avoidVelocity = AvoidOtherBoids(b);
      b.matchVelocity = MatchVelocityWIthNearBoids(b, averageVelocity);
      b.toGoalVelocity = MoveTowardsGoal(b);
      b.boundsVelocity = KeepWithinBounds(b);
      b.Velocity = b.Velocity + (b.centerOfMassVelocity + b.avoidVelocity + b.matchVelocity + b.toGoalVelocity + b.boundsVelocity + RandomVelocity());
      LimitVelocity(b);
      b.Position = b.Position + b.Velocity * timeStep;
    }
  }



  void LimitVelocity(BoidAgent b)
  {
    float speed = b.Velocity.magnitude;
    if (speed > bp.VelocityLimit)
    {
      b.Velocity /= speed;
      b.Velocity *= bp.VelocityLimit;
    }
    // if (b.Velocity.sqrMagnitude > bp.VelocityLimit * bp.VelocityLimit)
    // {
    //   b.Velocity = b.Velocity.normalized * bp.VelocityLimit;
    // }
  }

  List<BoidAgent> inRange = new List<BoidAgent>();
  List<BoidAgent> AgentsInRange(BoidAgent agent)
  {
    inRange.Clear();
    foreach (var b in Agents)
    {
      if (b != agent)
      {
        if (Vector3.Distance(agent.Position, b.Position) < bp.AwarenessRange)
        {
          inRange.Add(b);
        }
      }
    }

    return inRange;
  }


  Vector3 RandomVelocity()
  {
    return new Vector3(UnityEngine.Random.Range(-bp.RandomVelRange, bp.RandomVelRange), UnityEngine.Random.Range(-bp.RandomVelRange, bp.RandomVelRange), 0);
  }

  Vector3 CalculateAverageVelocity()
  {
    Vector3 vel = Vector3.zero;
    foreach (var b in Agents)
    {
      vel += b.Velocity;
    }
    return vel / Agents.Count;
  }

  Vector3 CalculateCenterOfMass()
  {
    Vector3 center = Vector3.zero;
    foreach (var b in Agents)
    {
      center += b.Position;
    }
    return center / Agents.Count;
  }

  Vector3 VelocityToPerceivedCenterOfMass(BoidAgent agent, Vector3 centerOfMass)
  {
    Vector3 com = Vector3.zero;
    if (inRange.Count == 0) return com;
    foreach (var b in inRange)
    {
      com += b.Position;
    }
    com = com / inRange.Count;
    return (com - agent.Position) * bp.toCenterFactor;
    // centerOfMass = centerOfMass * Agents.Count;
    // centerOfMass -= agent.Position;
    // centerOfMass /= (Agents.Count - 1);
    // return (centerOfMass - agent.Position) * bp.toCenterFactor;
  }


  Vector3 AvoidOtherBoids(BoidAgent agent)
  {
    Vector3 c = Vector3.zero;
    // foreach (var b in Agents)
    // {
    //   if (b != agent)
    //   {
    //     if (Vector3.Distance(b.Position, agent.Position) < bp.DistanceCheck)
    //     {
    //       c = c - (b.Position - agent.Position);
    //     }
    //   }
    // }
    foreach (var b in inRange)
    {
      float d = Vector3.Distance(b.Position, agent.Position);
      if (d < bp.DistanceCheck)
      {
        c -= (b.Position - agent.Position);// * (bp.DistanceCheck - d) / bp.DistanceCheck;
      }
    }
    return c * bp.AvoidanceFactor;
  }




  Vector3 MatchVelocityWIthNearBoids(BoidAgent agent, Vector3 avgVelocity)
  {
    Vector3 vel = Vector3.zero;
    if (inRange.Count == 0) return vel;
    foreach (var b in inRange)
    {
      vel += b.Velocity;
    }
    vel = vel / inRange.Count;
    return (vel - agent.Velocity) * bp.MatchVelocityFactor;
    // avgVelocity = avgVelocity * Agents.Count;
    // avgVelocity -= agent.Velocity;
    // avgVelocity = avgVelocity / (Agents.Count - 1);
    // return (avgVelocity - agent.Velocity) * bp.MatchVelocityFactor;
  }

  Vector3 MoveTowardsGoal(BoidAgent agent)
  {
    return (bp.Goal - agent.Position) * bp.GoalFactor;
  }


  Vector3 KeepWithinBounds(BoidAgent agent)
  {
    Vector3 p = agent.Position;
    Vector3 min = bp.Goal - bp.Boundaries;
    Vector3 max = bp.Goal + bp.Boundaries;
    Vector3 v = Vector3.zero;
    if (p.x < min.x)
    {
      v.x = bp.BoundaryFactor;
    }
    else if (p.x > max.x)
    {
      v.x = -bp.BoundaryFactor;
    }
    if (p.y < min.y)
    {
      v.y = bp.BoundaryFactor;
    }
    else if (p.y > max.y)
    {
      v.y = -bp.BoundaryFactor;
    }
    return v;
  }

  List<BoidAgent> Agents = new List<BoidAgent>();

  /// <summary>
  /// Adds an agent to the sim
  /// </summary>
  /// <param name="t"></param>
  /// <returns>id of the agent</returns>
  public BoidAgent AddAgent(Transform t)
  {
    var b = new BoidAgent(t.position, Vector3.zero);
    Agents.Add(b);
    return b;
  }
}

