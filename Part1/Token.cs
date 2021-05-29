using System;
using System.Collections.Generic;
using System.Text;

namespace TeenyTinyCompiler.Part1
{
    class Token
    {
        public string TokenText { get; set; }
        public TokenType TokenKind { get; set; }
    
    
        public Token(string tokenText, TokenType tokenKind)
        {
            TokenText = tokenText;
            TokenKind = tokenKind;
        }

        public static TokenType checkIfKeyword(string tokenText)
        {
            foreach (var kindName in Enum.GetNames(typeof(TokenType)))
            {
                if (kindName == tokenText)
                {
                    TokenType kindType = Enum.Parse<TokenType>(kindName);
                    int kindValue = (int)kindType;
                    if (kindValue >= 100 && kindValue < 200)
                    {
                        return kindType;
                    }
                }
            }
            return TokenType.None;
        }
    }
}
