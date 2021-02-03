using Godot;
using System;
using Godot.Collections;

namespace A_NEAT_arena.Game
{
    [Tool]
    public class Segment : Node2D
    {
        public delegate void Freed(Segment sender);
        public event Freed FreedEvent;

        [Export] public Rect2 UsedRect;
        [Export] public Dictionary<int, Array<Vector2>> Plan;
        public Flag Flag;
        [Export] string FlagName;

        public bool IsObsolete { get; set; }

        private TileMap Env, Danger;

        public Segment() : base()
        {
            IsObsolete = false;
        }

        public override void _Ready()
        {
            base._Ready();
            Flag = GetNode<Flag>(FlagName);
        }

        public void Build(string lvlName, Dictionary<int, Array<Vector2>> plan)
        {
            Name = lvlName;
            Plan = plan;

            //GD.Print("Check 1");
            UsedRect = new Rect2();
            Env = new TileMap()
            {
                Name = "Env",
                CellSize = new Vector2(60, 60),
                TileSet = Preloads.EnvTileSet,
                CollisionLayer = 2,
            };

            //GD.Print("Check 2");
            Danger = new TileMap()
            {
                Name = "Danger",
                CellSize = new Vector2(60, 60),
                TileSet = Preloads.DangerTileSet,
                CollisionLayer = 5,
            };
            //GD.Print("Check 3");
            Env.AddToGroup("Environment", true);
            Danger.AddToGroup("Danger", true);

            //GD.Print("Check 4");
            AddChild(Env);
            Env.Owner = this;
            AddChild(Danger);
            Danger.Owner = this;

            foreach (int i in plan.Keys)
            {
                switch (i)
                {

                    case 0:
                    case 1:
                        //GD.Print("Check 5 1");
                        foreach (Vector2 tile in plan[i])
                        {
                            Env.SetCell((int)tile.x, (int)tile.y, i);
                        }

                        UsedRect = Env.GetUsedRect();

                        break;
                    case 2:
                        //GD.Print("Check 5 2");
                        try
                        {
                            foreach (Vector2 tile in plan[2])
                            {
                                Danger.SetCell((int)tile.x, (int)tile.y, 2);
                            }
                            UsedRect = UsedRect.Merge(Danger.GetUsedRect());
                        }
                        catch (Exception ex)
                        {
                            GD.Print($"{ex.Message}");
                        }

                        break;

                    case 3:
                        //GD.Print("Check 5 3");
                        try
                        {
                            foreach (Vector2 tile in plan[3])
                            {
                                Node2D coin = (Node2D)Preloads.Coin.Instance();

                                coin.Name = $"Coin_{tile.x}_{tile.y}";
                                coin.Position = tile * 60f;
                                //GD.Print($"{coin.Name} {coin.Position}");

                                AddChild(coin);
                                coin.Owner = this;
                                UsedRect = UsedRect.Expand(tile);
                            }
                            //GD.Print($"{Coins.GetChildCount()}");
                        }
                        catch (Exception ex)
                        {
                            GD.Print($"{ex.Message}");
                        }

                        break;

                    case 4:
                        //GD.Print("Check 5 4");
                        try
                        {
                            Vector2 tile = plan[4][0];

                            Node2D flag = (Node2D)Preloads.Flag.Instance();

                            flag.Name = $"Flag_{tile.x}_{tile.y}";
                            flag.Position = tile * 60f;
                            //GD.Print($"{flag.Name} {flag.Position}");

                            AddChild(flag);
                            flag.Owner = this;
                            FlagName = flag.Name;
                            UsedRect = UsedRect.Expand(tile);
                        }
                        catch (Exception ex)
                        {
                            GD.Print($"{ex.Message}");
                        }
                        break;
                }
            }

            //GD.Print("Check 6");
            Env.UpdateBitmaskRegion();
        }

        public void OnViewportExited(Viewport vp)
        {
            if (IsObsolete)
            {
                FreedEvent?.Invoke(this);
                QueueFree();
            }
        }
    }
}
