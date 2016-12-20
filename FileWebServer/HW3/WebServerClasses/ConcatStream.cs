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
  public class ConcatStream : Stream
  {
    Stream _str1;
    Stream _str2;
    long _length;
    long _position;
    bool _fixed;

    public override long Length
    {
      get
      {
        if (_fixed)
        {
          return _length;
        }
        else
        {
          if (_length == -1)
          {
            return _str1.Length;
          }
          else
          {
            return _str1.Length + _str2.Length;
          }
        }
      }
    }

    public override bool CanRead
    {
      get
      {
        return _str1.CanRead && _str2.CanRead;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return _str1.CanSeek && _str2.CanSeek;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return _str1.CanWrite && _str2.CanWrite;
      }
    }

    public override long Position
    {
      get
      {
        return _position;
      }

      set
      {
        if (value < 0)
        {
          _position = 0;
          _str1.Seek(0, SeekOrigin.Begin);
          if (_str2.CanSeek)
          {
            _str2.Seek(0, SeekOrigin.Begin);
          }

        }
        else if (value > Length)
        {
          _position = Length;
          _str1.Seek(0, SeekOrigin.End);
          if (_str2.CanSeek)
          {
            _str2.Seek(0, SeekOrigin.End);
          }
        }
        else
        {
          _position = value;
          if (value <= _str1.Length)
          {
            _str1.Seek(value, SeekOrigin.Begin);
            if (_str2.CanSeek)
            {
              _str2.Seek(0, SeekOrigin.Begin);
            }
          }
          else
          {
            _str1.Seek(0, SeekOrigin.End);
            if (_str2.CanSeek)
            {
              _str2.Seek(value - _str1.Length, SeekOrigin.Begin);
            }
          }
        }
      }
    }

    public ConcatStream(Stream first, Stream second)
    {
      if(!first.CanSeek)
      {
        throw new NotSupportedException();
      }

      _str1 = first;
      _str2 = second;
      _position = 0;
      try
      {
        _length = first.Length + second.Length;
      }
      catch
      {
        _length = -1;
      }
      _fixed = false;
    }

    public ConcatStream(Stream first, Stream second, long fixedLength)
    {
      _str1 = first;
      _str2 = second;
      _length = fixedLength;
      _position = 0;
      _fixed = true;
    }

    public override void Flush()
    {
      _str1.Flush();
      _str2.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      switch(origin)
      {
        case SeekOrigin.Begin:
          Position = offset;
          break;

        case SeekOrigin.Current:
          Position += offset;
          break;

        case SeekOrigin.End:
          Position = Length + offset;
          break;
      }

      return Position;
    }

    public override void SetLength(long value)
    {
      _length = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      // Cases
      // All from str1
      if(Position + count <= _str1.Length)
      {
        _str1.Read(buffer, offset, count);
      }
      // All from str2
      else if (Position > _str1.Length)
      {
        _str2.Read(buffer, offset, count);
      }
      // Crossover
      else
      {
        int countStr1 = (int)(_str1.Length - _str1.Position);
        _str1.Read(buffer, offset, countStr1);
        _str2.Read(buffer, offset + countStr1, count - countStr1);
      }
      Position += count;

      return count;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      // Cases
      // All from str1
      if (Position + count <= _str1.Length)
      {
        _str1.Write(buffer, offset, count);
      }
      // All from str2
      else if (Position > _str1.Length)
      {
        _str2.Write(buffer, offset, count);
      }
      // Crossover
      else
      {
        int countStr1 = (int)(_str1.Length - _str1.Position);
        _str1.Write(buffer, offset, countStr1);
        _str2.Write(buffer, offset + countStr1, count - countStr1);
      }
      Position += count;
    }
  }
}
