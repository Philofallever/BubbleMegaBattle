//#if NET_4_6 || NET_STANDARD_2_0
using System;
using System.Collections.Generic;
using UnityEngine;

public class NextFrameHelper
{
    private struct Job
    {
        public int    Frame;
        public Action Action;
    }

    private readonly Queue<Job> _queue = new Queue<Job>();

    public void Enqueue(Action action) => _queue.Enqueue(new Job {Frame = Time.frameCount, Action = action});

    public void Update()
    {
        var currentFrame = Time.frameCount;
        while (_queue.Count > 0)
        {
            var job = _queue.Peek();
            if (job.Frame == currentFrame)
                break;

            _queue.Dequeue();
            job.Action();
        }
    }
}
//#endif