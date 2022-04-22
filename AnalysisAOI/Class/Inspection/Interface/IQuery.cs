using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisAOI.Class.Inspection.Interface
{
    public interface IQuery
    {
        string ColumnOne { get; }
        string ColumnTwo { get;  }

        string ColumnThree { get;  }
        List<DataLine> GetDataLines();
    }
}
