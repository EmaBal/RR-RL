using System;
using System.Collections;
using System.Collections.Generic;
using RedRunner.Characters;
using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    private TrackCheckpoints trackCheckpoints;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<RedCharacter>(out RedCharacter redCharacter))
        {
            trackCheckpoints.PlayerThroughCheckpoint(this);
            redCharacter.setTrackCheckpointsRed(trackCheckpoints);
        }
    }

    public void SetTrackCheckpoint(TrackCheckpoints trackCheckpoints)
    {
        this.trackCheckpoints = trackCheckpoints;
    }
    
    public string getBlockName()
    {
        return transform.parent.parent.name;
    }

    public TrackCheckpoints getBlockTrackCheckpoints()
    {
        return transform.parent.parent.GetComponent<TrackCheckpoints>();
    }
}
