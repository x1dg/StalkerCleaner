using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StalkerCleaner.Services
{
    public class OriginalFile
    {
        public string Path { get; set; }
        public long Size { get; set; }
    }
    
    public class FileService
    {
        public readonly DirectoryInfo gamedata;
        public readonly Dictionary<string, DirectoryInfo> Folders = new();

        public IEnumerable<FileInfo> Configs { get; }
        public IEnumerable<FileInfo> Levels { get; }
        public IEnumerable<FileInfo> Meshes { get; }
        public IEnumerable<OriginalFile> OriginalMeshes { get; }
        public IEnumerable<FileInfo> Scripts { get; }
        public IEnumerable<FileInfo> OriginalScripts { get; }
        public IEnumerable<FileInfo> Sounds { get; }
        public IEnumerable<FileInfo> Textures { get; }
        public IEnumerable<OriginalFile> OriginalTextures { get; }
        public List<OriginalFile> ProceededTextures { get; } = new();
        public IEnumerable<FileInfo> TexturesLtx { get; }
        public IEnumerable<FileInfo> TexturesThms { get; }
        public IEnumerable<FileInfo> Sequences { get; }
        public IEnumerable<FileInfo> Engine { get; }

        public FileService(string gamedata)
        {
            this.gamedata = new DirectoryInfo(gamedata);
            var mainFolders = new List<string>()
            {
                "config",
                "levels",
                "meshes",
                "scripts",
                "sounds",
                "textures"
            };

            foreach (var folder in mainFolders)
            {
                var directory = new DirectoryInfo(this.gamedata.FullName + "/" + folder);
                if (directory.Exists)
                {
                    Folders[folder] = directory;
                    switch (folder)
                    {
                        case "config":
                            Configs = TryGetFiles(Folders[folder], "*.*");
                            break;
                        // case "levels":
                        //     Levels = TryGetFiles(Folders[folder], "*.*");
                        //     Console.WriteLine($"#### Нашли {Levels.Count()} уровней!");
                        //     break;
                        // case "meshes":
                        //     Meshes = TryGetFiles(Folders[folder], "*.*");
                        //     Console.WriteLine($"#### Нашли {Meshes.Count()} мешей!");
                        //     break;
                        case "scripts":
                            Scripts = TryGetFiles(Folders[folder], "*.*");
                            break;
                        case "sounds":
                            Sounds = TryGetFiles(Folders[folder], "*.*");
                            break;
                        // case "textures":
                        //     Textures = TryGetFiles(Folders[folder], "*.dds")
                        //         .Where(x => !OriginalTextures.Any(y =>
                        //             y.Path == Path.GetRelativePath(Folders["textures"].FullName, x.FullName.Replace("_bump#", "").Replace("_bump", "")))).ToArray();
                        //     var foundedCount = Textures.Count();
                        //     Textures = Textures.Where(x => !ProceededTextures.Any(y =>
                        //         y.Path == Path.GetRelativePath(Folders["textures"].FullName, x.FullName.Replace("_bump#", "").Replace("_bump", ""))));
                        //     Console.WriteLine($"#### Нашли {foundedCount} текстур! Обработано {ProceededTextures.Count()}, приступаем к обработке {Textures.Count()} / {foundedCount} текстур.");
                        //     Sequences = TryGetFiles(Folders[folder], "*.seq");
                        //     Console.WriteLine($"#### Нашли {Sequences.Count()} seq-файлов!");
                        //     TexturesLtx = TryGetFiles(Folders[folder], "*.ltx");
                        //     Console.WriteLine($"#### Нашли {TexturesLtx.Count()} текстурных ltx-файлов!");
                        //     TexturesThms = TryGetFiles(Folders[folder], "*.thm");
                        //     Console.WriteLine($"#### Нашли {TexturesThms.Count()} текстурных thm-файлов!");
                        //     break;
                    }

                    Engine = TryGetFiles(this.gamedata.Parent, "xrGame.dll")
                        .Union(TryGetFiles(this.gamedata.Parent, "xrEngine.exe"));
                }
            }
        }

        private static IEnumerable<FileInfo> TryGetFiles(DirectoryInfo dir, string searchPattern)
            => dir.Exists ? (IEnumerable<FileInfo>) dir.GetFiles(searchPattern, SearchOption.AllDirectories) : new List<FileInfo>();
    }
}