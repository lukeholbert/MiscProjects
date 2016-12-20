using System;
using System.Text;
using System.IO;

namespace CS422
{
  public class FilesWebService : WebService
  {
    private readonly FileSys422 r_fs;
    public override string ServiceURI => "/files";
    private bool m_allowUploads;

    //Constructor
    public FilesWebService(FileSys422 fs)
    {
      r_fs = fs;
      m_allowUploads = true;
    }

    //Methods
    public override void Handler(WebRequest req)
    {
      if (!req.URI.StartsWith(ServiceURI))
      {
        throw new InvalidOperationException();
      }

      // File Upload
      if(req.Method.StartsWith("PUT"))
      {
        UploadFile(req);
        return;
      }

      if(!req.Method.StartsWith("GET"))
      {
        return;
      }

      if (req.URI == "/files" || req.URI == "/files/")
      {
        RespondWithList(r_fs.GetRoot(), req);
        return;
      }

      string[] pieces = req.URI.Substring(ServiceURI.Length).Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

      if (pieces == null || pieces.Length == 0)
      {
        req.WriteNotFoundResponse("404: Path error.");
        return;
      }

      Dir422 dir = r_fs.GetRoot();

      for (int i = 0; i < pieces.Length - 1; i++)
      {
        string piece = pieces[i];
        dir = dir.GetDir(piece);

        if (dir == null)
        {
          req.WriteNotFoundResponse("404: Path error.");
          return;
        }
      }

      File422 file = dir.GetFile(PercentDecoding(pieces[pieces.Length - 1]));

      if (file != null)
      {
        RespondWithFile(file, req);
        return;
      }

      dir = dir.GetDir(pieces[pieces.Length - 1]);

      if (dir == null)
      {
        req.WriteNotFoundResponse("404: Path error.");
        return;
      }

      RespondWithList(dir, req);
    }

    private void RespondWithList(Dir422 dir, WebRequest req)
    {
      req.WriteHTMLResponse(BuildDirHTML(dir));
    }

