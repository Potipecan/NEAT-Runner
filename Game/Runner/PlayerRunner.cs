using Godot;
using A_NEAT_arena.Game;

namespace A_NEAT_arena.Game
{
    public class PlayerRunner : BaseRunner
    {
        public PlayerRunner() : base() { }

        public override void Die(Node2D cause)
        {
        }

        protected override void HandleInput()
        {
            Jump = Input.IsActionPressed("ui_up");
            if (Input.IsActionPressed("ui_right")) Move = 1f;
            else if (Input.IsActionPressed("ui_left")) Move = -1f;
            else Move = 0f;
        }

        public override void PickupCoin(Coin coin)
        {
            if (!PickedUpCoins.Contains(coin))
            {
                Score += 100;
                PickedUpCoins.Add(coin);
                coin.QueueFree();
            }
        }

        public override void TouchFlag(Flag flag)
        {
            if (!TouchedFlags.Contains(flag))
            {
                Score += 1000;
            }
        }
    } 
}
