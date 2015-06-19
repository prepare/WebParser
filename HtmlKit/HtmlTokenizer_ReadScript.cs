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
        /// <summary>
        /// 8.2.4.6 Script data state
        /// </summary>
        void R06_ScriptData()
        {
            do
            {

                char c;
                if (!ReadNext(out c))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    break;
                }

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
                EmitDataToken();
            }
        }
        /// <summary>
        /// 8.2.4.33 Script data double escape end state
        /// </summary>
        void R33_ScriptDataDoubleEscapeEnd()
        {
            do
            {
                char c;
                if (!Peek(out c))
                { throw new System.Exception(); }

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
                        ReadNext();
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
                            ReadNext();
                        }
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataDoubleEscapeEnd);


        }
        /// <summary>
        /// 8.2.4.17 Script data less-than sign state
        /// </summary>
        void R17_ScriptDataLessThan()
        {

            char c;
            if (!Peek(out c))
            {
                throw new System.NotSupportedException(); //?
            }

            data.Append('<');
            switch (c)
            {
                case '/':
                    TokenizerState = HtmlTokenizerState.ScriptDataEndTagOpen;
                    data.Append('/');
                    name.Length = 0;
                    ReadNext();
                    break;
                case '!':
                    TokenizerState = HtmlTokenizerState.ScriptDataEscapeStart;
                    data.Append('!');
                    ReadNext();
                    break;
                default:
                    TokenizerState = HtmlTokenizerState.ScriptData;
                    break;
            }
        }
        /// <summary>
        /// 8.2.4.18 Script data end tag open state
        /// </summary>
        void R18_ScriptDataEndTagOpen()
        {

            char c;
            if (!Peek(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitScriptDataToken();
                return;
            }

            if (c == 'S' || c == 's')
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEndTagName;
                name.Append('s');
                data.Append(c);
                ReadNext();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptData;
            }
        }
        /// <summary>
        /// 8.2.4.19 Script data end tag name state
        /// </summary>
        void R19_ScriptDataEndTagName()
        {
            do
            {

                char c;
                if (!ReadNext(out c))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    name.Length = 0;
                    EmitScriptDataToken();
                    return;
                }

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

                            SetEmitToken(CreateTagTokenFromNameBuffer(true));
                            TokenizerState = HtmlTokenizerState.Data;
                            data.Length = 0;

                            return;
                        }
                        goto default;
                    default:
                        if (!IsAsciiLetter(c))
                        {
                            TokenizerState = HtmlTokenizerState.ScriptData;

                            return;
                        }

                        name.Append(c == '\0' ? '\uFFFD' : c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataEndTagName);

            tag = CreateTagTokenFromNameBuffer(true);

        }
        /// <summary>
        /// 8.2.4.20 Script data escape start state
        /// </summary>
        void R20_ScriptDataEscapeStart()
        {

            char c;
            if (!Peek(out c)) { throw new System.NotSupportedException(); }
            if (c == '-')
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEscapeStartDash;
                ReadNext();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptData;
            }

        }
        /// <summary>
        /// 8.2.4.21 Script data escape start dash state
        /// </summary>
        void R21_ScriptDataEscapeStartDash()
        {
            char c;
            if (!Peek(out c))
            { throw new System.Exception(); }

            if (c == '-')
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEscapedDashDash;
                ReadNext();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptData;
            }
        }
        /// <summary>
        /// 8.2.4.22 Script data escaped state
        /// </summary>
        void R22_ScriptDataEscaped()
        {
            do
            {

                char c;
                if (!ReadNext(out c))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitScriptDataToken();
                    return;
                }

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
        }
        /// <summary>
        /// 8.2.4.23 Script data escaped dash state
        /// </summary>
        void R23_ScriptDataEscapedDash()
        {

            char c;
            if (!Peek(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitScriptDataToken();
                return;
            }

            switch (c)
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

        }
        /// <summary>
        /// 8.2.4.24 Script data escaped dash dash state
        /// </summary>
        void R24_ScriptDataEscapedDashDash()
        {
            do
            {

                char c;
                if (!ReadNext(out c))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitScriptDataToken();
                    return;
                }

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
        }
        /// <summary>
        /// 8.2.4.25 Script data escaped less-than sign state
        /// </summary>
        void R25_ScriptDataEscapedLessThan()
        {


            char c;
            if (!Peek(out c))
            { throw new System.Exception(); }

            if (c == '/')
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEndTagOpen;
                name.Length = 0;
                ReadNext();
            }
            else if (IsAsciiLetter(c))
            {
                TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                data.Append('<');
                data.Append(c);
                name.Append(c);
                ReadNext();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                data.Append('<');
            }
        }
        /// <summary>
        /// 8.2.4.26 Script data escaped end tag open state
        /// </summary>
        void R26_ScriptDataEscapedEndTagOpen()
        {


            data.Append("</");
            char c;
            if (!Peek(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitScriptDataToken();
                return;
            }

            if (IsAsciiLetter(c))
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEscapedEndTagName;
                name.Append(c);
                ReadNext();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
            }
        }
        /// <summary>
        /// 8.2.4.27 Script data escaped end tag name state
        /// </summary>
        void R27_ScriptDataEscapedEndTagName()
        {
            do
            {

                char c;
                if (!ReadNext(out c))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    name.Length = 0; 
                    EmitScriptDataToken();
                    return;
                }

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
                            SetEmitToken(CreateTagTokenFromNameBuffer(true));
                            TokenizerState = HtmlTokenizerState.Data;
                            data.Length = 0;
                            return;
                        }
                        goto default;
                    default:
                        if (!IsAsciiLetter(c))
                        {
                            TokenizerState = HtmlTokenizerState.ScriptData;
                            data.Append(c);
                            return;
                        }

                        name.Append(c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.ScriptDataEscapedEndTagName);

            tag = CreateTagTokenFromNameBuffer(true);
        }

        /// <summary>
        /// 8.2.4.28 Script data double escape start state
        /// </summary>
        void R28_ScriptDataDoubleEscapeStart()
        {
            do
            {

                char c;
                if ((!ReadNext(out c)))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    name.Length = 0;
                    EmitScriptDataToken();
                    return;
                }

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
        }
        /// <summary>
        /// 8.2.4.29 Script data double escaped state
        /// </summary>
        void R29_ScriptDataDoubleEscaped()
        {
            do
            {

                char c;
                if (!(ReadNext(out c)))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitScriptDataToken();
                    return;
                }

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
        }
        /// <summary>
        /// 8.2.4.30 Script data double escaped dash state
        /// </summary>
        void R30_ScriptDataDoubleEscapedDash()
        {

            char c;
            if (!Peek(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitScriptDataToken();
                return;
            }
            switch (c)
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

        }
        /// <summary>
        /// 8.2.4.31 Script data double escaped dash dash state
        /// </summary>
        void R31_ScriptDataDoubleEscapedDashDash()
        {
            do
            {

                char c;
                if (!ReadNext(out c))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitScriptDataToken();
                    return;
                }

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

        }
        /// <summary>
        /// 8.2.4.32 Script data double escaped less-than sign state
        /// </summary>
        void R32_ScriptDataDoubleEscapedLessThan()
        {
             
            char c;
            if (!Peek(out c))
            { throw new System.Exception(); } //?

            if (c == '/')
            {
                TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapeEnd;
                data.Append('/');
                ReadNext();
            }
            else
            {
                TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
            }
        }
    }
}