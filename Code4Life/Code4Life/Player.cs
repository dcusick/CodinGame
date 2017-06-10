using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player {
    public int Id { get; set; }
    public string Target { get; set; }
    public int ETA { get; set; }
    public int Score { get; set; }
    public IList<SampleMolecule> MoleculeStorages { get; set; }
    public IList<SampleMolecule> Expertises { get; set; }
    public IList<Sample> Samples { get; set; }
    public string PriorTarget { get; set; }
    public int IdGoingFor { get; set; }
    public int TurnsWaiting { get; set; }
    public bool IsDumpMolecule { get; set; }
    public bool IsDumpAllMolecules { get; set; }
    
    public int MaxSlots { get { return 10; } }
    public int MaxSamples { get { return 3; } }
    public int WinningScore { get { return 570; } }
    public int ExpertiseUntilLevel2 { get { return 12; } }
    public int ExpertiseUntilLevel3 { get { return 18; } }
    public bool IsUseBlocking { get { return true; } }
    public bool IsMine { get { return Id == 0; } }
    
    public int TotalExpertisePoints { get { return Expertises.Sum(e => e.MoleculeCount); } }
    
    public IList<SampleMolecule> TotalStorages { 
        get { 
            return MoleculeStorages.Join(
                    Expertises, ms => ms.Id, e => e.Id
                    , (ms, e) => new SampleMolecule() { Id = ms.Id, MoleculeCount = ms.MoleculeCount + e.MoleculeCount }).ToList();
            }
    }
    
    public IList<SampleMolecule> MoleculesPlayerNeedsForSamples { 
        get { 
            var summedSamples = Samples.SelectMany (p => p.RequiredMolecules)
                                    .GroupBy(s => s.Id)
                                    .Select(s => new SampleMolecule
                                    {
                                        Id = s.Key,
                                        MoleculeCount = s.Sum(c => c.MoleculeCount)
                                    });

            return TotalStorages.Join(
                    summedSamples, ts => ts.Id, ss => ss.Id
                    , (ts, ss) => new SampleMolecule() { Id = ts.Id, MoleculeCount = ss.MoleculeCount - ts.MoleculeCount }).ToList();
            }
    }

    public IList<Sample> FinishedSamples {
        get {
            return Samples.Where(s => s.PlayerHasEnoughFor(this)).ToList();
        }
    }
    
    public Player()
    {
        MoleculeStorages = new List<SampleMolecule>();
        Expertises = new List<SampleMolecule>();
        IdGoingFor = -1;
        TurnsWaiting = 0;
        IsDumpMolecule = false;
        IsDumpAllMolecules = false;
    }
    
    public int AvailableSlots
    {
        get
        {
            return MaxSlots - MoleculeStorages.Sum(ms => ms.MoleculeCount);
        }
    }
    
    public string GetStorageContents()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Player {0}: "+Id);
        
        var fullList = MoleculeStorages
            .Join(Expertises, ms => ms.Id, e => e.Id, (ms, e) => new { ms.Id, StoredMoleculeCount = ms.MoleculeCount, ExpertiseCount = e.MoleculeCount });

        foreach(var molecule in fullList)
        {
            sb.AppendLine(string.Format("   Molecule {0}: {1} - ({2}).", molecule.Id, molecule.StoredMoleculeCount, molecule.ExpertiseCount));
        }

        return sb.ToString();
    }
    
    public List<Sample> CloudSamplesToWin(List<Sample> samples, AvailableMoleculesList availableMolecules)
    {
        return samples.Where(s => s.CarriedBy == -1 
                //&& s.Rank == -1
                && s.CanFulfill(this, availableMolecules)
                && this.Score + s.Health >= this.WinningScore).ToList();
        
    }
    
    public override string ToString()
    {
        return string.Format("Player {0}: Target-{1}, ETA-{2}, Score-{3}, MoleculeStorages-{4}, Expertises-{5}\nPriorTarget-{6}", Id, Target, ETA, Score, MoleculeStorages.Count(), Expertises.Count(), PriorTarget);
    }
}