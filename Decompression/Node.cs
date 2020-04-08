using System;

namespace Decompression
{
    /// <summary>
    /// Node class
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Most recently breathed gas.
        /// </summary>
        public static double dLastGas = 1.0;

        private double dTime                      = new double ( );
        private double dDepth                     = new double ( );
        private double dPressure                  = new double ( );
        private double dHePressure                = new double ( );
        private double dN2Pressure                = new double ( );
        private double dO2Pressure                = new double ( );
        private double dPressureRate              = new double ( );
        private double dHePressureRate            = new double ( );
        private double dN2PressureRate            = new double ( );
        private double dO2PressureRate            = new double ( );
        private double dPressureRateExponential   = new double ( );
        private double dHePressureRateExponential = new double ( );
        private double dN2PressureRateExponential = new double ( );
        private double dO2PressureRateExponential = new double ( );
        private double dGas                       = new double ( );
        private double dSwitchTime                = new double ( );
        private double dPDCS                      = new double ( );
        private double dExercise                  = new double ( );
        private bool bIsGasSwitchNode             = new bool ( );
        private bool bIsSwitchEndNode             = new bool ( );
        private bool bIsInsertedNode              = new bool ( );
        private bool bIsStraddledNode             = new bool ( );
        private bool bIsT1Node                    = new bool ( );
        private bool bIsT2Node                    = new bool ( );

        /// <summary>
        /// blank constructor
        /// </summary>
        public Node ( )
        {
            dTime = 0.0;
            dDepth = 0.0;
            dGas = dLastGas;
            dSwitchTime = -1.0;
            dPressure = 0.0;
            dHePressure = 0.0;
            dN2Pressure = 0.0;
            dO2Pressure = 0.0;
            dPressureRate = 0.0;
            dHePressureRate = 0.0;
            dN2PressureRate = 0.0;
            dO2PressureRate = 0.0;
            dExercise = 0.0;
            bIsGasSwitchNode = false;
            bIsSwitchEndNode = false;
            bIsInsertedNode = false;
            bIsStraddledNode = false;
            bIsT1Node = false;
            bIsT2Node = false;
        }

        /// <summary>
        /// two argument assignment constructor
        /// </summary>
        /// <param name="time">node time (min)</param>
        /// <param name="depth">node depth (fsw)</param>
        public Node ( double time, double depth )
        {
            dTime                       = time;
            dDepth                      = depth;
            dGas                        = dLastGas;
            dSwitchTime                 = -1.0;
            dPressure                   = 0.0;
            dHePressure                 = 0.0;
            dN2Pressure                 = 0.0;
            dO2Pressure                 = 0.0;
            dPressureRate               = 0.0;
            dHePressureRate             = 0.0;
            dN2PressureRate             = 0.0;
            dO2PressureRate             = 0.0;
            dPressureRateExponential    = 0.0;
            dHePressureRateExponential  = 0.0;
            dN2PressureRateExponential  = 0.0;
            dO2PressureRateExponential  = 0.0;
            dExercise                   = 0.0;
            bIsGasSwitchNode            = false;
            bIsSwitchEndNode            = false;
            bIsInsertedNode             = false;
            bIsStraddledNode            = false;
            bIsT1Node                   = false;
            bIsT2Node                   = false;
        }

        /// <summary>
        /// four argument constructor
        /// </summary>
        /// <param name="time">node time (min)</param>
        /// <param name="depth">node depth (fsw)</param>
        /// <param name="gas">new gas</param>
        /// <param name="swtime">gas switching time (min)</param>
        public Node ( double time, double depth, double gas, double swtime )
        {
            dTime                      = time;
            dDepth                     = depth;
            dGas                       = gas;
            dLastGas                   = gas;
            dSwitchTime                = swtime;
            dPressure                  = 0.0;
            dHePressure                = 0.0;
            dN2Pressure                = 0.0;
            dO2Pressure                = 0.0;
            dPressureRate              = 0.0;
            dHePressureRate            = 0.0;
            dN2PressureRate            = 0.0;
            dO2PressureRate            = 0.0;
            dPressureRateExponential   = 0.0;
            dHePressureRateExponential = 0.0;
            dN2PressureRateExponential = 0.0;
            dO2PressureRateExponential = 0.0;
            dExercise                  = 0.0;
            bIsGasSwitchNode           = true;
            bIsSwitchEndNode           = false;
            bIsInsertedNode            = false;
            bIsStraddledNode           = false;
            bIsT1Node                  = false;
            bIsT2Node                  = false;
        }

