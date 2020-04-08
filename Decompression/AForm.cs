using System;
using System.Windows.Forms;

namespace Decompression
{
    /// <summary>
    /// Test entry form class
    /// </summary>
    public partial class AForm : Form
    {
        /// <summary>
        /// Simple testing form
        /// </summary>
        public AForm ( )
        {
            InitializeComponent ( );
        }

        private void button1_Click ( object sender, EventArgs e )
        {
#if false
            DiveDataFile<DiveData<Profile<Node>, Node>, Profile<Node>, Node> f
                = new DiveDataFile<DiveData<Profile<Node>, Node>, Profile<Node>, Node> ();
            DiveData<Profile<Node>, Node> d = new DiveData<Profile<Node>, Node> ();
#endif
#if false
            DiveDataFile<DiveDataTissue<ProfileTissue<NodeTissue>, NodeTissue>, ProfileTissue<NodeTissue>, NodeTissue> f
                = new DiveDataFile<DiveDataTissue<ProfileTissue<NodeTissue>, NodeTissue>, ProfileTissue<NodeTissue>, NodeTissue> ();
            DiveDataTissue<ProfileTissue<NodeTissue>, NodeTissue> d = new DiveDataTissue<ProfileTissue<NodeTissue>, NodeTissue> ();
#endif
#if true
            DiveDataFile<DiveDataCondition<ProfileCondition<NodeCondition>, NodeCondition>, ProfileCondition<NodeCondition>, NodeCondition> f
                = new DiveDataFile<DiveDataCondition<ProfileCondition<NodeCondition>, NodeCondition>, ProfileCondition<NodeCondition>, NodeCondition> ( );
            DiveDataCondition<ProfileCondition<NodeCondition>, NodeCondition> d
                = new DiveDataCondition<ProfileCondition<NodeCondition>, NodeCondition> ( );
#endif

            f.ReadDiveDataFile ( d );

            d [ 0 ].SaveProfile ( );

#if false

            #region tissue tension test section

            for (int i = 0; i < d[0].Nodes; i++)
            {
                NodeTissue n = d[0].Node(i);
                double N2press = n.N2Pressure;
                double O2press = n.O2Pressure;
                double Hepress = n.HePressure;
                double[] N2tens = new double[NodeTissue.NumberOfTissues];
                double[] O2tens = new double[NodeTissue.NumberOfTissues];
                double[] Hetens = new double[NodeTissue.NumberOfTissues];

                for (int j = 0; j < NodeTissue.NumberOfTissues; j++)
                {
                    N2tens[j] = N2press;
                    O2tens[j] = O2press;
                    Hetens[j] = Hepress;
                }
                n.N2Tension = N2tens;
                n.O2Tension = O2tens;
                n.HeTension = Hetens;
            }

            #endregion tissue tension test section

            #region tissue rate test section

            double[] dN2 = new double[NodeTissue.NumberOfTissues];
            double[] dO2 = new double[NodeTissue.NumberOfTissues];
            double[] dHe = new double[NodeTissue.NumberOfTissues];
            for (int i = 0; i < NodeTissue.NumberOfTissues; i++)
            {
                dN2[i] = 1.0;
                dO2[i] = 2.0;
                dHe[i] = 3.0;
            }
            d.N2Rate = dN2;
            d.O2Rate = dO2;
            d.HeRate = dHe;

            #endregion tissue rate test section

            // d[0].SaveProfile();
#endif
        }

        private void button_ProfileViewer_Click ( object sender, EventArgs e )
        {
            // ConditionsViewer v = new ConditionsViewer();
            // v.ShowDialog();
        }
    }
}