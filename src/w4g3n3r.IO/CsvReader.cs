using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace w4g3n3r.IO
{
    public class CsvReader : StreamReader
    {
        private readonly char delimiter;
        private readonly char quote;

        public CsvReader(Stream stream) : base(stream)
        {
            delimiter = ',';
            quote = '"';
        }

        public CsvReader(Stream stream, char delimiter, char quote) : base(stream)
        {
            this.delimiter = delimiter;
            this.quote = quote;
        }

        public class Record
        {
            public readonly string[] Row;

            public string this[int index] => Row[index];

            public Record(string[] row)
            {
                Row = row;
            }
        }

        public Record ReadRecord() 
        {
            var field = new StringBuilder();
            var record = new List<string>();

            while(!this.EndOfStream) 
            {
                var line = this.ReadLine();
                if (Parse(line, field, record))
                {
                    return new Record(record.ToArray());
                }
            }

            throw new InvalidOperationException("Record is not terminated");
        }

        private bool Parse(string line, StringBuilder field, List<string> record)
        {
            int i = 0;

            if (ParsingQuotedLineBreak(field))
            {
                if (!ParseQuote(line, ref i, field, record))
                {
                    return false;
                }
            }

            while (i < line.Length)
            {
                var character = line[i++];
                if (IsDelimiter(character))
                {
                    EndParsingField(field, record);
                }
                else if (IsQuote(character))
                {
                    if (!ParseQuote(line, ref i, field, record))
                    {
                        return false;
                    }
                }
                else
                {
                    field.Append(character);
                }
            }

            if (FieldHasData(field))
            {
                EndParsingField(field, record);
            }

            return true;
        }

        private bool ParseQuote(string line, ref int i, StringBuilder field, List<string> record)
        {
            if (ParsingQuotedLineBreak(field)) field.AppendLine();

            while (i < line.Length)
            {
                var character = line[i++];
                if (IsQuote(character))
                {
                    if (NextCharacterIsQuote(i, line))
                    {
                        field.Append(quote);
                        i++;
                        continue;
                    }
                    return true;
                }
                else
                {
                    field.Append(character);
                }
            }
            return false;
        }

        private bool ParsingQuotedLineBreak(StringBuilder field) => FieldHasData(field);
        private bool IsDelimiter(char c) => c == this.delimiter;
        private bool IsQuote(char c) => c == this.quote;
        private bool FieldHasData(StringBuilder field) => field.Length > 0;
        private bool NextCharacterIsQuote(int index, string line) => index < line.Length && IsQuote(line[index]);
        private void EndParsingField(StringBuilder field, List<string> record)
        {
            record.Add(field.ToString().Trim());
            field.Length = 0;
        }
    }
}
