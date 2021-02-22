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
        public static int InputCount { get; } = 18;
        public static int OutputCount { get; } = 2;

        private delegate void TriggerRaycastUpdate();
        private event TriggerRaycastUpdate RaycastUpdate;

        private delegate void AddRayCastException(RID rid);
        private event AddRayCastException AddException;

        private IBlackBox _brain;
        private NeatGenome _genome;
        public NeatGenome Genome { get => _genome; }
        private List<RayCast2D> Rays;
        private Node2D Eye;

        private float positionPollTimer;
        private Vector2[] lastPolledPosition;
        private static float positionPollInterval = 1f;

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
            Score = -1000f;

        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Eye = GetNode<Node2D>("Eye");

            foreach (RayCast2D c in Eye.GetChildren())
            {
                Rays.Add(c);
                RaycastUpdate += c.ForceRaycastUpdate;
                RaycastUpdate += c.ForceUpdateTransform;
                AddException += c.AddExceptionRid;
            }

            lastPolledPosition = new Vector2[3];
            lastPolledPosition[0] = GlobalPosition;
            lastPolledPosition[1] = GlobalPosition;
            lastPolledPosition[2] = GlobalPosition;

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

            PollPosition(positionPollInterval);

        }

        public void Init(NeatGenome genome, IBlackBox phenome)
        {
            _brain = phenome;
            _genome = genome;

            Name = $"Gen_{genome.BirthGeneration}_Specie_{genome.SpecieIdx}_ID_{genome.Id}";
        }

        private void RotateEye()
        {
            if (State == RunnerState.OnLeftWall)
            {
                Eye.Rotation = 0f;
                return;
            }
            else if (State == RunnerState.OnRightWall)
            {
                Eye.Rotation = Mathf.Pi;
                return;
            }
            else if (Velocity == new Vector2())
            {
                Eye.Rotation = -Mathf.Pi / 2;
                return;
            }
            else Eye.Rotation = Velocity.Angle();
            RaycastUpdate?.Invoke();
        }

        protected void PollPosition(float delta)
        {
            positionPollTimer += delta;
            if (positionPollTimer >= positionPollInterval)
            {
                var pollCenter = new Vector2((lastPolledPosition[0].x + lastPolledPosition[1].x + lastPolledPosition[2].x) / 3f, (lastPolledPosition[0].y + lastPolledPosition[1].y + lastPolledPosition[2].y) / 3f);

                Score += pollCenter.DistanceSquaredTo(GlobalPosition) / 60f;
                lastPolledPosition[0] = lastPolledPosition[1];
                lastPolledPosition[1] = lastPolledPosition[2];
                lastPolledPosition[2] = GlobalPosition;
                positionPollTimer -= positionPollInterval;
            }
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
            c++;
            _brain.InputSignalArray[c] = (int)State; // Input 18: runner state

            // ANN activation
            _brain.Activate();

            // process outputs
            Move = (float)_brain.OutputSignalArray[0] * 2f - 1f;
            Jump = _brain.OutputSignalArray[1] > 0.5f;
        }

        public override void Die(CauseOfDeath cause)
        {
            if (State == RunnerState.Dead) return;
            State = RunnerState.Dead;
            //Task.Run(() =>
            //{
                switch (cause)
                {
                    case CauseOfDeath.Saw:
                        Score += 50;
                        break;
                    case CauseOfDeath.Laser:
                        Score += 100f;
                        break;
                    case CauseOfDeath.Idling:
                        Score += 10;
                        break;
                    case CauseOfDeath.Void:
                        Score -= 0f;
                        break;
                }

                PollPosition(2f);
                _genome.EvaluationInfo.SetFitness(Score);
                base.Die(cause);
            //});
        }

        public override void PickupCoin(Coin coin)
        {
            if (!PickedUpCoins.Contains(coin))
            {
                Score += Coin.Value;
                PickedUpCoins.Add(coin);
                AddException?.Invoke(coin.GetRid());
            }
        }

        public override void TouchFlag(Flag flag)
        {
            if (!TouchedFlags.Contains(flag))
            {
                Score += Flag.Value;
                TouchedFlags.Add(flag);
                AddException?.Invoke(flag.GetRid());
            }
        }
        #endregion
    }
}
