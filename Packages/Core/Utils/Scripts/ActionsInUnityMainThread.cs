using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsInUnityMainThread : MonoBehaviour
{
    static public ActionsInUnityMainThread actionsInUnityMainThread;
    Queue<Action> jobs = new Queue<Action>();

    void Awake()
    {
        actionsInUnityMainThread = this;
    }

    void Update()
    {
        while (jobs.Count > 0)
            jobs.Dequeue().Invoke();
    }

    public void AddJob(Action newJob)
    {
        jobs.Enqueue(newJob);
    }
}