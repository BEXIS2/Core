using System.Collections.Generic;

namespace IDIV.Modules.Mmm.UI.Models
{
    public class Point
    {
        public long Id;
        public double x;
        public double y;

        public Point()
        {
            this.Id = 0;
            this.x = 0.0;
            this.y = 0.0;
        }
    }

    public class Shape
    {
        public long Id;
        public long objRef;
        public double x;
        public double y;
        public List<Point> Points;

        public Shape()
        {
            this.Id = 0;
            this.objRef = 0;
            this.x = 0.0;
            this.y = 0.0;
            this.Points = new List<Point>();
        }
    }

    public class Measurement
    {
        public long Id;
        public string Type;
        public double Length;
        public double Width;
        public double Area;
        public double Perimeter;
        public double Circularity;
        public List<Measurement> Children { get; set; }

        public Measurement()
        {
            this.Id = 0;
            this.Type = "";
            this.Length = -1.0;
            this.Width = -1.0;
            this.Area = -1.0;
            this.Perimeter = -1.0;
            this.Circularity = -1.0;
            this.Children = new List<Measurement>();
        }
    }

    public class BundleInformation
    {
        public string BundlePath { get; set; }
        public List<ImageInformation> Images { get; set; }

        public BundleInformation()
        {
            this.BundlePath = null;
            this.Images = new List<ImageInformation>();
        }
    }

    public class ImageInformation
    {
        public string BundlePath { get; set; }
        public string Name { get; set; }
        public string Original { get; set; }
        public string Thumbnail { get; set; }
        public List<Measurement> Measurements { get; set; }

        public ImageInformation()
        {
            this.BundlePath = null;
            this.Name = null;
            this.Original = null;
            this.Thumbnail = null;
            this.Measurements = new List<Measurement>();
        }
    }
}