using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TestManager : MonoBehaviour
{
    public Text label;
    private Observation observation = new Observation("Test");

    void Start()
    {
        UpdateLabel();
    }

    public void OnObserveTapped()
    {
        observation.AddPerception(new Perception(PerceptionEvent.Observation, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()));
        UpdateLabel();
    }

    public void OnDoTapped()
    {
        observation.AddPerception(new Perception(PerceptionEvent.Execution, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()));
        UpdateLabel();
    }

    public void UpdateLabel()
    {
        label.text = $"{observation.name} - Average time: {observation.averageDuration}, Next due date: {observation.nextDueDate}\n Num perceptions: {observation.numPerceptions}, num intervals: {observation.numIntervals}";
    }
}
