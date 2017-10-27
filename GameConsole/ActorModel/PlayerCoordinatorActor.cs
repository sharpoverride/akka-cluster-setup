using Akka.Actor;
using Akka.Persistence;
using GameConsole.Messages;
namespace GameConsole.ActorModel
{
    public class PlayerCoordinatorActor: ReceivePersistentActor
    {
        private const int DefaultStartingHealth = 100;
        public PlayerCoordinatorActor()
        {
            PersistenceId = "MasterPlayerCoordination";
            Command<CreatePlayer>(Handler);
            Command<CauseError>(Handler);
            Command<DisplayPlayerStatus>(Handler);
            Command<HitPlayer>(Handler);


            Recover<CreatePlayer>(CreatePlayer);
        }

        private bool Handler(CreatePlayer createPlayer)
        {
            DisplayHelper.WriteLine($"{nameof(PlayerCoordinatorActor)} persists CreatePlayerMessage from {createPlayer.PlayerName}");
            Persist(createPlayer, player => CreatePlayer(player));
            return true;
        }

        private bool CreatePlayer(CreatePlayer player)
        {
            DisplayHelper.WriteLine($"PlayerCoordinatorActor received CreatePlayerMessage for {player.PlayerName}");
            Context.ActorOf(
                Props.Create(() =>
                    new PlayerActor(player.PlayerName, DefaultStartingHealth)), player.PlayerName);

            return true;
        }

        private bool Handler(CauseError causeError)
        {
            return true;
        }

        private bool Handler(DisplayPlayerStatus displayPlayerStatus)
        {
            return true;
        }

        private bool Handler(HitPlayer hitPlayer)
        {
            return true;
        }

        public override string PersistenceId { get; }
    }
}
