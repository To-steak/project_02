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

    public static void Smooth(PathGrid grid, List<int> path)
    {
        if (path.Count <= 2)
        {
            return;
        }

        int write = 1;
        int anchor = path[0];
        for (int i = 2; i < path.Count; i++)
        {
            if (!HasLine(grid, anchor, path[i]))
            {
                anchor = path[i - 1];
                path[write++] = anchor;
            }
        }

        path[write++] = path[^1];
        path.RemoveRange(write, path.Count - write);
    }

    private static bool HasLine(PathGrid grid, int from, int to)
    {
        int size = PathGrid.GRID_SIZE;
        int x = from % size; // 현재 X 좌표
        int z = from / size; // 현재 Z 좌표
        int toX = to % size; // 목표 X 좌표
        int toZ = to / size; // 목표 Z 좌표
        int nx = Mathf.Abs(toX - x); // 넘어야 할 X까지 총 개수
        int nz = Mathf.Abs(toZ - z); // 넘어야 할 Z까지 총 개수
        int sx = toX > x ? 1 : -1; // 가야할 X 방향(1 = 앞, -1 = 뒤)
        int sz = toZ > z ? 1 : -1; // 가야할 Z 방향(1 = 앞, -1 = 뒤)
        for (int ix = 0, iz = 0; ix < nx || iz < nz;)
        {
            // 다음 X 경계를 만나는 진행률 = (0.5 + ix) / nx  (출발 0, 도착 1)
            // 0.5 : 셀 중심에서 출발하므로 첫 경계는 반 칸 거리
            // + ix : 이후 경계는 한 칸 간격이므로 이미 넘은 경계 수만큼 더함
            // / nx : 칸 수를 진행률로 변환 (X를 다 넘으면 1을 넘어서 다시 선택되지 않음)
            // Z도 같은 방식. 두 진행률을 교차 곱셈으로 비교하고, 2를 곱해 0.5를 없애 정수로 계산
            // decision < 0: X 경계를 먼저 만남 / > 0: Z 경계를 먼저 만남 / == 0: 동시에 만남
            int decision = (1 + 2 * ix) * nz - (1 + 2 * iz) * nx;
            if (decision == 0) // 4개 셀이 만나는 꼭짓점을 지날 때: 양옆 경로가 모두 연결되어야 통과
            {
                if (!grid.IsConnected(x, z, x + sx, z) || !grid.IsConnected(x + sx, z, x + sx, z + sz))
                {
                    return false;
                }
                if (!grid.IsConnected(x, z, x, z + sz) || !grid.IsConnected(x, z + sz, x + sx, z + sz))
                {
                    return false;
                }
                x += sx;
                z += sz;
                ix++;
                iz++;
            }
            else if (decision < 0) // 음수이면 X 쪽으로 한 칸 가야할 때
            {
                if (!grid.IsConnected(x, z, x + sx, z))
                {
                    return false;
                }
                x += sx;
                ix++;
            }
            else // 양수이면 Z 쪽으로 한 칸 가야할 때
            {
                if (!grid.IsConnected(x, z, x, z + sz))
                {
                    return false;
                }
                z += sz;
                iz++;
            }
        }

        return true;
    }
}
