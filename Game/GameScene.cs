using Godot;
using System;

public class GameScene : Node2D
{
    [Export] public float LaserSpeed;

    private Flag StartFlag;
    private Camera2D Camera;
    private Sprite DeathLaser;

    public override void _Ready()
    {
        GetTree().Paused = true;
        StartFlag = GetNode<Flag>("StartArea/StartFlag");
        Camera = GetNode<Camera2D>("Camera2D");
        DeathLaser = GetNode<Sprite>("DeathLaser");
        Camera.MakeCurrent();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Camera.Position = new Vector2(Camera.Position.x + LaserSpeed * delta, Camera.Position.y);
        DeathLaser.Position = new Vector2(DeathLaser.Position.x + LaserSpeed * delta, DeathLaser.Position.y);
    }
}
