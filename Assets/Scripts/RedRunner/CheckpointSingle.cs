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
        }
    }

    public void SetTrackCheckpoint(TrackCheckpoints trackCheckpoints)
    {
        this.trackCheckpoints = trackCheckpoints;
    }
}
