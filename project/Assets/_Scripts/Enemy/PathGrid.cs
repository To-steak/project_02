using UnityEngine;

public class PathGrid : MonoBehaviour
{
    [SerializeField] private AgentProfile _profile; // 적마다 Profile을 만들고 각각 구워둔다.

    public const float CELL_SIZE = 1.0f;
    public const int GRID_SIZE = 10;
    public const int TOTAL_CELLS = GRID_SIZE * GRID_SIZE;

    private const float PROBE_UP = 5.0f;
    private const float PROBE_DOWN = 10.0f;
    private const float CLEARANCE = 0.05f;
    private const float MAX_DROP = 1.0f;
    private const float CONNECT_THRESHOLD = 0.1f; // 목표에 이만큼 근접하면 연결로 본다

    private GridCell[] _cells;

    public Vector3 Origin => transform.position - new Vector3(GRID_SIZE * CELL_SIZE, 0.0f, GRID_SIZE * CELL_SIZE) * 0.5f;

    private void Awake()
    {
        BakeWorld();
    }

    [ContextMenu("BakeWorld")]
    public void BakeWorld()
    {
        _cells = new GridCell[TOTAL_CELLS];
        LayerMask layer = _profile.GroundLayer | _profile.ObstacleLayer;
        for (int z = 0; z < GRID_SIZE; z++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                _cells[z * GRID_SIZE + x] = Probe(x, z, layer);
            }
        }
    }

    private GridCell Probe(int x, int z, LayerMask layer)
    {
        GridCell cell = default;

        Vector3 center = Origin + new Vector3((x + 0.5f) * CELL_SIZE, 0.0f, (z + 0.5f) * CELL_SIZE);
        Vector3 source = center + Vector3.up * PROBE_UP;
        if (!Physics.Raycast(source, Vector3.down, out RaycastHit hitInfo, PROBE_UP + PROBE_DOWN, layer, QueryTriggerInteraction.Ignore))
        {
            return cell;
        }

        cell.GroundHeight = hitInfo.point.y;
        if (!CharacterPhysics.IsWalkable(hitInfo.normal, _profile.SlopeLimit))
        {
            return cell;
        }

        Vector3 stand = new Vector3(center.x, hitInfo.point.y + CLEARANCE, center.z);
        CharacterPhysics.GetCapsule(stand, _profile.Radius, _profile.Height, out Vector3 bottom, out Vector3 top);
        if (Physics.CheckCapsule(bottom, top, _profile.Radius, layer, QueryTriggerInteraction.Ignore))
        {
            return cell;
        }

        cell.IsWalkable = true;
        return cell;
    }

    public bool TryConvertCellCoord(Vector3 worldPosition, out int x, out int z)
    {
        Vector3 localPosition = worldPosition - Origin;
        x = Mathf.FloorToInt(localPosition.x / CELL_SIZE);
        z = Mathf.FloorToInt(localPosition.z / CELL_SIZE);

        return x >= 0 && x < GRID_SIZE && z >= 0 && z < GRID_SIZE;
    }

    public bool TryGetRandomCell(out int x, out int z)
    {
        x = 0;
        z = 0;

        if (_cells == null)
        {
            return false;
        }

        // 시작 지점을 무작위로 잡고 순회하며 첫 번째 walkable을 고른다.
        int offset = Random.Range(0, TOTAL_CELLS);

        for (int i = 0; i < TOTAL_CELLS; i++)
        {
            int index = (offset + i) % TOTAL_CELLS;
            if (!_cells[index].IsWalkable)
            {
                continue;
            }

            x = index % GRID_SIZE;
            z = index / GRID_SIZE;
            return true;
        }

        return false;
    }

    public Vector3 ConvertWorldCoord(int index)
    {
        return ConvertWorldCoord(index % GRID_SIZE, index / GRID_SIZE);
    }

    public Vector3 ConvertWorldCoord(int x, int z)
    {
        _ = TryGetGridCell(x, z, out GridCell cell);
        return Origin + new Vector3((x + 0.5f) * CELL_SIZE, 0.0f, (z + 0.5f) * CELL_SIZE) + Vector3.up * cell.GroundHeight;
    }

    public bool IsConnected(int fromX, int fromZ, int toX, int toZ)
    {
        if (!TryGetGridCell(fromX, fromZ, out GridCell from) || !from.IsWalkable)
        {
            return false;
        }

        if (!TryGetGridCell(toX, toZ, out GridCell to) || !to.IsWalkable)
        {
            return false;
        }

        // 낭떠러지
        if (to.GroundHeight - from.GroundHeight < -MAX_DROP)
        {
            return false;
        }

        // 두 칸 사이를 실제 캡슐로 훑어본다.
        Vector3 source = ConvertWorldCoord(fromX, fromZ) + Vector3.up * CLEARANCE;
        Vector3 target = ConvertWorldCoord(toX, toZ) + Vector3.up * CLEARANCE;
        LayerMask layer = _profile.GroundLayer | _profile.ObstacleLayer;

        Vector3 moved = CharacterPhysics.Walk(source, target, _profile.Radius, _profile.Height, _profile.SlopeLimit, _profile.StepHeight, layer, true, out _);
        Vector3 step = moved - target;
        step.y = 0.0f;

        return step.sqrMagnitude < CONNECT_THRESHOLD * CONNECT_THRESHOLD;
    }

    public bool TryGetGridCell(int x, int z, out GridCell cell)
    {
        cell = default;
        if (_cells == null || x < 0 || x >= GRID_SIZE || z < 0 || z >= GRID_SIZE)
        {
            return false;
        }

        cell = _cells[z * GRID_SIZE + x];
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_profile == null) return;
        if (_cells == null) BakeWorld();

        for (int z = 0; z < GRID_SIZE; z++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                GridCell cell = _cells[z * GRID_SIZE + x];
                Vector3 center = ConvertWorldCoord(x, z) + Vector3.up * 0.02f;

                Gizmos.color = cell.IsWalkable ? new Color(0f, 1f, 0f, 0.3f) : new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawCube(center, new Vector3(CELL_SIZE * 0.9f, 0.01f, CELL_SIZE * 0.9f));
            }
        }

        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
        for (int z = 0; z < GRID_SIZE; z++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                Vector3 from = ConvertWorldCoord(x, z) + Vector3.up * 0.05f;

                if (IsConnected(x, z, x + 1, z) && IsConnected(x + 1, z, x, z))
                {
                    Gizmos.DrawLine(from, ConvertWorldCoord(x + 1, z) + Vector3.up * 0.05f);
                }

                if (IsConnected(x, z, x, z + 1) && IsConnected(x, z + 1, x, z))
                {
                    Gizmos.DrawLine(from, ConvertWorldCoord(x, z + 1) + Vector3.up * 0.05f);
                }
            }
        }

        Gizmos.color = Color.white;
        Vector3 size = new Vector3(GRID_SIZE * CELL_SIZE, 0.0f, GRID_SIZE * CELL_SIZE);
        Gizmos.DrawWireCube(transform.position, size);
    }
#endif
}
