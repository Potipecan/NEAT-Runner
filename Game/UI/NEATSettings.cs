using Godot;
using System;
using SharpNeat.EvolutionAlgorithms;
using System.Threading.Tasks;
using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using A_NEAT_arena.NEAT;

namespace A_NEAT_arena.Game
{
    public class NEATSettings : ColorRect
    {
        public event EventHandler ParametersSet;

        public delegate bool CheckStop(int gen, int eval, float score);
        public static CheckStop CheckStopCondition;

        private int stopConditionTreshold;

        // Experiment control inputs
        private SpinBox SpeciesNumSB, GenSizeSB, BatchSizeSB;
        // Reproduction settings inputs
        private SpinBox ElitismSB, SelectionSB, PropagationRatioSB, InterspeciesMatingSB;
        // Genome settings inputs
        private SpinBox NodeMutation, AddConn, DeleteConn, ConnWeight, InitialConnProportion;

        private OptionButton StopCondition;
        private SpinBox StopCondtionTreshold;


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
                ElitismSB.Value = neatParams.ElitismProportion * 100;
                SelectionSB.Value = neatParams.SelectionProportion * 100;
                PropagationRatioSB.Value = neatParams.OffspringSexualProportion * 100;
                InterspeciesMatingSB.Value = neatParams.InterspeciesMatingProportion;
            }
        }

        private NeatGenomeParameters genomeParams;
        public NeatGenomeParameters GenomeParams
        {
            get => genomeParams;
            set
            {
                genomeParams = value;
                NodeMutation.Value = genomeParams.AddNodeMutationProbability * 100.0;
                AddConn.Value = genomeParams.AddConnectionMutationProbability * 100.0;
                DeleteConn.Value = genomeParams.DeleteConnectionMutationProbability * 100.0;
                ConnWeight.Value = genomeParams.ConnectionWeightMutationProbability * 100.0;
                InitialConnProportion.Value = genomeParams.InitialInterconnectionsProportion * 100.0;
            }
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            SpeciesNumSB = GetNode<SpinBox>("ControlSettings/SpeciesNumSB");
            GenSizeSB = GetNode<SpinBox>("ControlSettings/PopulationSB");
            BatchSizeSB = GetNode<SpinBox>("ControlSettings/BatchSizeSB");

            ElitismSB = GetNode<SpinBox>("Reproduction/ElitismSB");
            SelectionSB = GetNode<SpinBox>("Reproduction/SelectionSB");
            PropagationRatioSB = GetNode<SpinBox>("Reproduction/PropagationRatioSB");
            InterspeciesMatingSB = GetNode<SpinBox>("Reproduction/InterspeciesMatingSB");

            NodeMutation = GetNode<SpinBox>("Genome/AddNode");
            AddConn = GetNode<SpinBox>("Genome/AddConn");
            DeleteConn = GetNode<SpinBox>("Genome/RemoveConn");
            ConnWeight = GetNode<SpinBox>("Genome/Weight");
            InitialConnProportion = GetNode<SpinBox>("Genome/Initial");

            StopCondition = GetNode<OptionButton>("StopCondition/Options");
            StopCondtionTreshold = GetNode<SpinBox>("StopCondition/Treshold");

            ConfirmDialog = GetNode<ConfirmationDialog>("ConfirmDialog");

            GetNode<Button>("SetParamsButton").Connect("pressed", this, nameof(On_SetParamsButton_Pressed));
            ConfirmDialog.Connect("confirmed", this, nameof(On_ConfirmDialog_Confirmed));
            StopCondition.Connect("item_selected", this, nameof(On_StopCondition_Changed));

            NeatParams = new NeatEvolutionAlgorithmParameters();
            GenomeParams = new NeatGenomeParameters();
            CheckStopCondition = GenerationStopCondition;
        }

        public BundledExperimentSettings GetBundledExperimetSettings()
        {
            //GetNeatParameters();
            //GetGenomeParameters();
            return new BundledExperimentSettings(Population, BatchSize, NeatParams, GenomeParams);
        }

        /// <summary>
        /// Sets NEAT evolution parameters
        /// </summary>
        private void GetNeatParameters()
        {
            NeatParams = new NeatEvolutionAlgorithmParameters()
            {
                SpecieCount = (int)SpeciesNumSB.Value,
                SelectionProportion = SelectionSB.Value / 100.0,
                ElitismProportion = ElitismSB.Value / 100.0,
                OffspringSexualProportion = PropagationRatioSB.Value / 100.0,
                OffspringAsexualProportion = 1 - PropagationRatioSB.Value / 100.0,
                InterspeciesMatingProportion = InterspeciesMatingSB.Value
            };
        }

        /// <summary>
        /// Sets the genome parameters according to input values
        /// </summary>
        private void GetGenomeParameters()
        {
            // normalize mutation chances
            var reduction = (NodeMutation.Value + AddConn.Value + DeleteConn.Value + ConnWeight.Value + genomeParams.NodeAuxStateMutationProbability * 100.0) / 100.0;
            NodeMutation.Value /= reduction;
            AddConn.Value /= reduction;
            DeleteConn.Value /= reduction;
            ConnWeight.Value /= reduction;
            GenomeParams.NodeAuxStateMutationProbability /= reduction;

            GenomeParams = new NeatGenomeParameters()
            {
                InitialInterconnectionsProportion = InitialConnProportion.Value / 100.0,
                AddConnectionMutationProbability = AddConn.Value / 100.0,
                DeleteConnectionMutationProbability = DeleteConn.Value / 100.0,
                ConnectionWeightMutationProbability = ConnWeight.Value / 100.0,
                AddNodeMutationProbability = NodeMutation.Value / 100.0,
                NodeAuxStateMutationProbability = GenomeParams.NodeAuxStateMutationProbability
            };
        }

        /// <summary>
        /// Connected to SetParamsButton (pressed)
        /// </summary>
        private void On_SetParamsButton_Pressed()
        {
            //if (NeatParams != null) ConfirmDialog.PopupCentered();
            On_ConfirmDialog_Confirmed();
        }

        /// <summary>
        /// Connected to ConfirmDialog (confirmed)
        /// </summary>
        private void On_ConfirmDialog_Confirmed()
        {
            stopConditionTreshold = (int)StopCondtionTreshold.Value;
            GetNeatParameters();
            GetGenomeParameters();
            ParametersSet?.Invoke(this, new EventArgs());
        }

        public void On_StopCondition_Changed(int id)
        {
            switch ((StopConditionOptions)id)
            {
                case StopConditionOptions.Generation:
                    CheckStopCondition = GenerationStopCondition;
                    break;
                case StopConditionOptions.EvaluationCount:
                    CheckStopCondition = EvaluationCountStopCondition;
                    break;
                case StopConditionOptions.Score:
                    CheckStopCondition = ScoreStopCondition;
                    break;
            }

        }

        private bool GenerationStopCondition(int gen, int eval, float maxscore)
        {
            return gen >= stopConditionTreshold;
        }

        private bool EvaluationCountStopCondition(int gen, int eval, float maxscore)
        {
            return eval >= stopConditionTreshold;
        }

        private bool ScoreStopCondition(int gen, int eval, float maxscore)
        {
            return maxscore >= stopConditionTreshold;
        }

        private enum StopConditionOptions
        {
            Generation = 0,
            EvaluationCount = 1,
            Score = 2
        }
    }
}