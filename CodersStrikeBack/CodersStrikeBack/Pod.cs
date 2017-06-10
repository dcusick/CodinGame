using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Pod : Unit
{
    public double Angle { get; set; }
    public int NextCheckPointId { get; set; }
    public int Checked { get; set; }
    public int Timeout { get; set; }
    public Pod Partner { get; set; }
    public bool IsThrustUsed { get; set; }
    public double AngleToCheckPoint { get; set; }    

    public double TurningRadius { get { return 2500; } }
    
    public bool IsMine { get { return Id == 0 || Id == 1; } }
    
    public double GetAngle(Point p) {
        var d = this.Distance(p);
        var dx = (p.X - this.X) / d;
        var dy = (p.Y - this.Y) / d;
    
        // Simple trigonometry. We multiply by 180.0 / PI to convert radiants to degrees.
        var a = (Math.Acos(dx) * 180.0 / Math.PI);
    
        // If the point I want is below me, I have to shift the angle for it to be correct
        if (dy < 0) {
            a = 360.0 - a;
        }
    
        return a;
    }
    
    public double GetAngleToPoint(Point p, Pod myLastPod)
    {
        //var lineOfSightUnitVector = new Unit() { X = Math.Sin(Angle), Y = Math.Cos(Angle) };
        //var originVector = new Unit() { X = p.X - this.X, Y = p.Y - this.Y };
        //var originVector = this.Normalize(p);
        //var normalVector = this.Normalize(myLastPod);
        var angleToCheckPoint = this.GetAngle(p);
        var relativeAngle = myLastPod.GetAngle(p);
        //var relativeAngle = this.GetAngle(myLastPod);

        Console.Error.WriteLine(string.Format("HP: angleToCheckPoint: {0}, lastAngleToCheckPoint: {1}.", angleToCheckPoint, relativeAngle));

        //return (Math.Atan2(normalVector.Y, normalVector.X) - Math.Atan2(originVector.Y, originVector.X));
        return Math.Floor(angleToCheckPoint - relativeAngle);
        //return this.GetVectorAngle(originVector);
    }

    public double GetAngleFromPoints(Point p2, Point p3)
    {
        var podP2 = new Point() { X = this.X - p2.X, Y = this.Y - p2.Y };
        var podP3 = new Point() { X = this.X - p3.X, Y = this.Y - p3.Y };

        var dotProduct = podP2.DotProduct(podP3);

        var podP2Magnitude = podP2.Magnitude();
        var podP3Magnitude = podP3.Magnitude();
        
        var angle = (Math.Acos(dotProduct / (podP2Magnitude * podP3Magnitude)));
        
        var result = Math.Atan2(p3.Y - this.Y, p3.X - this.X) -
                Math.Atan2(p2.Y - this.Y, p2.X - this.X);

        Console.Error.WriteLine(string.Format("angleFromPoints: {0}; atanAngle: {1};", angle, result));

        return result;
    }

    public double DiffAngle(Point p) {
        var a = this.GetAngle(p);
    
        // To know whether we should turn clockwise or not we look at the two ways and keep the smallest
        // The ternary operators replace the use of a modulo operator which would be slower
        var right = this.Angle <= a ? a - this.Angle : 360.0 - this.Angle + a;
        var left = this.Angle >= a ? this.Angle - a : 360.0 + this.Angle - a;
    
        if (right < left) {
            return right;
        } else {
            // We return a negative angle if we must rotate to left
            return -left;
        }
    }
    
    void Rotate(Point p) {
        var a = this.DiffAngle(p);
    
        // Can't turn by more than 18° in one turn
        if (a > 18.0) {
            a = 18.0;
        } else if (a < -18.0) {
            a = -18.0;
        }
    
        this.Angle += a;
    
        // The % operator is slow. If we can avoid it, it's better.
        if (this.Angle >= 360.0) {
            this.Angle = this.Angle - 360.0;
        } else if (this.Angle < 0.0) {
            this.Angle += 360.0;
        }
    }
}