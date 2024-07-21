namespace GrainInterfaces;

public interface IQueueGrain : IGrainWithIntegerKey
{
    Task AddPlayer(string playerName);
}
