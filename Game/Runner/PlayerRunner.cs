using Godot;

public class PlayerRunner : BaseRunner
{
    public PlayerRunner() : base() { }

    public override void Die()
    {
        GD.Print("I is ded");
    }

    protected override void HandleInput()
    {
        Jump = Input.IsActionPressed("ui_up");
        if (Input.IsActionPressed("ui_right")) Move = 1f;
        else if (Input.IsActionPressed("ui_left")) Move = -1f;
        else Move = 0f;
    }

    public override void PickupCoin(Coin coin)
    {
        if (!PickedUpCoins.Contains(coin))
        {
            Score += 100;
            PickedUpCoins.Add(coin);
            coin.QueueFree();
            GD.Print("Bling!");
        }
    }

    public override void TouchFlag(Flag flag)
    {
        GD.Print("Checkpoint?");
    }
}
