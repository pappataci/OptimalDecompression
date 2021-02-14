using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DCSUtilities;
using System.IO;
using Extreme.Mathematics;
using Extreme.Mathematics.EquationSolvers;
using Extreme.Mathematics.Optimization;

namespace Continuation_Optimal_Deco
{
    public partial class ContOptimalDeco : Form
    {

        #region Fields
        private double maxDepth;
        private double descentRate;
        private double bottomTime;
        private double ascentRate;
        private double targetPDCS;
        private double actualPDCS;
        private double surfaceTime;
        private double fractionO2;
        private double exponent;
        private double breakFraction;
        private double clearTime;

        private USN93_EXP model = null;

        #endregion

        public ContOptimalDeco ( )
        {
            InitializeComponent ( );
            
        }

        #region Call-back functions

        private void buttonCalculate_Click ( object sender , EventArgs e )
        {
            
            this.ReadInformationFromGUI ( );

            model                            = new USN93_EXP ( fractionO2 );
            model.Pressure.MaximumAscentRate = ascentRate;

            // this.PowerLawAscentFromBreak ( );
            // this.DebuggingProfile120x30 ( );
            // this.OptimalProfile120x30 ( );
            // this.ParameterSweep ( );
            // this.RootFindSurfaceTime ( );
            // this.RootFindExponent ( );
            // this.RootFindBreakFraction ( );
            this.OptimizeSurfaceTime ( );
            // this.SurfaceTimeToSurface ( );
            // this.DebuggingProfile180x20 ( );

            // MessageBox.Show ( "Calculations complete" );

        }

        private void buttonEvaluate_Click(object sender, EventArgs e)
        {

            this.ReadInformationFromGUI();
            
            model = new USN93_EXP(fractionO2);

            model.Pressure.MaximumAscentRate = ascentRate;

            // Profile descent portion
            var time = ArrivalTime(maxDepth, descentRate);
            model.IntegrateToTime(time, 0.0, maxDepth);
            // Profile bottom portion
            time = bottomTime - time;
            model.IntegrateToTime(time, maxDepth, maxDepth);
            
            
            

            // Profile linear ascent portion
            time = ArrivalTime(breakFraction * maxDepth, ascentRate);
            model.IntegrateToTime(time, maxDepth, (1.0 - breakFraction) * maxDepth);
            model.SaveOutputToDisk("LarsResults");
            // Profile power law ascent portion
            time = surfaceTime - (bottomTime + time);
            model.IntegrateToTime(time, (1.0 - breakFraction) * maxDepth, 0.0, exponent);
            // integrate until clear
            model.IntegrateToClearTime ( );
            textBoxClearTime.Text  = model.FinalClearTime().ToString("F2");
            textBoxActualPDCS.Text = model.FinalPDCS().ToString("F4");

            var simData = new ProfileParameters()
            {
                MaxDepth      = maxDepth,
                DescentRate   = descentRate,
                BottomTime    = bottomTime,
                AscentRate    = ascentRate,
                TargetPDCS    = targetPDCS,
                ActualPDCS    = actualPDCS,
                SurfaceTime   = surfaceTime,
                FractionO2    = fractionO2,
                Exponent      = exponent,
                BreakFraction = breakFraction,
                ClearTime     = clearTime,
            };

            if (USN93_EXP.AccumulateData && this.checkBoxSaveData.Checked)
                 
                model.SaveAccumulatedData ( );

            this.WriteInformationToGUI ( );

        }

        private void checkBoxSaveData_CheckedChanged ( object sender , EventArgs e )
        {

            if ( checkBoxSaveData.Checked )
                USN93_EXP.AccumulateData = true;
            else
                USN93_EXP.AccumulateData = false;

        }

        #endregion

        #region Reporting and data handlers

        private void ReadInformationFromGUI ( )
        {

            maxDepth      = Double.Parse ( textBoxMaxDepth.Text );
            descentRate   = Double.Parse ( textBoxDescentRate.Text );
            bottomTime    = Double.Parse ( textBoxBottomTime.Text );
            ascentRate    = Double.Parse ( textBoxMaxAscentRate.Text );
            targetPDCS    = Double.Parse ( textBoxTargetPDCS.Text );
            surfaceTime   = Double.Parse ( textBoxSurfaceTime.Text );
            fractionO2    = Double.Parse ( textBoxFractionO2.Text );
            exponent      = Double.Parse ( textBoxExponent.Text );
            breakFraction = Double.Parse ( textBoxBreakFraction.Text );

        }

