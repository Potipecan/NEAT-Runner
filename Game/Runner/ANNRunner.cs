using Godot;
using System.Collections.Generic;
using System;
using SharpNeat.Phenomes;
using SharpNeat.EvolutionAlgorithms;
using System.Threading;
using System.Threading.Tasks;
using A_NEAT_arena.NEAT;
using SharpNeat.Genomes.Neat;

namespace A_NEAT_arena.Game
{
    public class ANNRunner : BaseRunner
    {
        public static int InputCount { get; } = 17;
        public static int OutputCount { get; } = 2;

        private IBlackBox _brain;
        private NeatGenome _genome;
        private List<RayCast2D> Rays;
        private Node2D Eye;
        private float idleTimer, idleTimeout;
        private float IdleTimer
        {
            get => idleTimer;
            set
            {
                idleTimer = value;
                if (IdleTimer >= idleTimeout) Die(CauseOfDeath.Idling);
            }
        }
        //private PhenomePack pack;




        public ANNRunner() : base()
        {
            Rays = new List<RayCast2D>();
            idleTimeout = 2f;
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Eye = GetNode<Node2D>("Eye");

            foreach (var c in Eye.GetChildren())
            {
                Rays.Add(c as RayCast2D);
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            if (idle) IdleTimer += delta;
            else
            {
                //lastPosition = GlobalPosition;
                IdleTimer = 0f;
            }
        }

        public void Init(NeatGenome genome, IBlackBox phenome)
        {
            _brain = phenome;
            _genome = genome;

            Name = $"Gen_{genome.BirthGeneration}_Specie_{genome.SpecieIdx}_ID_{genome.Id}";
        }

        private void RotateEye()
        {
            if(State == RunnerState.OnLeftWall)
            {
                Eye.Rotation = 0f;
                return;
            }
            if(State == RunnerState.OnRightWall)
            {
                Eye.Rotation = Mathf.Pi;
                return;
            }
            if (Velocity == new Vector2())
            {
                Eye.Rotation = -Mathf.Pi / 2;
                return;
            }
            Eye.Rotation = Velocity.Angle();
        }

        #region inherited overridable functions
        protected override void HandleInput()
        {

            RotateEye();

            int c = 0;

            // Inputs 1 - 14: ray distances and collider types
            foreach (var r in Rays)
            {
                // get distance to colliding object
                _brain.InputSignalArray[c] = r.GetCollisionPoint().DistanceTo(r.GlobalPosition);
                c++;

                #region get type of colliding object
                var coll = (Node)r.GetCollider();
                if (coll == null)
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
                            _brain.InputSignalArray[c] = 0.2f;
                            break;
                        case "Coins":
                            _brain.InputSignalArray[c] = 0.3f;
                            break;
                        case "Flags":
                            _brain.InputSignalArray[c] = 0.4f;
                            break;
                        case "Danger":
                            _brain.InputSignalArray[c] = 0.5f;
                            break;
                        case "Laser":
                            _brain.InputSignalArray[c] = 0.6f;
                            break;
                        default:
                            _brain.InputSignalArray[c] = 0f;
                            break;
                    }
                }
                else _brain.InputSignalArray[c] = 0f;
                #endregion
                c++;
            }

            _brain.InputSignalArray[c] = Velocity.x; // Input 15: horizontal velocity
            c++;
            _brain.InputSignalArray[c] = Velocity.y; // Input 16: vertical velocity
            c++;
            _brain.InputSignalArray[c] = GlobalPosition.x - PlayArea.LaserPosition; // Input 17: distance to laser

            // ANN activation
            _brain.Activate();

            // process outputs
            Move = (float)_brain.OutputSignalArray[0] * 2f - 1f;
            Jump = _brain.OutputSignalArray[1] > 0.5f;
        }

        public override void Die(CauseOfDeath cause)
        {
            switch (cause)
            {
                case CauseOfDeath.Saw:
                    Score -= 200;
                    break;
                case CauseOfDeath.Laser:
                    Score -= 100f;
                    break;
                case CauseOfDeath.Idling:
                    Score -= 1000;
                    break;
                case CauseOfDeath.Void:
                    Score -= 0f;
                    break;
            }


            _genome.EvaluationInfo.SetFitness(Score);
            base.Die(cause);
        }

        public override void PickupCoin(Coin coin)
        {
            if (!PickedUpCoins.Contains(coin))
            {
                Score += 100;
                PickedUpCoins.Add(coin);
                Rays.ForEach(r => r.AddExceptionRid(coin.GetRid()));
            }
        }

        public override void TouchFlag(Flag flag)
        {
            if (!TouchedFlags.Contains(flag))
            {
                Score += 1000;
                TouchedFlags.Add(flag);
                Rays.ForEach(r => r.AddExceptionRid(flag.GetRid()));
            }
        }
        #endregion
    }
}
