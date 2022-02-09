using SerialExpress.Converter;
using SerialExpress.Model;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TestProject
{
    public class UnitTest_Converter
    {
        [Theory]
        [InlineData(@"01",new byte[] { 0x30, 0x31 })]
        [InlineData(@"01\r",new byte[] { 0x30, 0x31, 0x0d})]
        [InlineData(@"01\n", new byte[] { 0x30, 0x31, 0x0a })]
        [InlineData(@"01\r\n23", new byte[] { 0x30, 0x31, 0x0d, 0x0a, 0x32, 0x33 })]
        public void Test_s2b(string test, byte[] expected)
        {
            //string test_str = "Hello world";
            //byte[] expected = Encoding.ASCII.GetBytes(test_str);
            byte[] ans = StringToBytesConverter.Convert(test);
            Assert.Equal(expected, ans);
        }
        [Theory]
        [InlineData(new byte[] { 0x30, 0x31, 0x0d, 0x0a, 0x32, 0x33, 0x0d, 0x0a }, "0123")]
        [InlineData(new byte[] { 0x30, 0x31, 0x0d, 0x32, 0x33, 0x0a }, "0123")]
        public void Test_b2s(byte[] test, string expected)
        {
            string ans = BytesToStringConverter.Convert(test);
            Assert.Equal(expected, ans);
        }
    }
    public class UnitTest_Terminal
    {
        [Theory]
        [InlineData(new byte[] { 0x30, 0x31, 0x0d, 0x0a, 0x32, 0x33, 0x0d, 0x0a }, TerminalManager.TokenType.CRLF, new string[] {"01","23"})]
        [InlineData(new byte[] { 0x30, 0x31, 0x0d, 0x0a, 0x32, 0x33, 0x0d, 0x0a }, TerminalManager.TokenType.CR, new string[] {"01", "23" })]
        [InlineData(new byte[] { 0x30, 0x31, 0x0d, 0x0a, 0x32, 0x33, 0x0d, 0x0a }, TerminalManager.TokenType.LF, new string[] {"01","23"})]
        public void Test_DataReceived(byte[] test, TerminalManager.TokenType token, string[] expected_text)
        {
            TerminalManager tm = new TerminalManager();
            tm.Token = token;
            tm.Write(test);
            for (int i = 0; i < expected_text.Length; i++)
            {
                Assert.Equal(tm.DataList[i].Text, expected_text[i]);
            }
        }
    }
}