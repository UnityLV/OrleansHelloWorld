
using Common;
using GrainInterfaces;
using Orleans.Providers;

public class PlayerGrain : Grain, IPlayerGrain
{
    private readonly IPersistentState<PlayerState> _state;

    public PlayerGrain([PersistentState("PlayerState", Constans.GrainStrorageName)] IPersistentState<PlayerState> playerState)
    {
        _state = playerState;
    }
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await _state.ReadStateAsync();
    }
    public Task<int> GetScore() => Task.FromResult(_state.State.Score);

    public Task IncrementScore()
    {
        _state.State.Score++;
        Console.WriteLine($"Score added to {this.GetPrimaryKeyString()}, Now scrore is {_state.State.Score}");

        return _state.WriteStateAsync();
    }

}
