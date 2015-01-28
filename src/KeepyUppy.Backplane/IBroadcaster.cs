namespace KeepyUppy.Backplane
{
    public interface IBroadcaster
    {
        void BroadcastTokenAvailability(bool tokenAvailable);
    }
}