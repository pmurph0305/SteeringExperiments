using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidAgentScript : MonoBehaviour
{

  [SerializeField] BoidAgent agent;
  // Start is called before the first frame update
  void Start()
  {
    agent = BoidsSimulation.Instance.AddAgent(this.transform);
  }

  // Update is called once per frame
  void Update()
  {
    this.transform.position = agent.Position;
  }

  private void OnDrawGizmosSelected()
  {
    float mult = 10f;
    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + agent.centerOfMassVelocity * mult);
    Gizmos.color = Color.red;
    Gizmos.DrawLine(transform.position, transform.position + agent.avoidVelocity * mult);
    Gizmos.color = Color.yellow;
    Gizmos.DrawLine(transform.position, transform.position + agent.matchVelocity * mult);
    Gizmos.color = Color.blue;
    Gizmos.DrawLine(transform.position, transform.position + agent.toGoalVelocity * mult);
    Gizmos.color = Color.black;
    Gizmos.DrawLine(transform.position, transform.position + agent.boundsVelocity * mult);
  }
}
