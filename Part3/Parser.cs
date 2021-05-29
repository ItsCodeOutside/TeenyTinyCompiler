using System;
using System.Collections.Generic;
using System.Text;

namespace TeenyTinyCompiler.Part3
{
    class Parser
    {
        Lexer _lexer;
        Emitter _emitter;
        Token _currentToken = new Token(string.Empty, TokenType.None);
        Token _peekToken = new Token(string.Empty, TokenType.None);
        List<string> _labelsDeclared = new List<string>();
        List<string> _labelsGotoed = new List<string>();
        List<string> _symbols = new List<string>();

        public Parser(Lexer lexer, Emitter emitter)
        {
            _lexer = lexer;
            _emitter = emitter;

            NextToken();
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
            _emitter.HeaderLine("#include <stdio.h>");
            _emitter.HeaderLine("int main(void){");

            while (CheckToken(TokenType.NEWLINE))
            {
                NextToken();
            }
            while (!CheckToken(TokenType.EOF))
            {
                Statement();
            }
            _emitter.EmitLine("return 0;");
            _emitter.EmitLine("}");

            foreach (var label in _labelsGotoed)
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
                    _emitter.EmitLine("printf(\"" + _currentToken.TokenText + "\\n\");");
                    NextToken();
                }
                else
                {
                    _emitter.Emit("printf(\"%.2f" + _currentToken.TokenText + "\\n\", (float)(");
                    Expression();
                    _emitter.EmitLine("));");
                }
            }
            else if (CheckToken(TokenType.IF))
            {
                Print("STATEMENT-IF");
                NextToken();
                _emitter.Emit("if(");
                Comparison();
                Match(TokenType.THEN);
                Nl();
                _emitter.EmitLine("){");

                while (!CheckToken(TokenType.ENDIF))
                {
                    Statement();
                }
                Match(TokenType.ENDIF);
                _emitter.EmitLine("}");
            }
            else if (CheckToken(TokenType.WHILE))
            {
                Print("STATEMENT-WHILE");
                NextToken();
                _emitter.Emit("while(");
                Comparison();
                Match(TokenType.REPEAT);
                Nl();
                _emitter.EmitLine("){");

                while (!CheckToken(TokenType.ENDWHILE))
                {
                    Statement();
                }

                Match(TokenType.ENDWHILE);
                _emitter.EmitLine("}");
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

                _emitter.EmitLine(_currentToken.TokenText + ":");
                Match(TokenType.IDENT);
            }
            else if (CheckToken(TokenType.GOTO))
            {
                Print("STATEMENT-GOTO");
                NextToken();
                _labelsGotoed.Add(_currentToken.TokenText);
                _emitter.EmitLine("goto " + _currentToken.TokenText + ";");
                Match(TokenType.IDENT);
            }
            else if (CheckToken(TokenType.LET))
            {
                Print("STATEMENT-LET");
                NextToken();

                if (!_symbols.Contains(_currentToken.TokenText))
                {
                    _symbols.Add(_currentToken.TokenText);
                    _emitter.HeaderLine("float " + _currentToken.TokenText + ";");
                }

                _emitter.Emit(_currentToken.TokenText + " = ");
                Match(TokenType.IDENT);
                Match(TokenType.EQ);

                Expression();
                _emitter.EmitLine(";");
            }
            else if (CheckToken(TokenType.INPUT))
            {
                Print("STATEMENT-INPUT");
                NextToken();
                if (!_symbols.Contains(_currentToken.TokenText))
                {
                    _symbols.Add(_currentToken.TokenText);
                    _emitter.HeaderLine("float " + _currentToken.TokenText + ";");
                }
                _emitter.EmitLine("if(0 == scanf(\"%f\", &" + _currentToken.TokenText +")) {");
                _emitter.EmitLine(_currentToken.TokenText + " = 0;");
                _emitter.Emit("scanf(\"%");
                _emitter.EmitLine("*s\");");
                _emitter.EmitLine("}");
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
                _emitter.Emit(_currentToken.TokenText);
                NextToken();
                Expression();
            }

            while (IsComparisonOperator())
            {
                _emitter.Emit(_currentToken.TokenText);
                NextToken();
                Expression();
            }
        }
        void Expression()
        {
            Print("EXPRESSION");

            Term();
            while (CheckToken(TokenType.PLUS) || CheckToken(TokenType.MINUS))
            {
                _emitter.Emit(_currentToken.TokenText);
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
                _emitter.Emit(_currentToken.TokenText);
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
                _emitter.Emit(_currentToken.TokenText);
                NextToken();
            }
            Primary();
        }
        void Primary()
        {
            Print("PRIMARY (" + _currentToken.TokenText + ")");
            if (CheckToken(TokenType.NUMBER))
            {
                _emitter.Emit(_currentToken.TokenText);
                NextToken();
            }
            else if (CheckToken(TokenType.IDENT))
            {
                if (!_symbols.Contains(_currentToken.TokenText))
                {
                    Abort("Referencing variable before assignment: " + _currentToken.TokenText);
                }

                _emitter.Emit(_currentToken.TokenText);
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
            //Part 3 doesn't print the parsed tokens to the console
            //Console.WriteLine(message);
        }
    }
}
