using AnalysisAOI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace AnalysisAOI.Class
{
    public class Message
    {
        public Message(string Content, string Subject)
        { 
            this.Content = Content;
            this.Subject = Subject;
            RunEmail();
        }

        FASEntities fas = new FASEntities();
        string Content;
        string Subject;

        public void RunEmail()
        {
            var view = AlternateView.CreateAlternateViewFromString(Content, Encoding.UTF8, MediaTypeNames.Text.Html);
            var listemails = fas.EP_Email.Where(c => c.Type == "StatisticsAOI").Select(c => c.Email).ToList();


            using (var client = new SmtpClient("mail.technopolis.gs", 25)) // Set properties as needed or use config file
            using (MailMessage message = new MailMessage()
            {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                Subject = Subject,
                //Subject = $"Статистика АОИ за период {DateTime.UtcNow.AddHours(2).AddMonths(-1).ToString("dd.MM.yy")} - {DateTime.UtcNow.AddHours(2).ToString("dd.MM.yy")}",
                SubjectEncoding = Encoding.UTF8,

            })

            {
                message.AlternateViews.Add(view);
                message.From = new MailAddress("reporter@dtvs.ru", "ROBOT");
                //message.From = new MailAddress("volodin1971@gmail.com", "Чувак");
                foreach (var item in listemails) message.CC.Add(item);
                message.CC.Add("a.volodin@dtvs.ru");
             

                client.Send(message);

            }
        }
    }
}