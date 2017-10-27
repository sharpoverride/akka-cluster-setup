namespace GameConsole.Messages
{
    public class HitPlayer
    {
        public HitPlayer(int damage)
        {
            Damage = damage;
        }

        public int Damage { get; }
    }
}
