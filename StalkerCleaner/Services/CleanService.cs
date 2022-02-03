using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StalkerCleaner.Services
{
    public class CleanService
    {
        private FileService _fileService;
        private List<OriginalFile> _proceededTextures = new();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private object _xxx = new object();

        public CleanService()
        {
        }

        public void InitFileService(string gamedataPath)
        {
            _fileService = new FileService(gamedataPath);
            //ProceededTextures = _fileService.ProceededTextures.ToList();
        }

        public ConcurrentBag<string> ClearDirectories(string parentDirectory)
        {
            ConcurrentBag<string> directories = new ();
            System.Threading.Tasks.Parallel.ForEach(System.IO.Directory.GetDirectories(parentDirectory), directory =>
            {
                ClearDirectories(directory);
                if (!System.IO.Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    directories.Add(directory);
                    System.IO.Directory.Delete(directory, false);
                }
            });
            return directories;
        }

        public ConcurrentBag<string> ClearScripts()
        {
            ConcurrentBag<string> garbageScripts = new ();
            System.Threading.Tasks.Parallel.ForEach(_fileService.Scripts, script =>
            {
                var scriptName = script.Name;
                var scriptNameShort = script.Name.Split(new[] { ".script" }, StringSplitOptions.None)[0];

                var queryLtx =
                    from file in _fileService.Configs
                    let fileText = GetFileText(file.FullName)
                    where IsUsingInFile(fileText, scriptNameShort)
                    select file.FullName;

                var queryXml =
                    from file in _fileService.Configs
                    let fileText = GetFileText(file.FullName)
                    where IsUsingInFile(fileText, scriptNameShort)
                    select file.FullName;

                var queryScripts =
                    from file in _fileService.Scripts
                    where file.Name != scriptName
                    let fileText = GetFileText(file.FullName)
                    where IsUsingInFile(fileText, scriptNameShort)
                    select file.FullName;

                if (!queryScripts.Any() && !queryLtx.Any() && !queryXml.Any())
                {
                    garbageScripts.Add(script.FullName);
                    if (script.Directory != null)
                        Directory.CreateDirectory(script.Directory.FullName.Replace("gamedata", "gamedata_backup"));
                    File.Copy(script.FullName, script.FullName.Replace("gamedata", "gamedata_backup"));
                    File.Delete(script.FullName);

                }
            });
            
            return garbageScripts;
        }

        public ConcurrentBag<string> ClearConfigs()
        {
            ConcurrentBag<string> garbageConfigs = new ();

            var configsFolder = _fileService.Folders["config"];
            var directories = Directory.GetDirectories(configsFolder.FullName).Where(x => !x.Contains("scripts"));

            IEnumerable<System.IO.FileInfo> configFiles =
                configsFolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories).ToArray();
            
            foreach (var directory in directories)
            {
                var currentDir = new DirectoryInfo(directory);
                IEnumerable<System.IO.FileInfo> cfgs =
                    configFiles.Where(x =>
                        x.Directory?.FullName == currentDir.FullName ||
                        (x.Directory?.FullName.Contains((currentDir.FullName + "\\")) ?? false));

                Parallel.ForEach(cfgs, cfg =>
                    {
                        var queryConfigs =
                            from file in _fileService.Configs
                            where file.Name != cfg.Name
                            let fileText = GetFileText(file.FullName)
                            where IsContainInFile(fileText,
                                cfg.Name.Split(new[] { ".ltx" }, StringSplitOptions.None)[0].Split(new[] { ".xml" }, StringSplitOptions.None)[0])
                            select file.FullName;

                        var queryScripts =
                            from file in _fileService.Scripts
                            let fileText = GetFileText(file.FullName)
                            where IsContainInFile(fileText,
                                cfg.Name.Split(new[] { ".ltx" }, StringSplitOptions.None)[0].Split(new[] { ".xml" }, StringSplitOptions.None)[0])
                            select file.FullName;
                        
                        var queryEngine =
                            from file in _fileService.Engine
                            let fileText = GetFileText(file.FullName)
                            where IsContainInFile(fileText, cfg.Name)
                            select file.FullName;
                        
                        if (!queryConfigs.Any() && !queryScripts.Any() && !queryEngine.Any())
                        {
                            garbageConfigs.Add(cfg.FullName);
                            if (cfg.Directory != null)
                                Directory.CreateDirectory(
                                    cfg.Directory.FullName.Replace("gamedata", "gamedata_backup"));
                            if (File.Exists(cfg.FullName.Replace("gamedata", "gamedata_backup")))
                                File.Delete(cfg.FullName.Replace("gamedata", "gamedata_backup"));
                            File.Copy(cfg.FullName, cfg.FullName.Replace("gamedata", "gamedata_backup"));
                            File.Delete(cfg.FullName);
                        }
                    }
                );
            }

            return garbageConfigs;
        }
        
        public ConcurrentBag<string> ClearSounds()
        {
            ConcurrentBag<string> garbageSounds = new ();

            var configsFolder = _fileService.Folders["sounds"];
            var directories = Directory.GetDirectories(configsFolder.FullName);

            IEnumerable<System.IO.FileInfo> configFiles =
                configsFolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories).ToArray();
            
            foreach (var directory in directories)
            {
                var currentDir = new DirectoryInfo(directory);
                IEnumerable<System.IO.FileInfo> snds =
                    configFiles.Where(x =>
                        x.Directory?.FullName == currentDir.FullName ||
                        (x.Directory?.FullName.Contains((currentDir.FullName + "\\")) ?? false));

                Parallel.ForEach(snds, snd =>
                    {
                        var queryConfigs =
                            from file in _fileService.Sounds
                            where file.Name != snd.Name
                            let fileText = GetFileText(file.FullName)
                            where IsContainInFile(fileText,
                                snd.Name.Split(new[] { ".ogg" }, StringSplitOptions.None)[0]
                                    .Split(new[] { "_r" }, StringSplitOptions.None)[0]
                                    .Split(new[] { "_l" }, StringSplitOptions.None)[0])
                            select file.FullName;

                        var queryScripts =
                            from file in _fileService.Scripts
                            let fileText = GetFileText(file.FullName)
                            where IsContainInFile(fileText,
                                snd.Name.Split(new[] { ".ogg" }, StringSplitOptions.None)[0]
                                    .Split(new[] { "_r" }, StringSplitOptions.None)[0]
                                    .Split(new[] { "_l" }, StringSplitOptions.None)[0])
                            select file.FullName;
                        
                        // var queryEngine =
                        //     from file in _fileService.Engine
                        //     let fileText = GetFileText(file.FullName)
                        //     where IsContainInFile(fileText, snd.Name)
                        //     select file.FullName;
                        
                        if (!queryConfigs.Any() && !queryScripts.Any())// && !queryEngine.Any())
                        {
                            garbageSounds.Add(snd.FullName);
                            if (snd.Directory != null)
                                Directory.CreateDirectory(
                                    snd.Directory.FullName.Replace("gamedata", "gamedata_backup"));
                            if (File.Exists(snd.FullName.Replace("gamedata", "gamedata_backup")))
                                File.Delete(snd.FullName.Replace("gamedata", "gamedata_backup"));
                            File.Copy(snd.FullName, snd.FullName.Replace("gamedata", "gamedata_backup"));
                            File.Delete(snd.FullName);
                        }
                    }
                );
            }

            return garbageSounds;
        }


        private bool IsUsingInFile(IEnumerable<string> fileText, string scriptName)
        {
            return fileText?.Any(x => x.ToLower().Contains(scriptName.ToLower() + ".")
                                     || x.ToLower().Contains(scriptName.ToLower() + "[")
                                     || x.ToLower().Contains("'" + scriptName.ToLower() + "'")
                                     || x.ToLower().Contains("\"" + scriptName.ToLower() + "\"")
                                     || x.ToLower().Contains("," + scriptName.ToLower())) ?? false;
        }

        private bool IsContainInFile(IEnumerable<string> fileText, string scriptName) =>
            fileText?.Any(x => x.ToLower().Contains(scriptName.ToLower())) ?? true;

        private IEnumerable<string> GetFileText(string name)
        {
            if (File.Exists(name))
            {
                return File.ReadLines(name);
            }

            return null;
        }

        private void CopyMesh(FileInfo mesh, List<Tuple<string, string>> replacements)
        {
            var newFile = mesh.FullName;
            replacements.ForEach(x => newFile = newFile.Replace(x.Item1, x.Item2));
            var copiedMesh = new FileInfo(newFile);
            if (!(new DirectoryInfo(copiedMesh.DirectoryName ?? string.Empty).Exists))
                Directory.CreateDirectory(copiedMesh.DirectoryName ?? string.Empty);
            if (File.Exists(newFile))
                File.Delete(newFile);
            File.Copy(mesh.FullName,
                newFile, true);
        }

        private void CopyTexture(FileInfo texture, List<Tuple<string, string>> replacements)
        {
            var newFile = texture.FullName;
            replacements.ForEach(x => newFile = newFile.Replace(x.Item1, x.Item2));
            var copiedTexture = new FileInfo(newFile);
            if (!(new DirectoryInfo(copiedTexture.DirectoryName).Exists))
                Directory.CreateDirectory(copiedTexture.DirectoryName);
            if (File.Exists(newFile))
                File.Delete(newFile);
            File.Copy(texture.FullName,
                newFile, true);
            CopyOtherShit(texture, replacements);
        }

        private void CopyOtherShit(FileInfo texture, List<Tuple<string, string>> replacements)
        {
            _lock.EnterWriteLock();
            try
            {
                CopyThm(texture, replacements);
                CopyBump(texture, replacements);
                CopyBumpSharp(texture, replacements);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void CopyThm(FileInfo texture, List<Tuple<string, string>> replacements)
        {
            var thm = texture.FullName.Replace(".dds", ".thm");
            var newFile = thm;
            if (File.Exists(thm))
            {
                replacements.ForEach(x => newFile = newFile.Replace(x.Item1, x.Item2));
                var copiedTexture = new FileInfo(newFile);
                if (!(new DirectoryInfo(copiedTexture.DirectoryName).Exists))
                    Directory.CreateDirectory(copiedTexture.DirectoryName);
                if (File.Exists(newFile))
                    File.Delete(newFile);
                File.Copy(thm,
                    newFile);
            }
        }

        private void CopyBump(FileInfo texture, List<Tuple<string, string>> replacements)
        {
            var bump = texture.FullName.Replace(".dds", "_bump.dds");
            var newFile = bump;
            if (File.Exists(newFile))
            {
                replacements.ForEach(x => newFile = newFile.Replace(x.Item1, x.Item2));
                var copiedTexture = new FileInfo(newFile);
                if (!(new DirectoryInfo(copiedTexture.DirectoryName).Exists))
                    Directory.CreateDirectory(copiedTexture.DirectoryName);
                if (File.Exists(newFile))
                    File.Delete(newFile);
                File.Copy(bump,
                    newFile);
            }

            CopyThm(new FileInfo(bump), replacements);
        }

        private void CopyBumpSharp(FileInfo texture, List<Tuple<string, string>> replacements)
        {
            var bump = texture.FullName.Replace(".dds", "_bump#.dds");
            var newFile = bump;
            if (File.Exists(newFile))
            {
                replacements.ForEach(x => newFile = newFile.Replace(x.Item1, x.Item2));
                var copiedTexture = new FileInfo(newFile);
                if (!(new DirectoryInfo(copiedTexture.DirectoryName).Exists))
                    Directory.CreateDirectory(copiedTexture.DirectoryName);
                if (File.Exists(newFile))
                    File.Delete(newFile);
                File.Copy(bump,
                    newFile);
            }

            CopyThm(new FileInfo(bump), replacements);
        }

        private bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            } while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return ((file1byte - file2byte) == 0);
        }
    }

    public class SocCopConverter
    {
        public string SocPath { get; set; }
        public string CopPath { get; set; }
    }
}