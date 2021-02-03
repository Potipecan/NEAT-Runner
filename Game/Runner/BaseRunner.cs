using Godot;
using System.Collections.Generic;

namespace A_NEAT_arena.Game
{
    public abstract class BaseRunner : KinematicBody2D
    {
        //private CollisionPolygon2D LCollider, RCollider, UCollider, DCollider;

        /// <summary>
        /// Jump speed in pixels per second.
        /// </summary>
        [Export] public float JumpSpeed { get; set; } = 500;
        /// <summary>
        /// Gravitational acceleration in pixels per second sqared.
        /// </summary>
        [Export] private float Gravity { get; set; } = 500;
        /// <summary>
        /// Maximum horizontal velocity in pixels per second.
        /// </summary>
        [Export] private float MaxSpeed { get; set; } = 500;
        /// <summary>
        /// Deceleration caused by friction in pixels per second.
        /// </summary>
        [Export] private float Friction { get; set; } = 200;
        /// <summary>
        /// Horizontal acceleration in pixels per second sqared.
        /// </summary>
        [Export] private float Acceleration { get; set; } = 700;
        /// <summary>
        /// Friction sliding along a wall
        /// </summary>
        [Export] private float WallFriction { get; set; } = 300;
        /// <summary>
        /// The amount of time in seconds the runner ca accelerate upwards. 
        /// </summary>
        [Export] private float MaxJumpTime { get; set; }

        [Signal] public delegate void Died(BaseRunner sender);
        public event Died DiedEvent;

        protected bool Jump;
        protected float JumpTimeLeft;
        protected float Move;
        protected Vector2 Velocity;
        protected RunnerState State;

        protected List<Coin> PickedUpCoins;
        protected List<Flag> TouchedFlags;
        public float Score { get; set; }

        public BaseRunner() : base()
        {
            Velocity = new Vector2();
            State = RunnerState.Neutral;
            PickedUpCoins = new List<Coin>();
            TouchedFlags = new List<Flag>();
        }

        protected void WallJump()
        {
            if (Jump)
            {
                var wallJump = new Vector2();
                wallJump[0] = (State == RunnerState.OnLeftWall) ? 1 : -1;
                wallJump[1] = -1;
                wallJump = wallJump.Normalized() * JumpSpeed;
                Velocity += wallJump;
                State = RunnerState.Neutral;
            }
        }

        /// <summary>
        /// Calculates a new velocity.
        /// </summary>
        /// <param name="speed">Current speed</param>
        /// <param name="acc">Acceleration</param>
        /// <param name="delta">Time in seconds since last call</param>
        /// <param name="damp">Deceleration working in the opposite direction of movement</param>
        /// <param name="limit">Speed limit</param>
        /// <returns></returns>
        protected float CalcVelocityChange(float speed, float acc, float delta, float damp = 0, float limit = -1)
        {
            var dir = (speed >= 0) ? 1f : -1f;
            var newSpeed = (speed + acc * delta) * dir - damp * delta;
            if (newSpeed > limit && limit > 0) newSpeed = limit;
            else if (newSpeed < 0 && acc == 0f) newSpeed = 0;
            return newSpeed * dir;
        }


        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            HandleInput();

            // Process state and input
            switch (State)
            {
                case RunnerState.Neutral: // In air, not mid jump. Can move horizontally at half acceleation
                    Velocity.x = CalcVelocityChange(Velocity.x, Acceleration * Move / 2f, delta, limit: MaxSpeed);
                    break;

                case RunnerState.Grounded: // On ground. Can jump and move with full acceleration
                    JumpTimeLeft = 0.5f;
                    Velocity.x = CalcVelocityChange(Velocity.x, Acceleration * Move, delta, Friction, MaxSpeed);
                    if (Jump)
                    {
                        State = RunnerState.Airborne;
                        Velocity.y -= JumpSpeed;
                    }
                    break;

                case RunnerState.Airborne:
                    if (Move != 0) Velocity.x = CalcVelocityChange(Velocity.x, Acceleration * Move * 0.75f, delta, limit: MaxSpeed);
                    if (!Jump || JumpTimeLeft <= 0f) State = RunnerState.Neutral;
                    else
                    {
                        JumpTimeLeft -= delta;
                    }
                    break;

                case RunnerState.OnLeftWall:
                case RunnerState.OnRightWall:
                    Velocity.y = CalcVelocityChange(Velocity.y, Gravity, delta, WallFriction);
                    WallJump();
                    break;


            }

            if (State == RunnerState.Neutral || State == RunnerState.Grounded) Velocity.y += Gravity * delta;

            //GD.Print($"{State} {Velocity}");

            // Movement and collision processing
            var newVelocity = MoveAndSlide(Velocity, new Vector2(0, -1));
            int collNum = GetSlideCount();
            for (int i = 0; i < collNum; i++)
            {
                var collider = GetSlideCollision(i).Collider as Node2D;
                var groups = collider.GetGroups();

                if (groups.Contains("Environment")) continue;

                if (groups.Contains("Danger"))
                {
                    Die(collider);
                }
            }

            if (collNum > 0)
            {
                if (newVelocity.y == 0 && Velocity.y > 0)
                {
                    State = RunnerState.Grounded;
                }
                else if (newVelocity.x == 0)
                {
                    if (Velocity.x > 0) State = RunnerState.OnRightWall;
                    else if (Velocity.x < 0) State = RunnerState.OnLeftWall;
                }
            }
            else if (newVelocity.y > 0)
            {
                State = RunnerState.Neutral;
            }

            Velocity = newVelocity;
        }

        #region functions to override
        protected abstract void HandleInput();

        public virtual void Die(Node2D cause)
        {
            DiedEvent?.Invoke(this);
        }

        public abstract void PickupCoin(Coin coin);

        public abstract void TouchFlag(Flag flag);
        #endregion

        public enum RunnerState
        {
            Neutral = 0,
            Airborne = 1,
            Grounded = 2,
            OnRightWall = 3,
            OnLeftWall = 4,
            Dead = 5
        }
    }

}

