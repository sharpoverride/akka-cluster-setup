using System;
using Akka.Actor;
using Akka.Persistence;
using GameConsole.Messages;

namespace GameConsole.ActorModel
{
    class PlayerActor : ReceivePersistentActor
    {
        private readonly string _playerName;
        private int _health;

        public PlayerActor(string playerName, int startingHealth)
        {
            PersistenceId = $"player-{playerName}";

            _playerName = playerName;
            _health = startingHealth;

            DisplayHelper.WriteLine($"{_playerName} created");

            Command<HitPlayer>(message => HitPlayer(message));
            Command<DisplayPlayerStatus>(message => DisplayPlayerStatus());
            Command<CauseError>(message => SimulateError());

            Recover<HitPlayer>(hit =>
            {
                DisplayHelper.WriteLine($"{playerName} replaying HitMessage {hit} from journal");
                _health -= hit.Damage;
            });
        }

        private void HitPlayer(HitPlayer message)
        {
            DisplayHelper.WriteLine($"{_playerName} received HitMessage");
            DisplayHelper.WriteLine($"{_playerName} persisting HitMessage");

            Persist(message, hitMessage =>
            {
                
            DisplayHelper.WriteLine($"{_playerName} persisted HitMessage ok, updating actor player state");
                _health -= message.Damage;
            });
        }

        private void DisplayPlayerStatus()
        {
            DisplayHelper.WriteLine($"{_playerName} received DisplayStatusMessage");

            Console.WriteLine($"{_playerName} has {_health} health");
        }

        private void SimulateError()
        {
            DisplayHelper.WriteLine($"{_playerName} received CauseErrorMessage");

            throw new ApplicationException($"Simulated exception in player: {_playerName}");
        }

        public override string PersistenceId { get; }
    }
}
