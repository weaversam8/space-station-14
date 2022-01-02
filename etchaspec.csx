#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.CodeAnalysis.CSharp.Scripting, 4.0.1"
#r "nuget: Google.Protobuf, 3.19.1"

using System.IO;
using Google.Protobuf;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

// Console.WriteLine("Compiling assemblies based on latest protobuf.");
var process = System.Diagnostics.Process.Start("protoc", "--proto_path=./espec/schema --csharp_out=./espec/assemblies ./espec/schema/game.proto");
while (!process.HasExited) { }
// Console.WriteLine("Assemblies completed.");

string protoSource = File.ReadAllText("./espec/assemblies/Game.cs");

// remove all references to the global namespace, to resolve issues with running C# code interactively
// I believe this has to do with the fact that the code we're creating here via CSharpScript is in
// its own namespace for interactive execution purposes, so global:: takes us out of there.
protoSource = protoSource.Replace("global::", "");

var state = await CSharpScript.RunAsync(protoSource, ScriptOptions.Default.WithReferences(typeof(Google.Protobuf.IMessage).Assembly));
state = await state.ContinueWithAsync("Game game = new Game();");

// Load and execute each rule in the rules folder
DirectoryInfo d = new DirectoryInfo("./espec/rules");
FileInfo[] files = d.GetFiles("*.csx");

foreach (FileInfo file in files)
{
    state = await state.ContinueWithAsync(File.ReadAllText(file.FullName));
}

state = await state.ContinueWithAsync("var formatter = new Google.Protobuf.JsonFormatter(new Google.Protobuf.JsonFormatter.Settings(true));");
state = await state.ContinueWithAsync("formatter.Format(game)");

Console.WriteLine(state.ReturnValue);
