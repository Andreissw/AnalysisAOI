using AnalysisAOI.Class.Inspection.Line;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnalysisAOI.Class.Inspection
{
    public class DataLine
    {
        public DataLine(string _pgname)
        { 
            ProgrammName = _pgname;
        }
        public string ProgrammName { get; }

        public Statistics Statistics { get; set; } 

        public List<TOP> Tops { get; set; }
    }
}