    string BuildDirHTML(Dir422 directory)
    {
      // Build an HTML file listing 
      var sb = new StringBuilder("<html>");
      // We'll need a bit of script if uploading is allowed
      if (m_allowUploads)
      {
        sb.AppendLine(
          @"<script>
          function selectedFileChanged(fileInput, urlPrefix)
          { 
            document.getElementById('uploadHdr').innerText = 'Uploading ' + fileInput.files[0].name + '...';
            
            // Need XMLHttpRequest to do the upload
            if (!window.XMLHttpRequest)
            { 
              alert('Your browser does not support XMLHttpRequest. Please update your browser.');
              return;
            }

            // Hide the file selection controls while we upload
            var uploadControl = document.getElementById('uploader');
            if (uploadControl)
            {
              uploadControl.style.visibility = 'hidden';
            }
    
            // Build a URL for the request
            if (urlPrefix.lastIndexOf('/') != urlPrefix.length - 1)
            {
              urlPrefix += '/';
            }
            var uploadURL = urlPrefix + fileInput.files[0].name;

            // Create the service request object
            var req = new XMLHttpRequest();
            req.open('PUT', uploadURL);
            req.onreadystatechange = function()
            {
              document.getElementById('uploadHdr').innerText = 'Upload (request status == ' + req.status + ')';
              // Un-comment the line below and comment-out the line above if you want the page to
              // refresh after the upload
              //location.reload();
            };
            req.send(fileInput.files[0]);
          }
          </script>
          ");
      }

      sb.Append("   <h1>Files</h1>");

      //Files first
      String dirPath = "";
      Dir422 temp = directory;
      while (temp.Parent != null)
      {
        dirPath = temp.Name + "/" + dirPath;
        temp = temp.Parent;
      }
      dirPath = ServiceURI + "/" + dirPath;

      var files = directory.GetFiles();

      foreach (File422 file in files)
      {
        string href = dirPath + file.Name;
        sb.AppendFormat("<a href=\"{0}\">{1}</a>   <br>", href, file.Name);
      }

      sb.Append("<h1>Folders</h1>");

      //General Note: Don't forget percent encoding and decoding.
      foreach (Dir422 dir1 in directory.GetDirs())
      {
        string href = dirPath + dir1.Name;
        sb.AppendFormat("<a href=\"{0}\">{1}</a>   <br>", href, dir1.Name);
      }

      // If uploading is allowed, put the uploader at the bottom
      if (m_allowUploads)
      {
        sb.AppendFormat(
        "<hr><h3 id='uploadHdr'>Upload</h3><br>" +
        "<input id=\"uploader\" type='file' " +
        "onchange='selectedFileChanged(this,\"{0}\")' /><hr>", dirPath.TrimEnd('/'));
      }
      sb.Append("</html>");

      return sb.ToString();
    }

    private void UploadFile(WebRequest req)
    {
      string[] pieces = req.URI.Substring(ServiceURI.Length).Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

      if (pieces == null || pieces.Length == 0)
      {
        req.WriteNotFoundResponse("404: Path error.");
        return;
      }

      Dir422 dir = r_fs.GetRoot();

      for (int i = 0; i < pieces.Length - 1; i++)
      {
        string piece = pieces[i];
        dir = dir.GetDir(piece);

        if (dir == null)
        {
          req.WriteNotFoundResponse("404: Path error.");
          return;
        }
      }

      File422 file = dir.GetFile(PercentDecoding(pieces[pieces.Length - 1]));

      if (file != null)
      {

        req.WriteHTMLResponse("<html> File Already Exists! </html>");
        return;
      }

      File422 newFile = dir.CreateFile(PercentDecoding(pieces[pieces.Length - 1]));
      FileStream str = (newFile.OpenReadWrite() as FileStream);
      Stream reqStr = req.Body; 
      byte[] buf = new byte[4096];
      long len = req.Len;

      if(len < 4096)
      {
        reqStr.Read(buf, 0, (int)len);
        str.Write(buf, 0, (int)len);
        str.Close();
        return;
      }
      
      int count = reqStr.Read(buf, 0, 4096);
      int totalRead = count;

      while (count != 0 && totalRead < len)
      {
        str.Write(buf, 0, count);
        buf = new byte[4096];
        count = reqStr.Read(buf, 0, 4096);
        totalRead += count;
      }

      // If bytes were read last time, trim zeroes and write last bit
      if(count != 0)
      {
        str.Write(buf, 0, count);
      }

      str.Close();

      req.WriteHTMLResponse("<html> Upload Successful! </html>");

      return;
    }

    private void RespondWithFile(File422 file, WebRequest req)
    {
      Stream str = file.OpenReadOnly();

      string resp = "HTTP/1.1 200 OK\r\n" +
          "Content-Length: " + str.Length + "\r\n" +
          "Content-Type: " + GetContentType(file.Name) + "\r\n\r\n";

      // Write headers first
      byte[] buf = Encoding.ASCII.GetBytes(resp);
      req.WriteDirectResponse(buf, 0, buf.Length);

      buf = new byte[8192];
      int count = str.Read(buf, 0, buf.Length);

      while (count != 0)
      {
        req.WriteDirectResponse(buf, 0, count);
        buf = new byte[8192];
        count = str.Read(buf, 0, buf.Length);
      }
      str.Close();
    }

    private string PercentDecoding(string url)
    {
      url = url.Replace("%20", " ");
      url = url.Replace("%22", "\"");
      url = url.Replace("%25", "%");
      url = url.Replace("%2D", "-");
      url = url.Replace("%2E", ".");
      url = url.Replace("%3C", "<");
      url = url.Replace("%3E", ">");
      url = url.Replace("%5C", "\\");
      url = url.Replace("%5E", "^");
      url = url.Replace("%5F", "_");
      url = url.Replace("%60", "`");
      url = url.Replace("%7B", "{");
      url = url.Replace("%7C", "|");
      url = url.Replace("%7D", "}");
      url = url.Replace("%7E", "~");

      return url;
    }

    private string GetContentType(string name)
    {
      string ext = Path.GetExtension(name).ToLower().TrimStart('.');

      switch(ext)
      {
        case "jpeg":
        case "jpg":
        case "png":
          return "image/" + ext;
        case "avi":
        case "mp4":
        case "mov":
          return "video/" + ext;
        case "mp3":
        case "mpeg":
          return "audio/" + ext;
        case "txt":
        case "xml":
        case "html":
        case "rtf":
        case "htm":
          return "text/" + ext;
        case "pdf":
          return "application/" + ext;
        default:
          return "text/html";
      }
    }
  }
}
