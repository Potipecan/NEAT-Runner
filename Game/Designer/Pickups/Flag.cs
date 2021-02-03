using Godot;
using System;
using System.Collections.Generic;

namespace A_NEAT_arena.Game
{
    [Tool]
    public class Flag : Area2D
    {
        private List<BaseRunner> TouchedRunners;

        public Flag() : base()
        {
            TouchedRunners = new List<BaseRunner>();
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Connect("body_entered", this, nameof(OnBodyEntered));
        }

        public void OnBodyEntered(Node body)
        {
            if (body.GetType().IsSubclassOf(typeof(BaseRunner)))
            {
                if (!TouchedRunners.Contains(body as BaseRunner))
                {
                    (body as BaseRunner).TouchFlag(this);
                    TouchedRunners.Add(body as BaseRunner);
                }
            }
        }
    }

}