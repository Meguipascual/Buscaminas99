namespace ServerCore; 

public class ServerState {
    
    public const int GameDurationSeconds = 300;
    
    public long StartTimestamp { get; private set; }
    public long EndTimestamp => StartTimestamp + GameDurationSeconds;
    public bool IsGameActive => IsGameStarted && !IsGameFinished;
    private bool IsGameStarted => StartTimestamp > 0;
    private bool IsGameFinished => DateTimeOffset.UtcNow.ToUnixTimeSeconds() > StartTimestamp + GameDurationSeconds;

    public void StartGame() {
        
        StartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}