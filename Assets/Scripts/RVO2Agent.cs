using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RVO;
using System;
using Vector2 = UnityEngine.Vector2;
public class RVO2Agent : MonoBehaviour
{
  [Header("RVO2 Agent")]

  [SerializeField] RVO2AgentParams agentParams;



  [Header("Debug")]
  [SerializeField] Vector3 simPosition;
  [SerializeField] private int agentId;
  [SerializeField] bool RandomizeGoal;
  [SerializeField] bool UseGoalProvider;
  [SerializeField] float RandomSize = 125;
  [SerializeField] Vector3 GoalPosition;

  NavMeshPath path;
  [SerializeField] int pathLength;
  [SerializeField] int currentPathIndex = -1;
  [SerializeField] int nextPathIndex;
  [SerializeField] float totalDistanceToNext;
  [SerializeField] float remainingDistanceToNext;
  [SerializeField] Vector3 directionFromCorners;


  private void Start()
  {
    agentId = agentParams.AddAgent(GetPosition());
    RVO2Simulator.OnSimulationStepped += OnSimulationSteppedHandler;
    path = new NavMeshPath();
    GetNewPath();
  }

  bool HasGoalMoved()
  {
    if (!UseGoalProvider) { return false; }
    if (UseGoalProvider)
    {
      float d = Vector3.Distance(GoalPosition, GoalProvider.Goal);
      if (d > 10f)
      {
        return true;
      }
    }
    return false;
  }

  static readonly Unity.Profiling.ProfilerMarker pathMarker = new Unity.Profiling.ProfilerMarker("GetPath");

  void GetNewPath()
  {
    // pathMarker.Begin();
    // NavMeshHit hit;
    // if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
    // {
    //   if (RandomizeGoal)
    //   {
    //     GoalPosition = new Vector3(Random.Range(-RandomSize, RandomSize), 0, Random.Range(-RandomSize / 2, RandomSize / 2));
    //   }
    //   if (UseGoalProvider)
    //   {
    //     GoalPosition = GoalProvider.Goal;
    //   }
    //   transform.position = hit.position;
    //   if (NavMesh.CalculatePath(hit.position, GoalPosition, NavMesh.AllAreas, path))
    //   {
    //     // Debug.Log("Got a path.");
    //     currentPathIndex = -1;
    //     nextPathIndex = 0;
    //     StepToNextCorner();
    //     pathLength = path.corners.Length;
    //   }
    //   else
    //   {
    //     // Debug.LogWarning("No path found?");
    //     pathLength = -1;
    //   }
    // }
    // else
    // {
    //   // Debug.Log("Failed sample.");
    // }
    // pathMarker.End();
    GoalPosition = GoalProvider.Goal;
    PathSupplier.RequestPath(this.transform, GetGoal, OnGetNewPath);
  }

  Vector3 GetGoal()
  {
    return GoalProvider.Goal;
  }


  void OnGetNewPath(NavMeshPath p)
  {
    // Debug.Log("Get new path?");
    path = p;
    currentPathIndex = -1;
    nextPathIndex = 0;
    pathLength = path.corners.Length;
    StepToNextCorner();
  }

  // private void Update()
  // {
  //   // Simulator.Instance.setAgentPosition(agentId, GetPosition());
  //   // if (nextPathIndex < path.corners.Length)
  //   // {
  //   //   Simulator.Instance.setAgentPrefVelocity(agentId, GetPreferedVelocity());
  //   // }
  //   Simulator.Instance.setAgentPrefVelocity(agentId, GetPreferedVelocity());
  // }

  void StepToNextCorner()
  {
    currentPathIndex++;
    nextPathIndex++;
    if (nextPathIndex >= path.corners.Length)
    {
      GetNewPath();
    }
    else
    {
      remainingDistanceToNext = Vector3.Distance(path.corners[currentPathIndex], path.corners[nextPathIndex]);
      totalDistanceToNext = remainingDistanceToNext;
      directionFromCorners = (path.corners[nextPathIndex] - path.corners[currentPathIndex]).normalized;
    }
  }

  private void OnDestroy()
  {
    RVO2Simulator.OnSimulationStepped -= OnSimulationSteppedHandler;
  }

