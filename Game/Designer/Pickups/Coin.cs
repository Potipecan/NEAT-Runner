using Godot;
using System;

namespace A_NEAT_arena.Game
{
    public class Coin : Area2D
    {
        public override void _Ready()
        {
            base._Ready();
            Connect("body_entered", this, nameof(OnBodyEntered));
        }

        public void OnBodyEntered(Node body)
        {
            //GD.Print($"Bling {Name}");
            if (body.GetType().IsSubclassOf(typeof(BaseRunner)))
            {
                //GD.Print("Bling!");
                ((BaseRunner)body).PickupCoin(this);
            }
        }
    } 
}