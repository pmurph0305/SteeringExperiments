using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGoalProvider : MonoBehaviour
{

  public static Vector3 Goal;
  public static Transform GoalTransform;
  private void Awake()
  {
    Goal = transform.position;
    GoalTransform = transform;
  }

  // Update is called once per frame
  void Update()
  {
    Goal = transform.position;
  }
}
