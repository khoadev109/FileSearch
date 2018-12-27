using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace FileSearch
{
    class Program
    {
        private static readonly string _folderPath;
        private static readonly int _numberOfFiles = 500000;
        private static readonly int _numberOfTextLines = 20000;

        static Program()
        {
            var projectFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            _folderPath = Path.Combine(projectFolder, "TextFiles");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Create folder if not exists");
            CreateFolderIfNotExists();

            Console.WriteLine("Delete all files in folder");
            DeleteAllFilesInFolder();

            Console.WriteLine("Create files");
            CreateFiles();

            Console.WriteLine("Searching...");

            Console.WriteLine("Search for 'skwn'");
            PrintResult("skwn", () => Search("skwn", _folderPath));

            Console.WriteLine("Search for 'File created'");
            PrintResult("File created", () => Search("File created", _folderPath));

            Console.ReadLine();
        }

        private static void CreateFolderIfNotExists()
        {
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
        }

        private static void DeleteAllFilesInFolder()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_folderPath);

            foreach (FileInfo file in directoryInfo.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
            {
                directory.Delete(true);
            }
        }

        private static void CreateFiles()
        {
            Parallel.For(0, _numberOfFiles, i =>
            {
                CreateFileAndWriteContent($"test-file-{i}.txt");
            });
        }

        private static void CreateFileAndWriteContent(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_folderPath, fileName);

                using (StreamWriter streamWriter = File.CreateText(filePath))
                {
                    streamWriter.WriteLine("File created: {0}", fileName);
                    streamWriter.WriteLine("Date created: {0}", DateTime.Now.ToString());
                    streamWriter.WriteLine("Random text: ");

                    var stringGenerator = new StringRandomGenerator();

                    for (int i = 0; i < _numberOfTextLines; i++)
                    {
                        streamWriter.WriteLine(stringGenerator.Generate(10));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static int Search(string searchTerm, string directoryPath)
        {
            List<Task<int>> tasks = new List<Task<int>>();

            string[] files = Directory.GetFiles(directoryPath, "*.txt", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                Task<int> task = Task.Factory.StartNew((argument) =>
                {
                    string filePath = argument as string;
                    byte[] bytes = File.ReadAllBytes(filePath);
                    string content = Encoding.UTF8.GetString(bytes);

                    int occurenceNumber = Regex.Matches(content, searchTerm, RegexOptions.Compiled | RegexOptions.Multiline).Count;
                    if (occurenceNumber > 0)
                    {
                        Console.WriteLine($"Found {occurenceNumber} '{searchTerm}' in file {filePath}");
                    }
                    return occurenceNumber;

                }, file);

                tasks.Add(task);
            }

            List<int> results = GetTasksResult(tasks);

            int count = results.AsParallel().Sum();
            return count;
        }

        private static List<int> GetTasksResult(List<Task<int>> tasks)
        {
            List<int> results = new List<int>();

            while (tasks.Count > 0)
            {
                int i = Task.WaitAny(tasks.ToArray());
                results.Add(tasks[i].Result);
                tasks.RemoveAt(i);
            }

            return results;
        }

        private static void PrintResult(string searchTerm, Func<int> searchFunc)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int numberOfOccurence = searchFunc();

            stopWatch.Stop();

            Console.WriteLine($"Number occurence of {searchTerm}: {numberOfOccurence}");
            Console.WriteLine($"Total time: {stopWatch.Elapsed}");
        }
    }
}
