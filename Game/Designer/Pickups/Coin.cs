using Godot;
using System;

public class Coin : Area2D
{
    public void OnBodyEntered(Node body)
    {
        GD.Print($"Bling {Name}");
        if (body.GetType().IsSubclassOf(typeof(BaseRunner)))
        {
            GD.Print("Bling!");
            ((BaseRunner)body).PickupCoin(this);
        }
    }
}