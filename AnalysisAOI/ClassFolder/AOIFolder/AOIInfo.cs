using AnalysisAOI.ClassFolder.AOIFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnalysisAOI.ClassFolder
{
    public class AOIInfo
    {
        public float AOIPassCount { get; set; }
        public float RemCount { get; set; }
        public string SPY { get; set; }

        public List<LineSPY> LineSPY { get; set; }

        public List<AOIDefects> TopDefects { get; set; }
        public List<AOIDefects> TopRepair { get; set; }
        public List<AOIDefects> TopCIR { get; set; }
        public List<AOIDefects> TopCIRDefects { get; set; }     

        public List<AOIDefects> TopCirDefectsRepair { get; set; }



    }
}