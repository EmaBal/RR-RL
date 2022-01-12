using System;
using System.Collections;
using System.Collections.Generic;
using RedRunner.Characters;
using UnityEngine;
using UnityEngine.Events;

public class TrackCheckpoints : MonoBehaviour
{
    public event EventHandler OnPlayerCorrectCheckpoint;
    public event EventHandler OnPlayerWrongCheckpoint;

    private List<CheckpointSingle> checkpointSingleList;
    private int nextCheckpointSingleIndex;

    private String checkpointCorrect;
    private void Awake()
    {
        Transform checkpointTransform = transform.Find("Checkpoints");

        checkpointSingleList = new List<CheckpointSingle>();
        foreach (Transform checkpointSingleTransform in checkpointTransform)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
            checkpointSingle.SetTrackCheckpoint(this);
            
            checkpointSingleList.Add(checkpointSingle);
        }

        nextCheckpointSingleIndex = 0;

        checkpointCorrect = "not set";
    }

    public void PlayerThroughCheckpoint(CheckpointSingle checkpointSingle)
    {
        if (checkpointSingleList.IndexOf(checkpointSingle) == nextCheckpointSingleIndex)
        {
            checkpointCorrect = "correct";
            OnPlayerCorrectCheckpoint?.Invoke(this, EventArgs.Empty);
            Debug.Log("correct");
            nextCheckpointSingleIndex++;
        }
        else
        {
            checkpointCorrect = "wrong";
            OnPlayerWrongCheckpoint?.Invoke(this, EventArgs.Empty);
            Debug.Log("wrong");
        }
    }

    public int getCheckpointsNumber()
    {
        return checkpointSingleList.Count;
    }

    public void setCheckpointState(String checkpointCorrect)
    {
        this.checkpointCorrect = checkpointCorrect;
    }
    
    public String getCheckpointState()
    {
        return checkpointCorrect;
    }
}
