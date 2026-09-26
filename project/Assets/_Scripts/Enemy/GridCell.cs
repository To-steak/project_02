public struct GridCell
{
    public bool IsWalkable;
    public float GroundHeight;
    // 0 = +X 
    // 1 = -X
    // 2 = +Z
    // 3 = -Z
    public byte Connections;
}
