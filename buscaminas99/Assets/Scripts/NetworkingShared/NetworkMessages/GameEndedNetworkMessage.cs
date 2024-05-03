using System.Collections.Generic;
using Hazel;

public class GameEndedNetworkMessage : NetworkMessage<GameEndedNetworkMessage> {
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.GameEnded;
    public List<GameEndedPlayerScoreDto> Scores { get; set; } = new List<GameEndedPlayerScoreDto>();
    
    protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
        messageWriter.Write(Scores.Count);
        foreach (var scoreDto in Scores) {
            messageWriter.Write(scoreDto.PlayerId);
            messageWriter.Write(scoreDto.Score);
        }
    }

    protected override void FromMessageReaderImpl(MessageReader messageReader) {
        var scoresLength = messageReader.ReadInt32();
        Scores = new List<GameEndedPlayerScoreDto>(scoresLength);
        for (int i = 0; i < scoresLength; i++) {
            var playerId = messageReader.ReadInt32();
            var score = messageReader.ReadInt32();
            Scores.Add(new GameEndedPlayerScoreDto {
                PlayerId = playerId,
                Score = score,
            });
        }
    }
}

public class GameEndedPlayerScoreDto {
    public int PlayerId { get; set; }
    public int Score { get; set; }
}