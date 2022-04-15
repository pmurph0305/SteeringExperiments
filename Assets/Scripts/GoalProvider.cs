using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalProvider : MonoBehaviour
{

  public static Vector3 Goal;

  private void Awake()
  {
    Goal = transform.position;
  }

  // Update is called once per frame
  void Update()
  {
    Goal = transform.position;
  }
}
