using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
using System;
public class RVO2Simulator : MonoBehaviour
{
  public static event Action OnSimulationStepped;

  [SerializeField] RVO2AgentParams defaultAgentParams = new RVO2AgentParams();

  private void Awake()
  {
    Simulator.Instance.setAgentDefaults(defaultAgentParams.NeighbourDistance, defaultAgentParams.MaxNeighbours, defaultAgentParams.TimeHorizon, defaultAgentParams.TimeHorizonObst, defaultAgentParams.Radius, defaultAgentParams.MaxSpeed, defaultAgentParams.Velocity);
  }

  // Update is called once per frame
  void Update()
  {
    Simulator.Instance.setTimeStep(Time.deltaTime);
    Simulator.Instance.doStep();
    OnSimulationStepped?.Invoke();
  }
}
