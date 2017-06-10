using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Move
{
    public int EntityId { get; set; }
    public int OrderNum { get; set; }
    public string MoveType { get; set; }
    public string NextMove { get; set; }
    public bool IsMandatory { get; set; }
    public double DistanceFromTarget { get; set; }
}