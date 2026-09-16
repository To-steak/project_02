public interface IAnimationEventReceiver
{
    void NotifyAnimationCallback(AnimationID id);
    void NotifyAnimationCommit(AnimationID id);
}