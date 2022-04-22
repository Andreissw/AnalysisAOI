using AnalysisAOI.Class.Inspection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnalysisAOI.Class
{
    public class AOIResult
    {
        public AOIResult(List<AOI> listAOI, string _head)
        {
            ListAOI = listAOI;
            Head = _head;           
        }
     
        string Head { get; } 

        public List<AOI> ListAOI { get; }

        public void SendEmail()
        {
            Message message = new Message(GetHtml(),Head);
        }
        string Getline(AOI _aoi)
        {
            string line = "";

            line += $@"    
            <thread>
           <tr>
             <th colspan=""3"">
               {_aoi.Head}
             </th>
           </tr>
          </thread>";

            foreach (var item in _aoi.Lines.OrderBy(c=>c.Statistics.Procent))
            {
                string _tops = "";

                foreach (var i in item.Tops)
                {
                    _tops += $@" <tr>
                                  <td>{i.CIR}</td>
                                    <td>{i.PG}</td>
                                  <td>{i.Count}</td>                   
                                </tr>";
                }

                line += $@"
        
<tr>
             <td id = ""PG"">
               {item.ProgrammName}
             </td>  
             <td>
               <table id=""InfoT"" >
                 <tr>
                   <th> {_aoi.Query.ColumnOne} </th>
                   <th> {_aoi.Query.ColumnTwo}</th>
                   <th> {_aoi.Query.ColumnThree} </th>
                 </tr>
                 
                 <tr>
                   <td>{item.Statistics.CountAOI}</td>
                   <td>{item.Statistics.CountFAS}</td>
                   <td>{item.Statistics.Procent}%</td>
                 </tr>
                 
               </table>
             </td>
             <td>
                <table id=""InfoT"" >
                 <tr>
                   <th> Позиция </th>
                    <th> PG </th>
                   <th> шт.</th>                   
                 </tr>                 
                 {_tops}                 
               </table>
             </td>
           </tr>";
            }

            return line;
        }

        string GetHtml()
        {
            string lines = "";
            foreach (var item in ListAOI)   lines += Getline(item);

            return $@"<style>
                    #HeadT {{
                  width:80%;  
                }}

                #HeadT th{{
                  text-align: center;
                  font-size: 2.4em;  
                  background-color:lightgray;
                  padding: 0.4%;
                }}

                #T {{
                  width:80%;
                  border: 1px solid black;
                  padding:0.3%;
                }}

                #T th{{
                  text-align: left;
                  font-size: 2.1em;  
                  background-color: gold; 
                }}

                #T td{{
                  border-bottom: 0.2px solid gray;
                   border-right: 0.2px solid gray;
                }}


                #InfoT {{
                  width:95%;
                  margin-left:3%;
                  font-size:1.8em;
                }}
                #InfoT th {{
                  text-align: center;
                  font-size: 1.2em;   
                  background-color:khaki;
                }}

                #InfoT td {{
                  text-align: center;
                  font-size: 1.2em;   
                   border: 0.2px solid gray;
                }}


                #PG{{
                  font-size: 2.4em;
                  width: 30%;
                  padding-left:0.4%;
                  border-right: 0.5px dashed gray;
                  background-color: lightgrey;
                }} 
                </style>

                <html>
                <table id=""HeadT"">
  <thread>
      <tr>
        <th>Статистика АОИ накопительный за месяц | {DateTime.UtcNow.AddHours(2).AddMonths(-1).ToString("dd.MM.yy")} - {DateTime.UtcNow.AddHours(2).ToString("dd.MM.yy")} </th>
      </tr>
  </thread>
  
  <tbody>
    <tr>
      <table id = ""T"">  
         {lines}
       </table>      
    </tr>  
  
  </tbody>  
  
</table>

 </html>";

        }

    }
}