        private void WriteInformationToGUI ( )
        {

            textBoxMaxDepth.Text      = maxDepth.ToString ( );  
            textBoxDescentRate.Text   = descentRate.ToString ( );
            textBoxBottomTime.Text    = bottomTime.ToString ( );
            textBoxMaxAscentRate.Text = ascentRate.ToString ( );
            textBoxTargetPDCS.Text    = targetPDCS.ToString ( );
            textBoxSurfaceTime.Text   = surfaceTime.ToString ( );
            textBoxFractionO2.Text    = fractionO2.ToString ( );
            textBoxExponent.Text      = exponent.ToString ( );
            textBoxBreakFraction.Text = breakFraction.ToString ( );

            Application.DoEvents ( );

        }

        #endregion

        #region Time-related functions

        private double ArrivalTime ( double _depth, double _rate )
        {

            return _depth / _rate;

        }

        #endregion

        #region Profile integrators

        private void IntegrateToLeaveBottomTime ( )
        {

            // Profile descent portion
            var time = ArrivalTime ( maxDepth , descentRate );
            model.IntegrateToTime ( time , 0.0 , maxDepth );
            // Profile bottom portion
            time = bottomTime - time;
            model.IntegrateToTime ( time , maxDepth , maxDepth );       

        }

        #endregion
  
        #region Optimization methods

        private void RootFindSurfaceTime ( )
        {

            // Prepare for optimization. Store the leave-bottom information.
            model.ResetInformation ( );
            this.ReadInformationFromGUI ( );
            this.IntegrateToLeaveBottomTime ( );
            model.StoreLeaveBottomInformation ( );

            // find bounce time time to use as lower bound
            var bounceTime = bottomTime + maxDepth/ascentRate;
            // var bounceTime = 26.0;
            var latestTime = bounceTime + 2000;
            // var latestTime = 120;

            Func<double,double> f = new Func<double, double> ( ObjectiveFunctionSurfaceTime );

            var solver            = new BisectionSolver ( );
            solver.TargetFunction = f;
            solver.LowerBound     = bounceTime;
            solver.UpperBound     = latestTime;
            solver.AbsoluteTolerance = 1.0e-4;

            try
            {
                surfaceTime = solver.Solve ( );
            }
            catch ( Exception e)
            {
                var s = e.Message.ToString ( );
            }
            finally
            {

            }

            textBoxSurfaceTime.Text = surfaceTime.ToString ( "F9" );
            actualPDCS              = model.FinalPDCS ( );
            textBoxActualPDCS.Text  = actualPDCS.ToString ( "F9" );
            clearTime               = model.FinalClearTime ( );
            textBoxClearTime.Text   = clearTime.ToString ( "F9" );
            Application.DoEvents ( );      

        }
      
        private double ObjectiveFunctionSurfaceTime ( double _variable )
        {

            // Integrate from leavebottom time to clear time
            model.LoadLeaveBottomInformation ( );

            // Integrate to break fraction
            var time = ArrivalTime ( breakFraction * maxDepth , ascentRate );
            model.IntegrateToTime ( time , maxDepth , ( 1.0 - breakFraction ) * maxDepth );

            // Integrate the power law portion to surface time
            time = _variable - ( bottomTime + time );
            model.IntegrateToTime ( time , ( 1.0 - breakFraction ) * maxDepth , 0.0 , exponent );

            // integrate until clear
            model.IntegrateToClearTime ( );

            textBoxClearTime.Text = model.FinalClearTime ( ).ToString ( "F9" );
            textBoxActualPDCS.Text = model.FinalPDCS ( ).ToString ( "F9" );
            Application.DoEvents ( );
            
            return model.FinalPDCS ( ) - targetPDCS;
            
        }

        private void OptimizeSurfaceTime ()
        {

            model.ResetInformation ( );
            this.ReadInformationFromGUI ( );
            
            // Powell's method is a conjugate gradient method that
            // does not require the derivative of the objective function.
            // It is implemented by the PowellOptimizer class:
            PowellOptimizer pw = new PowellOptimizer();
            pw.ExtremumType    = ExtremumType.Minimum;
            pw.Dimensions      = 2;
            
            // Create the initial guess
            // The first element is the exponent and the second
            // element is the break fraction.  
            var initialGuess   = Vector.Create(0.0,0.0);
            initialGuess [ 0 ] = 10.0 *  exponent  ;
            initialGuess [ 1 ] = 10.0 * breakFraction;
            
            // Powell's method does not use derivatives:
            pw.InitialGuess = initialGuess;
            pw.ObjectiveFunction =  ObjectiveFunction;
            pw.FindExtremum ( );
            MessageBox.Show(pw.SolutionReport.ToString());
            MessageBox.Show("Optimization complete");

        }

