using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    private static readonly int[] DIRECTION_X = { 1, -1, 0, 0 };
    private static readonly int[] DIRECTION_Y = { 0, 0, 1, -1 };

    public static bool TryFindPath(PathGrid grid, int startX, int startZ, int goalX, int goalZ, List<int> path)
    {
        path.Clear();

        int start = ConvertGridIndex(startX, startZ);
        int goal = ConvertGridIndex(goalX, goalZ);
        if (start == goal)
        {
            path.Add(goal);
            return true;
        }

        int totalCells = PathGrid.TOTAL_CELLS;
        int[] cost = new int[totalCells]; // 시작 Cell에서 여기까지의 실제 비용. MAX값으로 초기화
        int[] parent = new int[totalCells]; // 어느 Cell에서 왔는지
        bool[] closed = new bool[totalCells]; // 방문했던 Cell들
        for (int i = 0; i < totalCells; i++)
        {
            cost[i] = int.MaxValue;
            parent[i] = -1;
        }

        cost[start] = 0;

        List<int> open = new List<int> { start }; // 다음에 방문할 Cell들
        while (open.Count > 0)
        {
            int bestIndex = -1;
            int bestScore = int.MaxValue;
            for (int i = 0; i < open.Count; i++)
            {
                int index = open[i];
                int score = cost[index] + Heuristic(index, goal);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            int current = open[bestIndex];
            open.RemoveAt(bestIndex);
            if (current == goal)
            {
                Retrace(parent, start, goal, path);
                return true;
            }

            closed[current] = true;

            int gridSize = PathGrid.GRID_SIZE;
            int x = current % gridSize;
            int z = current / gridSize;
            for (int j = 0; j < DIRECTION_X.Length; j++)
            {
                int nextX = x + DIRECTION_X[j];
                int nextZ = z + DIRECTION_Y[j];
                if (nextX < 0 || nextX >= gridSize || nextZ < 0 || nextZ >= gridSize)
                {
                    continue;
                }

                int next = ConvertGridIndex(nextX, nextZ);
                if (closed[next] || !grid.IsConnected(x, z, nextX, nextZ))
                {
                    continue;
                }

                int candidate = cost[current] + 1;
                if (candidate >= cost[next])
                {
                    continue;
                }

                cost[next] = candidate;
                parent[next] = current;
                if (!open.Contains(next))
                {
                    open.Add(next);
                }
            }
        }

        return false;
    }

    private static void Retrace(int[] parent, int start, int goal, List<int> path)
    {
        int current = goal;
        while (current != start)
        {
            path.Add(current);
            current = parent[current];
        }

        path.Add(start);
        path.Reverse();
    }

    private static int Heuristic(int from, int to)
    {
        int size = PathGrid.GRID_SIZE;
        int fromX = from % size;
        int fromZ = from / size;
        int toX = to % size;
        int toZ = to / size;

        return Mathf.Abs(fromX - toX) + Mathf.Abs(fromZ - toZ);
    }

    private static int ConvertGridIndex(int x, int z)
    {
        return z * PathGrid.GRID_SIZE + x;
    }
}
