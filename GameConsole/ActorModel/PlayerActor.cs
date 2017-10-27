using System;
using Akka.Actor;
using Akka.Persistence;
using GameConsole.Messages;

namespace GameConsole.ActorModel
{
    class PlayerActorState
    {
        public string PlayerName { get; set; }
        public int Health { get; set; }
        public override string ToString()
        {
            return $"[PlayerActorState {PlayerName} {Health}]";
        }
    }
    class PlayerActor : ReceivePersistentActor
    {
        private PlayerActorState _state;
        private int _eventCount;

        public PlayerActor(string playerName, int startingHealth)
        {
            PersistenceId = $"player-{playerName}";

            _state = new PlayerActorState{PlayerName = playerName, Health = startingHealth};


            DisplayHelper.WriteLine($"{_state.PlayerName} created");

            Command<HitPlayer>(message => HitPlayer(message));
            Command<DisplayPlayerStatus>(message => DisplayPlayerStatus());
            Command<CauseError>(message => SimulateError());

            Recover<HitPlayer>(hit =>
            {
                DisplayHelper.WriteLine($"{playerName} replaying HitMessage {hit} from journal");
                _state.Health -= hit.Damage;
            });

            Recover<SnapshotOffer>(offer =>
            {
                DisplayHelper.WriteLine($"{playerName} received SnapshotOffer from snapshot");

                _state = (PlayerActorState) offer.Snapshot;

                DisplayHelper.WriteLine($"{_state.PlayerName} state {_state} set from snapshot.");
            });
        }

        private void HitPlayer(HitPlayer message)
        {
            DisplayHelper.WriteLine($"{_state.PlayerName} received HitMessage");
            DisplayHelper.WriteLine($"{_state.PlayerName} persisting HitMessage");

            Persist(message, hitMessage =>
            {

                DisplayHelper.WriteLine($"{_state.PlayerName} persisted HitMessage ok, updating actor player state");
                _state.Health -= message.Damage;

                _eventCount++;

                if (_eventCount == 5)
                {
                    _eventCount = 0;
                    DisplayHelper.WriteLine($"{_state.PlayerName} save snapshot");
                    SaveSnapshot(_state);
                    DisplayHelper.WriteLine($"{_state.PlayerName} reset counter");

                }
            });
        }

        private void DisplayPlayerStatus()
        {
            DisplayHelper.WriteLine($"{_state.PlayerName} received DisplayStatusMessage");

            Console.WriteLine($"{_state.PlayerName} has {_state.Health} health");
        }

        private void SimulateError()
        {
            DisplayHelper.WriteLine($"{_state.PlayerName} received CauseErrorMessage");

            throw new ApplicationException($"Simulated exception in player: {_state.PlayerName}");
        }

        public override string PersistenceId { get; }
    }
}
