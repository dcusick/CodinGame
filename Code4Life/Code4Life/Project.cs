using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Project {
    public int ProjectId { get; set; }
    public IList<SampleMolecule> NeededMolecules { get; set; }

    public Project()
    {
        NeededMolecules = new List<SampleMolecule>();
    }
    
    public bool CompletedScienceProject(Player player)
    {
        var expertises = player.Expertises;
        
        foreach (var neededMolecule in NeededMolecules)
        {
            var expertise = expertises.Where(e => e.Id == neededMolecule.Id).FirstOrDefault();
            
            if (neededMolecule.MoleculeCount > expertise.MoleculeCount)
                return false;
        }
        
        return true;
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Format("Project {0}: ", ProjectId));
        
        foreach(var molecule in NeededMolecules)
            sb.AppendLine("   Molecule "+molecule.Id+" - " + molecule.MoleculeCount);
            
        return sb.ToString();
    }
}