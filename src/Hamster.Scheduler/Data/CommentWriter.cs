using System;
using System.IO;
using System.Text;

namespace Hamster.Scheduler.Data
{
  public class CommentWriter : TextWriter
  {
    private bool linebreak = true;
    private readonly TextWriter baseWriter;

    public CommentWriter(TextWriter baseWriter)
    {
      this.baseWriter = baseWriter ?? throw new ArgumentNullException(nameof(baseWriter));
    }

    public override Encoding Encoding => baseWriter.Encoding;

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
    }
  }
}
