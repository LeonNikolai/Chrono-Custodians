using System;
using System.Linq;
using UnityEngine;

public static class Stability
{
    public static LevelEndData ProgressStability(this LevelStability[] allLevelStabilities, LevelScene completedScene, int day, ItemSendEvent[] itemevents, int RemainingUnstableItems)
    {
        int SendWrongInstability = 0;
        int SendCorrectInstability = 0;
        foreach (ItemSendEvent item in itemevents)
        {
            if (item.ItemData.TimePeriods.Contains(completedScene.TimePeriod))
            {
                if (item.TargetPeriod == completedScene.TimePeriod) continue;
                SendWrongInstability += item.InstabilityWorth;
                continue;
            }

            if (item.ItemData.TimePeriods.Contains(item.TargetPeriod))
            {

                SendCorrectInstability += item.InstabilityWorth;
                continue;
            }
            SendWrongInstability += item.InstabilityWorth;
        }

        var completedLevel = allLevelStabilities.GetLevelStability(completedScene);
        var uncompletedLevels = allLevelStabilities.Where(x => x.scene != completedScene).ToArray();

        float completedLevelStability = completedLevel.Stability;

        completedLevel.Stability += SendCorrectInstability;
        int distributeWrong = SendWrongInstability / 2;
        completedLevel.Stability -= SendWrongInstability / 2;

        uncompletedLevels.DistributeInstabilityRandomly(distributeWrong);

        float rand1 = Mathf.Sin(day) * Mathf.Exp(day * 0.1f);
        float rand2 = 3 + Mathf.Sin(day * 3) * Mathf.Exp(day * 0.15f);
        int instabilityToDistribute = day + Mathf.FloorToInt(UnityEngine.Random.Range(rand1, rand2));

        uncompletedLevels.DistributeInstabilityRandomly(instabilityToDistribute);
        uncompletedLevels.DecreaseRandom(25 + day);

        int dayClamped = Mathf.Max(day - 2, 0);
        float exponentiaDecrese = Mathf.Exp(dayClamped / 5) - Mathf.Exp(dayClamped / 10);
        uncompletedLevels.DecreseAllStability(exponentiaDecrese);
        completedLevel.Stability = completedLevelStability;
        return new LevelEndData
        {
            Level = completedScene,
            RemainingUnstableItemStability = RemainingUnstableItems,
            SendWrongInstability = SendWrongInstability,
            SendCorrectInstability = SendCorrectInstability,
            OtherDecrease = exponentiaDecrese + instabilityToDistribute,
            Dayprogression = day,
            itemSendEvets = itemevents
        };
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
        if (levels.Length == 0)
        {
            Debug.LogError("No levels to distribute instability to");
            return;
        }
        // Distribute some random instability to all levels
        while (instabilityToDistribute > 0)
        {
            int randomLevel = UnityEngine.Random.Range(0, levels.Length);
            LevelStability scene = levels[randomLevel];
            if (scene == null)
            {
                Debug.LogWarning("Scene is null");
                instabilityToDistribute -= 1;
                continue;
            }

            // reduce the amount of instability to distribute to levels with more visits
            int max = instabilityToDistribute - Mathf.FloorToInt(scene?.Visits ?? 1);

            // Random amount of instability to distribute
            int amount = UnityEngine.Random.Range(1, Math.Max(max, 1));

            // Distribute the instability
            instabilityToDistribute -= amount;
            scene.Stability -= amount;
        }
    }


    public static LevelStability GetLevelStability(LevelScene scene)
    {
        foreach (var item in GameManager.instance.levelStabilities)
        {
            if (item.scene == scene)
            {
                return item;
            }
        }
        return null;
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