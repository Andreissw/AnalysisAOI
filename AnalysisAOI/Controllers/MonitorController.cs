using AnalysisAOI.Class;
using AnalysisAOI.Class.Inspection;
using AnalysisAOI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AnalysisAOI.Controllers
{
    public class MonitorController : Controller
    {
        // GET: Monitor
        public ActionResult Index()
        {
            if (!IsSend()) return View();

            string head = $"Статистика АОИ накопительный за месяц | {DateTime.UtcNow.AddHours(2).AddMonths(-1).ToString("dd.MM.yy")} - {DateTime.UtcNow.AddHours(2).ToString("dd.MM.yy")} ";
            AOIResult AOI = new AOIResult(new List<AOI> { new AOI("Успешная инспекция") }, head);
            AOI.SendEmail();

            head = $"Статистика монтажников накопительный за месяц | {DateTime.UtcNow.AddHours(2).AddMonths(-1).ToString("dd.MM.yy")} - {DateTime.UtcNow.AddHours(2).ToString("dd.MM.yy")} ";
            AOIResult AOIFalse = new AOIResult(new List<AOI> { new AOI("Ложный АОИ"), new AOI("Дефект АОИ") }, head);
            AOIFalse.SendEmail();
            UpdateDate();

            return View();
        }

        bool IsSend()
        {
            using (var fas = new FASEntities())
            {
                var result = fas.AOI_Statistics.Select(c=>c.Date).FirstOrDefault();
                if (result == null)
                    return true;

                if (result.Value.ToString("dd.MM.yyyy") == DateTime.UtcNow.AddHours(2).ToString("dd.MM.yyyy"))
                    return false;

                if (DateTime.UtcNow.AddHours(2).ToString("HH") == "08") return true;

                return false;
            }
        }

        void UpdateDate()
        {
            using (var fas = new FASEntities())
            {
                var result = fas.AOI_Statistics.FirstOrDefault();
                result.Date = DateTime.UtcNow.AddHours(2);
                fas.SaveChanges();
            }
        }
    }
}