        /// <summary>
        /// five argument constructor
        /// </summary>
        /// <param name="time">node time (min)</param>
        /// <param name="depth">node depth (fsw)</param>
        /// <param name="gas">new gas</param>
        /// <param name="swtime">gas switching time (min)</param>
        /// <param name="exercise">exercise</param>
        public Node ( double time, double depth, double gas, double swtime, double exercise )
        {
            dTime                      = time;
            dDepth                     = depth;
            dGas                       = gas;
            dLastGas                   = gas;
            dSwitchTime                = swtime;
            dPressure                  = 0.0;
            dHePressure                = 0.0;
            dN2Pressure                = 0.0;
            dO2Pressure                = 0.0;
            dPressureRate              = 0.0;
            dHePressureRate            = 0.0;
            dN2PressureRate            = 0.0;
            dO2PressureRate            = 0.0;
            dPressureRateExponential   = 0.0;
            dHePressureRateExponential = 0.0;
            dN2PressureRateExponential = 0.0;
            dO2PressureRateExponential = 0.0;
            dExercise                  = exercise;
            bIsGasSwitchNode           = true;
            bIsSwitchEndNode           = false;
            bIsInsertedNode            = false;
            bIsStraddledNode           = false;
            bIsT1Node                  = false;
            bIsT2Node                  = false;
        }

        /// <summary>
        /// assignment constructor
        /// </summary>
        /// <param name="n">Node n</param>
        public Node ( Node n )
        {
            dTime                      = n.Time;
            dDepth                     = n.Depth;
            dGas                       = n.Gas;
            dSwitchTime                = n.SwitchTime;
            dPressure                  = n.Pressure;
            dHePressure                = n.HePressure;
            dN2Pressure                = n.N2Pressure;
            dO2Pressure                = n.O2Pressure;
            dPressureRate              = n.PressureRate;
            dHePressureRate            = n.dHePressureRate;
            dN2PressureRate            = n.N2PressureRate;
            dO2PressureRate            = n.O2PressureRate;
            dPressureRateExponential   = n.PressureRateExponential;
            dHePressureRateExponential = n.HePressureRateExponential;
            dN2PressureRateExponential = n.N2PressureRateExponential;
            dO2PressureRateExponential = n.O2PressureRateExponential;
            dExercise                  = n.Exercise;
            bIsGasSwitchNode           = n.IsGasSwitchNode;
            bIsInsertedNode            = n.IsInsertedNode;
            bIsT1Node                  = n.IsT1Node;
            bIsT2Node                  = n.bIsT2Node;
        }

        /// <summary>
        /// time property - get/set the node time
        /// </summary>
        public double Time { set { dTime = value; } get { return dTime; } }

        /// <summary>
        /// depth property - get/set the node depth
        /// </summary>
        public double Depth { set { dDepth = value; } get { return dDepth; } }

        /// <summary>
        /// new gas property - get/set the new gas
        /// </summary>
        public double Gas { get { return dGas; } set { dGas = value; } }

        /// <summary>
        /// get/set the probability of DCS at the node
        /// </summary>
        public double DCSProbability { get { return dPDCS; } set { dPDCS = value; } }

        /// <summary>
        /// get/set the switch time
        /// </summary>
        public double SwitchTime { get { return dSwitchTime; } set { dSwitchTime = value; } }

        /// <summary>
        /// get/set the exercise level
        /// </summary>
        public double Exercise { get { return dExercise; } set { dSwitchTime = value; } }

        /// <summary>
        /// returns true if gas switch node, false otherwise
        /// </summary>
        public bool IsGasSwitchNode { get { return bIsGasSwitchNode; } set { bIsGasSwitchNode = value; } }

        /// <summary>
        /// true if node at the end of gas switch
        /// </summary>
        public bool IsSwitchEndNode { get { return bIsSwitchEndNode; } set { bIsSwitchEndNode = value; } }

        /// <summary>
        /// true if inserted node, false otherwise
        /// </summary>
        public bool IsInsertedNode { get { return bIsInsertedNode; } set { bIsInsertedNode = value; } }

        /// <summary>
        /// true if node is straddled by a gas switch
        /// </summary>
        public bool IsStraddledNode { get { return bIsStraddledNode; } set { bIsStraddledNode = value; } }

        /// <summary>
        /// returns true if T1 node, false otherwise
        /// </summary>
        public bool IsT1Node { get { return bIsT1Node; } set { bIsT1Node = value; } }

        /// <summary>
        /// returns true if T2 node, false otherwise
        /// </summary>
        public bool IsT2Node { get { return bIsT2Node; } set { bIsT2Node = value; } }

