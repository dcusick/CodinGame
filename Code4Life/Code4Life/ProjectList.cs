using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class ProjectList {
    public IList<Project> Projects { get; set; }

    public ProjectList()
    {
        Projects = new List<Project>();
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Format("ProjectList: "));
        
        foreach(var project in Projects)
            sb.AppendLine(project.ToString());

        return sb.ToString();
    }
}