        private PowellOptimizer OptimizeSurfaceTimeWithOutput()
        {

            model.ResetInformation();
            this.ReadInformationFromGUI();

            // Powell's method is a conjugate gradient method that
            // does not require the derivative of the objective function.
            // It is implemented by the PowellOptimizer class:
            PowellOptimizer pw = new PowellOptimizer();
            pw.ExtremumType = ExtremumType.Minimum;
            pw.Dimensions = 2;

            // Create the initial guess
            // The first element is the exponent and the second
            // element is the break fraction.  
            var initialGuess = Vector.Create(0.0, 0.0);
            initialGuess[0] = 10.0 * exponent;
            initialGuess[1] = 10.0 * breakFraction;

            // Powell's method does not use derivatives:
            pw.InitialGuess = initialGuess;
            pw.ObjectiveFunction = ObjectiveFunction;
            pw.MaxEvaluations = 50; 
            pw.FindExtremum();
            return pw;

        }

        private double ObjectiveFunction (Vector<double> x)
        {

            model.ResetInformation ( );
            
            // extract the search parameters
            exponent                  = x [ 0 ] / 10.0;
            exponent                  = Math.Min ( exponent , 10.0 );
            exponent                  = Math.Max ( exponent , 0.0 );
            breakFraction             = x [ 1 ] / 10.0;
            breakFraction             = Math.Min ( breakFraction , 1.0 );
            breakFraction             = Math.Max ( breakFraction , 0.0 );
            textBoxExponent.Text      = exponent.ToString ( "F9" );
            textBoxBreakFraction.Text = breakFraction.ToString ( "F9" );

            Application.DoEvents ( );

            this.RootFindSurfaceTime ( );

            return surfaceTime;

        }

        #endregion

        #region Luca's added code for stress test

        private void button1_Click(object sender, EventArgs e)
        {
            // for now parameters values are hard typed in this function
            // take advantage of extreme optimization package 

            var bottomTimes = Vector.Create<double>(10, i => 10.0 + i * 10.0);

            double[] surfaceValues = { 50.0, 75.0, 100.0, 125.0 , 150.0 };
            var surfaceTimes = Vector.Create(surfaceValues);

            var maxDepths = Vector.Create<double>(5, i => 60.0 + i * 30.0);

            var exponents = Vector.Create<double>(6, i =>  1 + i);

            var breakFractions = Vector.Create<double>(8, i =>  0.15  +  i * 0.1);


            //model = new USN93_EXP(fractionO2);
            //model.Pressure.MaximumAscentRate = ascentRate;
            var index = 1; 
            foreach (double bottomTime in bottomTimes)
            {
                foreach (double surfaceValue in surfaceValues)
                {
                    foreach (double maxDepth in maxDepths)
                    {
                        foreach (double exponent in exponents)
                        {
                            foreach (double breakFraction in breakFractions)
                            {
                                if (index == 1)
                                {
                                    SendActualInputsToTheGUI(bottomTime, surfaceValue, maxDepth, exponent, breakFraction);
                                    this.ReadInformationFromGUI();

                                    model = new USN93_EXP(fractionO2);
                                    model.Pressure.MaximumAscentRate = ascentRate;
                                    var optimizer = OptimizeSurfaceTimeWithOutput();
                                    MessageBox.Show(optimizer.Status.ToString());
                                    //MessageBox.Show(optimizer.Result.ToString() + " " + optimizer.Status.ToString());


                                }
                                index++;
                            }
                        }
                    }
                }
            }

        }

        private void SendActualInputsToTheGUI(double bottomTime, double surfaceValue, double maxDepth, double exponent, double breakFraction)
        {
            textBoxMaxDepth.Text = maxDepth.ToString("F9");
            textBoxBottomTime.Text = bottomTime.ToString("F9");
            textBoxSurfaceTime.Text = surfaceValue.ToString("F9");
            textBoxExponent.Text = exponent.ToString("F9");
            textBoxBreakFraction.Text = breakFraction.ToString("F9");
        }


