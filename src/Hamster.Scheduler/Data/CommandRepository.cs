using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Hamster.Scheduler.Repository;
using Microsoft.Scripting.Hosting;

namespace Hamster.Scheduler.Data
{
  public class CommandRepository : IRepository<string, CommandInfo>
  {
    private readonly string directory;
    private readonly ScriptRuntime runtime;

    public CommandRepository(string directory, ScriptRuntime runtime)
    {
      if (runtime == null)
        throw new ArgumentNullException(nameof(runtime));

      DirectoryInfo dir = new DirectoryInfo(directory);
      this.runtime = runtime;

      if (!dir.Exists)
        dir.Create();

      this.directory = dir.FullName;
    }

    public CommandInfo Get(string key)
    {
      CommandInfo result = null;
      string path = GetPath(key);
      if (path != null)
        result = LoadCommand(path);
      return result;
    }

    public IList<CommandInfo> GetItems()
    {
      return GetItems(0, 0);
    }

    public IList<string> GetKeys()
    {
      string[] files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
      for (int i = 0; i < files.Length; ++i)
      {
        files[i] = Path.GetFileNameWithoutExtension(files[i]);
      }
      return files;
    }

    public IList<CommandInfo> GetItems(int offset, int count)
    {
      var files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
          .Skip(offset * count);
      if (count > 0)
        files = files.Take(count);

      var result = from filename in files
                   select LoadCommand(Path.GetFullPath(filename));
      return result.ToArray();
    }

    public void Remove(string key)
    {
      string path = GetPath(key);
      if (path != null)
        File.Delete(path);
    }

    public void Add(CommandInfo item)
    {
      if (item == null)
        throw new ArgumentNullException(nameof(item));
      if (string.IsNullOrEmpty(item.Name))
        throw new ArgumentException("The 'Name' property of the item must be set.");

      string path = GetPath(item.Name);
      if (File.Exists(path))
        throw new ArgumentException($"There is already a command with the name '{item.Name}'.");
      SaveCommand(item);
    }

    public void Update(string key, CommandInfo item)
    {
      if (item == null)
        throw new ArgumentNullException(nameof(item));
      if (string.IsNullOrEmpty(item.Name))
        throw new ArgumentException("The 'Name' property of the item must be set.");

      if (key != item.Name)
      {
        string path = GetPath(item.Name);
        if (File.Exists(path))
          throw new ArgumentException($"There is already a command with the name '{item.Name}'.");

        if (!string.IsNullOrEmpty(key))
          Remove(key);
      }

      SaveCommand(item);
    }

    public IRepoService<CommandInfo> GetService()
    {
      return new RepoService<string, CommandInfo>(this);
    }

    protected void SaveCommand(CommandInfo item)
    {
      string ext = item.Language ?? ".py";
      if (!runtime.TryGetEngineByFileExtension(ext, out var engine))
      {
        engine = runtime.GetEngine(item.Language);
        ext = engine.Setup.FileExtensions.First();
      }

      if (!ext.StartsWith("."))
        ext = '.' + ext;

      string path = Path.Combine(directory, Path.GetFileName(item.Name) + ext);

      using (var writer = File.CreateText(path))
      {
        var xmlSettings = new XmlWriterSettings()
        {
          Indent = true,
          OmitXmlDeclaration = true
        };

        using (XmlWriter xml = XmlWriter.Create(new CommentWriter(writer), xmlSettings))
        {
          xml.WriteStartElement("command");
          xml.WriteElementString("description", item.Description);
          foreach (var p in item.Parameters)
          {
            xml.WriteStartElement("param");
            xml.WriteAttributeString("name", p.Name);
            xml.WriteAttributeString("type", p.Type);
            xml.WriteString(p.Description);
            xml.WriteEndElement();
          }
          xml.WriteEndElement();

          xml.Flush();
        }

        writer.WriteLine();
        writer.WriteLine();

        using (var reader = new StringReader(item.ScriptCode.TrimEnd()))
        {
          string line;
          while (null != (line = reader.ReadLine()))
          {
            writer.WriteLine(line);
          }
        }
      }
    }

    protected CommandInfo LoadCommand(string path)
    {
      if (path == null)
        return null;

      FileInfo file = new FileInfo(path);
      if (!file.Exists)
        return null;

      string[] lines = File.ReadAllLines(file.FullName);

      string[] headerLines = lines.TakeWhile(x => x.StartsWith("#"))
          .Select(x => x.Substring(1)).ToArray();
      int codeStart = headerLines.Length;
      while (codeStart < lines.Length && lines[codeStart].Length == 0)
        codeStart += 1;

      string header = string.Join("\n", headerLines);
      header = Regex.Replace(header, @"^.*\<command[^>]*\>", "<command>", RegexOptions.Singleline);
      header = Regex.Replace(header, @"\</command[^>]*\>.*$", "</command>", RegexOptions.Singleline);

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(header);

      CommandInfo cmd = new CommandInfo
      {
        Language = file.Extension,
        Name = file.Name.Substring(0, file.Name.Length - file.Extension.Length),
        ScriptCode = string.Join("\n", lines, codeStart, lines.Length - codeStart).TrimEnd()
      };

      if (doc.DocumentElement != null)
      {
        foreach (XmlElement desc in doc.DocumentElement.GetElementsByTagName("description"))
        {
          cmd.Description = desc.InnerText;
        }

        foreach (XmlElement param in doc.DocumentElement.GetElementsByTagName("param"))
        {
          cmd.Parameters.Add(new ParameterInfo()
          {
            Command = cmd,
            ParameterId = cmd.Parameters.Count + 1,
            Name = param.GetAttribute("name"),
            Type = param.GetAttribute("type"),
            Description = param.InnerText
          });
        }
      }

      return cmd;
    }

    protected string GetPath(string key)
    {
      key = Path.GetFileName(key);
      string[] files = Directory.GetFiles(directory, key + ".*", SearchOption.TopDirectoryOnly);
      foreach (string filename in files)
      {
        FileInfo file = new FileInfo(filename);
        string name = Path.GetFileNameWithoutExtension(file.Name);
        if (name == key)
          return file.FullName;
      }

      return null;
    }
  }
}
