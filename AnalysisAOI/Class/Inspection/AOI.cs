using AnalysisAOI.Class.Inspection.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnalysisAOI.Class.Inspection
{
    public class AOI
    {
        public AOI(string _head)
        { 
            Head = _head;
            Query = GetQuery();
            if (Query is null) return;

            Lines = Query.GetDataLines();
        }

        public IQuery Query { get; }
        public string Head { get; }    

        public List<DataLine> Lines { get; }

        IQuery GetQuery()
        {
            return Head.Contains("Успешная") ? new SuccessAOI() : Head.Contains("Ложный АОИ")? (IQuery) new FalseAOI() : Head.Contains("Дефект АОИ")?  new VerifAOI(): null;
        }
    }
}