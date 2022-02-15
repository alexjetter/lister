using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum PerceptionEvent
{
    Invalid,
    Observation,
    Execution
}

public struct Perception
{
    public PerceptionEvent perceptionEvent { get; private set; }
    public long timestamp { get; private set; }

    public Perception(PerceptionEvent perceptionEvent, long timestamp)
    {
        this.perceptionEvent = perceptionEvent;
        this.timestamp = timestamp;
    }
}

public struct CompletedTask
{
    // Note that these names are not correct - you can have multiple executions that complete a task.
    // These names are placeholder.
    public long executionTimestamp { get; private set; }
    public long observationTimestamp { get; private set; }

    public CompletedTask(Perception execution, Perception observation)
    {
        executionTimestamp = execution.timestamp;
        observationTimestamp = observation.timestamp;
    }

    public CompletedTask(long executionTimestamp, long observationTimestamp)
    {
        if (executionTimestamp > observationTimestamp)
        {
            throw new ArgumentException($"Cannot have an {nameof(executionTimestamp)} that happens after an {nameof(observationTimestamp)}");
        }

        this.executionTimestamp = executionTimestamp;
        this.observationTimestamp = observationTimestamp;
    }

    public long duration
    {
        get
        {
            return observationTimestamp - executionTimestamp;
        }
    }

    public long NextDueDate(long interval)
    {
        return observationTimestamp + interval;
    }
}

public class Observation
{
    public string name { get; private set; }

    public long nextDueDate
    {
        get
        {
            // Get the last executed event's timestamp, and add the average interal to it
            if (recordedPerceptions.Where(x => x.perceptionEvent == PerceptionEvent.Execution).ToArray().Length > 0)
            {
                return recordedPerceptions.Where(x => x.perceptionEvent == PerceptionEvent.Execution).Max(x => x.timestamp) + averageDuration;
            }
            return 0;
        }
    }

    public long averageDuration
    {
        get
        {
            if (recordedIntervals.Count > 0)
            {
                return (long)recordedIntervals.Average(x => x.duration);
            }
            return 0;
        }
    }

    public int numPerceptions
    {
        get
        {
            return recordedPerceptions.Count;
        }
    }

    public int numIntervals
    {
        get
        {
            return recordedIntervals.Count;
        }
    }

    private List<Perception> recordedPerceptions = new List<Perception>();
    private List<CompletedTask> recordedIntervals = new List<CompletedTask>();

    public Observation(string name)
    {
        this.name = name;
    }

    public Observation(string name, Perception perception)
    {
        this.name = name;
        recordedPerceptions.Add(perception);
    }
    
    public void AddPerception(Perception perception)
    {
        recordedPerceptions.Add(perception);
        UpdateRecordedIntervals();
    }

    public void UpdateRecordedIntervals()
    {
        recordedIntervals.Clear();
        //recordedPerceptions = recordedPerceptions.OrderBy(x => x.timestamp).ToList();

        Perception previousPerception = new Perception(PerceptionEvent.Invalid, long.MinValue);
        Perception currentPerception;

        for (int i = 0; i < recordedPerceptions.Count; i++)
        {
            bool validPerception = true;
            currentPerception = recordedPerceptions[i];

            // First iteration
            if (i == 0)
            {
                if (currentPerception.perceptionEvent == PerceptionEvent.Execution
                    || currentPerception.perceptionEvent == PerceptionEvent.Observation)
                {
                    // Placeholder for first iteration if needed
                }

                // Validation
                else
                {
                    Debug.LogError($"Invalid {nameof(PerceptionEvent)}: {currentPerception.perceptionEvent.ToString()}");
                    validPerception = false;
                }
            }

            // Only link Observations that come immediately after Executions as recordedIntervals, 
            // since those give us an interval from "when it was done" to "when it should be done again"
            else if (previousPerception.perceptionEvent != PerceptionEvent.Invalid)
            {
                // "I saw that x needed to be done, and then I did it"
                // That's not an interval we need to record since we only care about linking Observations that come after Executions
                if (currentPerception.perceptionEvent == PerceptionEvent.Execution
                    && previousPerception.perceptionEvent == PerceptionEvent.Observation)
                {
                    Debug.Log("Current: Execute, previous: Observe");
                }
                // "I did x, and then later I saw that x needed to be done again"
                if (currentPerception.perceptionEvent == PerceptionEvent.Observation
                    && previousPerception.perceptionEvent == PerceptionEvent.Execution)
                {
                    Debug.Log("Current: Observe, previous: Execute");
                    recordedIntervals.Add(new CompletedTask(previousPerception, currentPerception));
                }
                // Link Executions and Executions
                // "I did x, and then I did x"
                else if (currentPerception.perceptionEvent == PerceptionEvent.Execution
                    && previousPerception.perceptionEvent == PerceptionEvent.Execution)
                {
                    Debug.Log("Current: Execute, previous: Execute");
                    recordedIntervals.Add(new CompletedTask(previousPerception, currentPerception));
                }
                // Skip multiple Observations, we only care about the first Observe in a chain of Observes
                // "x still needs to be done"
                else if (currentPerception.perceptionEvent == PerceptionEvent.Observation
                    && previousPerception.perceptionEvent == PerceptionEvent.Observation)
                {
                    Debug.Log("Current: Observe, previous: Observe");
                    // Use the previous event so that we can link the next Execution with the oldest Observation in this chain
                    validPerception = false;
                }
            }
            
            if (validPerception)
            {
                previousPerception = recordedPerceptions[i];
            }
        }
    }

    /*
    // TODO: Write to disk
    public string Serialize()
    {
        // Write name
        // Write all Perceptions
    }


    public Observation Deserialize(string data)
    {
        // Read name
        // Read all Perceptions
        
        UpdateRecordedIntervals();
    }
    */
}