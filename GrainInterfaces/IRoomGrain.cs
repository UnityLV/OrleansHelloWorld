namespace GrainInterfaces;

public interface IRoomGrain : IGrainWithGuidKey
{
    Task StartGame(string player1Id, string player2Id);
    Task SubmitGuess(string playerId, int guess);
}
