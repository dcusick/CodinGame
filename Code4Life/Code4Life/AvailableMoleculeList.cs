using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class AvailableMoleculesList
{
    public IList<SampleMolecule> AvailableMolecules { get; set; }

    public AvailableMoleculesList()
    {
        AvailableMolecules = new List<SampleMolecule>();
    }
    
    public bool CanCover(string id, int count)
    {
        return AvailableMolecules.Count(am => am.Id == id && am.MoleculeCount >= count) > 0;
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("AvailableMoleculesList:");
        
        foreach(var molecule in AvailableMolecules)
            sb.AppendLine(string.Format("  AvailableMolecule Id {0}: {1}.", molecule.Id, molecule.MoleculeCount));

        return sb.ToString();
    }
}
