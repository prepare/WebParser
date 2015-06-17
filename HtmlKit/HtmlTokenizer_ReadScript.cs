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

 

namespace HtmlKit
{

    partial class HtmlTokenizer
    {
        void ReadScriptData()
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
                    case '<':
                        TokenizerState = HtmlTokenizerState.ScriptDataLessThan;
                        EmitScriptDataToken();
                        return;
                    default:
                        data.Append(c == '\0' ? '\uFFFD' : c);

                        // Note: we emit at 1024 characters simply to avoid
                        // consuming too much memory.
                        if (data.Length >= 1024)
                        {
                            EmitScriptDataToken();
                            return;
                        }
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptData);

            if (data.Length > 0)
            {
                EmitDataToken(false);
                return;
            }
            token = null;
        }
        void ReadScriptDataDoubleEscapeEnd()
        {
            do
            {
                int nc = Peek();
                char c = (char)nc;

                switch (c)
                {
                    case '\t':
                    case '\r':
                    case '\n':
                    case '\f':
                    case ' ':
                    case '/':
                    case '>':
                        if (NameIs("script"))
                            TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                        else
                            TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                        data.Append(c);
                        Read();
                        break;
                    default:
                        if (!IsAsciiLetter(c))
                        {
                            TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                        }
                        else
                        {
                            name.Append(c);
                            data.Append(c);
                            Read();
                        }
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataDoubleEscapeEnd);

            token = null;


        }
        void ReadScriptDataLessThan()
        {
            int nc = Peek();

            data.Append('<');

            switch ((char)nc)
            {
                case '/':
                    TokenizerState = HtmlTokenizerState.ScriptDataEndTagOpen;
                    data.Append('/');
                    name.Length = 0;
                    Read();
                    break;
                case '!':
                    TokenizerState = HtmlTokenizerState.ScriptDataEscapeStart;
                    data.Append('!');
                    Read();
                    break;
                default:
                    TokenizerState = HtmlTokenizerState.ScriptData;
                    break;
            }

            token = null;
        }

        void ReadScriptDataEndTagOpen()
        {
            int nc = Peek();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitScriptDataToken();
                return;
            }

            c = (char)nc;

            if (c == 'S' || c == 's')
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEndTagName;
                name.Append('s');
                data.Append(c);
                Read();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptData;
            }

            token = null;
        }

        void ReadScriptDataEndTagName()
        {
            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    name.Length = 0;
                    EmitScriptDataToken();
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
                        if (NameIs("script"))
                        {
                            TokenizerState = HtmlTokenizerState.BeforeAttributeName;
                            break;
                        }

                        goto default;
                    case '/':
                        if (NameIs("script"))
                        {
                            TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                            break;
                        }
                        goto default;
                    case '>':
                        if (NameIs("script"))
                        {
                            token = CreateTagToken(name.ToString(), true);
                            TokenizerState = HtmlTokenizerState.Data;
                            data.Length = 0;
                            name.Length = 0;
                            return;
                        }
                        goto default;
                    default:
                        if (!IsAsciiLetter(c))
                        {
                            TokenizerState = HtmlTokenizerState.ScriptData;
                            token = null;
                            return;
                        }

                        name.Append(c == '\0' ? '\uFFFD' : c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataEndTagName);

            tag = CreateTagToken(name.ToString(), true);
            name.Length = 0;
            token = null;
        }

        void ReadScriptDataEscapeStart()
        {
            int nc = Peek();

            if (nc == '-')
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEscapeStartDash;
                Read();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptData;
            }

            token = null;
        }

        void ReadScriptDataEscapeStartDash()
        {
            int nc = Peek();

            if (nc == '-')
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEscapedDashDash;
                Read();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptData;
            }

            token = null;
        }

        void ReadScriptDataEscaped()
        {
            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitScriptDataToken();
                    return;
                }

                c = (char)nc;

                switch (c)
                {
                    case '-':
                        TokenizerState = HtmlTokenizerState.ScriptDataEscapedDash;
                        data.Append('-');
                        break;
                    case '<':
                        TokenizerState = HtmlTokenizerState.ScriptDataEscapedLessThan;
                        break;
                    default:
                        data.Append(c == '\0' ? '\uFFFD' : c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);

            token = null;
        }

        void ReadScriptDataEscapedDash()
        {
            int nc = Peek();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitScriptDataToken();
                return;
            }

            switch ((c = (char)nc))
            {
                case '-':
                    TokenizerState = HtmlTokenizerState.ScriptDataEscapedDashDash;
                    data.Append('-');
                    break;
                case '<':
                    TokenizerState = HtmlTokenizerState.ScriptDataEscapedLessThan;
                    break;
                default:
                    TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                    data.Append(c == '\0' ? '\uFFFD' : c);
                    break;
            }

            token = null;
        }

        void ReadScriptDataEscapedDashDash()
        {
            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitScriptDataToken();
                    return;
                }

                c = (char)nc;

                switch (c)
                {
                    case '-':
                        TokenizerState = HtmlTokenizerState.ScriptDataEscapedDash;
                        data.Append('-');
                        break;
                    case '<':
                        TokenizerState = HtmlTokenizerState.ScriptDataEscapedLessThan;
                        break;
                    case '>':
                        TokenizerState = HtmlTokenizerState.ScriptData;
                        data.Append('>');
                        break;
                    default:
                        TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                        data.Append(c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);

            token = null;


        }

        void ReadScriptDataEscapedLessThan()
        {
            int nc = Peek();
            char c = (char)nc;

            if (c == '/')
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEndTagOpen;
                name.Length = 0;
                Read();
            }
            else if (IsAsciiLetter(c))
            {
                TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                data.Append('<');
                data.Append(c);
                name.Append(c);
                Read();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                data.Append('<');
            }

            token = null;


        }

        void ReadScriptDataEscapedEndTagOpen()
        {
            int nc = Peek();
            char c;

            data.Append("</");

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitScriptDataToken();
                return;
            }

            c = (char)nc;

            if (IsAsciiLetter(c))
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEscapedEndTagName;
                name.Append(c);
                Read();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
            }

            token = null;


        }

        void ReadScriptDataEscapedEndTagName()
        {
            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    name.Length = 0;

                    EmitScriptDataToken();
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
                        if (NameIs("script"))
                        {
                            TokenizerState = HtmlTokenizerState.BeforeAttributeName;
                            break;
                        }

                        goto default;
                    case '/':
                        if (NameIs("script"))
                        {
                            TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                            break;
                        }
                        goto default;
                    case '>':
                        if (NameIs("script"))
                        {
                            token = CreateTagToken(name.ToString(), true);
                            TokenizerState = HtmlTokenizerState.Data;
                            data.Length = 0;
                            name.Length = 0;
                            return;
                        }
                        goto default;
                    default:
                        if (!IsAsciiLetter(c))
                        {
                            TokenizerState = HtmlTokenizerState.ScriptData;
                            data.Append(c);
                            token = null;
                            return;
                        }

                        name.Append(c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataEscapedEndTagName);

            tag = CreateTagToken(name.ToString(), true);
            name.Length = 0;
            token = null;


        }

        void ReadScriptDataDoubleEscapeStart()
        {
            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    name.Length = 0;

                    EmitScriptDataToken();
                    return;
                }

                c = (char)nc;

                data.Append(c);

                switch (c)
                {
                    case '\t':
                    case '\r':
                    case '\n':
                    case '\f':
                    case ' ':
                    case '/':
                    case '>':
                        if (NameIs("script"))
                            TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                        else
                            TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                        name.Length = 0;
                        break;
                    default:
                        if (!IsAsciiLetter(c))
                            TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                        else
                            name.Append(c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataDoubleEscapeStart);

            token = null;


        }

        void ReadScriptDataDoubleEscaped()
        {
            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitScriptDataToken();
                    return;
                }

                c = (char)nc;

                switch (c)
                {
                    case '-':
                        TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedDash;
                        data.Append('-');
                        break;
                    case '<':
                        TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
                        break;
                    default:
                        data.Append(c == '\0' ? '\uFFFD' : c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);

            token = null;


        }

        void ReadScriptDataDoubleEscapedDash()
        {
            int nc = Peek();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitScriptDataToken();
                return;
            }

            switch ((c = (char)nc))
            {
                case '-':
                    TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedDashDash;
                    data.Append('-');
                    break;
                case '<':
                    TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
                    break;
                default:
                    TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                    data.Append(c == '\0' ? '\uFFFD' : c);
                    break;
            }

            token = null;


        }

        void ReadScriptDataDoubleEscapedDashDash()
        {
            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitScriptDataToken();
                    return;
                }

                c = (char)nc;

                switch (c)
                {
                    case '-':
                        data.Append('-');
                        break;
                    case '<':
                        TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
                        data.Append('<');
                        break;
                    case '>':
                        TokenizerState = HtmlTokenizerState.ScriptData;
                        data.Append('>');
                        break;
                    default:
                        TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                        data.Append(c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);

            token = null;


        }

        void ReadScriptDataDoubleEscapedLessThan()
        {
            int nc = Peek();

            if (nc == '/')
            {
                TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapeEnd;
                data.Append('/');
                Read();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
            }

            token = null;


        }
    }
}