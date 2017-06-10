using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Numerics;

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
        var turn = 0;
        var gameBoard = new Point() { X = 16000, Y = 9000 };
        var topLeft = new Point() { X = 0, Y = 0 };

        double xToGo = 0;
        double yToGo = 0;
        
        var checkPointRadius = 600;
        var MaxTimeout = 100;
        var MaxThrust = 100;
        
        var timeout = MaxTimeout;
        
        Pod myLastPod = null;
        
        var isDecelerate = false;
        var isStillOrienting = false;

        var stillOrientingThrust = 60; //70;                                          //10
        var currentCheckPointLargeAngleThrust = Math.Floor((double) (MaxThrust / 1.5));      //20
        var podDriftingFromCurrentCheckPointThrust = 80;                        //30
        var nextCheckPointLargeAngleAsApproachCurrentCheckPointThrust = Math.Floor((double) (MaxThrust / 3));      //60
        var approachingCurrentCheckPointThrust = 85;                            //70
        var maxProportionalApproachThrust = 20;                                 //80

        var isShortCircuitMidpointOnBigAngle = false;
        var isMadKnightTrick = false;
        var isAdjustForNextCheckpoint = false;
        var isOverrideBasedOnAngle = true;
        
        while (true)
        {
            var pods = new List<Pod>();
            var lastPods = new List<Pod>();
            string thrust;

            turn++;
            
            for (var i = 0; i < 4; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                double x = double.Parse(inputs[0]);
                double y = double.Parse(inputs[1]);
                double vx = double.Parse(inputs[2]); // x position of the next check point
                double vy = double.Parse(inputs[3]); // y position of the next check point
                int nextCheckpointAngle = int.Parse(inputs[4]); // angle between your pod orientation and the direction of the next checkpoint
                int nextCheckPointId = int.Parse(inputs[5]); // distance to the next checkpoint
                
                pods.Add( new Pod() { Id = i, X = x, Y = y, Vx = vx, Vy = vy, Angle = angle, NextCheckPointId = nextCheckPointId } );
            }
            /*
            var otherPod = new Pod() { Id = 1, X = opponentX, Y = opponentY };
            
            inputs = Console.ReadLine().Split(' ');
            int opponentX = int.Parse(inputs[0]);
            int opponentY = int.Parse(inputs[1]);
            */
            double velocity = 0;
            double velocityX = 0;
            double velocityY = 0;
    
            var point = new Point() {X = xToGo, Y = yToGo };
            var pointNextCheckpoint = new Unit() { X = nextCheckpointX, Y = nextCheckpointY };

            var myPod = new Pod() { Id = 0, X = x, Y = y };
            var otherPod = new Pod() { Id = 1, X = opponentX, Y = opponentY };
            //var myPod = new Pod() { Id = 0, X = x, Y = y, Vector = new Vector(x, y) };
            //var otherPod = new Pod() { Id = 1, X = opponentX, Y = opponentY, Vector = new Vector(opponentX, opponentY) };
            
            myPod.Angle = myPod.GetAngle(point);
            myPod.AngleToCheckPoint = nextCheckpointAngle;

            if (myLastPod != null)
            {
                velocity = myPod.Distance(myLastPod);
                velocityX = myPod.X - myLastPod.X;
                velocityY = myPod.Y - myLastPod.Y;

                myPod.Vx = velocityX;
                myPod.Vy = velocityY;
            }

            pods.Add(myPod);
            pods.Add(otherPod);
        
            var checkPoint = new CheckPoint() { X = nextCheckpointX, Y = nextCheckpointY };
            var isSameCheckPoint = checkPoint.IsEqual(nextCheckPoint);
            
            timeout--;
            double angleToPoint = 0;
            double angleToNextPoint = 0;

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

                if (numberOfCheckPoints > 1 && isShortCircuitMidpointOnBigAngle)
                {
                    isStillOrienting = true;
                }
            }
            
            if (isStillOrienting && ( nextCheckpointAngle > 10 || nextCheckpointAngle < -10))
            {
                    //var midPointOfNextCheckPoint = new Point() { X = Math.Abs(nextCheckpointX + myPod.X ) / 2, Y = Math.Abs(nextCheckpointY + myPod.Y) / 2 };
                    var midPointOfNextCheckPoint = nextCheckPoint.GetMidpoint(myPod);
                    xToGo = Math.Floor(midPointOfNextCheckPoint.X);
                    yToGo = Math.Floor(midPointOfNextCheckPoint.Y);
                    //thrust = "70";
                    thrust = stillOrientingThrust.ToString();

                    Console.Error.WriteLine("Printing from 10");
                    Console.Error.WriteLine(string.Format("{0} degrees and {1} units from next Checkpoint.", nextCheckpointAngle, nextCheckpointDist));

                    Console.WriteLine(string.Format("{0} {1} {2} Big Angle, still orienting!!!", xToGo, yToGo, thrust));
                    continue;
            } else
            {
                isStillOrienting = false;
            }

            if (nextNextCheckPoint != null)
            {
                angleToPoint = myPod.GetAngleToPoint(nextCheckPoint, myLastPod);
                //angleToNextPoint = myPod.GetAngle(nextNextCheckPoint);
                //angleToNextPoint = myPod.GetAngleFromPoints(nextCheckPoint, nextNextCheckPoint);
                angleToNextPoint = myPod.CalculateAngleFromPoints(nextCheckPoint, nextNextCheckPoint);

                if (isAdjustForNextCheckpoint)
                {
                    //Adjust the target, to go to the side of the current checkpoint that is opposite of the next checkpoint.
                    var newTargetPoint = myPod.AdjustedCoordinates(nextNextCheckPoint, pointNextCheckpoint);
                    pointNextCheckpoint.X = newTargetPoint.X;
                    pointNextCheckpoint.Y = newTargetPoint.Y;
                }

                Console.Error.WriteLine(string.Format("{0} degrees and {1} units from current Checkpoint.", angleToPoint, myPod.Distance(nextCheckPoint)));
                Console.Error.WriteLine(string.Format("{0} degrees and {1} units from next Checkpoint.", angleToNextPoint, myPod.Distance(nextNextCheckPoint)));
            }
            else
            {
                var midPoint = topLeft.GetMidpoint(gameBoard);
                angleToNextPoint = myPod.CalculateAngleFromPoints(nextCheckPoint, midPoint);
                Console.Error.WriteLine(string.Format("{0} degrees from current Checkpoint, and Next CheckPoint Unknown.", myPod.Angle));
                Console.Error.WriteLine(string.Format("{0} degrees and {1} units from next Checkpoint (approxmiated Midpoint).", angleToNextPoint, myPod.Distance(midPoint)));
            }
            
            xToGo = pointNextCheckpoint.X;
            yToGo = pointNextCheckpoint.Y;
            var checkPointsVisitedThreeTimes = checkPoints.Count(cp => cp.TimesVisited == 3);
            var checkPointsCount = checkPoints.Count();
            var isFinalCheckPoint = checkPointsVisitedThreeTimes == checkPointsCount;

            Console.Error.WriteLine(string.Format("{0} out of {1} CheckPoints have been Visited 3 Times.", checkPointsVisitedThreeTimes, checkPointsCount));
            
            if (isFinalCheckPoint)
                Console.Error.WriteLine(string.Format("Last Checkpoint.  Balls to the Wall!!!!!"));
                
            if (nextCheckpointAngle > 60 || nextCheckpointAngle < -60)
            {
                //thrust = (MaxThrust/3).ToString();
                thrust = currentCheckPointLargeAngleThrust.ToString();
                Console.Error.WriteLine("Printing from 20");
            //} else if (myLastPod != null && Math.Abs(myPod.AngleToCheckPoint) > Math.Abs(myLastPod.AngleToCheckPoint))
            //{
                //thrust = "80";
            //    thrust = podDriftingFromCurrentCheckPointThrust.ToString();
            //    Console.Error.WriteLine("Printing from 30");
            } else if ( isFinalCheckPoint || nextCheckpointDist >= myPod.TurningRadius)
            {
                thrust = MaxThrust.ToString();
                Console.Error.WriteLine("Printing from 40");
            } else if ( nextCheckpointDist < myPod.TurningRadius && nextCheckpointAngle == 0)
            {
                thrust = MaxThrust.ToString();
                Console.Error.WriteLine("Printing from 50");
            //} //else if ( nextNextCheckPoint != null && nextCheckpointDist < myPod.TurningRadius && angleToNextPoint != 0 && ((angleToNextPoint >= 170 && angleToNextPoint <= 180)|| (angleToNextPoint <= -170 && angleToNextPoint >= -180 )))
            //{
            //    thrust = MaxThrust.ToString();
            //    xToGo = nextNextCheckPoint.X;
            //    yToGo = nextNextCheckPoint.Y;
/*
            } else if ( nextCheckpointDist < myPod.TurningRadius 
                        && angleToNextPoint != 0
                        && (
                            (angleToNextPoint > 60 && angleToNextPoint < 120) 
                                || (angleToNextPoint < -60 && angleToNextPoint > -120 ) 
                                || (angleToNextPoint < 15 && angleToNextPoint > -15 && nextNextCheckPoint != null && myPod.IsBehind(nextNextCheckPoint))
                      ))
            {
                Console.Error.WriteLine("Printing from 60");

                if (nextNextCheckPoint != null)
                {
                    var midPointOfCheckpoints = nextNextCheckPoint.GetMidpoint(nextCheckPoint);
                    xToGo = Math.Floor(midPointOfCheckpoints.X);
                    yToGo = Math.Floor(midPointOfCheckpoints.Y);
                }
                //thrust = (MaxThrust / 6).ToString();
                thrust = nextCheckPointLargeAngleAsApproachCurrentCheckPointThrust.ToString();
*/
            } else if ( nextCheckpointDist < myPod.TurningRadius && nextCheckpointAngle < 5)
            {
                Console.Error.WriteLine("Printing from 70");

                //thrust = (MaxThrust / 2).ToString();
                //thrust = "70";
                thrust = approachingCurrentCheckPointThrust.ToString();
            } else {
                var proportionalThrust = (nextCheckpointDist / myPod.TurningRadius) * 100.0 * .2;
                var roundedThrust = Math.Round(proportionalThrust, 0);
                thrust = Math.Max(roundedThrust, maxProportionalApproachThrust).ToString();

                Console.Error.WriteLine("Printing from 80");
                Console.Error.WriteLine(string.Format("Proportional Thust: {0}, Rounded Thrust: {1}, Final Thrust: {2}.", proportionalThrust, roundedThrust, thrust));
            }
                
            if (velocity > 500)
                isDecelerate = true;
                
            //if (isDecelerate)
            //    thrust = "0";
                
            Console.Error.WriteLine(string.Format("NextCPDist: {0}, NextCPAngle: {1}, Vx: {2}, Vy: {3}, Timeout: {4}.", nextCheckpointDist, nextCheckpointAngle, velocityX, velocityY, timeout));
            
            //if (Math.Abs(opponentX - x) < 5 && Math.Abs(opponentY - y) < 5 && !thrustUsed) {
            if (!thrustUsed && ((nextCheckpointAngle == 0 && nextCheckpointDist >= 5000) || isFinalCheckPoint)) {
                thrust = "BOOST";
                thrustUsed = true;
            }


            if (isMadKnightTrick && thrust != "BOOST")
            {            
                if (nextCheckpointAngle > 30 || nextCheckpointAngle < -30)
                {
                    xToGo = Convert.ToInt32(nextCheckpointX - (velocityX * 3));
                    yToGo = Convert.ToInt32(nextCheckpointY - (velocityY * 3));
                    thrust = "100";
                } else {
                    xToGo = nextCheckpointX;
                    yToGo = nextCheckpointY;
                    thrust = "100";
                }
            }
            
            if (isOverrideBasedOnAngle && thrust != "BOOST")
            {
                xToGo = Convert.ToInt32(nextCheckpointX - (velocityX * 3));
                yToGo = Convert.ToInt32(nextCheckpointY - (velocityY * 3));
                thrust = "100";
                /*
                if (nextCheckpointAngle > 120 || nextCheckpointAngle < -120)    
                    thrust = "15";
                else if (nextCheckpointAngle > 110 || nextCheckpointAngle < -110)    
                    thrust = "25";
                else if (nextCheckpointAngle > 100 || nextCheckpointAngle < -100)    
                    thrust = "35";
                else if (nextCheckpointAngle > 90 || nextCheckpointAngle < -90)    
                    thrust = "45";    
                else if (nextCheckpointAngle > 80 || nextCheckpointAngle < -80)    
                    thrust = "55";
                else if (nextCheckpointAngle > 70 || nextCheckpointAngle < -70)    
                    thrust = "65";
                else if (nextCheckpointAngle > 60 || nextCheckpointAngle < -60)    
                    thrust = "75";
                else if (nextCheckpointAngle > 50 || nextCheckpointAngle < -50)    
                    thrust = "85";
                else if (nextCheckpointAngle > 40 || nextCheckpointAngle < -40)    
                    thrust = "95";
                else
                {
                    xToGo = nextCheckpointX;
                    yToGo = nextCheckpointY;
                
                    thrust = "100";
                }
                */
                    
            }

            //Console.WriteLine(string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", xToGo, yToGo, thrust, thrust, nextCheckpointAngle, myPod.Vx, myPod.Vy, myPod.R));  
            Console.WriteLine(string.Format("{0} {1} {2} {3} {4} {5} {6}", xToGo, yToGo, thrust, thrust, nextCheckpointAngle, angleToPoint, angleToNextPoint));  
                        
            myLastPod = myPod;
        }
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
    public double Angle { get; set; }
    public int NextCheckPointId { get; set; }
    public int Checked { get; set; }
    public int Timeout { get; set; }
    public Pod Partner { get; set; }
    public bool IsThrustUsed { get; set; }
    public double AngleToCheckPoint { get; set; }    

    public double TurningRadius { get { return 2500; } }
    
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

