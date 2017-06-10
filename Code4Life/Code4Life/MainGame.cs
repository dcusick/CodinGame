using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class MainGame
{
    static void Main(string[] args)
    {
        var PlayerDebug = false;

        var samples = new List<Sample>();
        var availableMolecules = new AvailableMoleculesList();
        var players = new List<Player>();
        var projectList = new ProjectList();
        
        players.Add(new Player() { Id = 0 });
        players.Add(new Player() { Id = 1 });
        
        ProcessOneTimeInputs( ref projectList );
        
        // game loop
        while (true)
        {            
            ProcessInputs( players, ref samples, ref availableMolecules );

            //foreach (var availableMolecule in availableMolecules)
            //Console.Error.WriteLine(availableMolecules);

            var myPlayer = players.Where(p => p.IsMine).First();
            var oppPlayer = players.Where(p => !p.IsMine).First();
            
            if (PlayerDebug)
            {
                Console.Error.WriteLine(myPlayer);
                Console.Error.WriteLine(".");
                Console.Error.WriteLine(".");
                Console.Error.WriteLine(".");
                Console.Error.WriteLine(oppPlayer);
            }
            var nextMove = GetNextMove( ref players, samples, availableMolecules, projectList);
            
            if (nextMove == "DUMP MOLECULES")
            {
                nextMove = "GOTO DIAGNOSIS Tired of Waiting!!";
                myPlayer.IsDumpAllMolecules = true;
            }
            
            Console.WriteLine(nextMove);
        }
    }
    
    public static string GetNextMove( ref List<Player> players, List<Sample> samples, AvailableMoleculesList availableMolecules, ProjectList projectList)
    {
        var myPlayer = players.Where(p => p.IsMine).First();
        var oppPlayer = players.Where(p => !p.IsMine).First();
        var nextMove = "";
        
//        foreach(var project in projectList.Projects)
//            Console.Error.WriteLine(string.Format("Did Complete SP {0}? : {1}", project.ProjectId, project.CompletedScienceProject(myPlayer)));

        var samplesCarried = samples.Where(s => s.CarriedBy == 0);
        var undiagnosedSamplesCarried = samples.Where(s => s.CarriedBy == 0 && !s.IsDiagnosed);
        var diagnosedSamplesCarried = samples.Where(s => s.CarriedBy == 0 && s.IsDiagnosed);
        var diagnosedSamplesFulfillable = samples.Where(s => s.CarriedBy == 0 && s.CanFulfill(myPlayer, availableMolecules)).OrderBy(s => s.Health).ThenByDescending(s => s.MoleculesRequiredByPlayer(myPlayer));
        var diagnosedSamplesUnfulfillable = samples.Where(s => s.CarriedBy == 0 && !s.CanFulfill(myPlayer, availableMolecules)).OrderBy(s => s.Health).ThenByDescending(s => s.MoleculesRequiredByPlayer(myPlayer));
        var cloudSamplesFulfillable = samples.Where(s => s.CarriedBy == -1 && s.CanFulfill(myPlayer, availableMolecules)).OrderByDescending(s => s.Health).ThenBy(s => s.MoleculesRequiredByPlayer(myPlayer));
        var cloudSamples = samples.Where(s => s.CarriedBy == -1);
        var finishedSamples = diagnosedSamplesCarried.Where(s => s.PlayerHasEnoughFor(myPlayer));
        var unfinishedSamples = diagnosedSamplesCarried.Where(s => !s.PlayerHasEnoughFor(myPlayer));

        Console.Error.WriteLine(string.Format("There are {0} out of {1} samples in the cloud that can be fulfilled.", cloudSamplesFulfillable.Count(), cloudSamples.Count()));
        Console.Error.WriteLine(string.Format("My Target is: {0}", myPlayer.Target));
        Console.Error.WriteLine(string.Format("IsDumpAllMolecules: {0}", myPlayer.IsDumpAllMolecules));

        Sample sample;
        
        if ( myPlayer.ETA > 0 )
            return string.Format("WAIT Wait ETA - {0}", myPlayer.ETA);

        if (myPlayer.Target != "MOLECULES")
            myPlayer.TurnsWaiting = 0;
/*
        if (myPlayer.IsDumpAllMolecules)
        {
            Console.Error.WriteLine("Oops!  Max Molecules, and no matched Diagnoses.  DUMP AWAY!");
            if (samplesCarried.Count() == 0)
            {
                myPlayer.IsDumpAllMolecules = false;
            }
            else
            {
                if (myPlayer.Target == "DIAGNOSIS")
                    return "CONNECT "+samplesCarried.First().Id;
                else
                    return "GOTO DIAGNOSIS Dump All!";
            }
        }
*/
        
        switch ( myPlayer.Target )
        {
            case "START_POS":
                nextMove = "GOTO SAMPLES";
                break;
            case "SAMPLES":
                if (samplesCarried.Count() < myPlayer.MaxSamples)
                {
                    var getRank = 1;
                    
                    if (myPlayer.AvailableSlots == 0)
                        getRank = 1;
                    else if (myPlayer.TotalExpertisePoints >= myPlayer.ExpertiseUntilLevel3)
                        getRank = 3;
                    else if (myPlayer.TotalExpertisePoints >= myPlayer.ExpertiseUntilLevel2)
                        getRank = 2;
/*
                    else if (samplesCarried.Count(s => s.Rank <= 2) >= 1 && myPlayer.TotalExpertisePoints >= 5)
                        getRank = 3;
                    else if (samplesCarried.Count(s => s.Rank <= 2) == 2 && myPlayer.TotalExpertisePoints >= 3)
                        getRank = 3;
                    else if (myPlayer.TotalExpertisePoints > 0 || samplesCarried.Count() >= 2)
                        getRank = 2;
*/
                    nextMove = string.Format("CONNECT {0}", getRank);
                } else {
                    nextMove = "GOTO DIAGNOSIS";
                }
                break;
            case "DIAGNOSIS":
                //var goDirectlyToLaboratory = false;
                Console.Error.WriteLine(string.Format("There are {0} out of {1} samples diagnosed, and {2} are fulfillable.", diagnosedSamplesCarried.Count(), samplesCarried.Count(), diagnosedSamplesFulfillable.Count()));
                
                if ( undiagnosedSamplesCarried.Count() == 0 )
                {
                    Console.Error.WriteLine(string.Format("IsDumpAllMolecules: {0}", myPlayer.IsDumpAllMolecules));
                    
                    if (diagnosedSamplesUnfulfillable.Count() > 0 && myPlayer.IsDumpAllMolecules)
                    {
                        nextMove = "CONNECT "+diagnosedSamplesUnfulfillable.First().Id;
                        myPlayer.IsDumpAllMolecules = false;
                    }
                    //var canFulfillAll = true;
                    
                    /*
                    foreach(var diagSample in diagnosedSamplesCarried)
                    {
                        if (diagSample.PlayerHasEnoughFor(myPlayer))
                            goDirectlyToLaboratory = true;

                        if (!diagSample.CanFulfill(myPlayer, availableMolecules) 
                            || (myPlayer.AvailableSlots == 0 && !diagSample.PlayerHasEnoughFor(myPlayer))) //|| diagSample.Health < 20)
                        {
                            nextMove = "CONNECT "+diagSample.Id;
                    //      canFulfillAll = false;
                            break;
                        }
                    }
                    */
                    if (cloudSamplesFulfillable.Count() > 0 && samplesCarried.Count() < myPlayer.MaxSamples ) // && nextMove == "")
                        nextMove = "CONNECT "+cloudSamplesFulfillable.First().Id;

                } else {
                    nextMove = "CONNECT "+undiagnosedSamplesCarried.First().Id;
                }
                
                if (nextMove == "" )
                {
                    if (finishedSamples.Count() > 0)
                        nextMove = "GOTO LABORATORY";
                    else if ( samplesCarried.Count() > 0 )
                        nextMove = "GOTO MOLECULES";
                    else if (nextMove == "")
                        nextMove = "GOTO SAMPLES";
                }
                break;
            case "MOLECULES":
                string nextRequiredType = null;

                if (!(myPlayer.IsUseBlocking && myPlayer.Score - oppPlayer.Score >= 50))
                {
                    var diagSamples = diagnosedSamplesCarried.Where(s => s.CanFulfill(myPlayer, availableMolecules)
                        && !s.PlayerHasEnoughFor(myPlayer)
                        //&& (myPlayer.IdGoingFor == -1 || myPlayer.IdGoingFor == s.Id)
                        ).OrderBy(s => myPlayer.IdGoingFor == s.Id ? 1 : 2).ThenByDescending(s => s.Health);
                    
                    if ((diagSamples == null || diagSamples.Count() == 0) && (finishedSamples == null || finishedSamples.Count() == 0))
                    {
                        Console.Error.WriteLine("Cannot fulfill any samples!!!");
                        if (samplesCarried.Count() < 3)
                        {
                            if (cloudSamplesFulfillable.Count() >= 1)
                            {
                                nextMove = string.Format("GOTO DIAGNOSIS Get Another Sample From Cloud While Waiting");
                            } else {
                                nextMove = string.Format("GOTO SAMPLES Get Another Sample While Waiting");
                            }
                        }
                        else
                        {
                            if (myPlayer.TurnsWaiting > 5 || (oppPlayer.Target != "MOLECULES" && oppPlayer.Target != "LABORATORY") )
                            {
                                myPlayer.IsDumpMolecule = true;
                                myPlayer.TurnsWaiting = 0;
                                nextMove = "DUMP MOLECULES";
                                //nextMove = "GOTO DIAGNOSIS Tired of Waiting!!";
                            } else {
                                nextMove = string.Format("WAIT Wait for {0} Molecule Release", myPlayer.IdGoingFor);
                                myPlayer.TurnsWaiting++;
                            }
                        }
                    } else {
                        if (unfinishedSamples.Count() == 0)
                        {
                            if (finishedSamples.Count() > 0)
                            {
                                nextMove = "GOTO LABORATORY";
                            } else {
                                nextMove = "GOTO DIAGNOSIS";
                            }
                            break;
                        }

                        sample = unfinishedSamples.First();
                            
                        myPlayer.TurnsWaiting = 0;
                        
                        myPlayer.IdGoingFor = sample.Id;
                        
                        Console.Error.WriteLine(sample);
                        
                        if (sample.PlayerHasEnoughFor(myPlayer))
                            nextMove = string.Format("GOTO LABORATORY ID {0} Ready!", sample.Id);
                        else
                        {
                            nextRequiredType = sample.GetRequiredType(myPlayer, availableMolecules);
                            if ( nextRequiredType != null && myPlayer.AvailableSlots > 0)
                            {
                                //playerHasEnough = false;
                                nextMove = string.Format("CONNECT {0}", nextRequiredType);
                            }
                            else if (finishedSamples != null && finishedSamples.Count() > 0)
                                nextMove = string.Format("GOTO LABORATORY ID {0} Ready!", finishedSamples.First().Id);
                        }
                        
                        if (nextMove == "")
                        {
                            //nextMove = "WAIT Confused!";
                            nextMove = "GOTO DIAGNOSIS Confused! Dump All";
                            myPlayer.IsDumpAllMolecules = true;
                        }
                    }
                } else {
                    if (myPlayer.AvailableSlots == 0)
                    {
                        nextMove = "WAIT HeHeHeHe!";
                    } else {
                        var  oppPlayerMoleculesNeeded = oppPlayer.MoleculesPlayerNeedsForSamples
                                        .Join(availableMolecules.AvailableMolecules.Where(am => am.MoleculeCount > 0), pn => pn.Id, am => am.Id, (pn, am) => new SampleMolecule() { Id = pn.Id, MoleculeCount = am.MoleculeCount - pn.MoleculeCount })
                                        .Where(opm => opm.MoleculeCount >= 0)
                                        .OrderBy(opm => opm.MoleculeCount);

                        //foreach (var oppPlayerMoleculesNeed in oppPlayerMoleculesNeeded)
                        //    Console.Error.WriteLine(oppPlayerMoleculesNeed.ToString());

                        if (oppPlayerMoleculesNeeded.Count() > 0)
                            nextMove = string.Format("CONNECT {0}", oppPlayerMoleculesNeeded.First().Id);
                        else
                            nextMove = "WAIT HaHaHaHa!";
                    }

                    if (nextMove == "") nextMove = "WAIT HELP!!!!!";
                }
                break;
            case "LABORATORY":
                if (diagnosedSamplesCarried.Count() > 0)
                {
                    var readySamples = diagnosedSamplesCarried.Where(s => s.PlayerHasEnoughFor(myPlayer));
                    
                    if (readySamples != null && readySamples.Count() > 0)
                    {
                        sample = readySamples.First();
                        nextMove = string.Format("CONNECT {0}", sample.Id);
                        sample.CarriedBy = -1;
                        myPlayer.IdGoingFor = -1;
                    } else if (samplesCarried.Count() > 1)
                        nextMove = "GOTO MOLECULES";
                    else if (cloudSamplesFulfillable.Count() >= 1)
                    {
                        nextMove = string.Format("GOTO DIAGNOSIS Get Another Sample From Cloud.");
                    } else {
                        nextMove = "GOTO SAMPLES";
                    }
                } else {
                    if (cloudSamplesFulfillable.Count() >= 1)
                    {
                        nextMove = string.Format("GOTO DIAGNOSIS Get Another Sample From Cloud.");
                    } else {
                        nextMove = "GOTO SAMPLES";
                    }
                }
                
                break;
            
        }
        
        return nextMove;
    }
    
    private static void ProcessOneTimeInputs( ref ProjectList projectList )
    {
        string[] inputs;
        int projectCount = int.Parse(Console.ReadLine());
        var debugInputs = false;
        
        var inpCount = 0;
        if (debugInputs) Console.Error.WriteLine(string.Format("Input #{0}: {1}", inpCount++, projectCount));
        
        projectList = new ProjectList();
        
        for (int i = 0; i < projectCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            
            if (debugInputs) Console.Error.WriteLine(string.Format("Input #{0}: {1}", inpCount++, string.Join(",", inputs)));
            
            var project = new Project() { ProjectId = i };
            project.NeededMolecules.Add(new SampleMolecule() { Id = "A", MoleculeCount = int.Parse(inputs[0]) } );
            project.NeededMolecules.Add(new SampleMolecule() { Id = "B", MoleculeCount = int.Parse(inputs[1]) } );
            project.NeededMolecules.Add(new SampleMolecule() { Id = "C", MoleculeCount = int.Parse(inputs[2]) } );
            project.NeededMolecules.Add(new SampleMolecule() { Id = "D", MoleculeCount = int.Parse(inputs[3]) } );
            project.NeededMolecules.Add(new SampleMolecule() { Id = "E", MoleculeCount = int.Parse(inputs[4]) } );
            
            projectList.Projects.Add(project);
        }
        
        Console.Error.WriteLine(projectList.ToString());
    }
    
    private static void ProcessInputs( List<Player> players, ref List<Sample> samples, ref AvailableMoleculesList availableMolecules )
    {
        string[] inputs;
        var inpCount = 0;
        var debugInputs = false;
        
        for (int i = 0; i < 2; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            
            if (debugInputs) Console.Error.WriteLine(string.Format("Input #{0}: {1}", inpCount++, string.Join(",", inputs)));
            
            string target = inputs[0];
            int eta = int.Parse(inputs[1]);
            int score = int.Parse(inputs[2]);
            
            var moleculeStorages = new List<SampleMolecule>();
            moleculeStorages.Add(new SampleMolecule() { Id = "A", MoleculeCount = int.Parse(inputs[3]) } );
            moleculeStorages.Add(new SampleMolecule() { Id = "B", MoleculeCount = int.Parse(inputs[4]) } );
            moleculeStorages.Add(new SampleMolecule() { Id = "C", MoleculeCount = int.Parse(inputs[5]) } );
            moleculeStorages.Add(new SampleMolecule() { Id = "D", MoleculeCount = int.Parse(inputs[6]) } );
            moleculeStorages.Add(new SampleMolecule() { Id = "E", MoleculeCount = int.Parse(inputs[7]) } );
            
            var expertises = new List<SampleMolecule>();
            expertises.Add(new SampleMolecule() { Id = "A", MoleculeCount = int.Parse(inputs[8]) } );
            expertises.Add(new SampleMolecule() { Id = "B", MoleculeCount = int.Parse(inputs[9]) } );
            expertises.Add(new SampleMolecule() { Id = "C", MoleculeCount = int.Parse(inputs[10]) } );
            expertises.Add(new SampleMolecule() { Id = "D", MoleculeCount = int.Parse(inputs[11]) } );
            expertises.Add(new SampleMolecule() { Id = "E", MoleculeCount = int.Parse(inputs[12]) } );
            
            
            var player = players.Where( p => p.Id == i).First();

            player.PriorTarget = player.Target;
            player.Target = target;
            player.ETA = eta;
            player.Score = score;
            player.MoleculeStorages = moleculeStorages;
            player.Expertises = expertises;
        }
        
        availableMolecules = new AvailableMoleculesList();
        
        inputs = Console.ReadLine().Split(' ');
        
        if (debugInputs) Console.Error.WriteLine(string.Format("Input #{0}: {1}", inpCount++, string.Join(",", inputs)));
        
        int sampleCount = int.Parse(Console.ReadLine());
        
        availableMolecules.AvailableMolecules.Add(new SampleMolecule() { Id = "A", MoleculeCount = int.Parse(inputs[0]) } );
        availableMolecules.AvailableMolecules.Add(new SampleMolecule() { Id = "B", MoleculeCount = int.Parse(inputs[1]) } );
        availableMolecules.AvailableMolecules.Add(new SampleMolecule() { Id = "C", MoleculeCount = int.Parse(inputs[2]) } );
        availableMolecules.AvailableMolecules.Add(new SampleMolecule() { Id = "D", MoleculeCount = int.Parse(inputs[3]) } );
        availableMolecules.AvailableMolecules.Add(new SampleMolecule() { Id = "E", MoleculeCount = int.Parse(inputs[4]) } );

        samples = new List<Sample>();

        for (int i = 0; i < sampleCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            
            if (debugInputs) Console.Error.WriteLine(string.Format("Input #{0}: {1}", inpCount++, string.Join(",", inputs)));
            
            int sampleId = int.Parse(inputs[0]);
            int carriedBy = int.Parse(inputs[1]);
            int rank = int.Parse(inputs[2]);
            string expertiseGain = inputs[3];
            int health = int.Parse(inputs[4]);
            
            var requiredMolecules = new List<SampleMolecule>();
            requiredMolecules.Add(new SampleMolecule() { Id = "A", MoleculeCount = int.Parse(inputs[5]) } );
            requiredMolecules.Add(new SampleMolecule() { Id = "B", MoleculeCount = int.Parse(inputs[6]) } );
            requiredMolecules.Add(new SampleMolecule() { Id = "C", MoleculeCount = int.Parse(inputs[7]) } );
            requiredMolecules.Add(new SampleMolecule() { Id = "D", MoleculeCount = int.Parse(inputs[8]) } );
            requiredMolecules.Add(new SampleMolecule() { Id = "E", MoleculeCount = int.Parse(inputs[9]) } );
    
            var sample = samples.Where( s => s.Id == sampleId ).FirstOrDefault();
            if (sample == null)
            {
                sample = new Sample() { Id = sampleId, CarriedBy = carriedBy, Rank = rank, ExpertiseGain = expertiseGain, Health = health, RequiredMolecules = requiredMolecules };
                samples.Add(sample);
            }
            else
            {
                sample.CarriedBy = carriedBy;
                sample.RequiredMolecules = requiredMolecules;
                sample.ExpertiseGain = expertiseGain;
                sample.Health = health;
                sample.Rank = rank;
            }
            
            //Console.Error.WriteLine(sample);
        }
        var myPlayer = players.Where( p => p.Id == 0).First();
        var oppPlayer = players.Where( p => p.Id == 1).First();
        
        myPlayer.Samples = samples.Where(s => s.CarriedBy == 0).ToList();
        oppPlayer.Samples = samples.Where(s => s.CarriedBy == 1).ToList();
        
    }
}
