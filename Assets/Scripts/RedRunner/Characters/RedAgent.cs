using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RedRunner.Characters;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityStandardAssets.CrossPlatformInput;

public class RedAgent : Agent
{
    private RedCharacter redrunner;
    private TrackCheckpoints trackCheckpoints;
    private GameObject startClone;
    private GameObject[] gameObjects;
    private List<GameObject> gameObjectsList;
    private GameObject[] newGameObjects;
    
    private void Start()
    {
        StartCoroutine(FindPieces());
    }
    
    IEnumerator FindPieces()
    {
        yield return new WaitForSeconds (1f);
        gameObjectsList = new List<GameObject>();
        gameObjects = GameObject.FindObjectsOfType<GameObject>();
    
        for (var i=0; i < gameObjects.Length; i++){
            if(gameObjects[i].name.StartsWith("Start(Clone") || gameObjects[i].name.StartsWith("Middle")){
                gameObjectsList.Add(gameObjects[i]);
                Debug.Log(gameObjects[i]);
            }
        }
        
        foreach (GameObject var in gameObjectsList)
        {
            if (var.name.Equals("Start(Clone)"))
            {
                Subscribe(var.GetComponent<TrackCheckpoints>());
            }
        }
    }

    private void FindNewPieces()
    {
        newGameObjects = GameObject.FindObjectsOfType<GameObject>();
        
        for (var i=0; i < newGameObjects.Length; i++){
            if(newGameObjects[i].name.StartsWith("Start(Clone") || newGameObjects[i].name.StartsWith("Middle")){
                if (!gameObjectsList.Contains(newGameObjects[i]))
                {
                    gameObjectsList.Add(newGameObjects[i]);
                }
            }
        }
    }
    
    private void Subscribe(TrackCheckpoints tc)
    {
        tc.OnPlayerCorrectCheckpoint += TrackCheckpoints_OnPlayerCorrectCheckpoint;
        tc.OnPlayerWrongCheckpoint += TrackCheckpoints_OnPlayerWrongCheckpoint;
    }

    private void TrackCheckpoints_OnPlayerCorrectCheckpoint(object sender, System.EventArgs e)
    {
        Debug.Log("reward added");
        AddReward(0.1f);
        //Debug.Log((TrackCheckpoints)sender.getParentName());
    }
    
    private void TrackCheckpoints_OnPlayerWrongCheckpoint(object sender, System.EventArgs e)
    {
        Debug.Log("penalty added");
        AddReward(-0.1f);
    }

    private void Awake()
    {
        redrunner = GetComponent<RedCharacter>();
        Academy.Instance.AutomaticSteppingEnabled = false;
    }
    
    void Update()
    {
        // startClone = GameObject.Find("Start(Clone)");
        // Debug.Log(startClone);
        // try
        // {
        //     trackCheckpoints = startClone.GetComponent<TrackCheckpoints>();
        // }
        // catch
        // {
        //     Debug.Log("catch");
        // }

        Academy.Instance.EnvironmentStep();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(redrunner.transform.localPosition);
        sensor.AddObservation(redrunner.m_CurrentRunSpeed);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        redrunner.directionFloat = actionBuffers.ContinuousActions[0];
        var jump = actionBuffers.DiscreteActions[0];

        switch (jump)
        {
            case 0:
                redrunner.jumping = 0; //non sta saltando
                break;
            case 1:
                redrunner.jumping = 1; //jump button down
                break;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = CrossPlatformInputManager.GetAxis("Horizontal");
        
        if (CrossPlatformInputManager.GetButtonDown ("Jump"))
        {
            discreteActionsOut[0] = 1;
        }
    }
}
