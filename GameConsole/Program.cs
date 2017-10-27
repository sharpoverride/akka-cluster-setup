using System;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using GameConsole.ActorModel;
using GameConsole.Messages;
using static System.Console;

namespace GameConsole
{
        class Program
        {
            private static ActorSystem System { get; set; }
            private static IActorRef PlayerCoordinator { get; set; }

            static void Main(string[] args)
            {
                System = ActorSystem.Create("Game",
                    GetConfig());

                PlayerCoordinator = System.ActorOf<PlayerCoordinatorActor>("PlayerCoordinator");

                ForegroundColor = ConsoleColor.White;

                DisplayInstructions();


                while (true)
                {
                    Thread.Sleep(2000); // ensure console color set back to white
                    ForegroundColor = ConsoleColor.White;

                    var action = ReadLine();

                    var playerName = action.Split(' ')[0];

                    if (action.Contains("create"))
                    {
                        CreatePlayer(playerName);
                    }
                    else if (action.Contains("hit"))
                    {
                        var damage = int.Parse(action.Split(' ')[2]);

                        HitPlayer(playerName, damage);
                    }
                    else if (action.Contains("display"))
                    {
                        DisplayPlayer(playerName);
                    }
                    else if (action.Contains("error"))
                    {
                        ErrorPlayer(playerName);
                    }
                    else
                    {
                        WriteLine("Unknown command");
                    }
                }
            }

            private static void ErrorPlayer(string playerName)
            {
                System.ActorSelection($"/user/PlayerCoordinator/{playerName}")
                      .Tell(new CauseError());
            }

            private static void DisplayPlayer(string playerName)
            {
                System.ActorSelection($"/user/PlayerCoordinator/{playerName}")
                      .Tell(new DisplayPlayerStatus());
            }

            private static void HitPlayer(string playerName, int damage)
            {
                System.ActorSelection($"/user/PlayerCoordinator/{playerName}")
                      .Tell(new HitPlayer(damage));
            }

            private static void CreatePlayer(string playerName)
            {
                PlayerCoordinator.Tell(new CreatePlayer(playerName));
            }

            private static void DisplayInstructions()
            {
                Thread.Sleep(2000); // ensure console color set back to white
                ForegroundColor = ConsoleColor.White;

                WriteLine("Available commands:");
                WriteLine("<playername> create");
                WriteLine("<playername> hit");
                WriteLine("<playername> display");
                WriteLine("<playername> error");
            }

            private static Config GetConfig()
            {
                var configString = @"
                              akka.persistence {
              
              journal {
                plugin = ""akka.persistence.journal.sql-server""                
                sql-server {
                      class = ""Akka.Persistence.SqlServer.Journal.SqlServerJournal, Akka.Persistence.SqlServer""
                      plugin-dispatcher = ""akka.actor.default-dispatcher""

                      # connection string used for database access
                      connection-string = ""Data Source=(local);Initial Catalog=PSAkka;User ID=sa;Password=yourStrong(!)Password""
                      # can alternativly specify: connection-string-name

                      # default SQL timeout
                      connection-timeout = 30s

                      # SQL server schema name
                      schema-name = dbo

                      # persistent journal table name
                      table-name = EventJournal

                      # initialize journal table automatically
                      auto-initialize = on

                      timestamp-provider = ""Akka.Persistence.Sql.Common.Journal.DefaultTimestampProvider, Akka.Persistence.Sql.Common""
                      metadata-table-name = Metadata
                }
              }
              
            } 
            ";
                return ConfigurationFactory.ParseString(configString);
            }
    }
}
