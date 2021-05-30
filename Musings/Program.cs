using System;
using System.IO;

namespace TeenyTinyCompiler.Musings
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Teeny Tiny Compiler");

            var arg = string.Join(" ", args);
            if (string.IsNullOrEmpty(arg))
            {
                Console.WriteLine("Error: Compiler needs source file as argument.");
            }
            else 
            {
                var lexer = new Lexer(ReadSourceFile(arg));
                var emitter = new Emitter("out.cs");
                var parser = new Parser(lexer, emitter);
                try
                {
                    parser.Program();
                    emitter.WriteFile();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.WriteLine("Parsing completed.");

            }
            Console.WriteLine();
            Console.WriteLine("Press enter to end program...");
            Console.ReadLine();
        }

        static string ReadSourceFile(string path)
        {
            string output = null;
            if (File.Exists(path))
            {
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        output = streamReader.ReadToEnd();
                    }
                }
            }
            return output;
        }
    }
}
