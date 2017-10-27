using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;

namespace AkkaSeed
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = ActorSystem.Create("ClusterSystem",
                GetConfig());

            system.ActorOf(HelloServerActor.Props(), "server-hello");
            KeepConsoleRunning();
        }

        private static void KeepConsoleRunning()
        {
            Thread.Sleep(Timeout.Infinite);
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
# will be populated with a dynamic host-name at runtime if left uncommented
# public-hostname = ""POPULATE STATIC IP HERE""
			                    hostname = ""akka-seed"" # rhino # 0.0.0.0
			                    port = 4053
		                    }
	                    }     
											
	                    cluster {
# will inject this node as a self-seed node at run-time
		                    seed-nodes = [
                            ""akka.tcp://ClusterSystem@akka-seed:4053"" # @rhino:4053 # 0.0.0.0
                            ] #manually populate other seed nodes here, i.e. ""akka.tcp://lighthouse@127.0.0.1:4053"", ""akka.tcp://lighthouse@127.0.0.1:4044""
		                    roles = [clustersystem]
	                    }
                    }
            ";
            return ConfigurationFactory.ParseString(configString);
        }
    }
}
