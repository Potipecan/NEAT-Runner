using Godot;
using System.Collections.Generic;
using System;
using SharpNeat.Phenomes;
using SharpNeat.EvolutionAlgorithms;
using System.Threading;
using System.Threading.Tasks;
using A_NEAT_arena.NEAT;

namespace A_NEAT_arena.Game
{
    public class ANNRunner : BaseRunner
    {
        IBlackBox _brain;
        private List<RayCast2D> Rays;
        private SemaphoreSlim DeathSignal;
        private PhenomePack pack;

        
        

        public ANNRunner() : base()
        {
            Rays = new List<RayCast2D>();
            DeathSignal = new SemaphoreSlim(0, 1);
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

        public void Init(PhenomePack set)
        {
            _brain = set.Phenome;
            pack = set;
        }

        protected override void HandleInput()
        {            
            int c = 0;

            foreach(var r in Rays)
            {
                // get distance to colliding object
                _brain.InputSignalArray[c] = r.GetCollisionPoint().DistanceTo(r.GlobalPosition);
                c++;

                #region get type of colliding object
                var coll = (Node)r.GetCollider();
                if(coll == null)
                {
                    _brain.InputSignalArray[c] = 0f;
                    c++;
                    continue;
                }

                var groups = coll.GetGroups();
                if (groups.Count > 0)
                {
                    switch (groups[0])
                    {
                        case "Environment":
                            _brain.InputSignalArray[c] = 2f;
                            break;
                        case "Coins":
                            _brain.InputSignalArray[c] = 3f;
                            break;
                        case "Flags":
                            _brain.InputSignalArray[c] = 4f;
                            break;
                        case "Danger":
                            _brain.InputSignalArray[c] = 5f;
                            break;
                        default:
                            _brain.InputSignalArray[c] = 0f;
                            break;
                    }
                }
                else _brain.InputSignalArray[c] = 0f;
                #endregion
                c++;
            };

            _brain.InputSignalArray[c] = Position.x;
            _brain.InputSignalArray[c + 1] = Position.y;

            //string chk = "";
            //foreach (var s in inputs) { chk += $"{s}, "; }
            //GD.Print($"{chk}");

            // process inputs
            _brain.Activate();

            // process outputs
            Move = (float)_brain.OutputSignalArray[0];
            Jump = _brain.OutputSignalArray[1] > 0f;
        }

        public override void Die(Node2D cause)
        {
            EmitSignal(nameof(Died), this);
            pack.Score = Score;
            pack.Destination.Start();
            base.Die(cause);
        }
    }
}
