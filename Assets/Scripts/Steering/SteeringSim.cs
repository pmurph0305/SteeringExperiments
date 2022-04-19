using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringSim
{
  private static SteeringSim _instance = new SteeringSim();

  public static SteeringSim Instance { get { return _instance; } }
  List<SteerAgent> Agents;

  public SteerAgent Add(Transform t)
  {
    SteerAgent agent = new SteerAgent(t);
    Agents.Add(agent);
    return agent;
  }


  void UpdateSteering()
  {

  }
}