class Unit : Point
{
    public int Id { get; set; }
    public double R { get; set; }
    public double Vx { get; set; }
    public double Vy { get; set; }
    
    public double GetUnitAngle(Point p)
    {
        return Math.Atan2(p.X - this.X, p.Y - this.Y) * (180 / Math.PI);
    }

    public string DirectionMoving()
    {
        if (this.Vx > 0 && this.Vy < 0) //Moving Up-Right
            return "UR";
        else if (this.Vx < 0 && this.Vy > 0) //Moving Down-Left
            return "DL";
        else if (this.Vx < 0 && this.Vy < 0) //Moving Up-Left
            return "UL";
        else if (this.Vx > 0 && this.Vy > 0) //Moving Down-Right
            return "DR";
        else if (this.Vx > 0 && this.Vy == 0) //Moving Right
            return "R";
        else if (this.Vx < 0 && this.Vy == 0) //Moving Left
            return "L";
        else if (this.Vx == 0 && this.Vy < 0) //Moving Up
            return "U";
        else if (this.Vx == 0 && this.Vy > 0) //Moving Down
            return "D";
        else
            return "";
    }

    public string PositionRelativeTo(Unit u)
    {
        if (this.X > u.X && this.Y < u.Y) //Is Down-Left from
            return "DL";
        else if (this.X < u.X && this.Y > u.Y) //Is Up-Right from
            return "UR";
        else if (this.Vx < u.X && this.Y < u.Y) //Is Down-Right from
            return "DR";
        else if (this.Vx > u.X && this.Y > u.Y) //Is Up-Left from
            return "UL";
        else if (this.Vx > u.X && this.Y == u.Y) //Is Left from
            return "L";
        else if (this.Vx < u.X && this.Y == u.Y) //Is Right from
            return "R";
        else if (this.Vx == u.X && this.Y < u.Y) //Is Down from
            return "D";
        else if (this.Vx == u.X && this.Y > u.Y) //Is Up from
            return "U";
        else
            return "";
    }

