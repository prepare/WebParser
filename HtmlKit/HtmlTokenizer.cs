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
        int cdataIndex;
        bool isEndTag;
        char quote;

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



        static bool IsAlphaNumeric(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
        }

        static bool IsAsciiLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        static char ToLower(char c)
        {
            return (c >= 'A' && c <= 'Z') ? (char)(c + 0x20) : c;
        }

        int Peek()
        {
            return text.Peek();
        }

        int Read()
        {
            int c;

            if ((c = text.Read()) == -1)
                return -1;

            if (c == '\n')
            {
                LinePosition = 1;
                LineNumber++;
            }
            else
            {
                LinePosition++;
            }

            return c;
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
        /// <summary>
        /// 8.2.4.1 Data state
        /// </summary>
        void R01_DataToken()
        {
            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    break;
                }

                c = (char)nc;

                switch (c)
                {
                    case '&':
                        if (DecodeCharacterReferences)
                        {
                            TokenizerState = HtmlTokenizerState.CharacterReferenceInData;
                            return;
                        }

                        goto default;
                    case '<':
                        TokenizerState = HtmlTokenizerState.TagOpen;
                        break;
                    //case 0: // parse error, but emit it anyway
                    default:
                        data.Append(c);

                        // Note: we emit at 1024 characters simply to avoid
                        // consuming too much memory.
                        if (data.Length >= 1024)
                        {
                            EmitDataToken(DecodeCharacterReferences);
                            return;
                        }

                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.Data);

            EmitDataToken(DecodeCharacterReferences);
        }

        /// <summary>
        /// 8.2.4.8 Tag open state
        /// </summary>        
        void R08_TagOpen()
        {
            int nc = Read();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                SetEmitToken(CreateDataToken("<"));
                return;
            }

            c = (char)nc;

            // Note: we save the data in case we hit a parse error and have to emit a data token
            data.Append('<');
            data.Append(c);

            switch ((c = (char)nc))
            {
                case '!': TokenizerState = HtmlTokenizerState.MarkupDeclarationOpen; break;
                case '?': TokenizerState = HtmlTokenizerState.BogusComment; break;
                case '/': TokenizerState = HtmlTokenizerState.EndTagOpen; break;
                default:
                    if (IsAsciiLetter(c))
                    {
                        TokenizerState = HtmlTokenizerState.TagName;
                        isEndTag = false;
                        name.Append(c);
                    }
                    else
                    {
                        TokenizerState = HtmlTokenizerState.Data;
                        return;
                    }
                    break;
            }
        }
        /// <summary>
        /// 8.2.4.9 End tag open state
        /// </summary>
        void R09_EndTagOpen()
        {
            int nc = Read();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitDataToken(false);
                return;
            }

            c = (char)nc;
            // Note: we save the data in case we hit a parse error and have to emit a data token
            data.Append(c);

            switch (c)
            {
                case '>': // parse error
                    TokenizerState = HtmlTokenizerState.Data;
                    data.Length = 0;
                    break;
                default:
                    if (IsAsciiLetter(c))
                    {
                        TokenizerState = HtmlTokenizerState.TagName;
                        isEndTag = true;
                        name.Append(c);
                    }
                    else
                    {
                        TokenizerState = HtmlTokenizerState.BogusComment;
                        return;
                    }
                    break;
            }

        }
        /// <summary>
        /// 8.2.4.10 Tag name state
        /// </summary>
        void R10_TagName()
        {
            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    name.Length = 0;

                    EmitDataToken(false);
                    return;
                }

                c = (char)nc;

                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);

                switch (c)
                {
                    case '\t':
                    case '\r':
                    case '\n':
                    case '\f':
                    case ' ':
                        TokenizerState = HtmlTokenizerState.BeforeAttributeName;
                        break;
                    case '/':
                        TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                        break;
                    case '>':
                        SetEmitToken(CreateTagTokenFromNameBuffer(isEndTag));
                        TokenizerState = HtmlTokenizerState.Data;
                        data.Length = 0;                         
                        return;
                    default:
                        name.Append(c == '\0' ? '\uFFFD' : c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.TagName);

            tag = CreateTagTokenFromNameBuffer(isEndTag);              
        }
        /// <summary>
        /// 8.2.4.43 Self-closing start tag state
        /// </summary>
        void R43_SelfClosingStartTag()
        {
            int nc = Read();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitDataToken(false);
                return;
            }

            c = (char)nc;

            if (c == '>')
            {
                tag.IsEmptyElement = true;

                EmitTagToken();
                return;
            }

            // parse error
            TokenizerState = HtmlTokenizerState.BeforeAttributeName;

            // Note: we save the data in case we hit a parse error and have to emit a data token
            data.Append(c);
        }


    }
}
