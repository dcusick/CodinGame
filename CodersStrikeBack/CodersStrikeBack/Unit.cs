using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

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