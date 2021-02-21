using Godot;
using System;

public class EvolutionInfoPanel : ColorRect
{
    private Label InfoLabel, StatLabel, BatchNoLabel, AliveLabel;

    public override void _Ready()
    {
        InfoLabel = GetNode<Label>("Label2/InfoLabel");
        StatLabel = GetNode<Label>("Label3/StatLabel");
        BatchNoLabel = GetNode<Label>("Label2/BatchNo");
        AliveLabel = GetNode<Label>("Label2/AliveCount");
    }

    public void UpdateEvoutionInfo(int generation, int pop, int speciecnt)
    {
        InfoLabel.Text = $"{generation}\n{pop}\n{speciecnt}";
    }

    public void UpdateBatchInfo(int batchnum, int batchsize, int alivecount)
    {
        BatchNoLabel.Text = $"{batchnum}";
        AliveLabel.Text = $"{alivecount} / {batchsize}";
    }

    public void UpdateStats(float maxfitness, float meanfitness, float maxcomplexity, float meancomplexity)
    {
        StatLabel.Text = $"{maxfitness:F2}\n{meanfitness:F2}\n{maxcomplexity:F2}\n{meancomplexity:F2}";
    }
}
