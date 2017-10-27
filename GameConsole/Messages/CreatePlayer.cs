namespace GameConsole.Messages
{
    public class CreatePlayer
    {
        public string PlayerName { get; }

        public CreatePlayer(string playerName)
        {
            PlayerName = playerName;
        }
    }
}
