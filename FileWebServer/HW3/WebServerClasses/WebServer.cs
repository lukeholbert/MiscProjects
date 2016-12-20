using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace CS422
{
  public class WebServer
  {
    static BlockingCollection<TcpClient> _collection;
    static int _threads;
    static TcpListener _listener;
    static List<WebService> _services;
    const int singleLineCheck = 2048;
    const int doubleLineCheck = 102400;

    public WebServer()
    {

    }

    public static bool Start(int port, int numThreads)
    {
      _listener = new TcpListener(IPAddress.Any, port);
      _listener.Start();
      _services = new List<WebService>();
     
      if (numThreads <= 0)
      {
        _threads = 64;
      }
      else
      {
        _threads = numThreads;
      }

      _collection = new BlockingCollection<TcpClient>();
      for (int i = 0; i < _threads; i++)
      {
        Thread t = new Thread(ThreadWork);
        t.Start();
      }

      Thread listen = new Thread(Listen);
      listen.Start();

      return true;
    }

    private static void Listen ()
    {
      TcpClient client;

      while(_listener != null)
      {
        try
        {
          client = _listener.AcceptTcpClient();
          _collection.Add(client);
        }
        catch
        {
          continue;
        }
      }
    }

    private static void ThreadWork()
    {
      while (true)
      {
        TcpClient client = _collection.Take();
        if (client == null)
        {
          return;
        }
        WebRequest request = BuildRequest(client);

        if(request == null)
        {
          client.Close();
          continue;
        }

        foreach(var service in _services)
        {
          if(request.URI.StartsWith(service.ServiceURI))
          {
            service.Handler(request);
            return;
          }
        }
        // No valid handler
        request.WriteNotFoundResponse(request.URI);
      }
    }

    private static bool ReadRequest(TcpClient client, ref byte[] buf)
    {
      NetworkStream ns = client.GetStream();
      ns.ReadTimeout = 1000;
      Stopwatch timer = new Stopwatch();
      timer.Start();
      int numBytes = 0;
      int totalBytes = 0;

      try
      {
        numBytes = ns.Read(buf, 0, doubleLineCheck);
      }
      catch
      {
        return false;
      }

      // While still reading OR reached the double line break (body)
      while (numBytes != 0 || !Encoding.ASCII.GetString(buf).Contains("\r\n\r\n"))
      {
        string test = Encoding.ASCII.GetString(buf);

        //if (timer.ElapsedMilliseconds > 10000)
        //{
        //  return false;
        //}

        totalBytes += numBytes;
        if (!CheckRequest(buf, totalBytes, false))
        {
          return false;
        }
        if (!ns.DataAvailable)
        {
          break;
        }
        try
        {
          numBytes = ns.Read(buf, totalBytes, doubleLineCheck - totalBytes);
        }
        catch
        {
          return false;
        }
      }

      return CheckRequest(buf, totalBytes, true);
    }

    private static bool CheckRequest(byte[] request, int bytes, bool finished)
    {
      Regex version = new Regex(@"HTTP/(\d+\.\d+)");

      if ((Encoding.ASCII.GetString(request, 0, bytes < 4 ? bytes : 4) != "GET " && Encoding.ASCII.GetString(request, 0, bytes < 4 ? bytes : 4) != "PUT ") || (finished && bytes < 4))
      {
        return false;
      }
      // If finished and no version match
      if (finished && !version.IsMatch(Encoding.ASCII.GetString(request)))
      {
        return false;
      }
      // Otherwise, if version match, check for valid version
      else if (version.IsMatch(Encoding.ASCII.GetString(request, 0, bytes)))
      {
        // parse out version and check for validity
        Regex deci = new Regex(@"\d+\.\d+");
        if (deci.Match(version.Match(Encoding.ASCII.GetString(request, 0, bytes)).ToString()).ToString() != "1.1")
        {
          return false;
        }
      }

      #region HW7 Size Checks
      // If no single line break && size greater than 2048
      if (!Encoding.ASCII.GetString(request).Contains("\r\n") && Encoding.ASCII.GetString(request).IndexOf("\r\n") != Encoding.ASCII.GetString(request).IndexOf("\r\n\r\n") && bytes >= singleLineCheck)
      {
        return false;
      }

      // If no double line break && size greater than 100*1024
      if(!Encoding.ASCII.GetString(request).Contains("\r\n\r\n") && bytes >= doubleLineCheck)
      {
        return false;
      }
      #endregion 

      return true;
    }

    private static WebRequest BuildRequest(TcpClient client)
    {
      byte[] buf = new byte[doubleLineCheck];

      // Read request is valid
      if (ReadRequest(client, ref buf))
      {
        // Create WebRequest item
        return new WebRequest(client.GetStream(), buf);
      }
      // Invalid request
      else
      {
        client.GetStream().Close();
        client.Close();
        return null;
      }
    }

    public static void Stop()
    {
      for (int i = 0; i < _threads; i++)
      {
        _collection.Add(null);
      }

      _listener.Stop();
      _listener = null;
    }

    public static void AddService(WebService service)
    {
      _services.Add(service);
    }
  }
}
