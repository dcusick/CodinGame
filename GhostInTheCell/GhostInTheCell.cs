using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


class Program
{
    static void Main(string[] args)
    {
        var factoryLinks = new List<FactoryLink>();
        var players = new List<Player>();
        players.Add(new Player() { Id = 1 });
        players.Add(new Player() { Id = -1 });

        var factoryCount = 0;
        var linkCount = 0;

        ProcessOneTimeInputs(ref factoryCount, ref linkCount, ref factoryLinks);

        // game loop
        while (true)
        {
            var entityCount = 0;
            var factories = new List<Factory>();
            var troops = new List<Troop>();

            ProcessGameLoopInputs(ref entityCount, ref factories, ref troops, factoryLinks);

            var nextMove = DetermineNextMove(players, factories, troops);

            Console.WriteLine(nextMove);
        }
    }

    static string DetermineNextMove(List<Player> players, List<Factory> factories, List<Troop> troops)
    {
        var myPlayer = players.Where(p => p.IsMe).First();

        var bestFactoryInfo = myPlayer.GetBestFactory(factories);

        if (bestFactoryInfo == null)
            return "WAIT";
        else
        {
            return string.Format("MOVE {0} {1} {2}", bestFactoryInfo.Item1.Id, bestFactoryInfo.Item2.Id, bestFactoryInfo.Item3);
        }
    }

    static void ProcessOneTimeInputs(ref int factoryCount, ref int linkCount, ref List<FactoryLink> factoryLinks)
    {
        string[] inputs;

        factoryCount = int.Parse(Console.ReadLine()); // the number of factories
        linkCount = int.Parse(Console.ReadLine()); // the number of links between factories
        
        for (int i = 0; i < linkCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int factory1 = int.Parse(inputs[0]);
            int factory2 = int.Parse(inputs[1]);
            int distance = int.Parse(inputs[2]);

            Console.Error.WriteLine(string.Format("Factory Id: {0} to {1} is {2} turns away.", factory1, factory2, distance));
            
            factoryLinks.Add( new FactoryLink() { Factory1Id = factory1, Factory2Id = factory2, Distance = distance } );
        }
    }

    static void ProcessGameLoopInputs(ref int entityCount, ref List<Factory> factories, ref List<Troop> troops, List<FactoryLink> factoryLinks)
    {
        string[] inputs;

        entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
        for (int i = 0; i < entityCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int entityId = int.Parse(inputs[0]);
            string entityType = inputs[1];
            int arg1 = int.Parse(inputs[2]);
            int arg2 = int.Parse(inputs[3]);
            int arg3 = int.Parse(inputs[4]);
            int arg4 = int.Parse(inputs[5]);
            int arg5 = int.Parse(inputs[6]);

            switch(entityType)
            {
                case "FACTORY":
                    var factory = new Factory() { Id = entityId, PlayerId = arg1, NumberOfCyborgs = arg2, FactoryProduction = arg3 };
                    var links = factoryLinks.Where(f => f.Factory1Id == entityId || f.Factory2Id == entityId);
                    
                    //Console.Error.WriteLine(string.Format("Loop Factory {0}, linked to {1}", entityId, string.Join(",", links)));
                    
                    foreach(var link in links)
                        factory.FactoryDistances.Add(new FactoryDistance() { FactoryId = entityId == link.Factory2Id ? link.Factory1Id : link.Factory2Id, Distance = link.Distance });

                    factories.Add(factory);
                    break;
                case "TROOP":
                    var troop = new Troop() { Id = entityId, PlayerId = arg1, FactoryIdLeaving = arg2, FactoryIdTargeting = arg3, NumberOfCyborgs = arg4, NumberOfTurnsBeforeArrival = arg5 };
                    troops.Add(troop);
                    break;
            }
        }
    }
}

class Entity
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
}

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

class FactoryDistance
{
    public int FactoryId { get; set; }
    public int Distance { get; set; }
}

class FactoryLink
{
    public int Factory1Id { get; set; }
    public int Factory2Id { get; set; }
    public int Distance { get; set; }
}

class MultiEntity : Entity
{
    public int NumberOfCyborgs { get; set; }
}

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

class Troop : MultiEntity
{
    public int FactoryIdLeaving { get; set; }
    public int FactoryIdTargeting { get; set; }
    public int NumberOfTurnsBeforeArrival { get; set; }
}
