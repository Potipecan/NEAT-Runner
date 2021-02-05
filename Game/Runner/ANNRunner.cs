using Godot;
using System.Collections.Generic;
using System;
using SharpNeat.Phenomes;
using SharpNeat.EvolutionAlgorithms;


namespace A_NEAT_arena.Game
{
    public class ANNRunner : BaseRunner
    {
        IBlackBox _brain;
        private List<RayCast2D> Rays;

        
        

        public ANNRunner() : base()
        {
            Rays = new List<RayCast2D>();
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Rays.Add(GetNode<RayCast2D>("BRRayCast"));
            Rays.Add(GetNode<RayCast2D>("MRRayCast"));
            Rays.Add(GetNode<RayCast2D>("TRRayCast"));
            Rays.Add(GetNode<RayCast2D>("TMRayCast"));
            Rays.Add(GetNode<RayCast2D>("TLRayCast"));
            Rays.Add(GetNode<RayCast2D>("MLRayCast"));
            Rays.Add(GetNode<RayCast2D>("BLRayCast"));
            Rays.Add(GetNode<RayCast2D>("BMRayCast"));
        }

        public void Init(IBlackBox brain)
        {
            _brain = brain;
        }

        protected override void HandleInput()
        {
            var inputs = new float[18];
            int c = 0;

            foreach(var r in Rays)
            {
                // get distance to colliding object
                inputs[c] = r.GetCollisionPoint().DistanceTo(r.GlobalPosition);
                c++;

                #region get type of colliding object
                var coll = (Node)r.GetCollider();
                if(coll == null)
                {
                    inputs[c] = 0f;
                    c++;
                    continue;
                }

                var groups = coll.GetGroups();
                if (groups.Count > 0)
                {
                    switch (groups[0])
                    {
                        case "Environment":
                            inputs[c] = 2f;
                            break;
                        case "Coins":
                            inputs[c] = 3f;
                            break;
                        case "Flags":
                            inputs[c] = 4f;
                            break;
                        case "Danger":
                            inputs[c] = 5f;
                            break;
                        default:
                            inputs[c] = 0f;
                            break;
                    }
                }
                else inputs[c] = 0f;
                #endregion
                c++;
            };

            inputs[c] = Position.x;
            inputs[c + 1] = Position.y;

            //string chk = "";
            //foreach (var s in inputs) { chk += $"{s}, "; }
            //GD.Print($"{chk}");
        }

        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        //  public override void _Process(float delta)
        //  {
        //      
        //  }
    }
}
