using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Player
{
    public int Id { get; set; }

    public bool IsMe
    {
        get { return Id == 1; }
    }

    public Tuple<Factory, Factory, int> GetBestFactory(List<Factory> factories)
    {
        var playerFactories = factories.Where(f => f.PlayerId == this.Id);
        Factory bestFactoryFrom = null;
        Factory bestFactoryTo = null;
        var numberOfTroopsNeeded = 0;
        int bestFactoryScore = Int32.MaxValue * -1;

        foreach(var factory in playerFactories)
        {
            foreach(var linkedFactory in factory.FactoriesCanTake(factories))
            {
                var factoryScore = 0;
                factoryScore += linkedFactory.Item1.FactoryProduction * 100;
                factoryScore -= linkedFactory.Item2;
                factoryScore -= linkedFactory.Item3;

                if (factoryScore > bestFactoryScore)
                {
                    bestFactoryScore = factoryScore;
                    bestFactoryFrom = factory;
                    bestFactoryTo = linkedFactory.Item1;
                    numberOfTroopsNeeded = linkedFactory.Item2;
                }
            }
        }

        return bestFactoryFrom == null ? null : new Tuple<Factory, Factory, int> (bestFactoryFrom, bestFactoryTo, numberOfTroopsNeeded );
    }
}