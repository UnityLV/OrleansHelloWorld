using Common;
using GrainInterfaces;
using Orleans.Streams;

public class RoomGrain : Grain, IRoomGrain
{
    private int _serverNumber;
    private readonly Dictionary<string, int> _playerGuesses = new();

    public async Task StartGame(string player1Name, string player2Name)
    {
        _serverNumber = new Random().Next(0, 101);
        _playerGuesses.Clear();
        Console.WriteLine($"Start Game");
        await Task.CompletedTask;
    }

    public async Task SubmitGuess(string playerId, int guess)
    {
        _playerGuesses[playerId] = guess;
        Console.WriteLine($"player {playerId} enter number {guess}");

        if (_playerGuesses.Count == 2)
        {
            KeyValuePair<string, int> winner = _playerGuesses.OrderBy(p => Math.Abs(p.Value - _serverNumber)).First();
            IPlayerGrain winnerGrain = GrainFactory.GetGrain<IPlayerGrain>(winner.Key);
            await winnerGrain.IncrementScore();
            await NofifyOnGameEnd(winner);
        }
    }

    private async Task NofifyOnGameEnd(KeyValuePair<string, int> winner)
    {
        IStreamProvider streamProvider = this.GetStreamProvider(Constans.StreamProvider);
        StreamId endStreamId = StreamId.Create(Constans.EndGameStream, this.GetPrimaryKey());
        IAsyncStream<(string, int, int)> endStream = streamProvider.GetStream<(string, int, int)>(endStreamId);

        await endStream.OnNextAsync((winner.Key, winner.Value, _serverNumber));
    }
}

