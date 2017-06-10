using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Wizard : Entity
{
    public int State { get; set; }
    public List<DistanceAway> SnaffleDistances { get; set; }
    public int Magic { get; set; }
    public int Score { get; set; }

    public bool IsHoldingSnaffle { get { return State == 1; } }
    
    public Wizard()
    {
         SnaffleDistances = new List<DistanceAway>();
    }

    public Snaffle SnaffleToFlipendo(List<Snaffle> snaffles, Goal goal)
    {
        foreach(var snaffle in snaffles)
        {
            var topY = (snaffle.X - this.X) * (goal.TopY - this.Y) - (snaffle.Y - this.Y) * (goal.X - this.X);
            var bottomY = (snaffle.X - this.X) * (goal.BottomY - this.Y) - (snaffle.Y - this.Y) * (goal.X - this.X);
            
            if (((topY < 0 && bottomY > 0) || (topY > 0 && bottomY < 0)) && ((this.X < snaffle.X && snaffle.X < goal.X) || (this.X > snaffle.X && snaffle.X > goal.X)))
                return snaffle;
        }

        return null;
    }

    public Snaffle SnaffleToAccio(List<Snaffle> snaffles, Goal goal)
    {
        if (snaffles.Count() == 1 || snaffles.All(s => (s.X < this.X && this.X < goal.X) || (s.X > this.X && this.X > goal.X)))
        {
            var snaffle = snaffles.First();
            if ((snaffle.X < this.X && this.X < goal.X) || (snaffle.X > this.X && this.X > goal.X))
            {
                return snaffle;
            }
        }

        return null;
    }
}