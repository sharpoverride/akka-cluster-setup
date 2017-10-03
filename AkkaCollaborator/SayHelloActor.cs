using System;
using Akka.Actor;
using Akka.Cluster;

namespace AkkaCollaborator
{
    public class SayHelloActor: ReceiveActor
    {
        protected Akka.Cluster.Cluster Cluster = Akka.Cluster.Cluster.Get(Context.System);

        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new SayHelloActor());
        }

        public SayHelloActor()
        {

            Receive<SampleMessages.HelloReply>(reply =>
            {
                Console.WriteLine($"Yeees! The server knows me {this.Self.Path}");
            });

            Receive<ClusterEvent.MemberUp>(memberUp =>
            {
                if (memberUp.Member.HasRole("clustersystem"))
                {
                    var member = memberUp.Member;
                    var receiver = Context.ActorSelection(member.Address + "/user/server-hello");
                    //Tell(new SampleMessages.Hello(), Self);

                    Context.System.Scheduler.ScheduleTellRepeatedly(
                        TimeSpan.FromSeconds(15),
                        TimeSpan.FromSeconds(5),
                        receiver,
                        new SampleMessages.Hello(), 
                        Self
                        );
                }

            });
        }

        protected override void PreStart()
        {
            base.PreStart();
            Cluster.Subscribe(Self, new[] { typeof(ClusterEvent.MemberUp) });
            Cluster.RegisterOnMemberUp(() =>
            {
                // create routers and other things that depend on me being UP in the cluster
            });

        }

        /// <summary>
        /// Re-subscribe on restart
        /// </summary>
        protected override void PostStop()
        {
            Cluster.Unsubscribe(Self);
        }

    }
}
