using System;

namespace TeenyTinyCompiler.Part1
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = "IF+-123\"\" 367.52 foo*THEN";
            var lexer = new Lexer(input);
            Token token = lexer.GetToken();
            while (token.TokenKind != TokenType.EOF)
            {
                Console.WriteLine(token.TokenKind.ToString());
                token = lexer.GetToken();
            }

            Console.WriteLine();
            Console.WriteLine("Press enter to end program...");
            Console.ReadLine();
        }
    }
}
