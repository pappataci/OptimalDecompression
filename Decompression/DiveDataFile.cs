using System.Collections;
using System.IO;
using System.Windows.Forms;
using DCSUtilities;

namespace Decompression
{
    /// <summary>
    /// DiveDataFile class
    /// </summary>
    /// <typeparam name="D">dive data type with base DiveData</typeparam>
    /// <typeparam name="P">profile type with base Profile</typeparam>
    /// <typeparam name="N">node type with base Node</typeparam>
    public class DiveDataFile<D, P, N>
        where D : DiveData<P, N>
        where P : Profile<N>
        where N : Node
    {
        /// <summary>
        /// current file open/closed status
        /// </summary>
        protected bool bFileOpen = new bool ( );

        /// <summary>
        /// file names for all dive data files
        /// </summary>
        protected ArrayList alFileNames = new ArrayList ( );

        /// <summary>
        /// FileStream for reading dive data
        /// </summary>
        protected FileStream fs;

        /// <summary>
        /// StreamReader for reading dive data
        /// </summary>
        protected StreamReader sr;

        /// <summary>
        /// the dive data file constructor
        /// </summary>
        public DiveDataFile ( )
        {
            bFileOpen = false;
            alFileNames.Clear ( );
        }

        /// <summary>
        /// open a dialog window to select and open a dive data file
        /// </summary>
        private bool Open ( )
        {
            if ( bFileOpen )
                Close ( );

            // create an open file dialog
            using ( OpenFileDialog cFile = new OpenFileDialog ( ) )
            {
                cFile.Title            = @"Open Dive Profile Files";
                cFile.Multiselect      = true;
                cFile.Filter           = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                cFile.FilterIndex      = 2;
                cFile.RestoreDirectory = true;

                if ( cFile.ShowDialog ( ) == DialogResult.OK )
                {
                    foreach ( string s in cFile.FileNames )
                    {
                        alFileNames.Add ( s );
                    }
                }
                else
                {
                    this.Close ( );
                    return false;
                }
            } // using

            return true;
        }

        /// <summary>
        /// read an individual data line from the dive data file
        /// </summary>
        /// <returns>string from a dive data file</returns>
        private string ReadLine ( )
        {
            if ( bFileOpen )
                return sr.ReadLine ( );

            return string.Empty;
        }

        /// <summary>
        /// read the contents of a dive data file.
        /// </summary>
        /// <param name="d">an instance of a DiveData class</param>
        /// <returns>string containing the file name</returns>
        public bool ReadDiveDataFile ( DiveData<P, N> d )
        {
            if ( !bFileOpen )
            {
                this.Open ( );
            }

            // read each data file
            foreach ( string sFileName in alFileNames )
            {
                // open the file
                try
                {
                    fs = new FileStream ( sFileName, FileMode.Open, FileAccess.Read, FileShare.Read );
                    sr = new StreamReader ( fs );
                    bFileOpen = true;
                }
                catch ( System.Exception e )
                {
                    string err = "Error in DiveDataFile.Open: ";
                    err += e.ToString ( );
                    MessageBox.Show ( err, "Error" );
                    bFileOpen = false;
                    return bFileOpen;
                }

                // store the name
                d.FileName = sFileName;

                // zero the profile counter
                Profile<N>.ZeroProfileCounter ( );

                // read the file
                string s = new string ( string.Empty.ToCharArray ( ) );
                bool topOfFile = true; //Detect white space at top of file.
                do	// TODO: read the entire file
                {
                    s = this.ReadLine ( );
                    if (topOfFile && s == "")
                        continue;

                    topOfFile = false;

                    if ( s != null )
                        if( s == "" || s.Length != 1 && !System.Char.IsSymbol(s.ToCharArray(0,1)[0]) )
                            d.Add ( s );
                } while ( s != null );

                // close the streamreader and filestream
                sr.Close ( );
                fs.Close ( );
                bFileOpen = false;

                this.Close ( );
            }

            return true;
        }

        /// <summary>
        /// Convert file to runtime
        /// </summary>
        /// <param name="d"></param>
        public void ConvertDataFileToRunTime ( DiveData<P, N> d )
        {
            // get the input file name
            OpenFileDialog inFile = new OpenFileDialog ( );
            inFile.Title = @"Open Dive Profile Input File";
            inFile.Multiselect = false;
            inFile.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            inFile.FilterIndex = 2;
            inFile.RestoreDirectory = true;
            if ( inFile.ShowDialog ( ) != DialogResult.OK )
                return;

            // open the input file
            FileStream infs = new FileStream ( inFile.FileName, FileMode.Open, FileAccess.Read, FileShare.None );
            StreamReader insr = new StreamReader ( infs );

            // read the contents
            ArrayList alDive = new ArrayList ( );
            string s = string.Empty;
            do
            {
                s = insr.ReadLine ( );
                if ( s != null )
                    alDive.Add ( s );
            } while ( s != null );

            // close the input file
            insr.Close ( );
            infs.Close ( );

            // get the output file name
            SaveFileDialog outFile = new SaveFileDialog ( );
            outFile.Title = @"Open Dive Profile Output File";
            outFile.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            outFile.FilterIndex = 2;
            outFile.RestoreDirectory = true;
            outFile.OverwritePrompt = true;
            if ( outFile.ShowDialog ( ) != DialogResult.OK )
                return;

            // open the output file
            FileStream outfs = new FileStream ( outFile.FileName, FileMode.CreateNew, FileAccess.Write, FileShare.None );
            StreamWriter outsw = new StreamWriter ( outfs );

            // write the contents
            double dLastDepth = 0.0;
            foreach ( string st in alDive )
            {
                DCS dcs = d.Type ( st );

                switch ( dcs )
                {
                    case DCS.HEADER1:
                        outsw.WriteLine ( st );
                        break;

                    case DCS.HEADER2:
                        dLastDepth = 0.0;
                        outsw.WriteLine ( st );
                        break;

                    case DCS.NODE:
                        string sDelim = ",";
                        char [ ] cDelim = sDelim.ToCharArray ( );
                        string [ ] sList = st.Split ( cDelim );
                        double dThisDepth = double.Parse ( sList [ 0 ] );
                        dLastDepth = dThisDepth += dLastDepth;
                        string sNewNode = dThisDepth.ToString ( "F2" );
                        sNewNode = sNewNode.PadLeft ( 10 );
                        for ( int i = 1; i < sList.Length; i++ )
                            if ( sList [ i ].Length != 0 )
                                sNewNode += ',' + sList [ i ];
                        outsw.WriteLine ( sNewNode );
                        break;

                    case DCS.END:
                        outsw.WriteLine ( st );
                        break;

                    default:
                        MessageBox.Show ( "Bad message type in DiveData.Add", "Error" );
                        break;
                }
            }

            // close the output file
            outsw.Close ( );
            outfs.Close ( );
        }

        /// <summary>
        /// Closes a previously opened dive data file.
        /// </summary>
        private void Close ( )
        {
            if ( bFileOpen )
            {
                fs.Close ( );
                sr.Close ( );
                bFileOpen = false;
                alFileNames.Clear ( );
            }
        }
    }
}