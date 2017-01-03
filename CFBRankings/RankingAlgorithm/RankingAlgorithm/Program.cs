using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace RankingAlgorithm
{
  class Program
  {
    static void Main(string[] args)
    {
      Team test = JsonConvert.DeserializeObject<Team>(File.ReadAllText(@"C:\Users\Luke\Desktop\MiscProjects\CFBRankings\RankingAlgorithm\TeamData.json"));
    }
  }
}
