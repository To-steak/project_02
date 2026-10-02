public enum WorldId
{
    Tutorial, Test
}
public interface IWorldSelectionService
{
    void RequestWorld(WorldId world);
}