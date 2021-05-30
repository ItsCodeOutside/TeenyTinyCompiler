using System;
using System.IO;

namespace TeenyTinyCompiler.Musings
{
    class Emitter
    {
        string _outputFile;
        string _header = string.Empty;
        string _code = string.Empty;
        public Emitter(string outputFile)
        {
            _outputFile = outputFile;
        }

        public void Emit(string code)
        {
            _code += code;
        }
        public void EmitLine(string code)
        {
            _code += code + '\n';
        }
        public void HeaderLine(string code)
        {
            _header += code + '\n';
        }
        internal void WriteFile()
        {
            using (var fileStream = new FileStream(_outputFile, FileMode.Create))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(_header + _code);
                }
            }
        }
    }
}
