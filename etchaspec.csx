#!/usr/bin/env dotnet-script
// #r "nuget: Microsoft.Extensions.Logging, 6.0.0"
// #r "nuget: Microsoft.CodeAnalysis.CSharp.Scripting, 4.0.1"
// #r "nuget: Google.Protobuf, 3.19.1"
// #r "nuget: Dotnet.Script.Core, 1.3.1"
// #r "nuget: Dotnet.Script.DependencyModel, 1.3.1"

using System.IO;
// using Google.Protobuf;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.Text;
// using Microsoft.CodeAnalysis.Scripting;
// using Microsoft.CodeAnalysis.CSharp.Scripting;
// using Dotnet.Script.Core;
// using Dotnet.Script.DependencyModel.Logging;

// Console.WriteLine("Compiling assemblies based on latest protobuf.");
var process = System.Diagnostics.Process.Start("protoc", "--proto_path=./espec/schema --csharp_out=./espec/assemblies ./espec/schema/game.proto");
process.WaitForExit();
// Console.WriteLine("Assemblies completed.");

//
string source = "#r \"nuget: Google.Protobuf, 3.19.1\"\n";

// when reading the generated protobuf C# code in,
// remove all references to the global namespace, to resolve issues with running C# code interactively
// I believe this has to do with the fact that the code we're creating here via CSharpScript is in
// its own namespace for interactive execution purposes, so global:: takes us out of there.
source += File.ReadAllText("./espec/assemblies/Game.cs").Replace("global::", "");

// ScriptOptions options = ScriptOptions.Default
//                             .WithReferences(typeof(Google.Protobuf.IMessage).Assembly)
//                             // .WithSourceResolver(new Dotnet.Script.DependencyModel.NuGet.NuGetSourceReferenceResolver())
//                             .WithMetadataResolver(new Dotnet.Script.DependencyModel.NuGet.NuGetMetadataReferenceResolver(ScriptMetadataResolver.Default));

source += "Game game = new Game();\n";

// Load and execute each rule in the rules folder
DirectoryInfo d = new DirectoryInfo("./espec/rules");
FileInfo[] files = d.GetFiles("*.csx");

foreach (FileInfo file in files)
{
    source += File.ReadAllText(file.FullName);
}

source += "var formatter = new Google.Protobuf.JsonFormatter(new Google.Protobuf.JsonFormatter.Settings(true));\n";
source += "Console.WriteLine(formatter.Format(game));\n";

// public Logger MakeLogger(Type t)
// {
//     // Type t here is the type that's logging messages
//     // Console.WriteLine(t);

//     void Log(LogLevel level, string message, Exception ex = null)
//     {
//         if (level == LogLevel.Error || level == LogLevel.Critical)
//         {
//             Console.Write("[" + level + "] ");
//             Console.WriteLine(message);
//         }
//     }

//     return Log;
// }

// LogFactory _logFactory = MakeLogger;

// var parsedSource = SourceText.From(source);
// var context = new ScriptContext(parsedSource, Directory.GetCurrentDirectory(), new string[] { });
// var compiler = new ScriptCompiler(_logFactory, false);
// var runner = new ScriptRunner(compiler, _logFactory, ScriptConsole.Default);

// string json = await runner.Execute<string>(context);

// Console.WriteLine(json);

// automatically reorder imports as necessary
string[] lines = source.Split('\n');
List<string> requires = new List<string>();
List<string> usings = new List<string>();
List<string> rest = new List<string>();

foreach (string line in lines)
{
    if (line.Contains("#r ")) requires.Add(line);
    else if (line.Contains("using")) usings.Add(line);
    else rest.Add(line);
}

source = string.Join('\n', requires) + '\n' + string.Join('\n', usings) + '\n' + string.Join('\n', rest);

// Console.WriteLine(source);

process = new Process();
process.StartInfo.FileName = "dotnet";
process.StartInfo.Arguments = "script eval";
process.StartInfo.UseShellExecute = false;
process.StartInfo.RedirectStandardInput = true;

process.Start();

StreamWriter processWriter = process.StandardInput;
processWriter.Write(source);
processWriter.Close();

process.WaitForExit();
