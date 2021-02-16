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

        private SpinBox SpeciesNumSB, GenSizeSB, BatchSizeSB, ElitismSB, SelectionSB, PropagationRatioSB, InterspeciesMatingSB;
        private ConfirmationDialog ConfirmDialog;


        public int SpeciesNum { get => (int)SpeciesNumSB.Value; }
        public int Population { get => (int)GenSizeSB.Value; }
        public int BatchSize { get => (int)BatchSizeSB.Value; }
        private NeatEvolutionAlgorithmParameters neatParams;
        public NeatEvolutionAlgorithmParameters NeatParams
        {
            get => neatParams; 
            set
            {
                neatParams = value;
                SpeciesNumSB.Value = neatParams.SpecieCount;
                ElitismSB.Value = neatParams.ElitismProportion;
                SelectionSB.Value = neatParams.SelectionProportion;
                PropagationRatioSB.Value = neatParams.OffspringSexualProportion * 100;
                InterspeciesMatingSB.Value = neatParams.InterspeciesMatingProportion;
            }
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            SpeciesNumSB = GetNode<SpinBox>("SpeciesNumSB");
            GenSizeSB = GetNode<SpinBox>("PopulationSB");
            BatchSizeSB = GetNode<SpinBox>("BatchSizeSB");
            ElitismSB = GetNode<SpinBox>("ElitismSB");
            SelectionSB = GetNode<SpinBox>("SelectionSB");
            PropagationRatioSB = GetNode<SpinBox>("PropagationRatioSB");
            InterspeciesMatingSB = GetNode<SpinBox>("InterspeciesMatingSB");

            ConfirmDialog = GetNode<ConfirmationDialog>("ConfirmDialog");

            GetNode<Button>("SetParamsButton").Connect("pressed", this, nameof(On_SetParamsButton_Pressed));
            ConfirmDialog.Connect("confirmed", this, nameof(On_ConfirmDialog_Confirmed));

            NeatParams = new NeatEvolutionAlgorithmParameters();

            //GD.Print($"Species num: {SpeciesNum}");
        }

        private void GetNeatParameters()
        {
            NeatParams = new NeatEvolutionAlgorithmParameters()
            {
                SpecieCount = (int)SpeciesNumSB.Value,
                SelectionProportion = SelectionSB.Value,
                ElitismProportion = ElitismSB.Value,
                OffspringSexualProportion = PropagationRatioSB.Value / 100,
                OffspringAsexualProportion = 1 - PropagationRatioSB.Value / 100,
                InterspeciesMatingProportion = InterspeciesMatingSB.Value
            };
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