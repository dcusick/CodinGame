using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Troop : MultiEntity
{
    public int FactoryIdLeaving { get; set; }
    public int FactoryIdTargeting { get; set; }
    public int NumberOfTurnsBeforeArrival { get; set; }
}