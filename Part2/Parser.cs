using System;
using System.Collections.Generic;
using System.Text;

namespace TeenyTinyCompiler.Part2
{
    class Parser
    {
        Lexer _lexer;
        Token _currentToken = new Token(string.Empty, TokenType.None);
        Token _peekToken = new Token(string.Empty, TokenType.None);
        List<string> _labelsDeclared = new List<string>();
        List<string> _labelsGotoed = new List<string>();
        List<string> _symbols = new List<string>();

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            NextToken(); //Seems to only need to be called once???
        }

        void Abort(string message)
        {
            throw new Exception("Error. " + message);
        }

        bool CheckToken(TokenType tokenKind)
        {
            return (tokenKind == _currentToken.TokenKind);
        }

        //Not used in this tutorial part
        bool CheckPeek(TokenType tokenKind)
        {
            return tokenKind == _peekToken.TokenKind;
        }

        bool IsComparisonOperator()
        {
            switch (_currentToken.TokenKind)
            {
                case TokenType.GT:
                case TokenType.GTEQ:
                case TokenType.LT:
                case TokenType.LTEQ:
                case TokenType.EQEQ:
                case TokenType.NOTEQ:
                    return true;
            }
            return false;
        }

        void Match(TokenType tokenKind)
        {
            if (!CheckToken(tokenKind))
            {
                Abort("Expected " + tokenKind.ToString() + " but got " + _currentToken.TokenKind.ToString());
            }
            NextToken();
        }

        void NextToken()
        {
            _currentToken = _peekToken;
            _currentToken = _lexer.GetToken();
        }



        public void Program()
        {
            Print("PROGRAM");
            while (CheckToken(TokenType.NEWLINE))
            {
                NextToken();
            }
            while (!CheckToken(TokenType.EOF))
            {
                Statement();
            }
            foreach(var label in _labelsGotoed)
            {
                if (!_labelsDeclared.Contains(label))
                {
                    Abort("Attempting to GOTO to an undeclared label: " + label);
                }
            }
        }
        void Statement()
        {
            if (CheckToken(TokenType.PRINT))
            {
                Print("STATEMENT-PRINT");
                NextToken();

                if (CheckToken(TokenType.STRING))
                {
                    NextToken();
                }
                else
                {
                    Expression();
                }
            }
            else if (CheckToken(TokenType.IF))
            {
                Print("STATEMENT-IF");
                NextToken();
                Comparison();
                Match(TokenType.THEN);
                Nl();

                while (!CheckToken(TokenType.ENDIF))
                {
                    Statement();
                }
                Match(TokenType.ENDIF);
            }
            else if (CheckToken(TokenType.WHILE))
            {
                Print("STATEMENT-WHILE");
                NextToken();
                Comparison();
                Match(TokenType.REPEAT);
                Nl();

                while (!CheckToken(TokenType.ENDWHILE))
                {
                    Statement();
                }

                Match(TokenType.ENDWHILE);
            }
            else if (CheckToken(TokenType.LABEL))
            {
                Print("STATEMENT-LABEL");
                NextToken();
                if (_labelsDeclared.Contains(_currentToken.TokenText))
                {
                    Abort("Label already exists: " + _currentToken.TokenText);
                }
                _labelsDeclared.Add(_currentToken.TokenText);

                Match(TokenType.IDENT);
            }
            else if (CheckToken(TokenType.GOTO))
            {
                Print("STATEMENT-GOTO");
                NextToken();
                _labelsGotoed.Add(_currentToken.TokenText);
                Match(TokenType.IDENT);
            }
            else if (CheckToken(TokenType.LET))
            {
                Print("STATEMENT-LET");
                NextToken();

                if (!_symbols.Contains(_currentToken.TokenText))
                {
                    _symbols.Add(_currentToken.TokenText);
                }

                Match(TokenType.IDENT);
                Match(TokenType.EQ);

                Expression();
            }
            else if (CheckToken(TokenType.INPUT))
            {
                Print("STATEMENT-INPUT");
                NextToken();
                if (!_symbols.Contains(_currentToken.TokenText))
                {
                    _symbols.Add(_currentToken.TokenText);
                }
                Match(TokenType.IDENT);
            }
            else
            {
                Abort("Invalid satement at " + _currentToken.TokenText + " (" + _currentToken.TokenKind + ")");
            }

            Nl();
        }

        void Comparison()
        {
            Print("COMPARISON");
            Expression();

            if (IsComparisonOperator())
            {
                NextToken();
                Expression();
            }
            else
            {
                Abort("Expected comparison operator at: " + _currentToken.TokenText);
            }
        }
        void Expression()
        {
            Print("EXPRESSION");

            Term();
            while (CheckToken(TokenType.PLUS) || CheckToken(TokenType.MINUS))
            {
                NextToken();
                Term();
            }
        }

        void Term()
        {
            Print("TERM");
            Unary();
            while (CheckToken(TokenType.ASTERISK) || CheckToken(TokenType.SLASH))
            {
                NextToken();
                Unary();
            }
        }
        void Unary()
        {
            Print("UNARY");
            if (CheckToken(TokenType.PLUS) 
                || CheckToken(TokenType.SLASH))
            {
                NextToken();
            }
            Primary();
        }
        void Primary()
        {
            Print("PRIMARY (" + _currentToken.TokenText + ")");
            if (CheckToken(TokenType.NUMBER))
            {
                NextToken();
            }
            else if (CheckToken(TokenType.IDENT))
            {
                if (!_symbols.Contains(_currentToken.TokenText))
                {
                    Abort("Referencing variable before assignment: " + _currentToken.TokenText);
                }
                NextToken();
            }
            else
            {
                Abort("Unexpected token at " + _currentToken.TokenText);
            }

        }
        void Nl()
        {
            Print("NEWLINE");
            Match(TokenType.NEWLINE);
            while (CheckToken(TokenType.NEWLINE))
            {
                NextToken();
            }
        }
        void Print(string message)
        {
            //Just for debugging
            Console.WriteLine(message);
        }
    }
}
