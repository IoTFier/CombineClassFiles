    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length != 2)
            {
                Console.WriteLine("You provide a directory and a destination file as arguments");
                Console.WriteLine();
                Console.WriteLine("CombineClassFiles.exe . output.cs");
                Console.WriteLine("CombineClassFiles.exe /Users/Documents/Projects/Classes/ ./output.cs");
                return;
            }

            var path = args[0];
            var destination = args[1];
            if (!Directory.Exists(path))
            {
                Console.WriteLine("You must provide an existing directory.");
                return;
            }

            if (File.Exists(path))
            {
                Console.WriteLine("You can write to an existing file for your output.");
                return;
            }

            try
            {
                var destFile = File.CreateText(destination);
                var destinationFileInfo = new FileInfo(destination);

                Console.WriteLine("Created destination file. {0}", destination);

                var files = Directory.GetFiles(path, "*.cs");

                Console.WriteLine("Found {0} source files.", files.Length);

                var program = new Program();



                foreach(var file in files)
                {
                    Task.Run(async () =>
                    {
                        await program.ProcessFile(file, destFile, destinationFileInfo.FullName);
                    }).GetAwaiter().GetResult();
                }

                Console.WriteLine("Finished Writing File: {0}", destination);
            }
            catch (Exception error)
            {
                Console.WriteLine("Error! {0}", error);
            }

            Console.WriteLine("Press enter to finish.");
            Console.ReadLine();
        }

        async Task ProcessFile(string file, StreamWriter writer, string destinationPath)
        {
            var newFile = new FileInfo(file);
            if (newFile.FullName == destinationPath)
            {
                Console.WriteLine("Skipping output file.");
                return;
            }

            if (!File.Exists(file))
            {
                throw new ArgumentException("Invalid file path!");
            }
            
            var skipNext = false;
            var lines = 0;
            using (var streamReader = File.OpenText(file))
            {
                Console.WriteLine("Reading from file {0}.", file);
                while (!streamReader.EndOfStream)
                {
                    var line = await streamReader.ReadLineAsync();

                    if (line.StartsWith("namespace") || line.StartsWith("using"))
                    {
                        if (!line.Contains("{"))
                        {
                            skipNext = true;
                        }
                    }
                    else if(!skipNext && !line.StartsWith("}"))
                    {
                        lines++;
                        await writer.WriteLineAsync(line);
                    }
                    else
                    {
                        skipNext = false;
                    }
                }
                await writer.FlushAsync();
                Console.WriteLine("Finished writing file {0} with {1} lines.", file, lines);
            }
        }
    }
