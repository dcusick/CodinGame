using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

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