        /// <summary>
        /// Get or set the node pressure (ata)
        /// </summary>
        public double Pressure { get { return dPressure; } set { dPressure = value; } }

        /// <summary>
        /// Get or set the helium partial pressure (ata)
        /// </summary>
        public double HePressure { get { return dHePressure; } set { dHePressure = value; } }

        /// <summary>
        /// Get or set the nitrogen partial pressure (ata)
        /// </summary>
        public double N2Pressure { get { return dN2Pressure; } set { dN2Pressure = value; } }

        /// <summary>
        /// Get or set the oxygen partial pressure (ata)
        /// </summary>
        public double O2Pressure { get { return dO2Pressure; } set { dO2Pressure = value; } }

        /// <summary>
        /// Get or set the pressure rate (ata/min)
        /// </summary>
        public double PressureRate { get { return dPressureRate; } set { dPressureRate = value; } }

        /// <summary>
        /// Get or set the helium partial pressure rate (ata/min)
        /// </summary>
        public double HePressureRate { get { return dHePressureRate; } set { dHePressureRate = value; } }

        /// <summary>
        /// Get or set the nitrogen partial pressure rate (ata/min)
        /// </summary>
        public double N2PressureRate { get { return dN2PressureRate; } set { dN2PressureRate = value; } }

        /// <summary>
        /// Get or set the oxygen partial pressure rate (ata/min)
        /// </summary>
        public double O2PressureRate { get { return dO2PressureRate; } set { dO2PressureRate = value; } }
        
        /// <summary>
        /// Get or set the exponential pressure rate (ata/min)
        /// </summary>
        public double PressureRateExponential { get { return dPressureRateExponential; } set { dPressureRateExponential = value; } }

        /// <summary>
        /// Get or set the exponential helium partial pressure rate (ata/min)
        /// </summary>
        public double HePressureRateExponential { get { return dHePressureRateExponential; } set { dHePressureRateExponential = value; } }

        /// <summary>
        /// Get or set the exponential nitrogen partial pressure rate (ata/min)
        /// </summary>
        public double N2PressureRateExponential { get { return dN2PressureRateExponential; } set { dN2PressureRateExponential = value; } }

        /// <summary>
        /// Get or set the exponential oxygen partial pressure rate (ata/min)
        /// </summary>
        public double O2PressureRateExponential { get { return dO2PressureRateExponential; } set { dO2PressureRateExponential = value; } }
        
        /// <summary>
        /// Get a string containing Node information
        /// </summary>
        /// <returns>string</returns>
        public override string ToString ( )
        {

            String s = dTime.ToString ( "F3" )
                + ","
                + dDepth.ToString ( "F3" )
                + ","
                + dPressure.ToString ( "F6" )
                + ","
                + dPressureRate.ToString ( "F6" )
                + ","
                + dPressureRateExponential.ToString ( "F6" )
                + ","
                + dN2Pressure.ToString ( "F6" )
                + ","
                + dN2PressureRate.ToString ( "F6" )
                + ","
                + dN2PressureRateExponential.ToString ( "F6" )
                + ","
                + dO2Pressure.ToString ( "F6" )
                + ","
                + dO2PressureRate.ToString ( "F6" )
                + ","
                + dO2PressureRateExponential.ToString ( "F6" )
                + ","
                + dHePressure.ToString ( "F6" )
                + ","
                + dHePressureRate.ToString ( "F6" )
                + ","
                + dHePressureRateExponential.ToString ( "F6" )
                + ","
                + dGas.ToString ( )
                + ","
                + dSwitchTime.ToString ( "F3" )
                + ","
                + bIsGasSwitchNode.ToString ( )
                + ","
                + bIsInsertedNode.ToString ( )
                + ","
                + bIsStraddledNode.ToString ( )
                + ","
                + bIsSwitchEndNode.ToString ( )
                + ","
                + bIsT1Node.ToString ( )
                + ","
                + bIsT2Node.ToString ( )
                + ","
                + dPDCS.ToString ( "F6" );

            return s;
        }

        /// <summary>
        /// Get a header string for node listing
        /// </summary>
        /// <returns>string</returns>
        public static string HeaderString ( )
        {

            String s = "Time,Depth,Pressure,Pressure Rate,Exponential Pressure Rate,N2 Pressure,N2 Rate,Exponential N2 Rate,O2 Pressure,O2 Rate,Exponential O2 Rate,He Pressure,He Rate,Exponential He Rate,Gas,Switch time,IsSwitchNode,IsInsertedNode,IsStraddledNode,IsSwitchEndNode,IsT1Node,IsT2Node,PDCS";
            return s;

        }

    }

}