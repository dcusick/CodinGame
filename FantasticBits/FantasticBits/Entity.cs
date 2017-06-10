using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Entity : Position
{
    public int Id {get; set; }
    public string EntityType { get; set; }
    public int VelX { get; set; }
    public int VelY { get; set; }
}