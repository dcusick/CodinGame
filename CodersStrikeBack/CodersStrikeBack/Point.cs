using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
//using System.Windows;
using System.Numerics;

class Point
{
    public double X { get; set; }
    public double Y { get; set; }
    //public Vector Vector { get; set; }    
    
    public double Distance2(Point p) {
        return (this.X - p.X)*(this.X - p.X) + (this.Y - p.Y)*(this.Y - p.Y);
        //return Vector.LengthSquared;
    }

    public double Distance(Point p) {
        return Math.Sqrt(this.Distance2(p));
        //return Vector.Length;
    }
    
    public double DotProduct(Point p)
    {
        return (this.X * p.X) + (this.Y * p.Y);
    }
    
    public double Magnitude()
    {
        return Math.Sqrt((this.X*this.X) + (this.Y*this.Y));
    }
    
    public double GetVectorAngle(Point p)
    {
        return (Math.Acos(this.DotProduct(p) / (this.Magnitude() * p.Magnitude())) * (180 / Math.PI));
    }

    public Point Normalize(Point p)
    {
        var normalP = new Point();
        var distance = this.Distance(p);
        normalP.X = this.X / distance;
        normalP.Y = this.Y / distance;

        return normalP;
    }

    public Point GetMidpoint(Point p)
    {
        var midPoint = new Point();
        midPoint.X = Math.Floor((this.X + p.X) / 2);
        midPoint.Y = Math.Floor((this.Y + p.Y) / 2);
        return midPoint;
    }

    public double CalculateAngleFromPoints(Point P2, Point P3, bool allowNegatives = true) 
    {
        double numerator = P2.Y*(this.X-P3.X) + this.Y*(P3.X-P2.X) + P3.Y*(P2.X-this.X);
        double denominator = (P2.X-this.X)*(this.X-P3.X) + (P2.Y-this.Y)*(this.Y-P3.Y);
        double ratio = numerator/denominator;

        double angleRad = Math.Atan(ratio);
        double angleDeg = (angleRad*180)/Math.PI;


        if( !allowNegatives && angleDeg < 0 ){
            angleDeg = 180+angleDeg;
        }

        return angleDeg;
    }
}