        #endregion
    }




        public class USN93_EXP
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

        // Fixed fields
        public static bool AccumulateData   = true;
        private static int numberOfTissues  = 3;
        private double TimeStep             = 0.1;

        private double [ ] Gain      = new double [ ] { 3.0918150923E-03 , 1.1503684782E-04 , 1.0805385353E-03 };
        private double [ ] Rate      = new double [ ] { 1.0 / 1.7727676636E+00 , 1.0 / 6.0111598753E+01 , 1.0 / 5.1128788835E+02 };
        private double [ ] CrossOver = new double [ ] { 9.9999999999E+09 , 2.9589519286E-02 , 9.9999999999E+09 };
        private double [ ] Threshold = new double [ ] { 0.0000000000E+00 , 0.0000000000E+00 , 6.7068236527E-02 };
        
        // Variable fields
        private double [ ] TissueTension                = new double [ numberOfTissues ];
        private double [ ] RiskInstantaneous            = new double [ numberOfTissues ];
        private double [ ] RiskIntegrated               = new double [ numberOfTissues ];
        private double [ ] Probability                  = new double [ numberOfTissues ];
        private double [ ] ClearTime                    = new double [ numberOfTissues ];
        private double [ ] LeaveBottomTissueTension     = new double [ numberOfTissues ];
        private double [ ] LeaveBottomRiskInstantaneous = new double [ numberOfTissues ];
        private double [ ] LeaveBottomRiskIntegrated    = new double [ numberOfTissues ];

        private List < double [ ] > ListTissueTension     = new List < double [ ] > ( );
        private List < double [ ] > ListRiskInstantaneous = new List < double [ ] > ( );
        private List < double [ ] > ListRiskIntegrated    = new List < double [ ] > ( );
        private List < double [ ] > ListProbability       = new List < double [ ] > ( );
        private List < double [ ] > ListClearTime         = new List < double [ ] > ( );
        private List < double > ListPressure              = new List < double > ( );
        private List < double > ListTime                  = new List < double > ( );

        // Luca added
        public List < double > ListN2Pressure            = new List < double > ( ); 
        
        public PressureFunction Pressure = null;
        public double FinalProbability;
        private double ProfileTime;
        private double LeaveBottomTime;
        private double fO2;
        
        public USN93_EXP ( double _fO2 )
        {

            fO2 = _fO2;
            this.ResetInformation ( );
            Pressure = new PressureFunction ( fO2 );
            
        }

        public double FinalClearTime ( )
        {

            return ClearTime.Max ( );

        }

        public double FinalPDCS ( )
        {

            return FinalProbability;

        }

        public void ResetInformation ( )
        {

            // Initialize calculated quantities
            //GAS.ThalmannError = false;
            var n2Pressure               = GAS.N2PressureAir ( 1.0 );
            GAS.ThalmannError = true;
            TissueTension                = new double [ ] { n2Pressure , n2Pressure , n2Pressure };
            RiskInstantaneous            = new double [ ] { 0.0 , 0.0 , 0.0 };
            RiskIntegrated               = new double [ ] { 0.0 , 0.0 , 0.0 };
            LeaveBottomTissueTension     = new double [ ] { n2Pressure , n2Pressure , n2Pressure };
            LeaveBottomRiskInstantaneous = new double [ ] { 0.0 , 0.0 , 0.0 };
            LeaveBottomRiskIntegrated    = new double [ ] { 0.0 , 0.0 , 0.0 };
            Probability                  = new double [ ] { 0.0 , 0.0 , 0.0 };
            ClearTime                    = new double [ ] { 0.0 , 0.0 , 0.0 };
            FinalProbability             = 0.0;
            ProfileTime                  = 0.0;
            LeaveBottomTime              = 0.0;
            ListTissueTension.Clear ( );
            ListRiskInstantaneous.Clear ( );
            ListRiskIntegrated.Clear ( );
            ListProbability.Clear ( );
            ListClearTime.Clear ( );
            ListPressure.Clear ( );
            ListTime.Clear ( );
            

        }

