using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Grab Snaffles and try to throw them through the opponent's goal!
 * Move towards a Snaffle and use your team id to determine where you need to throw it.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int myTeamId = int.Parse(Console.ReadLine()); // if 0 you need to score on the right of the map, if 1 you need to score on the left
        var wizards = new List<Wizard>();
        var oppWizards = new List<Wizard>();
        var snaffles = new List<Snaffle>();
        var MaxThrust = 150;
        var MaxPower = 500;
        var goals = new List<Goal>();
        var spells = new List<Spell>();

        goals.Add(new Goal() { Id = 0, X = 0, Y = 3750 });
        goals.Add(new Goal() { Id = 1, X = 16000, Y = 3750 });
        
        spells = PopulateSpells();

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int myScore = int.Parse(inputs[0]);
            int myMagic = int.Parse(inputs[1]);
            inputs = Console.ReadLine().Split(' ');
            int opponentScore = int.Parse(inputs[0]);
            int opponentMagic = int.Parse(inputs[1]);
            int entities = int.Parse(Console.ReadLine()); // number of entities still in game

            wizards = new List<Wizard>();
            oppWizards = new List<Wizard>();
            snaffles = new List<Snaffle>();
            
            for (int i = 0; i < entities; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]); // entity identifier
                string entityType = inputs[1]; // "WIZARD", "OPPONENT_WIZARD" or "SNAFFLE" (or "BLUDGER" after first league)
                int x = int.Parse(inputs[2]); // position
                int y = int.Parse(inputs[3]); // position
                int vx = int.Parse(inputs[4]); // velocity
                int vy = int.Parse(inputs[5]); // velocity
                int state = int.Parse(inputs[6]); // 1 if the wizard is holding a Snaffle, 0 otherwise
                
                switch (entityType)
                {
                    case "WIZARD":
                        var wizard = new Wizard() { Id = entityId, EntityType = entityType, X = x, Y = y, VelX = vx, VelY = vy, State = state, Magic = myMagic, Score = myScore };
                        wizards.Add(wizard);
                        break;
                    case "OPPONENT_WIZARD":
                        var oppWizard = new Wizard() { Id = entityId, EntityType = entityType, X = x, Y = y, VelX = vx, VelY = vy, State = state, Magic = opponentMagic, Score = opponentScore };
                        oppWizards.Add(oppWizard);
                        break;
                    case "SNAFFLE":
                        var snaffle = new Snaffle() { Id = entityId, EntityType = entityType, X = x, Y = y, VelX = vx, VelY = vy };
                        snaffles.Add(snaffle);
                        break;
                }
            }
            
            foreach(var wizard in wizards)
            {
                var snaffleDistances = new List<DistanceAway>();
                
                foreach(var snaffle in snaffles)
                {
                    snaffleDistances.Add(new DistanceAway() { EntityId = snaffle.Id, Distance = wizard.GetDistance(snaffle) });
                }
                wizard.SnaffleDistances.AddRange(snaffleDistances);
            }
            
            //for (int i = 0; i < 2; i++)
            //{
            var nextMoves = new List<Move>();
            
            foreach(var wizard in wizards)
            {
                var goal = goals.Where(g => g.IsToShootAt(myTeamId)).First();

                if (wizard.State == 1)
                {
                    nextMoves.Add(new Move() { EntityId = wizard.Id, OrderNum = 0, IsMandatory = true, MoveType = "THROW", NextMove = string.Format("THROW {0} {1} {2}", goal.X, goal.Y, MaxPower) } );
                } else {
                    Snaffle snaffleToFlipendo = null;
                    Snaffle snaffleToAccio = null;

                    
                    if (wizard.Magic >= spells.Where(s => s.SpellName == "FLIPENDO").First().MagicCost)
                        snaffleToFlipendo = wizard.SnaffleToFlipendo(snaffles, goal);

                    if (wizard.Magic >= spells.Where(s => s.SpellName == "ACCIO").First().MagicCost)
                        snaffleToAccio = wizard.SnaffleToAccio(snaffles, goal);

                    if (snaffleToFlipendo != null)
                        nextMoves.Add(new Move() { EntityId = wizard.Id, OrderNum = 0, IsMandatory = true, MoveType = "FLIPENDO", NextMove = string.Format("FLIPENDO {0}", snaffleToFlipendo.Id) } );
                    else if (snaffleToAccio != null)
                        nextMoves.Add(new Move() { EntityId = wizard.Id, OrderNum = 0, IsMandatory = true, MoveType = "ACCIO", NextMove = string.Format("ACCIO {0}", snaffleToAccio.Id), DistanceFromTarget = wizard.GetDistance(snaffleToAccio) } );
                    else
                    {
                        foreach(var snaffleDistance in wizard.SnaffleDistances.OrderBy(s => s.Distance))
                        {
                            var nextNum = (nextMoves.Count(m => m.EntityId == wizard.Id) == 0 ? -1 : nextMoves.Where(m => m.EntityId == wizard.Id).Max(nm => nm.OrderNum ) ) + 1;
                            var snaffle = snaffles.Where(s => s.Id == snaffleDistance.EntityId).FirstOrDefault();
                            //Console.Error.WriteLine(nextNum);
                            nextMoves.Add(new Move() { EntityId = wizard.Id, OrderNum = nextNum, IsMandatory = false, MoveType = "MOVE", NextMove = string.Format("MOVE {0} {1} {2}", snaffle.X, snaffle.Y, MaxThrust), DistanceFromTarget = snaffleDistance.Distance } );
                        }
                    }
                    //var minSnaffleId = GetClosestSnaffle(wizard);
                    //nextMoves.Add(new Move() { EntityId = wizard.Id, OrderNum = 0, IsMandatory = true, NextMove = string.Format("MOVE {0} {1} {2}", goal.X, goal.Y, MaxThrust) } );
                    //var minSnaffle = snaffles.Where(s => s.Id == minSnaffleId).First();
                    
                    //nextMove = string.Format("MOVE {0} {1} {2}", minSnaffle.X, minSnaffle.Y, MaxThrust);
                }
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");


                // Edit this line to indicate the action for each wizard (0 ≤ thrust ≤ 150, 0 ≤ power ≤ 500)
                // i.e.: "MOVE x y thrust" or "THROW x y power"
                
            }
            var wizard1 = wizards[0];
            var wizard2 = wizards[1];
            
            var wizard1Move = nextMoves.Where(m => m.EntityId == wizard1.Id && m.IsMandatory).FirstOrDefault();
            var wizard2Move = nextMoves.Where(m => m.EntityId == wizard2.Id && m.IsMandatory).FirstOrDefault();
            
            if (wizard1Move != null && wizard2Move == null)
            {
                wizard2Move = nextMoves.Where(m => m.EntityId == wizard2.Id).OrderBy(m => m.OrderNum).FirstOrDefault();
            } else if (wizard1Move == null && wizard2Move != null)
            {
                wizard1Move = nextMoves.Where(m => m.EntityId == wizard1.Id).OrderBy(m => m.OrderNum).FirstOrDefault();
            } else if (wizard1Move == null && wizard2Move == null) {
                //for (var i = 0; i < 2; i++)
                //{
                    wizard1Move = nextMoves.Where(m => m.EntityId == wizard1.Id && m.OrderNum == 0).FirstOrDefault();
                    wizard2Move = nextMoves.Where(m => m.EntityId == wizard2.Id && m.OrderNum == 0).FirstOrDefault();
                    
                    if (wizard1Move.NextMove == wizard2Move.NextMove)
                    {
                        if (wizard1Move.DistanceFromTarget <= wizard2Move.DistanceFromTarget)
                        {
                            wizard2Move = nextMoves.Where(m => m.EntityId == wizard2.Id && m.OrderNum == 1).FirstOrDefault();
                        } else {
                            wizard1Move = nextMoves.Where(m => m.EntityId == wizard1.Id && m.OrderNum == 1).FirstOrDefault();
                        }
                    }
                //}
            }
            
            if (wizard1Move != null && wizard2Move != null && wizard1Move.NextMove == wizard2Move.NextMove )
            {
                if (wizard1Move.MoveType == "ACCIO")
                    if (wizard1Move.DistanceFromTarget > wizard2Move.DistanceFromTarget)
                        wizard1Move.NextMove = "";
                    else
                        wizard2Move.NextMove = "";
            }
            if (wizard1Move != null && !string.IsNullOrEmpty(wizard1Move.NextMove))
                Console.WriteLine(wizard1Move.NextMove);
            else
                Console.WriteLine("MOVE 100 100 10");

            if (wizard2Move != null && !string.IsNullOrEmpty(wizard2Move.NextMove))
                Console.WriteLine(wizard2Move.NextMove);
            else
                Console.WriteLine("MOVE 100 100 10");
        }
    }
    
    private static int GetClosestSnaffle(Wizard wizard)
    {
        var minDist = double.MaxValue;
        var minSnaffleId = 0;
        
        
        foreach(var snaffleDistance in wizard.SnaffleDistances)
        {
            if (snaffleDistance.Distance < minDist)
            {
                minDist = snaffleDistance.Distance;
                minSnaffleId = snaffleDistance.EntityId;
            }
        }
        
        return minSnaffleId;
    }

    private static List<Spell> PopulateSpells()
    {
        var spells = new List<Spell>();
        spells.Add(new Spell() { SpellName = "OBLIVIATE", MagicCost = 5, Duration = 4 });
        spells.Add(new Spell() { SpellName = "PETRIFICUS", MagicCost = 10, Duration = 1 });
        spells.Add(new Spell() { SpellName = "ACCIO", MagicCost = 15, Duration = 6 });
        spells.Add(new Spell() { SpellName = "FLIPENDO", MagicCost = 20, Duration = 3 });

        return spells;
    }
}
