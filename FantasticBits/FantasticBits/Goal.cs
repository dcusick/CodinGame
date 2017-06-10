using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Goal : Entity
{
    public int Width { get { return 3700; } }

    public int TopY { get { return Y - (Width / 2); } }
    public int BottomY { get { return Y + (Width / 2); } }

    public bool IsToShootAt(int teamId)
    {
        return Id != teamId;
    }
}