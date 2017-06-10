using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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