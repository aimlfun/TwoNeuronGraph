namespace GraphForm
{
    /// <summary>
    /// This was born out of a blog posting (to be published in June), discussing points around an article written by Stephen Wolfram (of Wolfram Alpha fame).
    /// In that article, he discusses the idea of a neural network with a single input, 2 hidden layers, and a single output, and his training suggesting
    /// more than 1 million epochs, and how you cannot train 2 hidden layers to do it. Or at least that is my take on what he wrote.
    /// 
    /// So, I built app to investigate this, and I found that you can train 2 hidden layers to do it, and it was possible and the epochs were low...
    /// 
    /// That got me curious. What if I could see the graph of the neural network, and see how the weights and biases affect the graph? So I built this app.
    /// 
    /// A small application that allows you to visually see the affects of weightings an biases on a 
    /// single-input, 2 hidden layer, single-output neural network. Activation function is Tanh.
    /// 
    /// You can edit the weightings and biases, and the graph will update!
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Used to draw the axis on the graph.
        /// </summary>
        static readonly Pen penForAxis = new(Color.FromArgb(10, 255, 255, 255));

        /// <summary>
        /// The pen used to draw the curve/lines on the graph.
        /// </summary>
        static readonly Pen penLineToPlotGraph = new(Color.FromArgb(200, 0, 255, 0));

        // the last values we plotted, used to detect changes.
        private float lastW3 = 0;
        private float lastW1 = 0;
        private float lastB1 = 0;
        private float lastW4 = 0;
        private float lastW2 = 0;
        private float lastB2 = 0;
        private float lastB3 = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            PlotIfWeightingsOrBiasedChanged();
            timerRedraw.Start();
        }

        /// <summary>
        /// Every 0.2 seconds, redraw the graph _if_ the weightings or bias changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerRedraw_Tick(object sender, EventArgs e)
        {
            PlotIfWeightingsOrBiasedChanged();
        }

        /// <summary>
        /// Plots the graph of a single-input, 2 hidden layer, single-output neural network.
        /// y = Tanh(w3*Tanh(w1*x+b1) + w4*Tanh(w2*x+b2)+b3)
        /// </summary>
        /// <param name="w3"></param>
        /// <param name="w1"></param>
        /// <param name="b1"></param>
        /// <param name="w4"></param>
        /// <param name="w2"></param>
        /// <param name="b2"></param>
        /// <param name="b3"></param>
        /// <returns></returns>
        private static Bitmap PlotGraph(float w3, float w1, float b1, float w4, float w2, float b2, float b3)
        {
            Point centre = ConvertTrainingCoordinatesToPoint(0, 0);

            Bitmap graphBitmap = new(400, 400);

            using Graphics graphicsForGraphBitmap = Graphics.FromImage(graphBitmap);

            // dark background
            graphicsForGraphBitmap.Clear(Color.FromArgb(200, 0, 0, 0));

            graphicsForGraphBitmap.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphicsForGraphBitmap.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphicsForGraphBitmap.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            DrawAxisThruCentreOfGraph(centre, graphicsForGraphBitmap);

            // points are computer based on the formula for a single-input, 2 hidden layer, single-output neural network.
            List<Point> pointsToPlotOnGraph = GetPointsBasedOnTheNetworkOutput(w3, w1, b1, w4, w2, b2, b3);

            // draw all the lines in green
            graphicsForGraphBitmap.DrawLines(penLineToPlotGraph, pointsToPlotOnGraph.ToArray());

            return graphBitmap;
        }

        /// <summary>
        /// For x in [-1...1], calculate the points on the graph based on the output of a single-input, 2 hidden layer, single-output neural network.
        /// y = Tanh(w3*Tanh(w1*x+b1) + w4*Tanh(w2*x+b2)+b3)
        /// </summary>
        /// <param name="w3"></param>
        /// <param name="w1"></param>
        /// <param name="b1"></param>
        /// <param name="w4"></param>
        /// <param name="w2"></param>
        /// <param name="b2"></param>
        /// <param name="b3"></param>
        /// <returns></returns>
        private static List<Point> GetPointsBasedOnTheNetworkOutput(float w3, float w1, float b1, float w4, float w2, float b2, float b3)
        {
            List<Point> points = new();

            for (float x = -1; x < 1; x += 0.01f)
            {
                // this is the formula for a single-input, 2 hidden layer, single-output neural network.
                float y = (float)Math.Tanh(w3 * (Math.Tanh(w1 * x + b1)) + w4 * (Math.Tanh(w2 * x + b2)) + b3);

                points.Add(ConvertTrainingCoordinatesToPoint(x, y));
            }

            return points;
        }

        /// <summary>
        /// Draw a + shape at the centre of the graph.
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="graphicsForGraphBitmap"></param>
        private static void DrawAxisThruCentreOfGraph(Point centre, Graphics graphicsForGraphBitmap)
        {
            Point topLeft = ConvertTrainingCoordinatesToPoint(-1, 1);
            Point bottomRight = ConvertTrainingCoordinatesToPoint(1, -1);

            // draw axis (center X vertical and horizontal)
            graphicsForGraphBitmap.DrawLine(penForAxis, topLeft.X, centre.Y, bottomRight.X, centre.Y);

            // why -10..10? and not -1..1? Because float rounding errors cause the axis to miss items at the top/right.
            for (int graduations = -10; graduations <= 10; graduations++)
            {
                Point pointOnAxis = ConvertTrainingCoordinatesToPoint((float)graduations / 10f, 0);

                graphicsForGraphBitmap.DrawLine(penForAxis, pointOnAxis.X, pointOnAxis.Y - 3, pointOnAxis.X, pointOnAxis.Y + 3);

                pointOnAxis = ConvertTrainingCoordinatesToPoint(0, (float)graduations / 10f);
                graphicsForGraphBitmap.DrawLine(penForAxis, pointOnAxis.X - 3, pointOnAxis.Y, pointOnAxis.X + 3, pointOnAxis.Y);
            }

            graphicsForGraphBitmap.DrawLine(penForAxis, centre.X, topLeft.Y, centre.X, bottomRight.Y);
        }

        /// <summary>
        /// Maps coordinate values (-1..1,-1..1) to the Bitmap's coordinate system, on a 200x200 grid.
        /// Because we're plotting cartesian coordinates, we need to flip the Y axis.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>X & Y mapped to Bitmap</returns>
        static Point ConvertTrainingCoordinatesToPoint(float x, float y)
        {
            return new Point((int)Math.Round(x * 199 + 199), (int)Math.Round(398 - (y * 199 + 199)));
        }

        /// <summary>
        /// Plots the graph based on the current weightings and bias _IF_ the values have changed.
        /// </summary>
        private void PlotIfWeightingsOrBiasedChanged()
        {
            float w3 = (float)numericUpDownA.Value;
            float w1 = (float)numericUpDownB.Value;
            float b1 = (float)numericUpDownC.Value;
            float w4 = (float)numericUpDownD.Value;
            float w2 = (float)numericUpDownE.Value;
            float b2 = (float)numericUpDownF.Value;
            float b3 = (float)numericUpDownG.Value;

            if (lastW3 == w3 && lastW1 == w1 && lastB1 == b1 && lastW4 == w4 && lastW2 == w2 && lastB2 == b2 && lastB3 == b3) return;

            pictureBox1.Image?.Dispose();
            pictureBox1.Image = PlotGraph(w3, w1, b1, w4, w2, b2, b3);

            string formula = $"y=tanh({w3}*(tanh({w1}x+{b1}))+{w4}*(tanh({w2}x+{b2}))+{b3})".Replace("+-", "-");
            labelFormula.Text = formula;

            // put formula into clipboard, you can paste it into Wolfram Alpha to see the graph too.
            Clipboard.SetText(formula);

            // used to detect changes
            lastB3 = b3;
            lastB2 = b2;
            lastW2 = w2;
            lastW4 = w4;
            lastB1 = b1;
            lastW1 = w1;
            lastW3 = w3;
        }
    }
}