        public void SaveOutputToDisk ( string aFileName )
        {
            //FolderBrowserDialog fbd = new FolderBrowserDialog();
            //if (fbd.ShowDialog() != DialogResult.OK)
            //    return;
            string dir = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent" ; 
            var fileName = dir + "\\" + aFileName + ".csv";  

            FileStream fs = new FileStream(fileName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            for (int i = 0; i < ListN2Pressure.Count; i++)
            {

                string s = ListPressure[i].ToString("F6") + ", " + ListN2Pressure[i].ToString("F6") + ", "
                     + VectorToString(ListTissueTension[i]) + ", "
                     + VectorToString(ListRiskIntegrated[i]) + ", " + ambientPressure2Depth(ListPressure[i]).ToString("F6")
                     + ", " + ListTime[i].ToString("F6"); 
                sw.WriteLine(s);
            }
            sw.Close();
            fs.Close();
        }

        public static double ambientPressure2Depth(double ambientPressure)
        {
            var dDepthOverrelativePress = 33.066;
            return dDepthOverrelativePress * (ambientPressure - 1.0);
        }

        public static string VectorToString(double[] tissueTension)
        {
            var returnString = "";

            for (int item = 0; item < tissueTension.Length - 1 ; item++)
                returnString += tissueTension[item].ToString("F6") + ", ";

            returnString += tissueTension[tissueTension.Length - 1].ToString("F6");
            
            return returnString;
        }

        public void SaveAccumulatedData ( )
        {

            if ( !AccumulateData )
                return;

            // Open a directory for output
            FolderBrowserDialog fbd = new FolderBrowserDialog ( );
            if ( fbd.ShowDialog ( ) != DialogResult.OK )
                return;

            string dir = fbd.SelectedPath;


            // Tissue tension
            string fileName = dir + "\\Tissue_tension.csv";
            FileStream fs   = new FileStream ( fileName, FileMode.Create );
            StreamWriter sw = new StreamWriter ( fs );
            for ( int i = 0 ; i < ListTissueTension.Count ; i++ )
            {
                var vec = ListTissueTension [ i ];
                string s = ListTime [ i ].ToString( "F6" ) + "," + vec [ 0 ].ToString ( "F6" ) + "," + vec [ 1 ].ToString ( "F6" ) + "," + vec[ 2 ].ToString ( "F6" );
                sw.WriteLine ( s );
            }
            sw.Close ( );
            fs.Close ( );

            // Instantaneous risk
            fileName = dir + "\\Risk_instanteneous.csv";
            fs   = new FileStream ( fileName, FileMode.Create );
            sw = new StreamWriter ( fs );
            for ( int i = 0 ; i < ListRiskInstantaneous.Count ; i++ )
            {
                var vec = ListRiskInstantaneous [ i ]; 
                string s = ListTime [ i ].ToString( "F6" ) + "," + vec [ 0 ].ToString ( "F6" ) + "," + vec [ 1 ].ToString ( "F6" ) + "," + vec[ 2 ].ToString ( "F6" );
                sw.WriteLine ( s );
            }
            sw.Close ( );
            fs.Close ( );

            // integrated risk
            fileName = dir + "\\Risk_integrated.csv";
            fs = new FileStream ( fileName , FileMode.Create );
            sw = new StreamWriter ( fs );
            for ( int i = 0 ; i < ListRiskIntegrated.Count ; i++ )
            {
                var vec = ListRiskIntegrated [ i ];
                string s = ListTime [ i ].ToString( "F6" ) + "," + vec [ 0 ].ToString ( "F6" ) + "," + vec [ 1 ].ToString ( "F6" ) + "," + vec[ 2 ].ToString ( "F6" );
                sw.WriteLine ( s );
            }
            sw.Close ( );
            fs.Close ( );

            // Compartmental probabilities
            fileName = dir + "\\Tissue_probabilities.csv";
            fs = new FileStream ( fileName , FileMode.Create );
            sw = new StreamWriter ( fs );
            for ( int i = 0 ; i < ListProbability.Count ; i++ )
            {
                var vec = ListProbability [ i ];
                string s = ListTime[i].ToString( "F6" ) + "," + vec [ 0 ].ToString ( "F6" ) + "," + vec [ 1 ].ToString ( "F6" ) + "," + vec[ 2 ].ToString ( "F6" );
                sw.WriteLine ( s );
            }
            sw.Close ( );
            fs.Close ( );

            // Clear time
            fileName = dir + "\\Clear_time.csv";
            fs = new FileStream ( fileName , FileMode.Create );
            sw = new StreamWriter ( fs );
            for ( int i = 0 ; i < ListClearTime.Count ; i++ )
            {
                var vec = ListClearTime [ i ];
                string s = ListTime[i].ToString( "F6" ) + "," + vec [ 0 ].ToString ( "F6" ) + "," + vec [ 1 ].ToString ( "F6" ) + "," + vec[ 2 ].ToString ( "F6" );
                sw.WriteLine ( s );
            }
            sw.Close ( );
            fs.Close ( );

            // Ambient pressure
            fileName = dir + "\\Ambient_pressure.csv";
            fs = new FileStream ( fileName , FileMode.Create );
            sw = new StreamWriter ( fs );
            for ( int i = 0 ; i < ListPressure.Count ; i++ )
            {

                string s = ListTime [ i ].ToString( "F6" ) + "," + ListPressure [ i ].ToString ( "F6" );
                sw.WriteLine ( s );
            }


            sw.Close ( );
            fs.Close ( );
            
        }

        public void StoreLeaveBottomInformation ( )
        {

            Array.Copy ( TissueTension , LeaveBottomTissueTension, TissueTension.Length );
            Array.Copy ( RiskInstantaneous , LeaveBottomRiskInstantaneous , RiskInstantaneous.Length );
            Array.Copy ( RiskIntegrated , LeaveBottomRiskIntegrated , RiskIntegrated.Length );
            LeaveBottomTime  = ProfileTime;
            FinalProbability = 0.0;
            ClearTime        = new double [ ] { 0.0 , 0.0 , 0.0 };

        }

        public void LoadLeaveBottomInformation ( )
        {

            Array.Copy ( LeaveBottomTissueTension , TissueTension , LeaveBottomTissueTension.Length );
            Array.Copy ( LeaveBottomRiskInstantaneous , RiskInstantaneous , LeaveBottomRiskInstantaneous.Length );
            Array.Copy ( LeaveBottomRiskIntegrated , RiskIntegrated , LeaveBottomRiskIntegrated.Length );
            ProfileTime      = LeaveBottomTime;
            FinalProbability = 0.0;
            ClearTime        = new double [ ] { 0.0 , 0.0 , 0.0 };

        }

        public void IntegrateToTime ( double _time, double _depth1, double _depth2, double _exponent = 1.0 )
        {

            var timeSpan                        = _time;
            var numberOfSteps                   = ( int ) Math.Max ( 1.0, Math.Abs ( timeSpan / TimeStep ) );
            var deltaTime                       = timeSpan / ( double ) numberOfSteps;
            var localTime                       = 0.0;
            Pressure.SetParameters ( 0.0 , _time , _depth1 , _depth2 , _exponent );
            
            for (int step = 1 ; step <= numberOfSteps ; step ++ )
            {

                localTime = step * deltaTime;

                IntegrationStep ( deltaTime , localTime );
                
            }
            
        }

        public void IntegrateToClearTime ( )
        {

            var deltaTime                       = TimeStep;
            var localTime                       = 0.0;
            Pressure.SetParameters ( 0.0 , 10000.0 , 0.0 , 0.0 , 1.0 );

            var step = 1;
            do
            {

                localTime = step * deltaTime;

                IntegrationStep ( deltaTime , localTime );

            } while ( RiskInstantaneous [ 0 ] > 0.0 || RiskInstantaneous [ 1 ] > 0.0 || RiskInstantaneous [ 2 ] > 0.0 );
            
        }

        public void IntegrationStep ( double _deltaT, double _localTime )
        {

            // Calculate the profile time
            ProfileTime         += _deltaT;
            var pressureAmbient  = this.Pressure.GetAmbientPressure ( _localTime );
            var pressureNitrogen = this.Pressure.GetNitrogenPressure( pressureAmbient );

            for ( var tissue = 0 ; tissue < numberOfTissues ; tissue++ )
            {

                // Step the tissue tension
                if ( TissueTension [ tissue ] > pressureAmbient + CrossOver [ tissue ] - GAS.dPFVG )
                    // linear kinitics
                    TissueTension [ tissue ] = TissueTension [ tissue ] + _deltaT * Rate [ tissue ] * ( pressureNitrogen - pressureAmbient - CrossOver [ tissue ] + GAS.dPFVG );
                else
                    // exponential kinitics
                    TissueTension [ tissue ] = ( TissueTension [ tissue ] + _deltaT * Rate [ tissue ] * pressureNitrogen ) / ( 1 + _deltaT * Rate [ tissue ] );
                
                // Set the new instantaneous risk
                RiskInstantaneous [ tissue ] = Gain [ tissue ] * ( TissueTension [ tissue ] - pressureAmbient - ( Threshold [ tissue ] - GAS.dPFVG ) ) / pressureAmbient;

                // Calculate the integrated risk
                RiskIntegrated [ tissue ] += _deltaT * Math.Max ( RiskInstantaneous [ tissue ] , 0.0);

                // Calculate the tissue probabilities
                Probability [ tissue ] = 1.0 - Math.Exp ( -RiskIntegrated [ tissue ] );

                // Store the clear time
                if ( RiskInstantaneous [ tissue ] > 0.0 )
                    ClearTime [ tissue ] = ProfileTime;  // something is FUBAR with this

            }
            
            // Calculate the total probability
            FinalProbability = 1.0 - Math.Exp ( - RiskIntegrated [ 0 ] - RiskIntegrated [ 1 ] - RiskIntegrated [ 2 ] );

            if (AccumulateData)
            {

                var tension = new double [ numberOfTissues ];
                TissueTension.CopyTo ( tension , 0 );
                ListTissueTension.Add ( tension );

                var riskinst = new double [ numberOfTissues ];
                RiskInstantaneous.CopyTo ( riskinst , 0 );
                ListRiskInstantaneous.Add ( riskinst );

                var riskint = new double [ numberOfTissues ];
                RiskIntegrated.CopyTo ( riskint , 0 );
                ListRiskIntegrated.Add ( riskint );

                var probab = new double [ numberOfTissues ];
                Probability.CopyTo ( probab , 0 );
                ListProbability.Add ( probab );

                var clear = new double [ numberOfTissues ];
                ClearTime.CopyTo ( clear , 0 );
                ListClearTime.Add ( clear );

                ListPressure.Add ( pressureAmbient );

                // Luca added
                ListN2Pressure.Add(pressureNitrogen );

                ListTime.Add ( ProfileTime );
                
            }
        
        }
                        
    }

