using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Sample {
    public int Id { get; set; }
    public int CarriedBy { get; set; }
    public int Rank { get; set; }
    public string ExpertiseGain { get; set; }
    public int Health { get; set; }
    public IList<SampleMolecule> RequiredMolecules { get; set; }
    public bool IsDiagnosed { get { return Health != -1; } }
    
    public Sample()
    {
        RequiredMolecules = new List<SampleMolecule>();
    }
    
    
    public bool PlayerHasEnoughFor(Player player)
    {   
        var fullList = RequiredMolecules.Join(player.MoleculeStorages, rm => rm.Id, ms => ms.Id, (rm, ms) => new { rm.Id, RequiredMoleculeCount = rm.MoleculeCount, StoredMoleculeCount = ms.MoleculeCount })
            .Join(player.Expertises, nl => nl.Id, e => e.Id, (nl, e) => new { nl.Id, nl.RequiredMoleculeCount, nl.StoredMoleculeCount, ExpertiseCount = e.MoleculeCount });

        return !fullList.Any(fl => fl.RequiredMoleculeCount > fl.StoredMoleculeCount + fl.ExpertiseCount);
    }
    
    public bool CanFulfill(Player player, AvailableMoleculesList availableMolecules)
    {
        var fullList = RequiredMolecules
            .Join(player.MoleculeStorages, rm => rm.Id, ms => ms.Id, (rm, ms) => new { rm.Id, RequiredMoleculeCount = rm.MoleculeCount, StoredMoleculeCount = ms.MoleculeCount })
            .Join(player.Expertises, nl => nl.Id, e => e.Id, (nl, e) => new { nl.Id, nl.RequiredMoleculeCount, nl.StoredMoleculeCount, ExpertiseCount = e.MoleculeCount })
            .Join(availableMolecules.AvailableMolecules, nl => nl.Id, am => am.Id
                , (nl, am) => new { nl.Id, nl.RequiredMoleculeCount, nl.StoredMoleculeCount, nl.ExpertiseCount, AvailableCount = am.MoleculeCount });
        
        return !fullList.Any(fl => 
                fl.AvailableCount < 
                    fl.RequiredMoleculeCount - fl.StoredMoleculeCount - fl.ExpertiseCount)
            && (fullList
                .Where(fl => fl.RequiredMoleculeCount - fl.StoredMoleculeCount - fl.ExpertiseCount >= 0)
                .Sum( fl => fl.RequiredMoleculeCount - fl.StoredMoleculeCount - fl.ExpertiseCount) <= 10);
    }
    
    public string GetRequiredType(Player player, AvailableMoleculesList availableMolecules)
    {     
        var nextType = RequiredMolecules
            .Join(player.TotalStorages, rm => rm.Id, ts => ts.Id, (rm, ts) => new { rm.Id, RequiredMoleculeCount = rm.MoleculeCount, TotalStoredMoleculeCount = ts.MoleculeCount })
            .Join(availableMolecules.AvailableMolecules, nl => nl.Id, am => am.Id, (nl, am) => new { nl.Id, nl.RequiredMoleculeCount, nl.TotalStoredMoleculeCount, AvailableCount = am.MoleculeCount })
            .Where(fl => fl.AvailableCount > 0 && fl.RequiredMoleculeCount > fl.TotalStoredMoleculeCount).OrderBy(fl => fl.AvailableCount)
            .FirstOrDefault();
        
        return nextType == null ? null : nextType.Id;
    }
    
    
    public int MoleculesRequiredByPlayer(Player player)
    {
        
        var fullList = RequiredMolecules
            //.Join(player.MoleculeStorages, rm => rm.Id, ms => ms.Id, (rm, ms) => new { rm.Id, RequiredMoleculeCount = rm.MoleculeCount, StoredMoleculeCount = ms.MoleculeCount })
            .Join(player.TotalStorages, rm => rm.Id, ts => ts.Id, (rm, ts) => new { rm.Id, RequiredMoleculeCount = rm.MoleculeCount, TotalStoredMoleculeCount = ts.MoleculeCount });
            //.Join(player.Expertises, nl => nl.Id, e => e.Id, (nl, e) => new { nl.Id, nl.RequiredMoleculeCount, nl.StoredMoleculeCount, ExpertiseCount = e.MoleculeCount });
        
        return fullList.Sum(fl => fl.RequiredMoleculeCount - fl.TotalStoredMoleculeCount);
    }
    
    public int MoleculesRequired
    {
        get {
            return RequiredMolecules.Sum( rm => rm.MoleculeCount);
        }
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(string.Format("Sample {0}: ",Id));
        sb.Append("CarriedBy-"+CarriedBy);
        sb.Append("Rank-"+Rank);
        sb.Append("ExpertiseGain-"+ExpertiseGain);
        sb.AppendLine("Health-"+Health);
        sb.AppendLine("IsDiagnosed-" + (IsDiagnosed  ? "1" : "0" ));
        sb.AppendLine("MoleculesRequired-" + MoleculesRequired);
        
        foreach(var molecule in RequiredMolecules)
            sb.AppendLine("   Molecule "+molecule.Id+" - " + molecule.MoleculeCount);
            
        return sb.ToString();
    }
}