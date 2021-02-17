using Godot;
using System.Collections.Generic;
using System;

namespace A_NEAT_arena.Game
{
    public class Coin : Area2D
    {
        public static readonly float Value = 100;

        private List<BaseRunner> ignoreList;

        public override void _Ready()
        {
            ignoreList = new List<BaseRunner>();
            base._Ready();
            Connect("body_entered", this, nameof(OnBodyEntered));
        }

        public void OnBodyEntered(Node body)
        {
            //GD.Print($"Bling {Name}");
            if (body.GetType().IsSubclassOf(typeof(BaseRunner)) && !ignoreList.Contains(body as BaseRunner))
            {
                ignoreList.Add(body as BaseRunner);
                ((BaseRunner)body).PickupCoin(this);
            }
        }
    } 
}