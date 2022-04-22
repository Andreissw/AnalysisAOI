namespace AnalysisAOI.Class.Inspection.Line
{
    public class Statistics
    {
        public Statistics(int _countAOI, int _countFAS)
        {
            CountAOI = _countAOI;
            CountFAS = _countFAS;

            if (CountAOI == 0 & CountFAS == 0) return;



            Procent = decimal.Parse((100 - (float)_countFAS / (float)_countAOI * 100).ToString("##.##"));
            Procent = CountFAS == 0 ? 100 : Procent;

        }

        public int CountAOI { get; }
        public int CountFAS { get; }
        public decimal Procent { get; }
    }
}