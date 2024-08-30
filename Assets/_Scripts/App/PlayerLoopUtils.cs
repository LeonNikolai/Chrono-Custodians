using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

public static class PlayerLoopUtils
{
    public static void PrintPlayerLoop(PlayerLoopSystem playerLoop)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Player Loop:");
        foreach (var subsystem in playerLoop.subSystemList)
        {
            PrintSubSystem(subsystem, sb, 0);
        }

        Debug.Log(sb.ToString());
    }

    private static void PrintSubSystem(PlayerLoopSystem subsystem, StringBuilder sb, int level)
    {
        sb.Append(' ', level * 2);
        sb.AppendLine(subsystem.type.Name);
        if (subsystem.subSystemList == null || subsystem.subSystemList.Length == 0) return;
        foreach (var subSubSystem in subsystem.subSystemList)
        {
            PrintSubSystem(subSubSystem, sb, level + 1);
        }
    }

    public static bool InsertSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
    {
        if (loop.type != typeof(T)) {
            if (loop.subSystemList == null) return false;
            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                if (!InsertSystem<T>(ref loop.subSystemList[i], systemToInsert, index)) continue;
                return true;
            }
            return false;
        }
        var playerLoopSystemList = new List<PlayerLoopSystem>(loop.subSystemList);
        playerLoopSystemList.Insert(index, systemToInsert);
        loop.subSystemList = playerLoopSystemList.ToArray();
        return true;
    }
    public static void RemoveSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove)
    {
        if (loop.subSystemList == null) return;

        var playerLoopSystemList = new List<PlayerLoopSystem>(loop.subSystemList);
        for (int i = 0; i < playerLoopSystemList.Count; i++)
        {
            if (playerLoopSystemList[i].type == systemToRemove.type && playerLoopSystemList[i].updateDelegate == systemToRemove.updateDelegate)
            {
                playerLoopSystemList.RemoveAt(i);
                loop.subSystemList = playerLoopSystemList.ToArray();
            }
        }

        for (int i = 0; i < loop.subSystemList.Length; i++)
        {
            RemoveSystem<T>(ref loop.subSystemList[i], systemToRemove);
        }
    }


}