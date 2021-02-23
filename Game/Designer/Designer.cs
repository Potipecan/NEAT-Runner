using Godot;
using Godot.Collections;
using System;

namespace A_NEAT_arena.Game
{
    [Tool]
    public class Designer : TileMap
    {

        [Export] public string SavesFolder { get; set; } = "res://Saves/";
        [Export] public string FileName { get; set; }
        private bool save;
        [Export] public bool Save
        {
            get => save;
            set
            {
                save = false;
                if (value) SaveFile();
            }
        }

        public override void _Ready()
        {
            base._Ready();
        }

        public void SaveFile()
        {
            if (FileName == "")
            {
                var p = new Popup();
                return;
            }
            var plan = new Dictionary<int, Array<Vector2>>();
            foreach (int id in TileSet.GetTilesIds())
            {
                plan.Add(id, new Array<Vector2>());
            }

            var cells = GetUsedCells();
            foreach (Vector2 tile in cells)
            {
                plan[GetCell((int)tile.x, (int)tile.y)].Add(tile);
            }
            //if(plan[4].Count != 1)
            //{
            //    GD.Print("Level must have exactly one flag.");
            //    return;
            //}

            var seg = Preloads.Segment.Instance() as Segment;
            seg.Build(FileName, plan);

            var pack = new PackedScene();
            pack.Pack(seg);

            ResourceSaver.Save(SavesFolder + FileName + ".tscn", pack);
        }
    }

    public static class Preloads
    {
        public static TileSet EnvTileSet, DangerTileSet;
        public static PackedScene Coin, Flag, Segment, PlayerRunner, ANNRunner;

        static Preloads()
        {
            EnvTileSet = GD.Load<TileSet>("res://Game/Designer/TileSets/Env.tres");
            DangerTileSet = GD.Load<TileSet>("res://Game/Designer/TileSets/Danger.tres");
            Coin = GD.Load<PackedScene>("res://Game/Designer/Pickups/Coin.tscn");
            Flag = GD.Load<PackedScene>("res://Game/Designer/Pickups/Flag.tscn");
            Segment = GD.Load<PackedScene>("res://Game/Designer/Segment.tscn");
            PlayerRunner = GD.Load<PackedScene>("res://Game/Runner/PlayerRunner.tscn");
            ANNRunner = GD.Load<PackedScene>("res://Game/Runner/ANNRunner.tscn");
        }

    }

}