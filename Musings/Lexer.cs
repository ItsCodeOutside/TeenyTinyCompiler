using System;
using System.Collections.Generic;
using System.Text;

namespace TeenyTinyCompiler.Musings
{
    class Lexer
    {
        string _sourceText;
        char _currentCharacter;
        int _currentPosition;

        public char CurrentChar { get { return _currentCharacter; } }

        public Lexer(string input)
        {
            _sourceText = input + '\n';  //Source code to lex as a string. Append a newline to simplify lexing/parsin the last token/statement
            _currentCharacter = '\0';   //Current character in the string
            _currentPosition = -1;       //Current position in the string
            NextChar();
        }

        //Move to the next character
        public void NextChar()
        {
            _currentPosition++;
            if (_currentPosition >= _sourceText.Length)
                _currentCharacter = '\0';
            else
                _currentCharacter = _sourceText[_currentPosition];
        }

        public char Peek()
        {
            if (_currentPosition + 1 >= _sourceText.Length)
                return '\0';
            else
                return _sourceText[_currentPosition + 1];
        }

        void Abort(string message)
        {
            throw new Exception("Lexing error." + message);
        }

        void SkipWhitespace()
        {
            while (_currentCharacter == ' '
                || _currentCharacter == '\t'
                || _currentCharacter == '\r')
            {
                NextChar();
            }
        }

        void SkipComment()
        {
            if (_currentCharacter == '#')
            {
                while (_currentCharacter != '\n')
                {
                    NextChar();
                }
            }
        }

        public Token GetToken()
        {
            SkipWhitespace();
            SkipComment();
            Token token = null;
            char lastChar = '\0';
            int startPosition = -1;

            switch (_currentCharacter) {
                case '+':
                    token = new Token(new string(_currentCharacter, 1), TokenType.PLUS);
                    break;
                case '-':
                    token = new Token(new string(_currentCharacter, 1), TokenType.MINUS);
                    break;
                case '*':
                    token = new Token(new string(_currentCharacter, 1), TokenType.ASTERISK);
                    break;
                case '/':
                    token = new Token(new string(_currentCharacter, 1), TokenType.SLASH);
                    break;
                case '=':
                    if (Peek() == '=')
                    {
                        lastChar = _currentCharacter;
                        NextChar();
                        token = new Token(new string(new char[] { lastChar, _currentCharacter }, 0, 2), TokenType.EQEQ);
                    }
                    else
                    {
                        token = new Token(new string(_currentCharacter, 1), TokenType.EQ);
                    }
                    break;
                case '>':
                    if (Peek() == '=')
                    {
                        lastChar = _currentCharacter;
                        NextChar();
                        token = new Token(new string(new char[] { lastChar, _currentCharacter }, 0, 2), TokenType.GTEQ);
                    }
                    else
                    {
                        token = new Token(new string(_currentCharacter, 1), TokenType.GT);
                    }
                    break;
                case '<':
                    if (Peek() == '=')
                    {
                        lastChar = _currentCharacter;
                        NextChar();
                        token = new Token(new string(new char[] { lastChar, _currentCharacter }, 0, 2), TokenType.LTEQ);
                    }
                    else
                    {
                        token = new Token(new string(_currentCharacter, 1), TokenType.LT);
                    }
                    break;
                case '!':
                    if (Peek() == '=')
                    {
                        lastChar = _currentCharacter;
                        NextChar();
                        token = new Token(new string(new char[] { lastChar, _currentCharacter }, 0, 2), TokenType.NOTEQ);
                    }
                    else
                    {
                        Abort("Expected != but got !" + new string(Peek(), 1));
                    }
                    break;
                case '\"':
                    NextChar();
                    startPosition = _currentPosition;
                    while (_currentCharacter != '\"')
                    {
                        if (_currentCharacter == '\r'
                            || _currentCharacter == '\n'
                            || _currentCharacter == '\t'
                            || _currentCharacter == '\\'
                            || _currentCharacter == '%')
                        {
                            Abort("Illegal character in string: " + new string(_currentCharacter, 1));
                        }
                        NextChar();
                    }
                    //Does not capture the trailing "
                    token = new Token(_sourceText.Substring(startPosition, (_currentPosition - startPosition)), TokenType.STRING);
                    break;
                case '\n':
                    token = new Token(new string(_currentCharacter, 1), TokenType.NEWLINE);
                    break;
                case '\0':
                    token = new Token(new string(_currentCharacter, 1), TokenType.EOF);
                    break;
                default:
                    if (IsDigit(_currentCharacter))
                    {
                        startPosition = _currentPosition;
                        char peekedChar = Peek();
                        while (IsDigit(peekedChar))
                        {
                            NextChar();
                            peekedChar = Peek();
                        }
                        if (peekedChar == '.')
                        {
                            NextChar();
                            peekedChar = Peek();
                            if (!IsDigit(peekedChar))
                            {
                                Abort("Illegal character in number: " + new string(peekedChar, 1));
                            }
                            while (IsDigit(peekedChar))
                            {
                                NextChar();
                                peekedChar = Peek();
                            }
                        }
                        token = new Token(_sourceText.Substring(startPosition, 1 +(_currentPosition - startPosition)), TokenType.NUMBER);
                    }
                    else if (IsAlphabet(_currentCharacter))
                    {
                        startPosition = _currentPosition;
                        while (IsAlphabet(Peek()))
                        {
                            NextChar();
                        }
                        //SUBSTRING is different to the way Python does it. 1+(LENGTH) to capture the last character
                        var tokenType = Token.checkIfKeyword(_sourceText.Substring(startPosition, 1+(_currentPosition - startPosition)));
                        if (tokenType == TokenType.None)
                        {
                            token = new Token(_sourceText.Substring(startPosition, 1+(_currentPosition - startPosition)), TokenType.IDENT);
                        }
                        else
                        {
                            token = new Token(_sourceText.Substring(startPosition, 1+(_currentPosition - startPosition)), tokenType);
                        }
                    }
                    else
                    { 
                        Abort("Unknown token: " + new string(_currentCharacter, 1));
                    }
                    break;
            }

            NextChar();
            return token;
        }


        bool IsDigit(char input)
        {
            if (input >= 48 && input <= 57)
                return true;
            return false;
        }

        bool IsAlphabet(char input)
        {
            if ((input >= 65 && input <= 90)
                || (input >= 97 && input <= 122))
                return true;
            return false;
        }
    }
}
