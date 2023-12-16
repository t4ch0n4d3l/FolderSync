﻿if (args.Length < 2 || args.Length > 3 || args.Length == 3 && !new[] { "-d", "-D" }.Contains(args[2]))
{
    Console.WriteLine("Syntax: sync Quellverzeichnis Zielverzeichnis [-d] [-D]");
    return;
}

var sourceDirectory = args[0];
var source = new DirectoryInfo(sourceDirectory);
if (!source.Exists)
{
    Console.WriteLine("Source directory does not exist");
    return;
}

var targetDirectory = args[1];
var target = new DirectoryInfo(targetDirectory);
if (!target.Exists)
{
    Console.WriteLine("Target directory does not exist");
    return;
}

var deleteMode = DeleteMode.None;

if (args.Length == 3)
{
    if (args[2] == "-d")
    {
        deleteMode = DeleteMode.Prompt;
    }
    else if (args[2] == "-D")
    {
        deleteMode = DeleteMode.Silent;
    }
}

SyncFiles(source, target, deleteMode);

Console.ReadKey();

static void SyncFiles(DirectoryInfo source, DirectoryInfo target, DeleteMode deleteMode, string currentPath = null)
{
    currentPath ??= string.Empty;
    foreach (var sourceFile in source.GetFiles())
    {
        var targetFile = new FileInfo(Path.Combine(target.FullName, sourceFile.Name));
        var path = Path.Combine(currentPath, sourceFile.Name);
        if (!targetFile.Exists)
        {
            using (C.Color(ConsoleColor.Black, ConsoleColor.Green))
            {
                Console.Write(" + ");
            }
            Console.WriteLine(path);
            sourceFile.CopyTo(targetFile.FullName);
        }
        else if (targetFile.LastWriteTimeUtc != sourceFile.LastWriteTimeUtc || targetFile.Length != sourceFile.Length)
        {
            if (targetFile.LastWriteTimeUtc > sourceFile.LastWriteTimeUtc)
            {
                var color = Console.ForegroundColor;
                using (C.Color(ConsoleColor.Black, ConsoleColor.Red))
                {
                    Console.WriteLine($"Warning! File is newer in target than in source: ");
                }
            }
            using (C.Color(ConsoleColor.Black, ConsoleColor.Yellow))
            {
                Console.Write(" U ");
            }
            Console.WriteLine(path);
            sourceFile.CopyTo(targetFile.FullName, true);
        }
    }

    if (deleteMode != DeleteMode.None)
    {
        foreach (var targetFile in target.GetFiles())
        {
            var sourceFile = new FileInfo(Path.Combine(source.FullName, targetFile.Name));
            var path = Path.Combine(currentPath, targetFile.Name);
            if (!sourceFile.Exists)
            {
                var delete = true;
                if (deleteMode == DeleteMode.Prompt)
                {
                    for (; ; )
                    {
                        Console.WriteLine($"Do you want to delete {path}? ");
                        var key = Console.ReadKey();
                        Console.WriteLine();
                        if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                        {
                            break;
                        }
                        else if (key.KeyChar == 'n' || key.KeyChar == 'N')
                        {
                            delete = false;
                            break;
                        }
                    }
                }
                if (delete)
                {
                    using (C.Color(ConsoleColor.Black, ConsoleColor.Red))
                    {
                        Console.Write(" - ");
                    }
                    Console.WriteLine(path);
                    targetFile.Delete();
                }
            }
        }
    }

    foreach (var sourceDirectory in source.GetDirectories())
    {
        var targetDirectory = new DirectoryInfo(Path.Combine(target.FullName, sourceDirectory.Name));
        var path = Path.Combine(currentPath, sourceDirectory.Name);
        if (!targetDirectory.Exists)
        {
            targetDirectory.Create();
        }
        SyncFiles(sourceDirectory, targetDirectory, deleteMode, path);
    }

    if (deleteMode != DeleteMode.None)
    {
        foreach (var targetDirectory in target.GetDirectories())
        {
            var sourceDirectory = new DirectoryInfo(Path.Combine(source.FullName, targetDirectory.Name));
            var path = Path.Combine(currentPath, targetDirectory.Name);
            if (!sourceDirectory.Exists)
            {
                var delete = true;
                if (deleteMode == DeleteMode.Prompt)
                {
                    for (; ; )
                    {
                        Console.WriteLine($"Do you want to delete {path}?");
                        var key = Console.ReadKey();
                        Console.WriteLine();
                        if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                        {
                            break;
                        }
                        else if (key.KeyChar == 'n' || key.KeyChar == 'N')
                        {
                            delete = false;
                            break;
                        }
                    }
                }
                if (delete)
                {
                    using (C.Color(ConsoleColor.Black, ConsoleColor.Red))
                    {
                        Console.Write(" - ");
                    }
                    Console.WriteLine(path);
                    targetDirectory.Delete(true);
                }
            }
        }
    }
}

enum DeleteMode
{
    None, Prompt, Silent
}