using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalProvider : MonoBehaviour, IGoalProvider
{
  public static GoalProvider Instance;
  [SerializeField] Vector3 Velocity;
  [SerializeField] Vector3 Position;



  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(this.gameObject);
      return;
    }
    else
    {
      Instance = this;
    }
    Position = transform.position;
    p = transform.position;
  }

  Vector3 p;
  // Update is called once per frame
  void Update()
  {
    Position = transform.position;
    Velocity = (Position - p) / Time.deltaTime;
    p = Position;
  }

  public Transform GetTransform()
  {
    return this.transform;
  }

  public Vector3 GetPosition()
  {
    return this.transform.position;
  }

  public Vector3 GetVelocity()
  {
    return Velocity;
  }
}
