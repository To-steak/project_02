public class WaitingRoomPresenter
{
    public void RequestWorld(WorldId world)
    {
        WorldSelectionService.Instance.RequestWorld(world);
    }
}