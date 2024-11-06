using System;
using System.Linq;
using UnityEngine;

public static class Stability
{

    const int DefaultItemStability = 5;
    public static void ProgressStability(this LevelStability[] allLevelStabilities, LevelScene completedScene, LevelEndData levelEndData)
    {
        var completedLevel = allLevelStabilities.GetLevelStability(completedScene);
        var uncompletedLevels = allLevelStabilities.Where(x => x.scene != completedScene).ToArray();

        float completedLevelStability = completedLevel.Stability;

        for (int i = 0; i < levelEndData.CorrectSendCount; i++)
        {
            completedLevel.Stability += DefaultItemStability;
        }

        int distributeWrong = 0;
        for (int i = 0; i < levelEndData.WrongSendCount; i++)
        {
            completedLevel.Stability -= DefaultItemStability / 2;
            distributeWrong -= DefaultItemStability / 2;
        }

        uncompletedLevels.DistributeInstabilityRandomly(distributeWrong);

        int day = levelEndData.Dayprogression;

        float rand1 = Mathf.Sin(day) * Mathf.Exp(day * 0.1f);
        float rand2 = 3 + Mathf.Sin(day * 3) * Mathf.Exp(day * 0.15f);
        int instabilityToDistribute = day + Mathf.FloorToInt(UnityEngine.Random.Range(rand1, rand2));

        uncompletedLevels.DistributeInstabilityRandomly(instabilityToDistribute);
        uncompletedLevels.DecreaseRandom(10 + day);

        int dayClamped = Mathf.Max(day - 2, 0);
        float exponentiaDecrese = Mathf.Exp(dayClamped / 5) - Mathf.Exp(dayClamped / 10);
        uncompletedLevels.DecreseAllStability(exponentiaDecrese);

        completedLevel.Stability = completedLevelStability;
    }
    public static int TotalStability(this LevelStability[] levels)
    {
        int count = levels.Length;
        float totalStability = 0;
        foreach (var levelStability in levels)
        {
            totalStability += levelStability.Stability;
        }
        totalStability /= count;

        return Mathf.FloorToInt(totalStability);
    }
    static void DistributeInstabilityRandomly(this LevelStability[] levels, int instabilityToDistribute)
    {
        // Distribute some random instability to all levels
        while (instabilityToDistribute > 0)
        {
            int randomLevel = UnityEngine.Random.Range(0, levels.Length);
            LevelStability scene = levels[randomLevel];

            // reduce the amount of instability to distribute to levels with more visits
            int max = instabilityToDistribute - Mathf.FloorToInt(scene.Visits);

            // Random amount of instability to distribute
            int amount = UnityEngine.Random.Range(1, Math.Max(max, 1));

            // Distribute the instability
            instabilityToDistribute -= randomLevel;
            scene.Stability -= amount;
        }
    }


    public static LevelStability GetLevelStability(this LevelStability[] levels, LevelScene scene)
    {
        foreach (var item in levels)
        {
            if (item.scene == scene)
            {
                return item;
            }
        }
        return null;
    }
    static void DecreseAllStability(this LevelStability[] levels, float exponentiaDecrese)
    {
        foreach (var levelStability in levels)
        {
            levelStability.Stability -= exponentiaDecrese;
        }
    }
    static void DecreaseRandom(this LevelStability[] levels, float amount)
    {
        int randomLevel = UnityEngine.Random.Range(0, levels.Length);
        levels[randomLevel].Stability -= amount;
    }
}