    public Point AdjustedCoordinates(Unit referenceU, Unit targetU)
    {
        var positionRelativeTo = targetU.PositionRelativeTo(referenceU);
        var newTarget = new Point();

        switch (positionRelativeTo)
        {
            case "DL":
                newTarget.X = targetU.X + 500;
                newTarget.Y = targetU.Y - 500;
                break;
            case "DR":
                newTarget.X = targetU.X - 250;
                newTarget.Y = targetU.Y - 250;
                break;
            case "UL":
                newTarget.X = targetU.X + 500;
                newTarget.Y = targetU.Y + 500;
                break;
            case "UR":
                newTarget.X = targetU.X - 500;
                newTarget.Y = targetU.Y + 500;
                break;
            default:
                newTarget.X = targetU.X;
                newTarget.Y = targetU.Y;
                break;
        }

        return newTarget;
    }
    public bool IsBehind(Unit u)
    {
        var directionMoving = this.DirectionMoving();
        var positionRelativeTo = this.PositionRelativeTo(u);

        if (directionMoving == "UR") //Moving Up-Right
        {
            if (positionRelativeTo == "DL")
                return true;
        } else if (directionMoving == "DL") //Moving Down-Left
        {
            if (positionRelativeTo == "UR")
                return true;
        } else if (directionMoving == "UL") //Moving Up-Left
        {
            if (positionRelativeTo == "DR")
                return true;
        } else if (directionMoving == "DR") //Moving Down-Right
        {
            if (positionRelativeTo == "UL")
                return true;
        }

        return false;
    }

    public override string ToString()
    {
        return string.Format("Unit Id: {0}. R: {1}, Vx: {2}, Vy: {3}.", Id, R, Vx, Vy);
    }

}
