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
  public class ConcatStreamTests
  {
    [Test()]
    public void ConcatStreamTest()
    {
      ConcatStream cstr = new ConcatStream(new MemoryStream(Encoding.ASCII.GetBytes("newstream")), new NoSeekMemoryStream(Encoding.ASCII.GetBytes("newstream2")));
      byte[] buf = new byte[19];
      cstr.Read(buf, 0, 19);
      string str = Encoding.ASCII.GetString(buf);

      Assert.That("newstreamnewstream2" == str);
    }

    [Test()]
    public void GetLengthTest()
    {
      ConcatStream cstr = new ConcatStream(new MemoryStream(Encoding.ASCII.GetBytes("newstream")), new MemoryStream(Encoding.ASCII.GetBytes("newstream2")), 50);
      Assert.That(cstr.Length == 50);

      cstr = new ConcatStream(new MemoryStream(Encoding.ASCII.GetBytes("newstream")), new MemoryStream(Encoding.ASCII.GetBytes("newstream2")));
      Assert.That(cstr.Length == 19);

      cstr = new ConcatStream(new MemoryStream(Encoding.ASCII.GetBytes("newstream")), new NoSeekMemoryStream(Encoding.ASCII.GetBytes("newstream2")));
      Assert.That(cstr.Length == 9);

      try
      {
        cstr = new ConcatStream(new NoSeekMemoryStream(Encoding.ASCII.GetBytes("newstream")), new NoSeekMemoryStream(Encoding.ASCII.GetBytes("newstream2")));
        Assert.Fail();
      }
      catch (NotSupportedException)
      { }
      catch (Exception)
      {
        Assert.Fail();
      }
    }

    [Test()]
    public void ReadTest()
    {
      MemoryStream str = new MemoryStream(Encoding.ASCII.GetBytes("newstreamnewstream2"));
      ConcatStream cstr = new ConcatStream(new MemoryStream(Encoding.ASCII.GetBytes("newstream")), new MemoryStream(Encoding.ASCII.GetBytes("newstream2")), 50);
      Random rand = new Random();

      for(int i = 0; i < 19;)
      {
        int num = rand.Next() % 10;
        i += num;
        byte[] buf1 = new byte[10];
        byte[] buf2 = new byte[10];

        str.Read(buf1, 0, num);
        cstr.Read(buf2, 0, num);
        Assert.That(buf1.SequenceEqual(buf2));
      }
    }

    // Checks to see if writing, seeking, and resizing works
    [Test()]
    public void WriteTest()
    {
      ConcatStream cstr = new ConcatStream(new MemoryStream(Encoding.ASCII.GetBytes("newstream")), new MemoryStream());
      string newStr = "stringthatshouldoverwriteeverythingelse";

      cstr.Write(Encoding.ASCII.GetBytes(newStr), 0, newStr.Length);
      cstr.Seek(0, SeekOrigin.Begin);
      byte[] buf = new byte[newStr.Length];

      cstr.Read(buf, 0, newStr.Length);

      Assert.That(newStr == Encoding.ASCII.GetString(buf));
    }
  }
}