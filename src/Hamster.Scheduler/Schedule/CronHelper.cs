using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace Hamster.Scheduler.Schedule
{
  public static class CronHelper
  {
    private static Regex cronRegex = null;

    private static void Init()
    {
      if (cronRegex == null)
      {
        cronRegex = new Regex(@"^((?<at>\d+)|(((?<all>\*)|((?<from>\d+)\-(?<to>\d+)))(/(?<step>\d+))?))$", RegexOptions.Compiled);
      }
    }

    private static void SetRange(BitArray result, Match match, int min, int max)
    {
      Group atGroup = match.Groups["at"];
      if (atGroup.Success)
      {
        int index = int.Parse(atGroup.Value);
        if (min > index || max < index)
          throw new FormatException(string.Format("Number {0} at {1} is out of range ({2}-{3}).", index, match.Index + 1, min, max));

        result.Set(index - min, true);
      }
      else
      {
        int from = min;
        int to = max;

        if (!match.Groups["all"].Success)
        {
          Group fromGroup = match.Groups["from"];
          Group toGroup = match.Groups["to"];

          from = int.Parse(fromGroup.Value);
          to = int.Parse(toGroup.Value);

          if (from < min || from > max)
            throw new FormatException(string.Format("Number {0} at {1} is out of range ({2}-{3}).", from, fromGroup.Index + 1, min, max));

          if (to < min || to > max)
            throw new FormatException(string.Format("Number {0} at {1} is out of range ({2}-{3}).", to, toGroup.Index + 1, min, max));

          if (from > to)
          {
            int buff = from;
            from = to;
            to = buff;
          }
        }

        int step = 1;
        Group stepGroup = match.Groups["step"];
        if (stepGroup.Success)
        {
          step = int.Parse(stepGroup.Value);
        }

        for (int i = from; i <= to; i += step)
        {
          result.Set(i - min, true);
        }
      }
    }

    public static BitArray Parse(string text, int min, int max)
    {
      Init();

      BitArray result = new BitArray(max - min + 1);
      Match m;

      int start = 0;
      int end = text.IndexOf(',');

      while (end != -1)
      {
        m = cronRegex.Match(text, start, end - start);
        if (!m.Success)
        {
          throw new FormatException(string.Format("Invalid format after '{0}'.", text.Substring(0, start)));
        }

        SetRange(result, m, min, max);

        start = end + 1;
        end = text.IndexOf(',', start);
      }

      m = cronRegex.Match(text, start, text.Length - start);
      if (!m.Success)
      {
        throw new FormatException(string.Format("Invalid format after '{0}'.", text.Substring(0, start)));
      }

      SetRange(result, m, min, max);
      return result;
    }
  }
}
