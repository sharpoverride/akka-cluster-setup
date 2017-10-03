using System;
using Akka.Actor;

namespace AkkaSeed
{
    public class HelloServerActor: UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is SampleMessages.Hello)
            {
                Sender.Tell(new SampleMessages.HelloReply());
            }

            if (message is SampleMessages.HelloReply)
            {
                Console.WriteLine("Why did I receive this HelloReply?");
            }
        }

        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new HelloServerActor());
        }
    }
}
