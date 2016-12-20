using NUnit.Framework;
using CS422;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CS422.Tests
{
  [TestFixture()]
  public class NoSeekMemoryStreamTests
  {
    [Test()]
    public void NoSeekMemoryStreamTest()
    {
      NoSeekMemoryStream str1 = new NoSeekMemoryStream(Encoding.ASCII.GetBytes("newstream"));
      MemoryStream str2 = new MemoryStream(Encoding.ASCII.GetBytes("newstream"));

      byte[] buf1 = new byte[10];
      byte[] buf2 = new byte[10];
      str1.Read(buf1, 0, 10);
      str2.Read(buf2, 0, 10);
      Assert.That(buf1.SequenceEqual(buf2));
    }

    [Test()]
    public void SeekTest()
    {
      NoSeekMemoryStream str = new NoSeekMemoryStream(Encoding.ASCII.GetBytes("newstream"));

      try
      {
        str.Position = 6;
        Assert.Fail();
      }
      catch (NotSupportedException)
      { }
      catch (Exception)
      {
        Assert.Fail();
      }

      try
      {
        str.Seek(4, System.IO.SeekOrigin.Begin);
        Assert.Fail();
      }
      catch (NotSupportedException)
      { }
      catch (Exception)
      {
        Assert.Fail();
      }

      try
      {
        long len = str.Length;
        Assert.Fail();
      }
      catch (NotSupportedException)
      { }
      catch (Exception)
      {
        Assert.Fail();
      }
    }
  }
}