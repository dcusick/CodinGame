using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

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
        
        var checkPoints = new List<CheckPoint>();
        var laps = int.Parse(Console.ReadLine());
        var checkPointCount = int.Parse(Console.ReadLine());
        for (var i = 0; i < checkPointCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            checkPoints.Add(new CheckPoint() { Id = i, X = int.Parse(inputs[0]), Y = int.Parse(inputs[1]) });
        }
        
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

        var isStillOrienting = false;

        var stillOrientingThrust = 60; //70;                                          //10
        var currentCheckPointLargeAngleThrust = Math.Floor((double) (MaxThrust / 1.5));      //20
        var podDriftingFromCurrentCheckPointThrust = 80;                        //30
        var nextCheckPointLargeAngleAsApproachCurrentCheckPointThrust = Math.Floor((double) (MaxThrust / 3));      //60
        var approachingCurrentCheckPointThrust = 85;                            //70
        var maxProportionalApproachThrust = 20;                                 //80

        var isShortCircuitMidpointOnBigAngle = false;
        var isMadKnightTrick = true;
        var isAdjustForNextCheckpoint = false;
        var isOverrideBasedOnAngle = false;
        
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
                int angle = int.Parse(inputs[4]); // angle between your pod orientation and the direction of the next checkpoint
                int nextCheckPointId = int.Parse(inputs[5]); // distance to the next checkpoint
                var pod = new Pod() { Id = i, X = x, Y = y, Vx = vx, Vy = vy, Angle = angle, NextCheckPointId = nextCheckPointId };
                var cp = checkPoints.Where(c => c.Id == nextCheckPointId).First();
                pod.AngleToCheckPoint =pod.GetAngle(new Point() { X = cp.X, Y = cp.Y });
                pods.Add( pod );
            }

            var myPods = pods.Where(p => p.IsMine).ToList();
            var otherPods = pods.Where(p => !p.IsMine).ToList();

            foreach(var myPod in myPods)
            {
                double velocity = 0;
                
                nextCheckPoint = checkPoints.Where(c => c.Id == myPod.NextCheckPointId).First();
                if (checkPoints.Any(c => c.Id == myPod.NextCheckPointId+1))
                    nextNextCheckPoint = checkPoints.Where(c => c.Id == myPod.NextCheckPointId+1).First();
                else
                    nextNextCheckPoint = checkPoints.Where(c => c.Id == 0).First();

                var pointNextCheckpoint = new Unit() { X = nextCheckPoint.X, Y = nextCheckPoint.Y };
                var nextCheckpointDist = myPod.Distance(nextCheckPoint);
                
                myLastPod = lastPods.Where(lp => lp.Id == myPod.Id).SingleOrDefault();

                if (myLastPod != null)
                    velocity = myPod.Distance(myLastPod);
            
                var checkPoint = new CheckPoint() { X = nextCheckPoint.X, Y = nextCheckPoint.Y };
                var isSameCheckPoint = checkPoint.IsEqual(nextCheckPoint);
                
                timeout--;
                double angleToPoint = 0;
                double angleToNextPoint = 0;

                if (!isSameCheckPoint)
                {
                    if (nextCheckPoint != null)
                    {
                        nextCheckPoint.TimesVisited++;
                        
                        Console.Error.WriteLine(nextCheckPoint.ToString());
                        Console.Error.WriteLine(nextNextCheckPoint == null ? "No Next Next Checkpoint Yet" : nextNextCheckPoint.ToString());
                    }

                    if ( isShortCircuitMidpointOnBigAngle )
                    {
                        isStillOrienting = true;
                    }
                }
                
                if (isStillOrienting && ( myPod.AngleToCheckPoint > 10 || myPod.AngleToCheckPoint < -10))
                {
                        //var midPointOfNextCheckPoint = new Point() { X = Math.Abs(nextCheckPoint.X + myPod.X ) / 2, Y = Math.Abs(nextCheckPoint.Y + myPod.Y) / 2 };
                        var midPointOfNextCheckPoint = nextCheckPoint.GetMidpoint(myPod);
                        xToGo = Math.Floor(midPointOfNextCheckPoint.X);
                        yToGo = Math.Floor(midPointOfNextCheckPoint.Y);
                        //thrust = "70";
                        thrust = stillOrientingThrust.ToString();

                        Console.Error.WriteLine("Printing from 10");
                        Console.Error.WriteLine(string.Format("{0} degrees and {1} units from next Checkpoint.", myPod.AngleToCheckPoint, nextCheckpointDist));

                        Console.WriteLine(string.Format("{0} {1} {2} Big Angle, still orienting!!!", xToGo, yToGo, thrust));
                        continue;
                } else
                {
                    isStillOrienting = false;
                }

                if (nextNextCheckPoint != null)
                {
                    if (myLastPod != null)
                        angleToPoint = myPod.GetAngleToPoint(nextCheckPoint, myLastPod);
                    
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
                    
                if (myPod.AngleToCheckPoint > 60 || myPod.AngleToCheckPoint < -60)
                {
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
                } else if ( nextCheckpointDist < myPod.TurningRadius && myPod.AngleToCheckPoint == 0)
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
                } else if ( nextCheckpointDist < myPod.TurningRadius && myPod.AngleToCheckPoint < 5)
                {
                    Console.Error.WriteLine("Printing from 70");
                    thrust = approachingCurrentCheckPointThrust.ToString();
                } else {
                    var proportionalThrust = (nextCheckpointDist / myPod.TurningRadius) * 100.0 * .2;
                    var roundedThrust = Math.Round(proportionalThrust, 0);
                    thrust = Math.Max(roundedThrust, maxProportionalApproachThrust).ToString();

                    Console.Error.WriteLine("Printing from 80");
                    Console.Error.WriteLine(string.Format("Proportional Thust: {0}, Rounded Thrust: {1}, Final Thrust: {2}.", proportionalThrust, roundedThrust, thrust));
                }
                                        
                Console.Error.WriteLine(string.Format("NextCPDist: {0}, NextCPAngle: {1}, Vx: {2}, Vy: {3}, Timeout: {4}.", nextCheckpointDist, myPod.AngleToCheckPoint, myPod.Vx, myPod.Vy, timeout));
                
                if (!thrustUsed && (( myPod.AngleToCheckPoint == 0 && nextCheckpointDist >= 5000) || isFinalCheckPoint)) {
                    thrust = "BOOST";
                    thrustUsed = true;
                }


                if (isMadKnightTrick && thrust != "BOOST" && myPod.Id == 0)
                {            
                        Console.Error.WriteLine("MadKnight Trick");
                        xToGo = Convert.ToInt32(nextCheckPoint.X - (myPod.Vx * 3));
                        yToGo = Convert.ToInt32(nextCheckPoint.Y - (myPod.Vy * 3));
                        thrust = "100";
                }
                
                if (myPod.Id == 1)
                {
                    var otherPod = otherPods.FirstOrDefault(op => op.Id == 2);
                    CheckPoint opponentNextCheckPoint = null;
                    CheckPoint opponentNextNextCheckPoint = null;

                    opponentNextCheckPoint = checkPoints.Where(c => c.Id == otherPod.NextCheckPointId).First();
                    if (checkPoints.Any(c => c.Id == otherPod.NextCheckPointId+1))
                        opponentNextNextCheckPoint = checkPoints.Where(c => c.Id == otherPod.NextCheckPointId+1).First();
                    else
                        opponentNextNextCheckPoint = checkPoints.Where(c => c.Id == 0).First();                    

                    xToGo = opponentNextNextCheckPoint.X;
                    yToGo = opponentNextNextCheckPoint.Y;

                    thrust = "100";
                }

                
                if (isOverrideBasedOnAngle && thrust != "BOOST")
                {
                    xToGo = Convert.ToInt32(nextCheckPoint.X - (myPod.Vx * 3));
                    yToGo = Convert.ToInt32(nextCheckPoint.Y - (myPod.Vy * 3));
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
                        xToGo = nextCheckPoint.X;
                        yToGo = nextCheckPoint.Y;
                    
                        thrust = "100";
                    }
                    */
                        
                }

                //Console.WriteLine(string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", xToGo, yToGo, thrust, thrust, myPod.AngleToCheckPoint, myPod.Vx, myPod.Vy, myPod.R));  
                Console.WriteLine(string.Format("{0} {1} {2} {3} {4} {5} {6}", xToGo, yToGo, thrust, thrust, myPod.AngleToCheckPoint, angleToPoint, angleToNextPoint));  
            }            
            lastPods = pods.Select(p => p).ToList();
        }
    }
}