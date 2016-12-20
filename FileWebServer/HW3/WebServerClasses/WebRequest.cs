using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

namespace CS422
{
  public class WebRequest
  {
    NetworkStream _netStream;
    byte[] _buf;
    Dictionary<string, string> _headers;
    long _length;
    string _method;
    string _uri;
    string _version;
    Stream _body;

    public string Method => _method;
    public string URI => _uri;
    public string Length => _length.ToString();
    public long Len => _length;
    public Dictionary<string, string> Headers => _headers;
    public Stream Body => _body;

    public WebRequest(NetworkStream ns, byte[] newBuf)
    {
      _netStream = ns;
      _buf = newBuf;
      _length = -1;
      _headers = ParseHeaders();
      CreateBodyStream();
    }

    private Dictionary<string, string> ParseHeaders()
    {
      Dictionary<string, string> dict = new Dictionary<string, string>();
      string req = Encoding.ASCII.GetString(_buf).Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None)[0];

      string[] headers = req.Split("\r\n".ToCharArray());

      string[] items = headers[0].Split(' ');
      _method = items[0];
      _uri = items[1];
      _version = items[2];

      for (int i = 2; i < headers.Length - 3; i = i + 2)
      {
        dict.Add(headers[i].Split(':')[0], headers[i].Split(':')[1]);
      }

      Regex cl = new Regex("content-length", RegexOptions.IgnoreCase);

      if (cl.IsMatch(String.Join(" ", dict.Keys)))
      {
        _length = long.Parse(dict[cl.Match(String.Join(" ", dict.Keys)).ToString()]);
      }

      return dict;
    }

    void CreateBodyStream()
    {

      int index = Encoding.ASCII.GetString(_buf).Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None)[0].Length + 4;
      byte[] newBuf = _buf.Skip(index).ToArray();

      MemoryStream ms = new MemoryStream(newBuf);

      if (_length != -1)
      {
        _body = new ConcatStream(ms, _netStream, _length);
      }
      else
      {
        _body = new ConcatStream(ms, _netStream);
      }
    }

    public void WriteNotFoundResponse(string pageHTML)
    {
      const string DefaultTemplate = "HTTP/1.1 404 Not Found\r\n" +
                                           "Content-Type: text/html\r\n" +
                                           "Content-Length: {0}\r\n" +
                                           "\r\n\r\n" + "{1}";

      string response = String.Format(DefaultTemplate, (pageHTML.Length * 2).ToString(), pageHTML);
      _netStream.Write(Encoding.ASCII.GetBytes(response), 0, response.Length);
    }

    public bool WriteHTMLResponse(string htmlString)
    {
      const string DefaultTemplate = "HTTP/1.1 200 OK\r\n" +
                                           "Content-Type: text/html\r\n" +
                                           "Content-Length: {0}\r\n" +
                                           "\r\n\r\n" + "{1}";

      string response = String.Format(DefaultTemplate, (htmlString.Length * 2).ToString(), htmlString);
      _netStream.Write(Encoding.ASCII.GetBytes(response), 0, response.Length);
      return true;
    }

    public bool WriteDirectResponse(byte[] buf, int offset, int length)
    {
      try
      {
        _netStream.Write(buf, offset, length);
      }
      catch
      {
        return false;
      }

      return true;
    }
  }
}
