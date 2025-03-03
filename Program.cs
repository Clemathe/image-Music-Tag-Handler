// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using musicTag;
using DotNetEnv;

Console.WriteLine("🔍 Scan des dossiers en cours...");
Stopwatch stopwatch = Stopwatch.StartNew();

// Load environment variables from .env file
Env.Load();

// Get the base directory from the environment variables
string baseDirectory = Env.GetString("base_directory");

CoverOptimizer.ProcessMusicFolders(baseDirectory);

stopwatch.Stop();
Console.WriteLine($"⏱️ Programme terminée en : {stopwatch.ElapsedMilliseconds} ms");
