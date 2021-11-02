using AnalysisAOI.ClassFolder;
using AnalysisAOI.ClassFolder.AOIFolder;
using AnalysisAOI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AnalysisAOI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Indexx()
        {
            return View();
        }

        void RemoveData(string ip)
        {
            Omron.SelectString($@"use fas delete [FAS].[dbo].[SPY_Table] where [IpAdress] = '{ip}'");
        }

        public ActionResult GetData(string date_st, string date_end, string time_st, string time_end)
        {
            string ip = HttpContext.Request.UserHostAddress;
            var T = ParseDate(date_st,date_end,time_st,time_end);

            if (T == null)
            {
                //Дата ДО не может быть меньше ОТ
            }

            //===============================================================================================
            //Выгрузка из OMRON
            RemoveData(ip);
            Info info = new Info();
           var result =  Omron.LoadGridOmron($@"SELECT L.SYS_MACHINE_NAME, ONE.BOARD_BARCODE, MOD.PG_NAME
                FROM PRISM.INSP_RESULT_SUMMARY_INFO one  LEFT JOIN COMP_RESULT_INFO two ON one.INSP_ID = two.INSP_ID
                LEFT JOIN COMP_INFO three ON one.PG_ITEM_ID = three.PG_ITEM_ID AND two.COMP_ID = three.COMP_ID  
                LEFT JOIN USR_INSP_RESULT_NAME six ON one.INSP_RESULT_CODE = six.USR_INSP_RESULT_CODE 
                LEFT JOIN CIR_INFO four ON three.CIR_ID = four.CIR_ID AND  three.PG_ITEM_ID = four.PG_ITEM_ID
                Left JOIN PG_INFO mod ON one.PG_ITEM_ID = mod.PG_ITEM_ID
                INNER JOIN CONNECTION_MACHINE_INFO L ON one.SYS_MACHINE_NAME = l.SYS_MACHINE_NAME
                WHERE INSP_END_DATE BETWEEN TO_DATE(CONCAT('{T.Date_St}','{T.Time_St}'),'DD.MM.YY HH24:MI:SS') AND TO_DATE(CONCAT('{T.Date_End}','{T.Time_End}'),'DD.MM.YY HH24:MI:SS')       
                AND six.LANG_ID = 3  AND L.SYS_MACHINE_NAME NOT IN ('VT-S530-1087', 'VP9000-MC1') AND ONE.INSP_RESULT_CODE = 0");

            AddFasTable(result); //Добавляем в таблицу FAS данные с Oracle

            //===============================================================================================
            //Общий SPY

            
            var countrem = Omron.SelectString($@"use FAS select  count( distinct barcode) barcode from M_Repair_Table r
                                      where Barcode in (SELECT barcode
                                      FROM [FAS].[dbo].[SPY_Table]) and RepairCode is not null and RepairCode != 'Y'");

            var countAOI = Omron.SelectString($@"use fas SELECT count(distinct(barcode))  FROM [FAS].[dbo].[SPY_Table]");

            //float z = 0;
            //if (!float.TryParse(countrem, out float k) || !float.TryParse(countAOI, out z))
            //{
            //    //Ошибка преобразования
            //}

            var _aoiinfo = GetAOIInfo(countrem, countAOI);

            //===============================================================================================
            //SPY по линии

            var LineSpy = Omron.Loadgrid($@"use fas SELECT distinct linename Line, count(linename) PassAOI, count(rep.barcode) RemFAS
                                              FROM [FAS].[dbo].[SPY_Table] s
                                              left join M_Repair_Table rep on s.barcode = rep.Barcode
                                              where IpAdress = '{ip}'
                                              group by linename");

            var _linespy = GetLines(LineSpy);

            //===============================================================================================
            //Топ по дефектам
            var TopDefects = Omron.Loadgrid($@"use fas select distinct [Линия], [КодДефекта], count(1)'Кол_во'
                                                from( SELECT  linename 'Линия'
                                                ,(select concat(r.NameCode,'-',r.DescriptionCode) from FAS_RepairCode r where r.NameCode = rep.RepairCode) 'КодРемонта'
                                                ,(select concat(d.NameCode,'-',d.DescriptionCode) from FAS_DefectCode d where d.NameCode = rep.DefectCode) 'КодДефекта'
                                                ,Position 'Позиция'
                                                  FROM [FAS].[dbo].[SPY_Table] s
                                                  left join M_Repair_Table rep on s.barcode = rep.Barcode where IpAdress = '{ip}' ) tt where [КодДефекта] is not null 
                                                  group by [Линия],[КодДефекта]
                                                  order by [Линия], [Кол_во] desc");

            var _topdefects = GetDefects(TopDefects);

            //===============================================================================================
            //Топ по ремонтам
            var TopRepair = Omron.Loadgrid($@"use fas select distinct Линия, КодРемонта, count(1)Кол_во
                                                from( SELECT   linename Линия
                                                ,(select concat(r.NameCode,'-',r.DescriptionCode) from FAS_RepairCode r where r.NameCode = rep.RepairCode) КодРемонта
                                                ,(select concat(d.NameCode,'-',d.DescriptionCode) from FAS_DefectCode d where d.NameCode = rep.DefectCode) КодДефекта
                                                ,Position Позиция
                                                  FROM [FAS].[dbo].[SPY_Table] s
                                                  left join M_Repair_Table rep on s.barcode = rep.Barcode where IpAdress = '{ip}') tt where КодДефекта is not null
                                                  group by Линия,КодРемонта 
                                                  order by Линия, Кол_во desc");

            var _toprepair = Getrepair(TopRepair);


            //===============================================================================================
            //ТОП по позициям
            var TOPCIR = Omron.Loadgrid($@"use fas 
                                            select Линия,Позиция,Кол_во from (select distinct Линия, Позиция, count(1)Кол_во, ROW_NUMBER() over( partition by Линия order by Линия,count(1) desc) num  
                                            from( SELECT 
                                             linename Линия
                                            ,(select concat(r.NameCode,'-',r.DescriptionCode) from FAS_RepairCode r where r.NameCode = rep.RepairCode) КодРемонта
                                            ,(select concat(d.NameCode,'-',d.DescriptionCode) from FAS_DefectCode d where d.NameCode = rep.DefectCode) КодДефекта
                                            ,Position Позиция

                                              FROM [FAS].[dbo].[SPY_Table] s
                                              left join M_Repair_Table rep on s.barcode = rep.Barcode where IpAdress = '{ip}') tt 
                                              where КодДефекта is not null and Позиция <> '' 
                                               group by Линия,Позиция ) ttt
                                               where num <=10 ");

            var _topcir = GetCIR(TOPCIR);
            //===============================================================================================
            //ТОП по дефектам и позициям

            var TOPCIRDefects = Omron.Loadgrid($@"use fas 
                                            select Линия,Позиция,КодДефекта,Кол_во from (select distinct Линия, Позиция, КодДефекта, count(1)Кол_во, ROW_NUMBER() over( partition by Линия order by Линия,count(1) desc) num  
                                            from( SELECT 
                                             linename Линия
                                            ,(select concat(r.NameCode,'-',r.DescriptionCode) from FAS_RepairCode r where r.NameCode = rep.RepairCode) КодРемонта
                                            ,(select concat(d.NameCode,'-',d.DescriptionCode) from FAS_DefectCode d where d.NameCode = rep.DefectCode) КодДефекта
                                            ,Position Позиция

                                              FROM [FAS].[dbo].[SPY_Table] s
                                              left join M_Repair_Table rep on s.barcode = rep.Barcode where IpAdress = '{ip}') tt 
                                              where КодДефекта is not null and Позиция <> '' 
                                               group by Линия,Позиция,КодДефекта ) ttt
                                               where num <=10 ");


            var _topcirdefects = GetCIRDefects(TOPCIRDefects);
            //===============================================================================================

            var TOPCIRDefectsRepair = Omron.Loadgrid($@"use fas select Линия,Позиция, КодДефекта,КодРемонта , Кол_во from 
                                            (select distinct Линия, Позиция,КодДефекта , КодРемонта ,count(1)Кол_во, ROW_NUMBER() over( partition by Линия order by Линия,count(1) desc) num  

                                            from( SELECT 
                                             linename Линия
                                            ,(select concat(r.NameCode,'-',r.DescriptionCode) from FAS_RepairCode r where r.NameCode = rep.RepairCode) КодРемонта
                                            ,(select concat(d.NameCode,'-',d.DescriptionCode) from FAS_DefectCode d where d.NameCode = rep.DefectCode) КодДефекта
                                            ,Position Позиция

                                              FROM [FAS].[dbo].[SPY_Table] s
                                              left join M_Repair_Table rep on s.barcode = rep.Barcode where IpAdress = '{ip}') tt 
                                              where КодДефекта is not null and Позиция <> '' 
                                               group by Линия,Позиция,КодДефекта,КодРемонта ) ttt
                                               where num <=10  ");

            var _topcirdefectsrep = GetCIRDefectsRepair(TOPCIRDefectsRepair);

            _aoiinfo.LineSPY = _linespy;
            _aoiinfo.TopDefects = _topdefects;
            _aoiinfo.TopRepair = _toprepair;
            _aoiinfo.TopCIR = _topcir;
            _aoiinfo.TopCIRDefects = _topcirdefects;
            _aoiinfo.TopCirDefectsRepair = _topcirdefectsrep;
            RemoveData(ip);
            return PartialView(_aoiinfo);
        }

        List<AOIDefects> GetCIRDefectsRepair(DataSet data)
        {
            List<AOIDefects> aOIDefects = new List<AOIDefects>();

            foreach (DataRow item in data.Tables[0].Rows)
            {
                AOIDefects defects = new AOIDefects()
                {
                    Line = item.ItemArray[0].ToString(),
                    CIR = item.ItemArray[1].ToString(),
                    DefectCode = item.ItemArray[2].ToString(),
                    RepairCode = item.ItemArray[3].ToString(),
                    Count = item.ItemArray[4].ToString()

                };
                aOIDefects.Add(defects);
            }

            return aOIDefects;

        }

        List<AOIDefects> GetCIRDefects(DataSet data)
        {
            List<AOIDefects> aOIDefects = new List<AOIDefects>();

            foreach (DataRow item in data.Tables[0].Rows)
            {
                AOIDefects defects = new AOIDefects()
                {
                    Line = item.ItemArray[0].ToString(),
                    CIR = item.ItemArray[1].ToString(),
                    DefectCode = item.ItemArray[2].ToString(),
                    Count = item.ItemArray[3].ToString()

                };
                aOIDefects.Add(defects);
            }

            return aOIDefects;

        }

        List<AOIDefects> GetCIR(DataSet data)
        {
            List<AOIDefects> aOIDefects = new List<AOIDefects>();

            foreach (DataRow item in data.Tables[0].Rows)
            {
                AOIDefects defects = new AOIDefects()
                {
                    Line = item.ItemArray[0].ToString(),
                    CIR = item.ItemArray[1].ToString(),
                    Count = item.ItemArray[2].ToString()

                };
                aOIDefects.Add(defects);
            }

            return aOIDefects;

        }

        List<AOIDefects> Getrepair(DataSet data)
        {
            List<AOIDefects> aOIDefects = new List<AOIDefects>();

            foreach (DataRow item in data.Tables[0].Rows)
            {
                AOIDefects defects = new AOIDefects()
                {
                    Line = item.ItemArray[0].ToString(),
                    RepairCode = item.ItemArray[1].ToString(),
                    Count = item.ItemArray[2].ToString()

                };
                aOIDefects.Add(defects);
            }

            return aOIDefects;

        }

        List<AOIDefects> GetDefects(DataSet data)
        {
            List<AOIDefects> aOIDefects = new List<AOIDefects>();

            foreach (DataRow item in data.Tables[0].Rows)
            {
                AOIDefects defects = new AOIDefects()
                {
                    Line = item.ItemArray[0].ToString(),
                    DefectCode = item.ItemArray[1].ToString(),
                    Count = item.ItemArray[2].ToString()

                };
                aOIDefects.Add(defects);
            }

            return aOIDefects;

        }

        AOIInfo GetAOIInfo(float k, float z)
        {
            AOIInfo AOI = new AOIInfo();
            AOI.RemCount = k;
            AOI.AOIPassCount = z;
            AOI.SPY = (100 - (k / z * 100)).ToString("##.##");
            return AOI;
        }


        List<LineSPY> GetLines(DataSet data)
        {
            List<LineSPY> LineSPYS = new List<LineSPY>();

            foreach (DataRow item in data.Tables[0].Rows)
            {
                LineSPY lineSPY = new LineSPY();
                lineSPY.LineName = item.ItemArray[0].ToString();
                lineSPY.CountAOI = float.Parse(item.ItemArray[1].ToString());
                lineSPY.CountRem = float.Parse(item.ItemArray[2].ToString());
                lineSPY.SPY = (100 - (lineSPY.CountRem / lineSPY.CountAOI * 100)).ToString("##.##");
                LineSPYS.Add(lineSPY);
            }

            return LineSPYS;
        }

        void AddFasTable(DataSet data)
        {
            var date = DateTime.UtcNow.AddHours(2).ToString("yyyy-MM-dd HH:mm:ss");
            string ip = HttpContext.Request.UserHostAddress;

           var re = Parallel.For(0, data.Tables[0].Rows.Count, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, x =>
            {
                var p = data.Tables[0].Rows[x];
                var i = p.ItemArray;
                Omron.SelectString($@" use FAS insert into [FAS].[dbo].[SPY_Table]
                                  (Barcode,LineName,ModelName,DateLoad,[IpAdress]) values
                                  ('{i[1]}','{i[0]}','{i[2]}','{date}','{ip}')");

            });
            
        }

        TimesOracle ParseDate(string date_st, string date_end, string time_st, string time_end)
        {
            TimesOracle timesOracle = new TimesOracle();

            var st = DateTime.Parse(date_st + " " + time_st).AddHours(-2);
            var end = DateTime.Parse(date_end + " " + time_end).AddHours(-2);

            if (end < st)
            {
                return null;
            }

            timesOracle.Date_St = st.ToString("dd.MM.yyyy");
            timesOracle.Date_End = end.ToString("dd.MM.yyyy");
            timesOracle.Time_St = st.ToString("HH:mm:ss");
            timesOracle.Time_End = end.ToString("HH:mm:ss");

            return timesOracle;

        }

    }
}