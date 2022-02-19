using UnityEngine;
using UnityEngine.UI;

public class ObservationView : MonoBehaviour
{
    public int index { get; private set; }

    public Image icon;
    public Text displayNameField;
    public Text dueTimeField;
    public Text frequencyField;

    private Observation observation;

    private void Awake()
    {
        if (icon == null)
        {
            Debug.LogError($"{nameof(icon)} is unset");
        }
        if (displayNameField == null)
        {
            Debug.LogError($"{nameof(displayNameField)} is unset");
        }
        if (dueTimeField == null)
        {
            Debug.LogError($"{nameof(dueTimeField)} is unset");
        }
        if (frequencyField == null)
        {
            Debug.LogError($"{nameof(frequencyField)} is unset");
        }
    }

    public void UpdateView()
    {
        if (TestManager.instance.observations.Count > index)
        {
            observation = TestManager.instance.observations[index];

            displayNameField.text = observation.name;
            dueTimeField.text = GetNextDueDateFormatted(observation.nextDueDate);
            frequencyField.text = GetFrequencyFormatted(observation.averageDuration);
        }
        else
        {
            Debug.LogError($"{nameof(index)} {index} is invalid, exceeds {nameof(TestManager.observations)} size of: {TestManager.instance.observations.Count}");
        }
    }

    private string GetNextDueDateFormatted(long nextDueDate)
    {
        return $"{nextDueDate}";
    }

    private string GetFrequencyFormatted(long frequency)
    {
        return $"{frequency}";
    }

    public void SetIndex(int i)
    {
        index = i;
        UpdateView();
    }

    public void OnSeeTapped()
    {
        observation?.AddPerception(new Perception(PerceptionEvent.Observation, TestManager.instance.nowTimestamp));
        UpdateView();
    }

    public void OnDoTapped()
    {
        observation?.AddPerception(new Perception(PerceptionEvent.Execution, TestManager.instance.nowTimestamp));
        UpdateView();
    }
}
