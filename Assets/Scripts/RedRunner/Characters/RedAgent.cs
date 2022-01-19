using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using RedRunner;
using RedRunner.Characters;
using RedRunner.TerrainGeneration;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityStandardAssets.CrossPlatformInput;

public class RedAgent : Agent
{
    private RedCharacter redrunner;
    private TrackCheckpoints trackCheckpoints;
    //private GameObject startClone;
    //private GameObject[] gameObjects;
    //private List<GameObject> gameObjectsList;
    // GameObject[] newGameObjects;
    private int currentBlockCheckpointsNumber;
    private TrackCheckpoints currentTrackCheckpoints;
    //private bool isBlockFinished = false;
    private bool firstCheckpointPassed;
    private bool agentDead;
    //private GameManager gm;
    private int stepsSinceLastCheckpoint;
    Rigidbody2D redrunnerRigidbody;

    [SerializeField] private int maxEnvironmentStep;

    public override void OnEpisodeBegin()
    {
        currentBlockCheckpointsNumber = 0;
        firstCheckpointPassed = false;
        StartCoroutine(FirstTrackCheckpoint());
        agentDead = false;
        stepsSinceLastCheckpoint = 0;
        redrunnerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void setAgentDead(bool agentDead)
    {
        this.agentDead = agentDead;
    }
    
    IEnumerator FirstTrackCheckpoint()
    {
        yield return new WaitForSeconds (1f * Time.timeScale);
        trackCheckpoints = TerrainGenerator.Singleton.GetCharacterBlock().GetComponent<TrackCheckpoints>();
        currentTrackCheckpoints = trackCheckpoints;
        Subscribe(trackCheckpoints);
        redrunnerRigidbody.constraints &= ~RigidbodyConstraints2D.FreezePosition;
    }

    private void Subscribe(TrackCheckpoints tc)
    {
        Debug.Log("SUBSCRIBED TO " + tc);
        tc.OnPlayerCorrectCheckpoint += TrackCheckpoints_OnPlayerCorrectCheckpoint;
        tc.OnPlayerWrongCheckpoint += TrackCheckpoints_OnPlayerWrongCheckpoint;
    }

    private void TrackCheckpoints_OnPlayerCorrectCheckpoint(object sender, System.EventArgs e)
    {
        stepsSinceLastCheckpoint = 0;
        firstCheckpointPassed = true;
        Debug.Log("reward added");
        AddReward(1f);
        currentBlockCheckpointsNumber++;
        if (currentBlockCheckpointsNumber == currentTrackCheckpoints.getCheckpointsNumber())
        {
            //isBlockFinished = true;
            currentBlockCheckpointsNumber = 0;
        }
    }
    
    private void TrackCheckpoints_OnPlayerWrongCheckpoint(object sender, System.EventArgs e)
    {
        stepsSinceLastCheckpoint = 0;
        Debug.Log("penalty added");
        AddReward(-1f);
        Unsubscribe(trackCheckpoints);
        redrunner.Die();
        EndEpisode();
    }

    private void Awake()
    {
        redrunner = GetComponent<RedCharacter>();
        //gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
        Academy.Instance.AutomaticSteppingEnabled = false;
        redrunnerRigidbody = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        Academy.Instance.EnvironmentStep();
        
        stepsSinceLastCheckpoint++;
        if (stepsSinceLastCheckpoint >= maxEnvironmentStep)
        {
            redrunner.Die(false);
            EndEpisode();
        }
    }

    private void FixedUpdate()
    {
        if (firstCheckpointPassed){
            trackCheckpoints = redrunner.getTrackCheckpointsRed();
            if (trackCheckpoints != currentTrackCheckpoints)
            {
                currentTrackCheckpoints = trackCheckpoints;
                Subscribe(trackCheckpoints);
                Debug.Log("rew fixedupdate");
                Debug.Log("reward added");
                AddReward(1f);
            }
        }
        
        if (agentDead)
        {
            AddReward(-1f);
            Debug.Log("character dead");
            Unsubscribe(trackCheckpoints);
            EndEpisode();
            agentDead = false;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
        sensor.AddObservation(redrunner.transform.localPosition);
        sensor.AddObservation(redrunner.GetComponent<Rigidbody2D>().velocity.x);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        redrunner.directionFloat = actionBuffers.ContinuousActions[0];
        var jump = actionBuffers.DiscreteActions[0];

        switch (jump)
        {
            case 0:
                redrunner.jumping = 0; // not jumping
                break;
            case 1:
                redrunner.jumping = 1; // jump button pressed
                AddReward(-0.1f);
                break;
        }
        AddReward(-0.1f / maxEnvironmentStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Time.timeScale = 1f;
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = CrossPlatformInputManager.GetAxis("Horizontal");
        
        if (CrossPlatformInputManager.GetButtonDown ("Jump"))
        {
            discreteActionsOut[0] = 1;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Instakill"))
        {
            redrunner.Die();
            AddReward(-1f);
            EndEpisode();
        }
    }

    private void Unsubscribe(TrackCheckpoints tc)
    {
        Debug.Log("UNSUBSCRIBED TO " + tc);
        tc.OnPlayerCorrectCheckpoint -= TrackCheckpoints_OnPlayerCorrectCheckpoint;
        tc.OnPlayerWrongCheckpoint -= TrackCheckpoints_OnPlayerWrongCheckpoint;
    }
}
