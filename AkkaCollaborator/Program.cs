using System.Net;
using System.Net.Sockets;
using Akka.Actor;
using Akka.Configuration;

namespace AkkaCollaborator
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostname = System.Net.Dns.GetHostName();
            var system = ActorSystem.Create("ClusterSystem",
                ConfigurationFactory.ParseString(
                    $@"akka.remote.dot-netty.tcp.hostname={hostname}"
                    ).WithFallback(
                GetConfig()));

            var helloActor = system.ActorOf(SayHelloActor.Props(), "say-hello");


            KeepConsoleRunning();
        }

        private static void KeepConsoleRunning()
        {
#if DEBUGINCONTAINER
            KeepAppRunningHackDuringDebugInWindowsContainer();
#else
            Console.WriteLine("Press ENTER to exit..");
            Console.ReadLine();// For some reason this does not work 
            // when visual studio is attached
#endif
        }

        private static void KeepAppRunningHackDuringDebugInWindowsContainer()
        {
            var server = new TcpListener(
                IPAddress.IPv6Any,
                65056
            );
            server.Start();
            server.AcceptTcpClient();
        }

        private static Config GetConfig()
        {
            var configString = @"
                    clustersystem{
		                    actorsystem: ""ClusterSystem"" #POPULATE NAME OF YOUR ACTOR SYSTEM HERE
	                    }
			
                    akka {
	                    actor { 
		                    provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
	                    }
						
	                    remote {
		                    log-remote-lifecycle-events = DEBUG
		                    dot-netty.tcp {
			                    transport-class = ""Akka.Remote.Transport.DotNetty.TcpTransport, Akka.Remote""
			                    applied-adapters = []
			                    transport-protocol = tcp
			                    #will be populated with a dynamic host-name at runtime if left uncommented
			                    #public-hostname = ""POPULATE STATIC IP HERE""
			                    hostname = ""0.0.0.0""
			                    port = 0
		                    }
	                    }     
											
	                    cluster {
		                    #will inject this node as a self-seed node at run-time
		                    seed-nodes = [
                            ""akka.tcp://ClusterSystem@akka-seed:4053""
                            ] #manually populate other seed nodes here, i.e. ""akka.tcp://lighthouse@127.0.0.1:4053"", ""akka.tcp://lighthouse@127.0.0.1:4044""
		                    roles = [colaborator]
	                    }
                    }
            ";
            return ConfigurationFactory.ParseString(configString);
        }
    }
}
