using Common;
using GrainInterfaces;
using Orleans.Streams;

namespace Client
{
    public class GuessGame
    {
        private static string _thisPlayerName;
        private static StreamSubscriptionHandle<Guid> _roomFindingSubscription;
        private static StreamSubscriptionHandle<(string, int, int)> _endOfGameSubscription;
        private IClusterClient _client;

        public GuessGame(IClusterClient client)
        {
            _client = client;
        }

        public async void StartGameplay()
        {
            await InitPlayerFields();

            await SubscribeOnRoomFound();
            await EnterQueue();
            Console.WriteLine("Waiting for a match...");
        }

        private async Task InitPlayerFields()
        {
            Console.Write("Enter Player Name: ");
            _thisPlayerName = await GeInputAsync() ?? Guid.NewGuid().ToString()[0..2];
            Console.WriteLine($"You are {_thisPlayerName}");
        }

        async Task<string> GeInputAsync() => await Task.Run(Console.ReadLine);

        private async Task EnterQueue()
        {
            var queueGrain = _client.GetGrain<IQueueGrain>(Constans.MainQueueKey);
            await queueGrain.AddPlayer(_thisPlayerName);
        }

        #region Subscribtion
        private async Task SubscribeOnRoomFound()
        {
            var streamProvider = _client.GetStreamProvider(Constans.StreamProvider);
            var startStreamId = StreamId.Create(Constans.QueueFindRoomStream, Constans.MainQueueKey);
            var roomFoundStream = streamProvider.GetStream<Guid>(startStreamId);

            _roomFindingSubscription = await roomFoundStream.SubscribeAsync(OnRoomFound);

        }

        private async Task SubscribeOnEndOfGame(Guid roomGuid)
        {

            var streamProvider = _client.GetStreamProvider(Constans.StreamProvider);
            var endGameId = StreamId.Create(Constans.EndGameStream, roomGuid);
            var endGameStream = streamProvider.GetStream<(string, int, int)>(endGameId);

            _endOfGameSubscription = await endGameStream.SubscribeAsync(OnEndOfGame);
        }
        #endregion

        #region Subscribers
        private async Task OnRoomFound(Guid roomGuid, StreamSequenceToken token)
        {
            await _roomFindingSubscription.UnsubscribeAsync();
            Console.WriteLine($"Room field Up! Room ID: {roomGuid.ToString()[0..3]}...");
            Console.WriteLine("Enter your guess (0-100): ");

            var roomGrain = _client.GetGrain<IRoomGrain>(roomGuid);
            await SubscribeOnEndOfGame(roomGuid);

            string input = await GeInputAsync();
            if (int.TryParse(input, out int guess) == false)
                guess = 0;

            guess = Math.Clamp(guess, 0, 100);

            await roomGrain.SubmitGuess(_thisPlayerName, guess);
        }

        private async Task OnEndOfGame((string winner, int winnerGuess, int serverNumber) gameEndContext, StreamSequenceToken token)
        {
            await _endOfGameSubscription.UnsubscribeAsync();
            Console.WriteLine($"End of Game, Winner is {gameEndContext.winner} with gues {gameEndContext.winnerGuess} server number was {gameEndContext.serverNumber}");
            Console.WriteLine($"Restart in 5 seconds");
            await Task.Delay(4000);
            StartGameplay();

        }
        #endregion

    }
}