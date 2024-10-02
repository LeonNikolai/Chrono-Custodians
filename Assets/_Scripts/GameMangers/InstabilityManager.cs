using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstabilityManager : NetworkBehaviour
{
    public static InstabilityManager instance;

    [SerializeField] private List<string> levels = new List<string>();

    private int day = 1; // This counts the days until a week has passed then returns to 0
    [SerializeField] private AnimationCurve weekCurve;

    [SerializeField] private int curInstability; // Current instability
    [SerializeField] private int maxInstability; // Max instability before game over.

    [SerializeField] private int totalNumberOfObjects; // Number of objects in the timeline.
    private Dictionary<string, int> levelInstability; // Instability in a level

    [Header("Local Instability")]
    [SerializeField] private float curLocalInstability;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        base.OnNetworkSpawn();
        if (instance == null)
        {
            instance = this;
        } else if (instance != null || instance != this)
        {
            Destroy(this);
        }
        levels = LevelManager.instance.GetLevelSceneNames();
    }

    // Update the level's instability. This should happen at the end of a level/start of level selection
    public void UpdateLevelInstability(string levelName, int instabilityToAdd)
    {
        if (!IsServer) return; 
        if (!levels.Contains(levelName))
        {
            levels.Add(levelName);
        }
        levelInstability[levelName] += instabilityToAdd;
    }

    private void RemoveObjectsAndInstability()
    {
        foreach(var item in ItemSender.SentItems)
        {
        }
    }


    public int GetLevelInstability(string levelName)
    {
        if (!IsServer) return 0;
        if (!levels.Contains(levelName))
        {
            levels.Add(levelName);
        }
        levelInstability.TryGetValue(levelName, out int _levelInstability);
        return _levelInstability;
    }



    // adds instability and objects to levels. It should visit multiple locations (levels) and
    // then it should add a random number of objects. Instability should be added with a random number from 1-5 for each object
    // but generally prefer the numbers 1-2 with higher numbers being rarer.
    // It should do this until it has gone to all visit locations. The total objects and instability should be divided amongst the locations.
    private IEnumerator IncreaseInstability()
    {
        int instabilityPerVisit = 5;
        int touristVisitLocations = Random.Range(2, 6);
        if (levels.Count < touristVisitLocations) touristVisitLocations = levels.Count;
        int totalInstabilityToAdd = instabilityPerVisit * touristVisitLocations;

        // List of locations to visit
        List<string> unvisitedLocations = new(levels);

        while (touristVisitLocations > 0)
        {
            yield return null;
            // Select a random location
            int randomLocationIndex = Random.Range(0, unvisitedLocations.Count);
            string selectedLocation = unvisitedLocations[randomLocationIndex];

            instabilityPerVisit = 5 * day + Random.Range(-2, 2);
            if (touristVisitLocations == 1)
            {
                instabilityPerVisit = totalInstabilityToAdd;
            }
            totalInstabilityToAdd -= instabilityPerVisit;

            UpdateLevelInstability(selectedLocation, instabilityPerVisit);
            

            // Remove location from the list
            unvisitedLocations.RemoveAt(randomLocationIndex);
            touristVisitLocations--;
            if (unvisitedLocations.Count == 0)
            {
                yield break;
            }
        }
    }


}
