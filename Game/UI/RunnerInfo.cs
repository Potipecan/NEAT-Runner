using Godot;
using SharpNeat.Genomes.Neat;
using System;

public class RunnerInfo : ColorRect
{
    private Label RunnerData;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        RunnerData = GetNode<Label>("RunnerInfoLabel/Data");
    }

    public void UpdateRunnerData(NeatGenome genome)
    {
        UpdateRunnerData((int)genome.Id, genome.SpecieIdx, (float)genome.Complexity, genome.NodeList.Count, genome.ConnectionList.Count);
    }

    public void UpdateRunnerData(int id, int specieId, float complexity, int neuronCount, int connectionCount)
    {
        RunnerData.Text = $"{id}\n{specieId}\n{complexity:F2}\n{neuronCount}\n{connectionCount}";
    }
}
