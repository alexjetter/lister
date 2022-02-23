using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.IO;

public class TestManager : MonoBehaviour
{
    public enum SortingCategory
    {
        DurationAscending,
        DurationDescending,
        TimeEndingSoonest,
        TimeEndingFarthest,
        NameAscending,
        NameDescending
    }

    public InputField newObservationName;
    public Text label;
    public GameObject newObservationPrefab;
    public GameObject listContainer;
    private string defaultNewObservationText = "Name";
    public List<Observation> observations = new List<Observation>();
    private List<ObservationView> currentCardPool = new List<ObservationView>();
    private SortingCategory currentSortingMethod = SortingCategory.NameAscending;

    private static TestManager _instance;
    public static TestManager instance
    {
        get
        {
            return _instance;
        }
    }

    public long nowTimestamp
    {
        get
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
    }

    private string DataPath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "settings.txt");
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        ReadAllFromDisk();
        Observation.DataUpdated += OnObservationDataUpdated;
    }

    private void OnDestroy()
    {
        Observation.DataUpdated -= OnObservationDataUpdated;
        if (this == _instance)
        {
            _instance = null;
        }
    }

    void Start()
    {
        newObservationName.text = defaultNewObservationText;
    }

    public void OnTapNewObservation()
    {
        string newName = newObservationName.text;
        if (newName == defaultNewObservationText)
        {
            return;
        }

        if (observations.FirstOrDefault(x => x.name == newName) != null)
        {
            return;
        }

        observations.Add(new Observation(newName));
        newObservationName.text = defaultNewObservationText;

        UpdateView();

        // Write here since even though the Observation changing raises the event, it won't serialize the updated observations list until after it is added.
        WriteAllToDisk();
    }

    private void SortObservations(SortingCategory category)
    {
        switch (category)
        {
            case SortingCategory.DurationAscending:
                {
                    observations = observations.OrderBy(x => x.averageDuration).ToList();
                    break;
                }
            case SortingCategory.DurationDescending:
                {
                    observations = observations.OrderByDescending(x => x.averageDuration).ToList();
                    break;
                }
            case SortingCategory.TimeEndingSoonest:
                {
                    observations = observations.OrderBy(x => x.nextDueDate).ToList();
                    break;
                }
            case SortingCategory.TimeEndingFarthest:
                {
                    observations = observations.OrderByDescending(x => x.nextDueDate).ToList();
                    break;
                }
            case SortingCategory.NameDescending:
                {
                    observations = observations.OrderByDescending(x => x.name).ToList();
                    break;
                }
            case SortingCategory.NameAscending:
            default:
                {
                    observations = observations.OrderBy(x => x.name).ToList();
                    break;
                }
        }
    }

    private void UpdateView()
    {
        SortObservations(currentSortingMethod);

        for (int i = 0; i < observations.Count; i++)
        {
            if (currentCardPool.Count > i && currentCardPool[i] != null)
            {
                currentCardPool[i].UpdateView();
            }
            else
            {
                var newCard = Instantiate(newObservationPrefab, listContainer.transform)?.GetComponent<ObservationView>();
                if (newCard != null)
                {
                    newCard.SetIndex(i);
                    currentCardPool.Add(newCard);
                }
                else
                {
                    Debug.LogError($"Unable to Instantiate prefab for index: {i}");
                }
            }
        }
    }

    public void SetSortingTypeDurationAscending()
    {
        SetSortingType(SortingCategory.DurationAscending);
    }
    public void SetSortingTypeDurationDescending()
    {
        SetSortingType(SortingCategory.DurationDescending);
    }

    public void SetSortingTypeTimeEndingSoonest()
    {
        SetSortingType(SortingCategory.TimeEndingSoonest);
    }

    public void SetSortingTypeTimeEndingFarthest()
    {
        SetSortingType(SortingCategory.TimeEndingFarthest);
    }

    public void SetSortingTypeNameAscending()
    {
        SetSortingType(SortingCategory.NameAscending);
    }

    public void SetSortingTypeNameDescending()
    {
        SetSortingType(SortingCategory.NameDescending);
    }

    private void SetSortingType(SortingCategory sort)
    {
        currentSortingMethod = sort;
        UpdateView();
    }

    private void WriteAllToDisk()
    {
        File.WriteAllText(DataPath, Newtonsoft.Json.JsonConvert.SerializeObject(observations));
    }

    private void ReadAllFromDisk()
    {
        if (File.Exists(DataPath))
        {
            observations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Observation>>(File.ReadAllText(DataPath));

            UpdateView();
        }
    }

    private void OnObservationDataUpdated(object sender, DataUpdatedEventArgs e)
    {
        // For now, just write everything.
        WriteAllToDisk();
    }
}
