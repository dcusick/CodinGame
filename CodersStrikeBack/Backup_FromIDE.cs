using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        bool thrustUsed = false;
        // game loop
        var checkPoints = new List<CheckPoint>();
        var allCheckPointsFound = false;
        var currentId = 0;
        var numberOfCheckPoints = 0;
        CheckPoint nextCheckPoint = null;
        CheckPoint nextNextCheckPoint = null;
        
        float xToGo = 0;
        float yToGo = 0;
        
        var checkPointRadius = 600;
        var MaxTimeout = 100;
        var MaxThrust = 100;
        
        var timeout = MaxTimeout;
        
        Pod myLastPod = null;
        
        var isDecelerate = false;
        
        while (true)
        {
            var pods = new List<Pod>();
        
            inputs = Console.ReadLine().Split(' ');
            float x = float.Parse(inputs[0]);
            float y = float.Parse(inputs[1]);
            float nextCheckpointX = float.Parse(inputs[2]); // x position of the next check point
            float nextCheckpointY = float.Parse(inputs[3]); // y position of the next check point
            int nextCheckpointDist = int.Parse(inputs[4]); // distance to the next checkpoint
            int nextCheckpointAngle = int.Parse(inputs[5]); // angle between your pod orientation and the direction of the next checkpoint
            inputs = Console.ReadLine().Split(' ');
            int opponentX = int.Parse(inputs[0]);
            int opponentY = int.Parse(inputs[1]);
        
            var point = new Point() {X = xToGo, Y = yToGo };
            var myPod = new Pod() { Id = 0, X = x, Y = y };
            var otherPod = new Pod() { Id = 1, X = opponentX, Y = opponentY };
            
            myPod.Angle = myPod.GetAngle(point);
            
            pods.Add(myPod);
            pods.Add(otherPod);
        
            var checkPoint = new CheckPoint() { X = nextCheckpointX, Y = nextCheckpointY };
            var isSameCheckPoint = checkPoint.IsEqual(nextCheckPoint);
            
            timeout--;
            
            if (!isSameCheckPoint)
            {
                timeout = MaxTimeout;
                
                nextCheckPoint = checkPoints.FirstOrDefault(cp => cp.X == nextCheckpointX && cp.Y == nextCheckpointY);
                
                nextNextCheckPoint = null;
            
                if (nextCheckPoint != null)
                {
                    allCheckPointsFound = true;
                    nextCheckPoint.TimesVisited++;
                    currentId = nextCheckPoint.Id;
                    nextNextCheckPoint = checkPoints.FirstOrDefault(cp => cp.Id == (currentId == numberOfCheckPoints - 1 ? 0 : currentId + 1));
                    
                    Console.Error.WriteLine(nextCheckPoint.ToString());
                    Console.Error.WriteLine(nextNextCheckPoint == null ? "No Next Next Checkpoint Yet" : nextNextCheckPoint.ToString());
                }
                else
                {
                    nextCheckPoint = new CheckPoint() { Id = currentId++, X = nextCheckpointX, Y = nextCheckpointY, TimesVisited = 1 };
                    checkPoints.Add(nextCheckPoint);
                    numberOfCheckPoints++;
                }
            }
            
            if (nextNextCheckPoint != null)
            {
                Console.Error.WriteLine(string.Format("{0} degrees (or {1}) and {2} units from current Checkpoint.", myPod.GetAngle(nextCheckPoint), myPod.GetVectorAngle(nextCheckPoint), myPod.Distance(nextCheckPoint)));
                Console.Error.WriteLine(string.Format("{0} degrees and {1} units from next Checkpoint.", myPod.GetAngle(nextNextCheckPoint), myPod.Distance(nextNextCheckPoint)));
            }
            else
                Console.Error.WriteLine(string.Format("{0} degrees from current Checkpoint, and Next CheckPoint Unknown.", myPod.Angle));
            
            string thrust;
            xToGo = nextCheckpointX;
            yToGo = nextCheckpointY;
            var checkPointsVisitedThreeTimes = checkPoints.Count(cp => cp.TimesVisited == 3);
            var checkPointsCount = checkPoints.Count();
            var isFinalCheckPoint = checkPointsVisitedThreeTimes == checkPointsCount;
            
            Console.Error.WriteLine(string.Format("{0} out of {1} CheckPoints have been Visited 3 Times.", checkPointsVisitedThreeTimes, checkPointsCount));
            
            if (isFinalCheckPoint)
                Console.Error.WriteLine(string.Format("Last Checkpoint.  Balls to the Wall!!!!!"));
                
            if (nextCheckpointAngle > 60 || nextCheckpointAngle < -60)
                thrust = (MaxThrust/5).ToString();
            else if ( isFinalCheckPoint || nextCheckpointDist >= myPod.TurningRadius)
                thrust = MaxThrust.ToString();
            else if ( nextCheckpointDist < myPod.TurningRadius && nextCheckpointAngle == 0)
            {
                thrust = MaxThrust.ToString();
            } else if ( nextCheckpointDist < myPod.TurningRadius && nextCheckpointAngle < 5)
            {
                thrust = (MaxThrust / 2).ToString();
            } else {
                var proportionalThrust = (nextCheckpointDist / myPod.TurningRadius) * 100.0 * .2;
                var roundedThrust = Math.Round(proportionalThrust, 0);
                thrust = Math.Max(roundedThrust, 20).ToString();
                
                Console.Error.WriteLine(string.Format("Proportional Thust: {0}, Rounded Thrust: {1}, Final Thrust: {2}.", proportionalThrust, roundedThrust, thrust));
            }

            float velocity = 0;
            
            if (myLastPod != null)
            {
                velocity = myPod.Distance(myLastPod);
                myPod.SetVector(myLastPod);
                Console.Error.WriteLine((myPod as Unit).ToString());
            }
                
            if (velocity > 500)
                isDecelerate = true;
                
            //if (isDecelerate)
            //    thrust = "0";
                
            Console.Error.WriteLine(string.Format("NextCheckpointDist: {0}, NextCheckpointAngle: {1}, Velocity: {2}, Timeout: {3}.", nextCheckpointDist, nextCheckpointAngle, velocity, timeout));
            
            //if (Math.Abs(opponentX - x) < 5 && Math.Abs(opponentY - y) < 5 && !thrustUsed) {
            if (!thrustUsed && ((nextCheckpointAngle == 0 && nextCheckpointDist >= 5000) || isFinalCheckPoint)) {
                //thrust = "BOOST";
                thrustUsed = true;
            }
            // You have to output the target position
            // followed by the power (0 <= thrust <= 100)
            // i.e.: "x y thrust"
/*            
            if (nextCheckpointAngle > 30 || nextCheckpointAngle < -30)
            {
                xToGo = xToGo * -3;
                yToGo = yToGo * -3;
                thrust = "100";
            }
  */          
            Console.WriteLine(xToGo + " " + yToGo + " " + thrust + " " + thrust + " " + nextCheckpointAngle);
            
            myLastPod = myPod;
        }
    }
}

