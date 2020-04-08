using System.IO;
using System.Windows.Forms;
using DCSFormBase;
using DCSModel;
using Decompression;

namespace DCSModelFormUSN93
{

    public partial class DCSModelFormUSN93 : DCSBaseForm
    {

        public DCSModelFormUSN93 ( )
        {

            InitializeComponent ( );
            button_Optimize.Enabled = false;
            button_Shotgun.Enabled  = false;

        }

        private void SetModel ( )
        {

            ELModel.Model = DCSUtilities.MODEL.LE1;
            base.SetModel ( );

        }

        protected override void Optimize ( )
        {

            return;

        }

        protected override void Evaluate ( )
        {

            // NBN1x3g.out parameters for USN93

            // 10  0  3.0918150923E-06    Gain / 10 * *3 [/ min ], tis 1
            // 11  0  1.1503684782E-07    Gain / 10 * *3 [/ min ], tis 2
            // 12  0  1.0805385353E-06    Gain / 10 * *3 [/ min ], tis 3
            // 70  0  1.7727676636E+00    TC ( t ); gas exchange time constant [min], tis 1
            // 71  0  6.0111598753E+01    TC ( t ); gas exchange time constant [min], tis 2
            // 72  0  5.1128788835E+02    TC ( t ); gas exchange time constant [min], tis 3
            // 80  1  9.9999999999E+09    PXO ( t ); E->L kinetic threshold (atm), tissue 1
            // 81  0  2.9589519286E-02    PXO(t); E->L kinetic threshold (atm), tissue 2
            // 82  1  9.9999999999E+09    PXO(t); E->L kinetic threshold (atm), tissue 3
            // 90  1  0.0000000000E+00    THR(t); risk threshold [atm], in tissue 1
            // 91  1  0.0000000000E+00    THR ( t ); risk threshold [atm], in tissue 2
            // 92  0  6.7068236527E-02    THR ( t ); risk threshold [atm], in tissue 3

            base.Evaluate ( );

            // set parameter values
            m.Gain                = new double [ ] { 3.0918150923E-03 , 1.1503684782E-04 , 1.0805385353E-03 };
            d.N2TissueRate        = new double [ ] { 1.0 / 1.7727676636E+00 , 1.0 / 6.0111598753E+01 , 1.0 / 5.1128788835E+02 };
            m.LECrossoverPressure = new double [ ] { 9.9999999999E+09 , 2.9589519286E-02 , 9.9999999999E+09 };
            m.Threshold           = new double [ ] { 0.0000000000E+00 , 0.0000000000E+00 , 6.7068236527E-02 };

            // select the model variant
            SetModel ( );

            base.DoEvaluation ( );

            m.StoreNodeLevelDCSBinaryProfileProbability ( );

        }

        protected override void Shotgun ( )
        {

            return;

        }

    }

}
