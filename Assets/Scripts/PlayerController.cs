﻿using UnityEngine;

public enum RoleType
{
  OppsCommander,
  WeaponsOfficer,
  Captain
}

public enum Axis { X, Y, Z }

public class PlayerController : MonoBehaviour
{
  public string Id { get; set; }
  public bool Available { get; set; }
  public Vector3 Acceleration { get; set; }
  public Quaternion Rotation { get; set; }

  private bool capturePhoneStraight;
  public bool CapturePhoneStraight
  {
    get { return capturePhoneStraight; }
    set { capturePhoneStraight = value; }
  }

  private bool captureFlashlightStraight;
  public bool CaptureFlashlightStraight
  {
    get { return captureFlashlightStraight; }
    set { captureFlashlightStraight = value; }
  }

  [SerializeField]
  private RoleType role = RoleType.OppsCommander;

  public bool OnAction { get; set; }

  private GameObject submarine;
  private GameObject rotationDummy;

  private SubmarineController submarineController;
  private GyroscopeController gyroscopeController;

  [SerializeField]
  private float clampAngle = 45f;
  [SerializeField]
  private float threshold = 0.1f;
  [SerializeField]
  private float speed = 25f;

  private static int rotationDummyCounter = 0;

  private void Start()
  {
    OnAction = false;
    rotationDummy = new GameObject("RotationDummy" + ++rotationDummyCounter);

    submarine = GameObject.Find("Submarine");
    submarineController = submarine.GetComponent<SubmarineController>();
    gyroscopeController = transform.GetComponent<GyroscopeController>();
  }

  private void Update()
  {
    if (Available)
      return;

    if (role == RoleType.OppsCommander)
    {
      if (InCave())
        return;

      switch (submarineController.Level)
      {
        case 1:
          RotateSubmarine(Axis.Y, Acceleration.x);
          break;

        case 2:
          RotateSubmarine(Axis.X, -Acceleration.z);
          break;

        case 3:
          RotateHeadlight();
          break;

        case 4:
          RotateSubmarine(Axis.Y, Acceleration.x);
          break;

        case 5:
          RotateSubmarine(Axis.X, -Acceleration.z);
          break;
      }
    }
    else if (role == RoleType.WeaponsOfficer)
    {
      if (InCave())
        return;

      switch (submarineController.Level)
      {
        case 1:
          RotateSubmarine(Axis.X, -Acceleration.z);
          break;

        case 2:
          RotateHeadlight();
          break;

        case 3:
          RotateSubmarine(Axis.Y, Acceleration.x);
          break;

        case 4:
          RotateSubmarine(Axis.X, -Acceleration.z);
          break;

        case 5:
          RotateHeadlight();
          break;
      }
    }
    else if (role == RoleType.Captain)
    {
      switch (submarineController.Level)
      {
        case 1:
          RotateHeadlight();
          break;

        case 2:
          RotateSubmarine(Axis.Y, Acceleration.x);
          break;

        case 3:
          RotateSubmarine(Axis.X, -Acceleration.z);
          break;

        case 4:
          RotateHeadlight();
          break;

        case 5:
          RotateSubmarine(Axis.Y, Acceleration.x);
          break;
      }
    }
  }

  private void RotateSubmarine(Axis axis, float acceleration)
  {
    Vector3 dir = Vector3.zero;

    float dirAxis = 0f;
    if (axis == Axis.Y)
    {
      dir.y = acceleration;
      dirAxis = dir.y;
    }
    else if (axis == Axis.X)
    {
      dir.x = acceleration;
      dirAxis = dir.x;
    }
    else
      Debug.LogWarning("PlayerController: Input on axis " + axis.ToString() + " not supported");

    if (0f > dirAxis + threshold || 0f < dirAxis - threshold)
    {
      if (dir.sqrMagnitude > 1)
        dir.Normalize();

      rotationDummy.transform.position = submarine.transform.position;
      rotationDummy.transform.rotation = submarine.transform.rotation;
      rotationDummy.transform.forward = submarine.transform.forward;

      rotationDummy.transform.Rotate(dir * speed * Time.deltaTime);

      if (Vector3.Angle(rotationDummy.transform.forward, submarine.transform.forward) < clampAngle)
        submarine.transform.Rotate(dir * speed * Time.deltaTime);
    }
  }

  private bool InCave()
  {
    if (submarineController.InCave)
    {
      gyroscopeController.UpdateGyroscope(Rotation, ref capturePhoneStraight, ref captureFlashlightStraight);
      return true;
    }

    gyroscopeController.DisableLight();
    return false;
  }

  private void RotateHeadlight()
  {
    gyroscopeController.UpdateGyroscope(Rotation, ref capturePhoneStraight, ref captureFlashlightStraight);
  }
}