//
// HtmlTokenizer.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System.IO;
using System.Text;

namespace HtmlKit
{
    /// <summary>
    /// An HTML tokenizer.
    /// </summary>
    /// <remarks>
    /// Tokenizes HTML text, emitting an <see cref="HtmlToken"/> for each token it encounters.
    /// </remarks>
    public partial class HtmlTokenizer
    {
        const string DocType = "doctype";
        const string CData = "[CDATA[";

        readonly HtmlEntityDecoder entity = new HtmlEntityDecoder();
        readonly StringBuilder data = new StringBuilder();
        readonly StringBuilder name = new StringBuilder();
        readonly char[] cdata = new char[3];
        HtmlDocTypeToken doctype;
        HtmlAttribute attribute;
        string activeTagName;
        HtmlTagToken tag;
       
        bool isEndTag;
        int quote;

        TextReader text;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlTokenizer"/> class.
        /// </summary>
        /// <remarks>
        /// Creates a new <see cref="HtmlTokenizer"/>.
        /// </remarks>
        /// <param name="reader">The <see cref="TextReader"/>.</param>
        public HtmlTokenizer(TextReader reader)
        {
            DecodeCharacterReferences = true;
            LinePosition = 1;
            LineNumber = 1;
            text = reader;
        }

        /// <summary>
        /// Get or set whether or not the tokenizer should decode character references.
        /// </summary>
        /// <remarks>
        /// <para>Gets or sets whether or not the tokenizer should decode character references.</para>
        /// <para>Note: Character references in attribute values will still be decoded even if this
        /// value is set to <c>false</c>.</para>
        /// </remarks>
        /// <value><c>true</c> if character references should be decoded; otherwise, <c>false</c>.</value>
        public bool DecodeCharacterReferences
        {
            get; set;
        }

        /// <summary>
        /// Get the current HTML namespace detected by the tokenizer.
        /// </summary>
        /// <remarks>
        /// Gets the current HTML namespace detected by the tokenizer.
        /// </remarks>
        /// <value>The html namespace.</value>
        public HtmlNamespace HtmlNamespace
        {
            get; private set;
        }

        /// <summary>
        /// Gets the current line number.
        /// </summary>
        /// <remarks>
        /// <para>This property is most commonly used for error reporting, but can be called
        /// at any time. The starting value for this property is <c>1</c>.</para>
        /// <para>Combined with <see cref="LinePosition"/>, a value of <c>1,1</c> indicates
        /// the start of the document.</para>
        /// </remarks>
        /// <value>The current line number.</value>
        public int LineNumber
        {
            get; private set;
        }

        /// <summary>
        /// Gets the current line position.
        /// </summary>
        /// <remarks>
        /// <para>This property is most commonly used for error reporting, but can be called
        /// at any time. The starting value for this property is <c>1</c>.</para>
        /// <para>Combined with <see cref="LineNumber"/>, a value of <c>1,1</c> indicates
        /// the start of the document.</para>
        /// </remarks>
        /// <value>The current line number.</value>
        public int LinePosition
        {
            get; private set;
        }

        /// <summary>
        /// Get the current state of the tokenizer.
        /// </summary>
        /// <remarks>
        /// Gets the current state of the tokenizer.
        /// </remarks>
        /// <value>The current state of the tokenizer.</value>
        public HtmlTokenizerState TokenizerState
        {
            get; private set;
        }

        static char ToLower(char c)
        {
            return (c >= 'A' && c <= 'Z') ? (char)(c + 0x20) : c;
        }

        bool Peek(out char c)
        {
            int nc = text.Peek();
            if (nc == -1)
            {
                c = '\0';
                return false;
            }
            c = (char)nc;
            return true;
        }

        void ReadNext()
        {
            char c;
            ReadNext(out c);
        }

        bool ReadNext(out char c)
        {
            int nc;
            if ((nc = text.Read()) == -1)
            {
                c = '\0';
                return false;
            }

            c = (char)nc;
            switch (c)
            {
                case '\n':
                    LinePosition = 1;
                    LineNumber++;
                    return true;
                default:
                    LinePosition++;
                    return true;
            }
        }

        bool Peek(out char c, out CharMode charMode)
        {
            int nc = text.Peek();
            if (nc == -1)
            {
                c = '\0';
                charMode = CharMode.Eof;
                return false;
            }
            c = (char)nc;
            switch (c)
            {
                case '\n':
                    charMode = CharMode.NewLine;
                    return true;
                case '\t':
                case ' ':
                case '\r':
                case '\f':
                    charMode = CharMode.WhiteSpace;
                    return true;
                case '!':
                    charMode = CharMode.Bang;
                    return true;
                case '/':
                    charMode = CharMode.Slash;
                    return true;
                case '?':
                    charMode = CharMode.Quest;
                    return true;
                case '>':
                    charMode = CharMode.Gt;
                    return true;
                case '<':
                    charMode = CharMode.Lt;
                    return true;
                case '=': 
                    charMode = CharMode.Assign;
                    return true;
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                    charMode = CharMode.LowerAsciiLetter;
                    return true;
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                    charMode = CharMode.UpperAsciiLetter;
                    return true;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    charMode = CharMode.Numeric;
                    return true;
                default:
                    charMode = CharMode.Others;
                    return true;
            }
        }

        /// <summary>
        /// read next and analyze char group
        /// </summary>
        /// <param name="c"></param>
        /// <param name="charMode"></param>
        /// <returns></returns>
        bool ReadNext(out char c, out CharMode charMode)
        {
            int nc;
            if ((nc = text.Read()) == -1)
            {
                c = '\0';
                charMode = CharMode.Eof;
                return false;
            }

            c = (char)nc;
            switch (c)
            {
                case '\n':
                    LinePosition = 1;
                    LineNumber++;
                    charMode = CharMode.NewLine;
                    return true;
                case '\t':
                case ' ':
                case '\r':
                case '\f':
                    LinePosition++;
                    charMode = CharMode.WhiteSpace;
                    return true;
                case '!':
                    LinePosition++;
                    charMode = CharMode.Bang;
                    return true;
                case '/':
                    LinePosition++;
                    charMode = CharMode.Slash;
                    return true;
                case '?':
                    LinePosition++;
                    charMode = CharMode.Quest;
                    return true;
                case '=':
                    LinePosition++;
                    charMode = CharMode.Assign;
                    return true;
                case '>':
                    LinePosition++;
                    charMode = CharMode.Gt;
                    return true;
                case '<':
                    LinePosition++;
                    charMode = CharMode.Lt;
                    return true;
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                    charMode = CharMode.LowerAsciiLetter;
                    LinePosition++;
                    return true;
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                    charMode = CharMode.UpperAsciiLetter;
                    LinePosition++;
                    return true;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    charMode = CharMode.Numeric;
                    LinePosition++;
                    return true;
                default:
                    charMode = CharMode.Others;
                    LinePosition++;
                    return true;
            }
        }
        // Note: value must be lowercase
        bool NameIs(string value)
        {
            if (name.Length != value.Length)
                return false;

            for (int i = 0; i < name.Length; i++)
            {
                if (ToLower(name[i]) != value[i])
                    return false;
            }

            return true;
        }

       

    }
}
