using AnalysisAOI.Class.Inspection.Line;
using AnalysisAOI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnalysisAOI.Class.Inspection.Interface
{
    public class FalseAOI : IQuery
    {
        public string ColumnOne { get; } = "Вериф шт.";
        public string ColumnTwo { get; } = "Цсб шт.";
        public string ColumnThree { get; } = "Процент";
        public List<DataLine> GetDataLines()
        {
            var _datalines = new List<DataLine>();
            var listPG = GetListPG();

            foreach (var line in listPG) _datalines.Add(GetDataLine(line));

            return _datalines;
        }

        List<string> GetListPG()
        {
            var list = Base.Loadgrid($@" use SMDCOMPONETS SELECT  distinct(User_INSPect)
                      FROM [SMDCOMPONETS].[dbo].[AOIresult]
                      where inspectionDate > '{DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd")}'  and InspectResult != '' and User_INSPect not in ( '','tehnolog')");

            if (list == null) return new List<string>();
            List<string> result = new List<string>();

            for (int i = 0; i < list.Tables[0].Rows.Count; i++) result.Add(list.Tables[0].Rows[i].ItemArray[0].ToString());

            return result;

        }

        DataLine GetDataLine(string _pg)
        {
            DataLine dataLine = new DataLine(_pg);
            dataLine.Statistics = GetStatistics(_pg);
            dataLine.Tops = GetTOPs(_pg);

            return dataLine;
        }

        Statistics GetStatistics(string _pg)
        {
            var _countAOI = Base.SelectString($@" use SMDCOMPONETS select count(1) from ( SELECT  distinct PCBnumber
                        FROM[SMDCOMPONETS].[dbo].[AOIresult]
                        where inspectionDate > '{DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd")}'   and InspectResult != '' and User_INSPect = '{_pg}' 
                        and  UserInspectionResult = 'OK') tt");

            var _countFas = Base.SelectString($@" use SMDCOMPONETS select count(1) from ( SELECT distinct a.PCBnumber
                            FROM [SMDCOMPONETS].[dbo].[AOIresult] a
                            left join fas.dbo.M_Repair_Table r on r.Barcode = a.PCBnumber and a.CIRNAME = r.Position
                            where inspectionDate > '{DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd")}'   and InspectResult != '' and User_INSPect = '{_pg}'
                            and r.Barcode is not null and position <> '' and  UserInspectionResult = 'OK') tt");

            Statistics statistics = new Statistics(_countAOI, _countFas);
            return statistics;
        }

        List<TOP> GetTOPs(string _pg)
        {
            var list = Base.Loadgrid($@"  select tt.Position,tt.ProgramName, count(1) Кол_во from (SELECT distinct r.Barcode, r.Position, a.ProgramName
                          FROM [SMDCOMPONETS].[dbo].[AOIresult] a
                          left join fas.dbo.M_Repair_Table r on r.Barcode = a.PCBnumber and a.CIRNAME = r.Position
                          where inspectionDate > '{DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd")}'   and InspectResult != '' and User_INSPect ='{_pg}'
                          and r.Barcode is not null and  UserInspectionResult = 'OK'  and position <> '' ) tt group by tt.Position, tt.ProgramName
                          order by count(1) desc");

            List<TOP> listTops = new List<TOP>();

            for (int i = 0; i < list.Tables[0].Rows.Count; i++)
            {
                if (i > 2) break;

                TOP _top = new TOP()
                {
                    Count = list.Tables[0].Rows[i].ItemArray[2].ToString(),
                    PG = list.Tables[0].Rows[i].ItemArray[1].ToString(),
                    CIR = list.Tables[0].Rows[i].ItemArray[0].ToString()
                };

                listTops.Add(_top);
            }

            return listTops;
        }
    }
}