class Point
{
    public float X { get; set; }
    public float Y { get; set; }
    
    public float Distance2(Point p) {
        return (this.X - p.X)*(this.X - p.X) + (this.Y - p.Y)*(this.Y - p.Y);
    }

    public float Distance(Point p) {
        return (float) Math.Sqrt(this.Distance2(p));
    }
    
    public float DotProduct(Point p)
    {
        return (this.X * p.X) + (this.Y * p.Y);
    }
    
    public float Magnitude()
    {
        return (float) Math.Sqrt((this.X*this.X) + (this.Y*this.Y));
    }
    
    public float GetVectorAngle(Point p)
    {
        return (float) Math.Acos(this.DotProduct(p) / (this.Magnitude() * p.Magnitude()));
    }
}

class Unit : Point
{
    public int Id { get; set; }
    public float R { get; set; }
    public float Vx { get; set; }
    public float Vy { get; set; }

    public void SetVector(Unit oldPosition)
    {
        Vx = this.X - oldPosition.X;
        Vy = this.Y - oldPosition.Y;
        R = this.Distance(oldPosition);
    }
    
    public override string ToString()
    {
        return string.Format("Unit Id: {0}. R: {1}, Vx: {2}, Vy: {3}.", Id, R, Vx, Vy);
    }
}

class CheckPoint: Unit
{
    public int TimesVisited { get; set; }
    
    public override string ToString()
    {
        return string.Format("Checkpoint- Id: {0}, X: {1}, Y: {2}, Visited: {3}", Id, X, Y, TimesVisited);
    }
    
    public bool IsEqual(CheckPoint checkPoint)
    {
        return checkPoint != null && checkPoint.X == this.X && checkPoint.Y == this.Y;
    }
}

class Pod : Unit
{
    public float Angle { get; set; }
    public int NextCheckPointId { get; set; }
    public int Checked { get; set; }
    public int Timeout { get; set; }
    public Pod Partner { get; set; }
    public bool IsThrustUsed { get; set; }
    
    public float TurningRadius { get { return 2000; } }
    
    public float GetAngle(Point p) {
        float d = this.Distance(p);
        float dx = (p.X - this.X) / d;
        float dy = (p.Y - this.Y) / d;
    
        // Simple trigonometry. We multiply by 180.0 / PI to convert radiants to degrees.
        float a = (float) (Math.Acos(dx) * 180.0 / Math.PI);
    
        // If the point I want is below me, I have to shift the angle for it to be correct
        if (dy < 0) {
            a = (float) 360.0 - a;
        }
    
        return a;
    }
    
    public float DiffAngle(Point p) {
        float a = this.GetAngle(p);
    
        // To know whether we should turn clockwise or not we look at the two ways and keep the smallest
        // The ternary operators replace the use of a modulo operator which would be slower
        float right = this.Angle <= a ? a - this.Angle : (float) 360.0 - this.Angle + a;
        float left = this.Angle >= a ? this.Angle - a : (float) 360.0 + this.Angle - a;
    
        if (right < left) {
            return right;
        } else {
            // We return a negative angle if we must rotate to left
            return -left;
        }
    }
    
    void Rotate(Point p) {
        float a = this.DiffAngle(p);
    
        // Can't turn by more than 18° in one turn
        if (a > 18.0) {
            a = (float) 18.0;
        } else if (a < -18.0) {
            a = (float) -18.0;
        }
    
        this.Angle += a;
    
        // The % operator is slow. If we can avoid it, it's better.
        if (this.Angle >= 360.0) {
            this.Angle = this.Angle - ((float) 360.0);
        } else if (this.Angle < 0.0) {
            this.Angle += (float) 360.0;
        }
    }
}