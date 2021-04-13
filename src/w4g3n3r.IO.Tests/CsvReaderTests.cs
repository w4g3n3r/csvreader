using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Xunit;

namespace w4g3n3r.IO.Tests
{
    public class CsvReaderTests
    {
        public static IEnumerable<object[]> GetCsvEdgeCases()
        {
            // Field with linebreak
            yield return new object[] {"a,\"b\r\nc\",d", new string[] {"a", $"b{Environment.NewLine}c", "d"}};
            // Field with comma
            yield return new object[] {"a,\"b,c\",d", new string[] {"a", "b,c", "d"}};
            // Field with double quote
            yield return new object[] {"a,\"b\"\"c\",d", new string[] {"a", "b\"c", "d"}};
            // Field wrapped in single quotes
            yield return new object[] {"a,'b,c'", new string[] {"a", "'b", "c'"}};
        }

        [Theory]
        [MemberData(nameof(GetCsvEdgeCases))]
        public void ReadRecord_CsvEdgeCase_ReturnsExpectedResult (string edgeCase, string[] expectedResult)
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes(edgeCase)))
            using(var sut = new w4g3n3r.IO.CsvReader(csv))
            {
                //When
                var result = sut.ReadRecord();
                //Then
                Assert.Equal(expectedResult, result.Row);
            }
        }

        [Fact]
        public void ReadRecord_SingleRecordMultipleFields_ReturnMatchingRecords()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("a,b,c")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv))
            {
            //When
                var result = sut.ReadRecord();
            //Then
                Assert.Equal("a", result[0]);
                Assert.Equal("b", result[1]);
                Assert.Equal("c", result[2]);
            }
        }

        [Fact]
        public void ReadRecord_SingleRecordSingleField_ReturnMatchingRecord()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("a")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv))
            {
            //When
                var result = sut.ReadRecord();
            //Then
                Assert.Equal("a", result[0]);
            }
        }

        [Fact]
        public void ReadRecord_SingleRecordSingleField_ReturnOnlyOneField()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("a")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv))
            {
            //When
                var result = sut.ReadRecord();
            //Then
                Assert.Single(result.Row);
            }
        }

        [Fact]
        public void ReadRecord_SingleQuotedFieldContainingComma_ReturnsRecordWthComma()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("\"a,b\"")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv))
            {
            //When
                var result = sut.ReadRecord();
            //Then
                Assert.Equal("a,b", result[0]);
            }
        }

        [Fact]
        public void ReadRecord_SingleQuotedFieldContainingComma_ReturnsOnlyOneField()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("\"a,b\"")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv))
            {
            //When
                var result = sut.ReadRecord();
            //Then
                Assert.Single(result.Row);
            }
        }

        [Fact]
        public void ReadRecord_SingleQuotedFieldContainingLinebreak_ReturnsOnlyOneField()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("\"a\r\nb\"")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv))
            {
            //When
                var result = sut.ReadRecord();
            //Then
                Assert.Single(result.Row);
            }
        }

        [Fact]
        public void ReadRecord_SingleQuotedFieldContainingLinebreak_ReturnsFieldWithLinebreak()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("\"a\nb\"")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv))
            {
            //When
                var result = sut.ReadRecord();
            //Then
                Assert.Equal($"a{Environment.NewLine}b", result[0]);
            }
        }

        [Fact]
        public void ReadRecord_SingleQuotedFieldContainingCarriageReturnLinebreak_ReturnsFieldWithLinebreak()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("\"a\r\nb\"")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv))
            {
            //When
                var result = sut.ReadRecord();
            //Then
                Assert.Equal($"a{Environment.NewLine}b", result[0]);
            }
        }

        [Fact]
        public void ReadRecord_MultipleCalls_AdvanceRecords()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("a,b\nc,d")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv))
            {
            //When
                sut.ReadRecord();
                var result = sut.ReadRecord();
            //Then
                Assert.Equal("c", result[0]);
            }
        }

        [Fact]
        public void ReadRecord_CsvWithPileDelimiter_ParsesWithPipe()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("a|b|c")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv, '|', '"'))
            {
            //When
                var result = sut.ReadRecord();
            //Then
                Assert.Equal(new string[]{"a", "b", "c"}, result.Row);
            }
        }

        [Fact]
        public void ReadRecord_CsvWithHyphenQuote_ParsesFieldsQuotedQithHyphens()
        {
            //Given
            using(var csv = new MemoryStream(Encoding.UTF8.GetBytes("a,-b,c-,d")))
            using(var sut = new w4g3n3r.IO.CsvReader(csv, ',', '-'))
            {
            //When
                var result = sut.ReadRecord();
            //Then
                Assert.Equal(new string[]{"a", "b,c", "d"}, result.Row);
            }
        }
    }
}
