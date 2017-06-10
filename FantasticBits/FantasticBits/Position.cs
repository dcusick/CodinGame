using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Position
{
    public int X { get; set; }
    public int Y { get; set; }
    
    public double GetDistance(Position position)
    {
        return Math.Sqrt(Math.Pow(position.X - this.X, 2)  + Math.Pow(position.Y - this.Y, 2));
    }
}