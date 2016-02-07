using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using Microsoft.Kinect.Face;

namespace FaceTrack
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // Provides a Kinect sensor reference.
        private KinectSensor _sensor = null;


        ColorFrameReader _colorReader = null;

        // Acquires body frame data.
        private BodyFrameSource _bodySource = null;

        // Reads body frame data.
        private BodyFrameReader _bodyReader = null;

        // Acquires HD face data.
        private HighDefinitionFaceFrameSource _faceSource = null;
        private HighDefinitionFaceFrameSource _faceSourceSub = null;
        // Reads HD face data.
        private HighDefinitionFaceFrameReader _faceReader = null;
        private HighDefinitionFaceFrameReader _faceReaderSub = null;

        // Required to access the face vertices.
        private FaceAlignment _faceAlignment = null;
        private FaceAlignment _faceAlignmentSub = null;

        // Required to access the face model points.
        private FaceModel _faceModel = null;

        // Used to display 1,000 points on screen.
        private List<Ellipse> _points = new List<Ellipse>();


        Image img = new Image();

        public MainWindow()
        {
            InitializeComponent();
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                // Listen for body data.
                _bodySource = _sensor.BodyFrameSource;
                _bodyReader = _bodySource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;

                _colorReader = _sensor.ColorFrameSource.OpenReader();
                _colorReader.FrameArrived += ColorReader_FrameArrived;

                // Listen for HD face data.
                _faceSource = new HighDefinitionFaceFrameSource(_sensor);
                _faceSourceSub = new HighDefinitionFaceFrameSource(_sensor);
               // _faceSource.TrackingIdLost += OnTrackingIdLost;
                _faceReader = _faceSource.OpenReader();
                _faceReaderSub = _faceSourceSub.OpenReader();

                _faceReader.FrameArrived += FaceReader_FrameArrived;
                _faceReaderSub.FrameArrived += FaceReaderSub_FrameArrived;

                _faceModel = new FaceModel();
                _faceAlignment = new FaceAlignment();
                _faceAlignmentSub = new FaceAlignment();
                // Start tracking!        
                _sensor.Open();
            }
        }

        void ColorReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();
                }
            }
        }
        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    Body[] bodies = new Body[frame.BodyCount];
                    frame.GetAndRefreshBodyData(bodies);

                    Body body1 = bodies.Where(b => b.IsTracked).FirstOrDefault();
                    Body body2 = bodies.Where(b => b.IsTracked).LastOrDefault();

                    if (!_faceSource.IsTrackingIdValid)
                    {
                        if (body1 != null)
                        {
                            _faceSource.TrackingId = body1.TrackingId;
                        }
                    }
                    if (!_faceSourceSub.IsTrackingIdValid)
                    {
                        if (body2 != null)
                        {
                            if(_faceSource.TrackingId!=body2.TrackingId) _faceSourceSub.TrackingId = body2.TrackingId;
                            if (_faceSource.TrackingId != body1.TrackingId) _faceSourceSub.TrackingId = body1.TrackingId;
                        }
                    }
                    //MessageBox.Show(_faceSource.TrackingId.ToString() +"##"+ _faceSourceSub.TrackingId.ToString());
                }
            }
        }
        private void FaceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null && frame.IsFaceTracked)
                {
                    frame.GetAndRefreshFaceAlignmentResult(_faceAlignment);
                    UpdateFacePoints(1);
                }
            }
        }

        private void FaceReaderSub_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null && frame.IsFaceTracked)
                {
                    frame.GetAndRefreshFaceAlignmentResult(_faceAlignmentSub);
                    UpdateFacePoints(2);
                }
            }
        }
        bool P1 = false;
        bool P2 = false;
        private void UpdateFacePoints(int NoOfPeople)
        {
            //MessageBox.Show(NoOfPeople.ToString());
            if (_faceModel == null) return;

            IReadOnlyList<Microsoft.Kinect.CameraSpacePoint> vertices1;
            IReadOnlyList<Microsoft.Kinect.CameraSpacePoint> vertices2;
            vertices1 = _faceModel.CalculateVerticesForAlignment(_faceAlignment);
            vertices2 = _faceModel.CalculateVerticesForAlignment(_faceAlignmentSub);
            if (vertices1.Count > 0)
            {
                if (!P1 && _points.Count == 0)
                {
                    for (int index = 0; index < vertices1.Count; index++)
                    {
                        Ellipse ellipse = new Ellipse
                        {
                            Width = 4.0,
                            Height = 4.0,
                            Fill = new SolidColorBrush(Colors.Red)
                        };
                        _points.Add(ellipse);
                        canvas.Children.Add(ellipse);
                    }
                    P1 = true;
                }
                if (!P2 && _points.Count == vertices1.Count )
                {
                    for (int index = 0; index < vertices2.Count; index++)
                    {
                        Ellipse ellipse = new Ellipse
                        {
                            Width = 4.0,
                            Height = 4.0,
                            Fill = new SolidColorBrush(Colors.Red)
                        };
                        _points.Add(ellipse);
                        canvas.Children.Add(ellipse);
                    }
                    P2 = true;
                }
               // MessageBox.Show(vertices.Count.ToString());
                if(P1)
                {
                    for (int index = 0; index < vertices1.Count; index++)
                    {
                        CameraSpacePoint vertice = vertices1[index];
                        ColorSpacePoint point = _sensor.CoordinateMapper.MapCameraPointToColorSpace(vertice);
                        if (float.IsInfinity(point.X) || float.IsInfinity(point.Y)) return;

                        Ellipse ellipse = _points[index];
                        Canvas.SetLeft(ellipse, point.X);
                        Canvas.SetTop(ellipse, point.Y);
                    }
                }
                if (P2)
                {
                    for (int index = 0; index < vertices2.Count; index++)
                    {

                        CameraSpacePoint vertice = vertices2[index];
                        ColorSpacePoint point = _sensor.CoordinateMapper.MapCameraPointToColorSpace(vertice);
                        if (float.IsInfinity(point.X) || float.IsInfinity(point.Y)) return;

                        Ellipse ellipse = _points[vertices1.Count+index];
                        Canvas.SetLeft(ellipse, point.X);
                        Canvas.SetTop(ellipse, point.Y);
                    }
                }
                
            }
        }
    }
}
