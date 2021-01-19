using Godot;
using System;

public class MainMenu : ColorRect
{
    public void OnPlayBtnPressed()
    {
        GetTree().ChangeScene("res://Game/GameScene.tscn");
    }
}
