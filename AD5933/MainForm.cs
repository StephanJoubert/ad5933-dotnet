using AD5933_Lib;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace AD5933
{
    public partial class MainForm : Form
    {
        private AD5933_Eval eval = null;
     //   private static LineItem CurveReal;
     //   private static LineItem CurveImaginary;
        private static LineItem CurveMagnitude;
    //    private static LineItem CurvePhase;
        private static LineItem CurveCole;
        private static bool mustCalibrate = true;

        public MainForm()
        {
            InitializeComponent();
            initGraph();
            Application.Idle += Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            btnConnect.Enabled = (eval == null) && (cbBoardList.SelectedIndex > -1);
            btnDisconnect.Enabled = eval != null;
            btnGetTemperature.Enabled = eval != null;
            btnCalibrate.Enabled = eval != null;
            btnSweep.Enabled = (!mustCalibrate) && (eval != null);
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            cbBoardList.Items.Clear();
            cbBoardList.Items.AddRange((
                from b in AD5933_Eval.Boards
                select b.ToString()).ToArray());
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (cbBoardList.SelectedIndex > -1)
            {
                var item = cbBoardList.SelectedItem.ToString();

                byte part = Convert.ToByte(item);
                eval = new AD5933_Eval(part)
                {
                    StartFrequency = 100,
                    IncFrequency = 1000,
                    Steps = 100,
                    SettlingCycles = 15,
                    PGAControl = AD5933_Eval.PgaGain.x1,
                    CalibrationResistor = 10000.0,
                    ExcitationVoltage = AD5933_Eval.OutputRange.Range1,
                };

                propertyGrid1.SelectedObject = eval;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (eval != null)
            {
                propertyGrid1.SelectedObject = null;

                eval.Dispose();
                eval = null;

            }
        }

        private void btnGetTemperature_Click(object sender, EventArgs e)
        {

            var temp = eval.ReadTemperature();
            MessageBox.Show(String.Format("Temperature {0}", temp));

            
          
        }

        private void initGraph()
        {
            
            var pane = mainGraph.GraphPane;
            pane.Title.Text = "AD5933";

            pane.XAxis.Title.Text = "Frequency";
            pane.XAxis.Scale.MinAuto = true;
            pane.XAxis.Scale.MaxAuto = true;

            pane.YAxis.Title.Text = "Impedance";
            pane.YAxis.Scale.MinAuto = true;
            pane.YAxis.Scale.MaxAuto = true;

            pane.Y2Axis.Title.Text = "Phase";
            pane.Y2Axis.Scale.MinAuto = true;
            pane.Y2Axis.Scale.MaxAuto = true;
            pane.Y2Axis.IsVisible = true;

            mainGraph.AxisChange();

          //  CurveReal = pane.AddCurve("Real", new PointPairList(), Color.Blue, SymbolType.Diamond);
          //  CurveImaginary = pane.AddCurve("Imaginary", new PointPairList(), Color.Green, SymbolType.Diamond);

            CurveMagnitude = pane.AddCurve("Absolute Imp.", new PointPairList(), Color.Black, SymbolType.Circle);
         //   CurvePhase = pane.AddCurve("Phase", new PointPairList(), Color.Red, SymbolType.Circle);
         //   CurvePhase.IsY2Axis = true;


            var panec = colePlot.GraphPane;
            panec.XAxis.Title.Text = "Real";
            panec.XAxis.Scale.MinAuto = true;
            panec.XAxis.Scale.MaxAuto = true;

            panec.YAxis.Title.Text = "Imaginary";
            panec.YAxis.Scale.MinAuto = true;
            panec.YAxis.Scale.MaxAuto = true;

            CurveCole = panec.AddCurve("", new PointPairList(), Color.Black);
            colePlot.AxisChange();
        }

        private void btnSweep_Click(object sender, EventArgs e)
        {
            var save = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
              //  CurveReal.Clear();
              //  CurveImaginary.Clear();
              //  CurvePhase.Clear();
                CurveMagnitude.Clear();
                CurveCole.Clear();

                foreach (var snap in eval.SweepMeasure())
                {
                //    CurveReal.AddPoint(snap.Frequency, snap.RealPart);
                 //   CurveImaginary.AddPoint(snap.Frequency, snap.ImaginaryPart);
                 //   CurvePhase.AddPoint(snap.Frequency, snap.Phase);
                    CurveMagnitude.AddPoint(snap.Frequency, snap.Impedance);

                    CurveCole.AddPoint(snap.RealPart, snap.ImaginaryPart);



                    mainGraph.AxisChange();
                    mainGraph.Invalidate();

                }
                colePlot.AxisChange();
                colePlot.Invalidate();
            }
            finally
            {
                this.Cursor = save;
            }



/*            if (eval != null)
           {
                eval.DoSweep().Subscribe((snap) =>
                {
                    zedGraphControl1.
                });
            } */
        }

        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            eval.CalibrateMultipoint();
            propertyGrid1.Refresh();
            mustCalibrate = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

     
    }
}
