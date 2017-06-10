using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Factory : MultiEntity
{
    public int FactoryProduction { get; set; }
    public List<FactoryDistance> FactoryDistances { get; set; }

    public Factory()
    {
        FactoryDistances = new List<FactoryDistance>();
    }

    public bool CanIncreaseProduction 
    {
        get { return FactoryProduction < 3; }
    }

    public List<Tuple<Factory, int, int>> FactoriesCanTake(List<Factory> factories, bool ExcludeMine = true)
    {
        var factoriesCanTake = factories.Join(FactoryDistances
                , f => f.Id, fd => fd.FactoryId
                , (f, fd) => new Tuple<Factory, int, int> ( f, f.NumberOfCyborgs + (f.FactoryProduction * fd.Distance) + 1, fd.Distance) )
            .Where(f => (!ExcludeMine || (ExcludeMine && f.Item1.PlayerId != 1)) && f.Item2 < this.NumberOfCyborgs).ToList();

        Console.Error.WriteLine(string.Format("In FactoriesCanTake.  Factory {0} can take {1} Factories.", this.Id, string.Join(",", factoriesCanTake.Select(f => f.Item1.Id))));

        return factoriesCanTake;
    }
}