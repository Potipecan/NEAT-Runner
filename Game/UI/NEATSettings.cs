using Godot;
using System;

public class NEATSettings : ColorRect
{
    private SpinBox SpeciesNumSB, GenSizeSB, BatchesSB;

    public int SpeciesNum { get => (int)SpeciesNumSB.Value; }
    public int GenSize { get => (int)GenSizeSB.Value; }
    public int Batches { get => (int)BatchesSB.Value; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SpeciesNumSB = GetNode<SpinBox>("SpeciesNumSB");
        GenSizeSB = GetNode<SpinBox>("GenSizeSB");
        BatchesSB = GetNode<SpinBox>("BstchesSB");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
