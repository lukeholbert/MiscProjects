using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CS422;

namespace HW3
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("WebServer starting...");
      WebServer.Start(4220, 60);

      WebServer.AddService(new FilesWebService(StandardFileSystem.Create(@"C:\Users\Luke\Desktop\100GOPRO")));
      Console.ReadLine();

      WebServer.Stop();
    }
  }
}
