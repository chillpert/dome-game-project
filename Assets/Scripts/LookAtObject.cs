﻿using UnityEngine;

public class LookAtObject : MonoBehaviour
{
  [SerializeField]
  private float viewingDistance = 30f;

  private SubmarineController submarineController;
  private GameObject lastObjectHit;

  private void Start()
  {
    submarineController = GameObject.Find("Submarine").GetComponent<SubmarineController>();

    //Debug.LogWarning("Ray casting is currently enabled in tunnels. This needs to be disabled later on");
  }

  private void Update()
  {
    // only apply ray casting when inside a cave
    if (!submarineController.InCave)
    return;

    // only use layer "LookAtInteractive" on position 10
    int layerMask = 1 << 10;

    RaycastHit hit;

    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, viewingDistance, layerMask))
    {
      Debug.Log(hit.collider.gameObject.name);

      lastObjectHit = hit.collider.gameObject;
      var outlineController = lastObjectHit.GetComponent<OutlineController>();
      
      if (outlineController == null)
        return;

      outlineController.ShowOutline();
    }
    else
    {
      if (lastObjectHit != null)
      {
        var temp = lastObjectHit.GetComponent<OutlineController>();
        if (temp != null)
          temp.HideOutline();
      }
    }
  }
}
