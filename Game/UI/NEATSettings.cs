using Godot;
using System;
using SharpNeat.EvolutionAlgorithms;
using System.Threading.Tasks;
using SharpNeat.Core;

namespace A_NEAT_arena.Game
{
    public class NEATSettings : ColorRect
    {
        public event EventHandler ParametersSet;

        private SpinBox SpeciesNumSB, GenSizeSB, BatchesSB;
        private ConfirmationDialog ConfirmDialog;


        public int SpeciesNum { get => (int)SpeciesNumSB.Value; }
        public int Population { get => (int)GenSizeSB.Value; }
        public int Batches { get => (int)BatchesSB.Value; }
        public NeatEvolutionAlgorithmParameters NeatParams { get; set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            SpeciesNumSB = GetNode<SpinBox>("SpeciesNumSB");
            GenSizeSB = GetNode<SpinBox>("PopulationSB");
            BatchesSB = GetNode<SpinBox>("BatchesSB");
            ConfirmDialog = GetNode<ConfirmationDialog>("ConfirmDialog");


            GetNode<Button>("SetParamsButton").Connect("pressed", this, nameof(On_SetParamsButton_Pressed));
            ConfirmDialog.Connect("confirmed", this, nameof(On_ConfirmDialog_Confirmed));

            GD.Print($"Species num: {SpeciesNum}");
        }

        private void GetNeatParameters()
        {
            Task.Run(() =>
            {
                NeatParams = new NeatEvolutionAlgorithmParameters();
                NeatParams.SpecieCount = (int)SpeciesNumSB.Value;
            }).Wait();
        }

        /// <summary>
        /// Connected to SetParamsButton (pressed)
        /// </summary>
        private void On_SetParamsButton_Pressed()
        {
            if (NeatParams != null) ConfirmDialog.PopupCentered();
            else On_ConfirmDialog_Confirmed();
        }

        /// <summary>
        /// Connected to ConfirmDialog (confirmed)
        /// </summary>
        private void On_ConfirmDialog_Confirmed()
        {
            GD.Print("1");
            GetNeatParameters();
            ParametersSet?.Invoke(this, new EventArgs());
        }
    }

}