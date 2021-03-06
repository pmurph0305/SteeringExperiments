using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
public class PathSupplier : MonoBehaviour
{
  [SerializeField] List<NavMeshAgent> agents;
  private static PathSupplier instance;
  private void Awake()
  {
    instance = this;
    foreach (var a in agents)
    {
      a.updatePosition = false;
      a.updateRotation = false;
      // a.isStopped = true;
    }
  }
  Queue<PathRequest> Requests = new Queue<PathRequest>();
  HashSet<int> QueuedTransforms = new HashSet<int>();
  public int RPU = 10;
  NavMeshHit hit;

  public int RequestCount;
  public int QueuedTransformCount;

  static readonly Unity.Profiling.ProfilerMarker pathMarker = new Unity.Profiling.ProfilerMarker("PathQueue");
  private void Update()
  {
    RequestCount = Requests.Count;
    QueuedTransformCount = QueuedTransforms.Count;
    pathMarker.Begin();
    for (int i = 0; i < RPU; i++)
    {
      if (Requests.Count == 0) break;
      PathRequest r = Requests.Peek();
      if (NavMesh.SamplePosition(r.T.position, out hit, 1f, NavMesh.AllAreas))
      {
        NavMeshAgent a = agents[UnityEngine.Random.Range(0, agents.Count)];
        if (a.Warp(hit.position))
        {
          if (a.CalculatePath(r.GoalGetter(), r.path))
          {
            Requests.Dequeue();
            QueuedTransforms.Remove(r.T.GetInstanceID());
            r.completed = true;
            r.OnCompleted(r.path);
          }
        }
        // if (a.CalculatePath(hi))
        // if (NavMesh.CalculatePath(hit.position, r.GoalGetter(), NavMesh.AllAreas, r.path))
        // {
        //   // Debug.Log("Dequeue");
        //   Requests.Dequeue();
        //   QueuedTransforms.Remove(r.T.GetInstanceID());
        //   r.OnCompleted(r.path);
        // }
      }
      r.attempts += 1;
      if (r.attempts > 3 && !r.completed)
      {
        Requests.Dequeue();
        Requests.Enqueue(r);
      }
    }
    pathMarker.End();
  }

  public static void RequestPath(Transform t, Func<Vector3> goalGetter, Action<NavMeshPath> onComplete)
  {
    // Debug.Log("Try quest path?");
    if (instance.QueuedTransforms.Add(t.GetInstanceID()))
    {
      // Debug.Log("Request path.");
      instance.Requests.Enqueue(new PathRequest(t, goalGetter, onComplete));
    }
  }


  class PathRequest
  {
    public NavMeshPath path;
    public Vector3 Goal;
    public Transform T;
    public Action<NavMeshPath> OnCompleted;

    public int attempts = 0;
    public bool completed = false;
    public Func<Vector3> GoalGetter;
    public PathRequest(Transform t, Vector3 goal, Action<NavMeshPath> onComplete)
    {
      T = t;
      Goal = goal;
      OnCompleted = onComplete;
      path = new NavMeshPath();
    }

    public PathRequest(Transform t, Func<Vector3> goalGetter, Action<NavMeshPath> onComplete)
    {
      T = t;
      GoalGetter = goalGetter;
      OnCompleted = onComplete;
      path = new NavMeshPath();
    }
  }
}
