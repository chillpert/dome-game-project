﻿using System.Collections.Generic;
using UnityEngine;

public class FirmCollider : MonoBehaviour
{
  [SerializeField]
  private float caveAnimationSpeed = 5f;
  
  private SubmarineController submarineController;
  private AudioController audioController;
  private GameObject submarine;

  private void Start()
  {
    submarine = GameObject.Find("Submarine");
    audioController = GameObject.Find("AudioController").GetComponent<AudioController>();
    submarineController = submarine.GetComponent<SubmarineController>();
  }

  private void Update()
  {
    transform.position = submarine.transform.position;
    transform.rotation = submarine.transform.rotation;
    transform.forward = submarine.transform.forward;

    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
      submarineController.Level = 1;
      UpdateLevel();
    }
    else if (Input.GetKeyDown(KeyCode.Alpha2))
    {
      submarineController.Level = 2;
      UpdateLevel();
    }
    else if (Input.GetKeyDown(KeyCode.Alpha3))
    {
      submarineController.Level = 3;
      UpdateLevel();
    }
    else if (Input.GetKeyDown(KeyCode.Alpha4))
    {
      submarineController.Level = 4;
      UpdateLevel();
    }
    else if (Input.GetKeyDown(KeyCode.Alpha5))
    {
      submarineController.Level = 5;
      UpdateLevel();
    }
  }

  private void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.CompareTag("EnterCave"))
      EnterCave(collision);

    if (collision.gameObject.CompareTag("EnterLevel"))
      EnterLevel(++submarineController.Level);

    if (collision.gameObject.CompareTag("TunnelMesh"))
    {
      StartCoroutine(submarineController.Cam.Shake());

      if (!submarineController.IFrames)
        submarineController.Health -= submarineController.DamageTunnelMesh;

      submarineController.updateDamageTexture();
    }

    if (collision.gameObject.CompareTag("LerpStopIn"))
      submarineController.TurnCamStraight = false;

    if (collision.gameObject.CompareTag("LerpStopOut"))
      submarineController.TurnCamStraight = false;

    if (collision.gameObject.CompareTag("Destructables"))
    {
      StartCoroutine(submarineController.Cam.Shake());

      if (!submarineController.IFrames)
        submarineController.Health -= submarineController.DamageDestructables;

      Destroy(collision.gameObject); // or play destruction animation or similar effects
    }

    // audio
    if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Bridge") || collision.gameObject.CompareTag("Destructables"))
      audioController.PlayDamageSound();

    if (collision.gameObject.name.Equals("FoundCave1"))
      audioController.PlayFoundCave1();

    if (collision.gameObject.name.Equals("EnterCave1"))
      audioController.PlayEnterCave1();

    Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
  }

  private void EnterCave(Collision collision)
  {
    submarineController.InCave = true;

    UpdateLevel();

    if (submarine.GetComponent<CustomFollowerPath>() == null)
    {
      submarine.AddComponent<CustomFollowerPath>();
      var temp = submarine.GetComponent<CustomFollowerPath>();
      temp.EndOfPathInstruction = PathCreation.EndOfPathInstruction.Stop;
      temp.Speed = caveAnimationSpeed;
    }

    // set waypoints
    GameObject path = GameObject.Find("Path");
    // create path
    if (path.GetComponent<CustomPathCreator>() == null)
    {
      path.AddComponent<CustomPathCreator>();
      path.GetComponent<PathCreation.PathCreator>().bezierPath.IsClosed = false;
      submarine.GetComponent<CustomFollowerPath>().PathCreator = path.GetComponent<PathCreation.PathCreator>();
    }

    // set start object
    var customPath = path.GetComponent<CustomPathCreator>();
    customPath.start = collision.contacts[0].point;

    string name = "PathCave" + submarineController.Level.ToString();
    GameObject newPath = GameObject.Find(name);

    List<Transform> newWayPoints = new List<Transform>(newPath.transform.childCount);

    for (int i = 0; i < newPath.transform.childCount; ++i)
      newWayPoints.Add(newPath.transform.GetChild(i));

    customPath.waypoints = newWayPoints;
    customPath.updateWaypoints();
  }

  private void EnterLevel(int level)
  {
    submarineController.InCave = false;

    // delete custom path creator
    GameObject path = GameObject.Find("Path");

    if (path.GetComponent<CustomPathCreator>() != null)
    {
      Destroy(path.GetComponent<CustomPathCreator>());
      Destroy(path.GetComponent<PathCreation.PathCreator>());
    }

    if (submarine.GetComponent<CustomFollowerPath>() != null)
      Destroy(submarine.GetComponent<CustomFollowerPath>());

    submarineController.Level = level;
    UpdateLevel();
  }

  public void UpdateLevel()
  {
    Package levelUpdate = new Package(PackageType.Level, null);

    if (submarineController.InCave)
      levelUpdate.data.Add(0);
    else
      levelUpdate.data.Add(submarineController.Level);

    submarineController.TcpHost.GetComponent<TCPHost>().Send(levelUpdate);
  }
}