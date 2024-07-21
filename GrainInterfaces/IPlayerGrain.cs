namespace GrainInterfaces;

public interface IPlayerGrain : IGrainWithStringKey
{
    Task<int> GetScore();
    Task IncrementScore();
 
}

