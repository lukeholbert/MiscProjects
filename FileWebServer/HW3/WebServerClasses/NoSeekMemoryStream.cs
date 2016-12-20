using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CS422
{
  public class NoSeekMemoryStream : MemoryStream
  {
    MemoryStream str;

    public NoSeekMemoryStream(byte[] buffer) // implement
    {
      str = new MemoryStream(buffer);
    }

    public NoSeekMemoryStream(byte[] buffer, int offset, int count) // implement
    {
      str = new MemoryStream(buffer, offset, count);
    }

    public override bool CanSeek
    {
      get
      {
        return false;
      }
    }
    
    public override long Seek(long offset, SeekOrigin loc)
    {
      throw new NotSupportedException();
    }

    public override long Position
    {
      get
      {
        return str.Position;
      }

      set
      {
        throw new NotSupportedException();
      }
    }

    public override long Length
    {
      get
      {
        throw new NotSupportedException();
      }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      str.Write(buffer, offset, count);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return str.Read(buffer, offset, count);
    }
  }
}
