using GrainInterfaces;
using Orleans.Streams;
using Common;

public class QueueGrain : Grain, IQueueGrain
{
    private readonly Queue<string> _waitingPlayers = new();

    public async Task AddPlayer(string playerName)
    {
        _waitingPlayers.Enqueue(playerName);
        Console.WriteLine($"Players {_waitingPlayers.Count}. last is {playerName}");
        await TryStarRoom();
    }

    private async Task TryStarRoom()
    {
        if (_waitingPlayers.Count >= 2)
        {
            string player1 = _waitingPlayers.Dequeue();
            string player2 = _waitingPlayers.Dequeue();


            CreateRoom(out Guid roomGuid, out IRoomGrain roomGrain);

            await NotyfyOnRoomCreated(roomGuid);

            await roomGrain.StartGame(player1, player2);
        }
    }

    private async Task NotyfyOnRoomCreated(Guid roomGuid)
    {
        IStreamProvider streamProvider = this.GetStreamProvider(Constans.StreamProvider);
        StreamId roomFindStreamId = StreamId.Create(Constans.QueueFindRoomStream, Constans.MainQueueKey);
        IAsyncStream<Guid> roomFindStream = streamProvider.GetStream<Guid>(roomFindStreamId);

        await roomFindStream.OnNextAsync(roomGuid);
    }

    private void CreateRoom(out Guid roomGuid, out IRoomGrain roomGrain)
    {
        roomGuid = Guid.NewGuid();
        roomGrain = GrainFactory.GetGrain<IRoomGrain>(roomGuid);
    }
}
