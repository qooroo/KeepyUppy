namespace KeepyUppy.Backplane.Broadcast
{
    public interface IBroadcaster
    {
        void BroadcastMessage(string message);
        void UpdateTokenAvailability(bool tokenAvailable);
    }
}