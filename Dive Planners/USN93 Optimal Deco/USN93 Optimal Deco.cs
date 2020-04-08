using DCSFormBase;
using DCSModel;

namespace USN93_Optimal_Deco
{
    public partial class USN93Ascent : DCSBaseForm
    {

        public USN93Ascent ( )
        {

            InitializeComponent ( );
            base.button_Optimize.Enabled = false;
            base.button_Optimize.Visible = false;
            base.button_Shotgun.Enabled  = false;
            base.button_Shotgun.Visible  = false;
                        
        }

        private void SetModel ( )
        {

            ELModel.Model = DCSUtilities.MODEL.LE1;

        }

        protected override void Optimize ( )
        {
           
            // call base method to set up problem
            base.Optimize ( );

            // select the model variant
            SetModel ( );

            // perform optimization
            base.DoOptimization ( );

        }

        protected override void Evaluate ( )
        {

            base.Evaluate ( );

            // set USN93 parameter values
            d.N2TissueRate    = new double[] { 1.0 / 1.7727676636E+00, 1.0 / 6.0111598753E+01, 1.0 / 5.1128788835E+02 };
            d.O2TissueRate    = new double[] { 0.0, 0.0, 0.0 };
            d.HeTissueRate    = new double[] { 0.0, 0.0, 0.0 };
            d.N2Factor        = new double[] { 1.0, 1.0, 1.0 };
            d.O2Factor        = new double[] { 1.0, 1.0, 1.0 };
            d.HeFactor        = new double[] { 1.0, 1.0, 1.0 };
            d.ImmersionFactor = new double[] { 1.0, 1.0, 1.0 };

            // load model parameters
            m.Gain                   = new double[] { 1000.0 * 3.0918150923E-06, 1000.0 * 1.1503684782E-07, 1000.0 * 1.0805385353E-06 };
            m.LECrossoverPressure    = new double[] { 9.9999999999E+09, 2.9589519286E-02, 9.9999999999E+09 };
            m.Threshold              = new double[] { 0.0000000000E+00, 0.0000000000E+00, 6.7068236527E-02 };
            m.TrinomialScaleFactor   = 1.0;
            m.TetranomialScaleFactor = 1.0;

            // select the model variant
            SetModel ( );

            base.DoEvaluation ( );

            m.StoreNodeLevelDCSBinaryProfileProbability ( );
            d.GenerateNodeDump ( 0 );

            //ProfileView p = new ProfileView ( );
            //p.LoadData ( d );
            //p.ShowDialog ( );

        }

        protected override void Shotgun ( )
        {

            base.Shotgun ( );

            // select the model variant
            SetModel ( );

            base.DoShotgunning ( );

        }
        
    }

}