    public class PressureFunction
    {
        #region Fields Pressure Function
        private double time1;
        private double time2;
        private double depth1;
        private double depth2;
        private double exponent;
        private double fO2;
        private double maxAscentRate;
        private double breakTime;
        #endregion

        public PressureFunction ( double _fo2 )
        {

            fO2           = _fo2;
            exponent      = 1.0;    // load default exponent value
            maxAscentRate = 30.0;   // load maximum ascent rate (ft/min)

        }

        public double MaximumAscentRate { set { maxAscentRate = value; } get { return maxAscentRate; } }

        public void SetParameters ( double _time1, double _time2, double _depth1, double _depth2, double _exponent = 1.0 )
        {

            time1    = _time1;
            time2    = _time2;
            depth1   = _depth1;
            depth2   = _depth2;
            exponent = _exponent;   
        }


        public double GetAmbientPressure (double _time)
        {

            var pressure1 = GAS.Pressure(depth1);
            var pressure2 = GAS.Pressure(depth2);

            // use this method if descending or at constant depth
            if ( depth2 >= depth1 )
                return pressure1 + ( pressure2 - pressure1 ) * Math.Pow ( ( _time - time1 ) / ( time2 - time1 ) , exponent );
            
            // linear ascent at maximum rate
            var linear = pressure1 - _time*maxAscentRate/33.066;

            // power law ascent
            var power = pressure1 + ( pressure2 - pressure1 ) * Math.Pow ( ( _time - time1 ) / ( time2 - time1 ) , exponent );

            // store the crossover time
            if ( linear >= power )
                breakTime = _time;

            // return the greater of the two pressures (slower of the two ascents).
            return Math.Max ( linear , power );

        }

        public double GetNitrogenPressure (double _pressure)
        {
            return GAS.N2PressureFO2 ( _pressure , fO2 );
        }

    } 

    public struct ProfileParameters
    {
        public double MaxDepth { set; get; }
        public double DescentRate { set; get; }
        public double BottomTime { set; get; }
        public double AscentRate { set; get; }
        public double TargetPDCS { set; get; }
        public double ActualPDCS { set; get; }
        public double SurfaceTime { set; get; }
        public double FractionO2 { set; get; }
        public double Exponent { set; get; }
        public double BreakFraction { set; get; }
        public double ClearTime { set; get; }

    }

}