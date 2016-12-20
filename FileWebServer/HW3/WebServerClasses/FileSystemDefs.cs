using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CS422
{
  public abstract class Dir422
  {
    public abstract string Name { get; }
    public abstract Dir422 Parent { get; }
    public abstract IList<Dir422> GetDirs();
    public abstract IList<File422> GetFiles();

    public abstract bool ContainsFile(string fileName, bool recursive);
    public abstract bool ContainsDir(string dirName, bool recursive);
    public abstract Dir422 GetDir(string name);
    public abstract File422 GetFile(string name);
    public abstract File422 CreateFile(string name);
    public abstract Dir422 CreateDir(string name);
  }

  public abstract class File422
  {
    public abstract string Name { get; }
    public abstract Dir422 Parent { get; }

    public abstract Stream OpenReadOnly();
    public abstract Stream OpenReadWrite(); 
  }

  public abstract class FileSys422
  {
    public abstract Dir422 GetRoot();
    
    public virtual bool Contains(File422 file)
    {
      return Contains(file.Parent);
    }

    public virtual bool Contains(Dir422 dir)
    {
      if(dir == null) { return false; }
      if (dir == GetRoot()) { return true; }
      return (Contains(dir.Parent));
    }
  }

  public class StdFSDir : Dir422
  {
    private string m_path;
    private string root;

    public StdFSDir(string path, string newRoot)
    {
      m_path = path;
      root = newRoot;
    }

    public override string Name
    {
      get
      {
        return m_path.Split(new char[] {'\\', '/' }).Last();
      }
    }

    public override Dir422 Parent
    {
      get
      {
        if (m_path == root)
        {
          return null;
        }
        return new StdFSDir(Directory.GetParent(m_path).FullName, root);
      }
    }

    public override bool ContainsDir(string dirName, bool recursive)
    {
      if (dirName.Contains('\\') || dirName.Contains('/'))
      {
        return false;
      }

      if (!recursive)
      {
        if (GetDir(dirName) != null)
        {
          return true;
        }
        else
        {
          return false;
        }
      }

      // recursive
      var dirs = GetDirs();

      if(GetDir(dirName) != null)
      {
        return true;
      }
      foreach(var dir in dirs)
      {
        if(dir.ContainsDir(dirName, true))
        {
          return true;
        }
      }

      return false;
    }

    public override bool ContainsFile(string fileName, bool recursive)
    {
      if (fileName.Contains('\\') || fileName.Contains('/'))
      {
        return false;
      }

      if (!recursive)
      {
        if (GetFile(fileName) != null)
        {
          return true;
        }
        else
        {
          return false;
        }
      }

      // recursive
      var dirs = GetDirs();

      if (GetFile(fileName) != null)
      {
        return true;
      }
      foreach (var dir in dirs)
      {
        if (dir.ContainsFile(fileName, true))
        {
          return true;
        }
      }

      return false;
    }

    public override Dir422 CreateDir(string name)
    {
      if (name.Contains('\\') || name.Contains('/'))
      {
        return null;
      }

      if(ContainsDir(name, false))
      {
        return GetDir(name);
      }

      Directory.CreateDirectory(m_path + "/" + name);

      return new StdFSDir(m_path + "/" + name, root);
    }

    public override File422 CreateFile(string name)
    {
      if(name.Contains('\\') || name.Contains('/'))
      {
        return null;
      }

      if(m_path.Contains('\\'))
      {
        File.Create(m_path + "\\" + name).Close();
        return new StdFSFile(m_path + "\\" + name, root);
      }

      File.Create(m_path + "/" + name).Close();
      return new StdFSFile(m_path + "/" + name, root);
    }

    public override Dir422 GetDir(string name)
    {
      if (name.Contains('\\') || name.Contains('/'))
      {
        return null;
      }

      if(!GetDirs().Any(x => x.Name == name))
      {
        return null;
      }

      return new StdFSDir(m_path + "/" + name, root);
    }

    public override IList<Dir422> GetDirs()
    {
      List<Dir422> dirs = new List<Dir422>();
      foreach (string dir in Directory.GetDirectories(m_path))
      {
        dirs.Add(new StdFSDir(dir, root));
      }

      return dirs;
    }

    public override File422 GetFile(string name)
    {
      if (name.Contains('\\') || name.Contains('/'))
      {
        return null;
      }

      if (!GetFiles().Any(x => x.Name == name))
      {
        return null;
      }

      return new StdFSFile(m_path + "/" + name, root);
    }

    public override IList<File422> GetFiles()
    {
      List<File422> files = new List<File422>();
      foreach(string file in Directory.GetFiles(m_path).ToList())
      {
        files.Add(new StdFSFile(file, root));
      }

      return files;
    }

    public static bool operator ==(StdFSDir d1, StdFSDir d2)
    {
      return d1.Name == d2.Name;
    }

    public static bool operator !=(StdFSDir d1, StdFSDir d2)
    {
      return d1.Name == d2.Name;
    }
  }

  public class StdFSFile : File422
  {
    string m_path;
    string root;

    public StdFSFile(string path, string newRoot)
    {
      m_path = path;
      root = newRoot;
    }

    public override string Name
    {
      get
      {
        return m_path.Split(new char[] { '\\', '/' }).Last();
      }
    }

    public override Dir422 Parent
    {
      get
      {
        return new StdFSDir(Directory.GetParent(m_path).FullName, root);
      }
    }

    public override Stream OpenReadOnly()
    {
      try
      {
        return File.OpenRead(m_path);
      }
      catch
      {
        return null;
      }
    }

    public override Stream OpenReadWrite()
    {
      try
      {
        return File.Open(m_path, FileMode.Open, FileAccess.ReadWrite);
      }
      catch
      {
        return null;
      }
    }
  }

  public class StandardFileSystem : FileSys422
  {
    StdFSDir root;

    StandardFileSystem(string rootDir)
    {
      root = new StdFSDir(rootDir, rootDir);
    }

    public override bool Contains(Dir422 dir)
    {
      if (dir == null) { return false; }
      if ((StdFSDir)dir == (StdFSDir)GetRoot()) { return true; }
      return (Contains(dir.Parent));
    }

    public override bool Contains(File422 file)
    {
      return Contains(file.Parent);
    }

    public static StandardFileSystem Create(string rootDir)
    {
      if (!Directory.Exists(rootDir))
      {
        return null;
      }
      return new StandardFileSystem(rootDir);
    }

    public override Dir422 GetRoot()
    {
      return root;
    }
  }

  public class MemoryFileSystem : FileSys422
  {
    MemFSDir root;

    public MemoryFileSystem()
    {
      root = new MemFSDir("/", null);
    }

    public override Dir422 GetRoot()
    {
      return root;
    }
  }

  public class MemFSDir : Dir422
  {
    string name;
    MemFSDir parent;
    List<MemFSDir> dirs;
    List<MemFSFile> files;

    public MemFSDir(string newName, MemFSDir newParent)
    {
      name = newName;
      dirs = new List<MemFSDir>();
      files = new List<MemFSFile>();
      parent = newParent;
    }

    public override string Name
    {
      get
      {
        return name;
      }
    }

    public override Dir422 Parent
    {
      get
      {
        return parent;
      }
    }

    public override bool ContainsDir(string dirName, bool recursive)
    {
      if(GetDir(dirName) != null)
      {
        return true;
      }

      if (recursive)
      {
        foreach (var dir in dirs)
        {
          if (dir.ContainsDir(dirName, true))
          {
            return true;
          }
        }
      }

      return false;
    }

    public override bool ContainsFile(string fileName, bool recursive)
    {
      if (GetFile(fileName) != null)
      {
        return true;
      }

      if (recursive)
      {
        foreach (var dir in dirs)
        {
          if (dir.ContainsFile(fileName, true))
          {
            return true;
          }
        }
      }

      return false;
    }

    public override Dir422 CreateDir(string name)
    {
      MemFSDir dir = new MemFSDir(name, this);
      dirs.Add(dir);
      return dir;
    }

    public override File422 CreateFile(string name)
    {
      MemFSFile file = new MemFSFile(name, this);
      files.Add(file);
      return file;
    }

    public override Dir422 GetDir(string name)
    {
      return dirs.FirstOrDefault(x => x.Name == name);
    }

    public override IList<Dir422> GetDirs()
    {
      return dirs.ToList<Dir422>();
    }

    public override File422 GetFile(string name)
    {
      return files.FirstOrDefault(x => x.Name == name);
    }

    public override IList<File422> GetFiles()
    {
      return files.ToList<File422>();
    }
  }

  public class MemFSFile : File422
  {
    string name;
    MemFSDir parent;
    MemoryStream stream;
    List<MemoryStream> reads;
    bool write;

    public MemFSFile(string newName, MemFSDir newParent)
    {
      name = newName;
      parent = newParent;
      stream = new MemoryStream();
      reads = new List<MemoryStream>();
      write = false;

    }

    public override string Name
    {
      get
      {
        return name;
      }
    }

    public override Dir422 Parent
    {
      get
      {
        return parent;
      }
    }

    public override Stream OpenReadOnly()
    {
      MemoryStream newst = new MemoryStream();

      lock (reads)
      {
        if(write == true)
        {
          return null;
        }

        stream.CopyTo(newst);
        newst.Seek(0, SeekOrigin.Begin);
        stream.Seek(0, SeekOrigin.Begin);
        reads.Add(newst);
      }
      
      return newst;
    }

    public override Stream OpenReadWrite()
    {
      lock (reads)
      {
        if (write == true)
        {
          return null;
        }
        if(reads.Any(x => x.CanRead))
        {
          return null;
        }

        write = true;
        MemoryStream ms = new MemoryStream();
        stream.CopyTo(ms);
        stream.Seek(0, SeekOrigin.Begin);
        ms.Seek(0, SeekOrigin.Begin);
        // ms.Write(stream.GetBuffer(), 0, stream.GetBuffer().Length);
        NotifyingStream ns = new NotifyingStream(ms);
        ns.Closed += NSClosed;

        return ns;
      }
    }

    private void NSClosed(object sender, EventArgs e)
    {
      stream = new MemoryStream((sender as MemoryStream).GetBuffer());
      stream.Seek(0, SeekOrigin.Begin);
      (sender as MemoryStream).Close();
      write = false;
    }
    
  }

  public class NotifyingStream : MemoryStream
  {
    MemoryStream str;
    public event EventHandler Closed;

    public NotifyingStream(MemoryStream newst)
    {
      str = newst;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      str.Write(buffer, offset, count);
    }

    public override bool CanRead
    {
      get
      {
        return str.CanRead;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return str.CanSeek;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return str.CanWrite;
      }
    }

    public override int Capacity
    {
      get
      {
        return str.Capacity;
      }

      set
      {
        str.Capacity = value;
      }
    }

    public override bool CanTimeout
    {
      get
      {
        return str.CanTimeout;
      }
    }

    public override int WriteTimeout
    {
      get
      {
        return str.WriteTimeout;
      }

      set
      {
        str.WriteTimeout = value;
      }
    }

    public override long Length
    {
      get
      {
        return str.Length;
      }
    }

    public override int ReadTimeout
    {
      get
      {
        return str.ReadTimeout;
      }

      set
      {
        str.ReadTimeout = value;
      }
    }

    public override long Position
    {
      get
      {
        return str.Position;
      }

      set
      {
        str.Position = value;
      }
    }

    public override long Seek(long offset, SeekOrigin loc)
    {
      return str.Seek(offset, loc);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return str.Read(buffer, offset, count);
    }

    public override void Close()
    {
      EventArgs e = new EventArgs();
      StreamCloseEvent(e);
    }

    protected virtual void StreamCloseEvent (EventArgs e)
    {
      Closed?.Invoke(str, e);
    }
  }
}
