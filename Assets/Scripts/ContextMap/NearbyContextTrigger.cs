using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Adds nearby context objects to the parent context steering's hashset.
/// </summary>
public class NearbyContextTrigger : MonoBehaviour
{
  ContextSteering steering;
  private void Start()
  {
    steering = GetComponentInParent<ContextSteering>();
  }
  ContextSteering triggerCache;
  private void OnTriggerEnter(Collider other)
  {
    triggerCache = other.GetComponent<ContextSteering>();
    if (triggerCache != null)
    {
      steering.AddNearbyContext(triggerCache);
    }
  }

  private void OnTriggerExit(Collider other)
  {
    triggerCache = other.GetComponent<ContextSteering>();
    if (triggerCache != null)
    {
      steering.RemoveNearbyContext(triggerCache);
    }
  }

}
