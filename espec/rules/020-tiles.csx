#r "nuget: YamlDotNet, 11.2.1"

using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

DirectoryInfo tilesDir = new DirectoryInfo("./Resources/Prototypes/Tiles");
IEnumerable<FileInfo> fileList = tilesDir.GetFiles("*.yml", System.IO.SearchOption.AllDirectories);

foreach (FileInfo f in fileList)
{
    string yaml = File.ReadAllText(f.FullName);
    Console.WriteLine(yaml);
}

game.Tiles.Add(new Tile { Name = "Test Tile" });
