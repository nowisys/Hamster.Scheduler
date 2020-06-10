using System;
using System.IO;
using System.Text;

namespace Hamster.Scheduler.Data
{
  public class CommentWriter : TextWriter
  {
    private char prev;
    private bool linebreak = true;
    private TextWriter baseWriter;

    public CommentWriter(TextWriter baseWriter)
    {
      if (baseWriter == null)
        throw new ArgumentNullException("baseWriter");

      this.baseWriter = baseWriter;
    }

    public override Encoding Encoding
    {
      get { return baseWriter.Encoding; }
    }

    public override void Write(char value)
    {
      switch (value)
      {
        case '\n':
          if (linebreak)
          {
            baseWriter.WriteLine("#");
          }
          else
          {
            baseWriter.WriteLine();
          }
          linebreak = true;
          break;

        case '\r':
          break;

        default:
          if (linebreak)
            baseWriter.Write("# ");
          baseWriter.Write(value);
          linebreak = false;
          break;
      }

      prev = value;
    }
  }
}