  static readonly Unity.Profiling.ProfilerMarker steppedMarker = new Unity.Profiling.ProfilerMarker("Agent-SteppedHandler");
  void OnSimulationSteppedHandler()
  {
    steppedMarker.Begin();
    // agentParams.DebugAgent();

    // travel the path.
    Vector3 simulationVelocity = GetSimulationVelocity();

    Vector3 prev = transform.position;
    transform.position += simulationVelocity * Time.deltaTime;
    NavMeshHit sample;
    if (NavMesh.SamplePosition(transform.position, out sample, 1f, NavMesh.AllAreas))
    {
      // Vector3 p = transform.position;
      // p.y = sample.position.y;
      transform.position = sample.position;
    }
    Vector3 vectorTraveled = transform.position - prev;
    remainingDistanceToNext -= vectorTraveled.magnitude;
    simPosition = GetSimulationPosition();
    // transform.position = simPosition;
    // Debug.Log($"transform:{transformPosition} sim:{simPosition}");

    if (nextPathIndex < path.corners.Length)
    {
      // here we essentially are just seeing if we've moved enough to try going to the next corner.
      if (remainingDistanceToNext <= 0)
      {
        // Debug.Log("Sqr distance to next");
        StepToNextCorner();
      }
      // no longer needed with remaining distance?
      // else
      // {
      //   Debug.Log("distance to thing");
      //   float d = Vector3.Distance(transform.position, path.corners[nextPathIndex]);
      //   if (d < agentParams.Radius * 2)
      //   {
      //     StepToNextCorner();
      //   }
      // }
      //calculate deviance from path.
      // distance from line
      if (nextPathIndex < path.corners.Length)
      {
        Vector3 v = transform.position - path.corners[currentPathIndex];
        Vector3 projection = Vector3.Project(v, directionFromCorners);
        float dist = Vector3.Distance(path.corners[currentPathIndex] + projection, transform.position);
        if (dist > agentParams.Radius * 2 + Mathf.Abs(path.corners[currentPathIndex].y - path.corners[nextPathIndex].y))
        {
          // Debug.Log("New path from dist from path.");
          GetNewPath();
        }
      }
      // by angle.
      // float a = Vector3.Angle(vectorTraveled, directionFromCorners);
      // if (a > 45f)
      // {
      //   Debug.Log("Angle deviance?");
      //   GetNewPath();
      // }

    }
    else
    {
      // we need a new path because we've reached the end.
      GetNewPath();
    }





    if (HasGoalMoved())
    {
      GetNewPath();
    }

    Simulator.Instance.setAgentPrefVelocity(agentId, GetPreferedVelocity());
    Simulator.Instance.setAgentPosition(agentId, new RVO.Vector2(transform.position.x, transform.position.z));
    steppedMarker.End();
  }


  Vector2 GetPosition()
  {
    return new Vector2(transform.position.x, transform.position.z);
  }

  Vector2 GetVelocity()
  {
    return Vector2.one;
  }

  Vector3 GetSimulationVelocity()
  {
    Vector2 dir = Simulator.Instance.getAgentVelocity(agentId);
    return new Vector3(dir.x, 0, dir.y);
  }

  Vector3 GetSimulationPosition()
  {
    Vector2 pos = Simulator.Instance.getAgentPosition(agentId);
    return new Vector3(pos.x, 0, pos.y);
  }


  Vector3 prefVel;
  Vector2 GetPreferedVelocity()
  {
    Vector3 dir;
    if (nextPathIndex < path.corners.Length)
    {
      dir = (path.corners[nextPathIndex] - transform.position);
    }
    else
    {
      dir = (GoalProvider.Goal - transform.position);
    }
    dir.y = 0;
    dir.Normalize();
    dir *= agentParams.MaxSpeed;
    prefVel = dir;
    return new Vector2(dir.x, dir.z);
  }

  private void OnDrawGizmosSelected()
  {
    if (path != null)
    {
      Gizmos.color = Color.green;
      for (int i = currentPathIndex; i < path.corners.Length - 1; i++)
      {
        Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
      }
    }
    else
    {
      // Gizmos.DrawLine(transform.position, transformPosition + prefVel);
    }
  }

  void OnDrawGizmos()
  {
    Gizmos.color = Color.white;
    Gizmos.DrawLine(transform.position, transform.position + prefVel);
    if (path != null)
    {
      Gizmos.color = Color.green;
      for (int i = currentPathIndex; i < path.corners.Length - 1; i++)
      {
        Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
      }
    }
  }
}
