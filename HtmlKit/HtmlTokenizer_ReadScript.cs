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
            char c;
            while (ReadNext(out c))
            {
                switch (c)
                {
                    case '<':
                        TokenizerState = HtmlTokenizerState.ScriptDataLessThan;
                        EmitScriptDataToken();
                        return;
                    case '\0':
                        c = '\uFFFD';
                        goto default;
                    default:
                        data.Append(c);
                        // Note: we emit at 1024 characters simply to avoid
                        // consuming too much memory.
                        if (data.Length >= 1024)
                        {
                            EmitScriptDataToken();
                            return;
                        }
                        break;
                }
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile; 
            EmitDataToken();

        }
        /// <summary>
        /// 8.2.4.33 Script data double escape end state
        /// </summary>
        void R33_ScriptDataDoubleEscapeEnd()
        {
            char c;
            CharMode charMode;
            while (Peek(out c, out charMode))
            {
                switch (charMode)
                {
                    default:
                        TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                        return;
                    case CharMode.NewLine:
                    case CharMode.WhiteSpace:
                    case CharMode.Gt:
                    case CharMode.Slash:
                        if (NameIs("script"))
                            TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                        else
                            TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                        data.Append(c);
                        ReadNext();
                        return;
                    case CharMode.UpperAsciiLetter:
                    case CharMode.LowerAsciiLetter:
                        name.Append(c);
                        data.Append(c);
                        ReadNext();
                        break;
                }
            }


            //eof?
            throw new System.Exception();
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

            char c;
            CharMode charMode;
            while (ReadNext(out c, out charMode))
            {

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
                            tag = CreateTagTokenFromNameBuffer(true);
                            return;
                        }

                        goto default;
                    case '/':
                        if (NameIs("script"))
                        {
                            TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                            tag = CreateTagTokenFromNameBuffer(true);
                            return;
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
                        switch (charMode)
                        {
                            default:
                                TokenizerState = HtmlTokenizerState.ScriptData;
                                tag = CreateTagTokenFromNameBuffer(true);
                                return;
                            case CharMode.NullChar:
                                name.Append('\uFFFD');
                                break;
                            case CharMode.LowerAsciiLetter:
                            case CharMode.UpperAsciiLetter:
                                name.Append(c);
                                break;
                        }
                        break;
                }
            }
            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            name.Length = 0;
            EmitScriptDataToken();

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
            char c;
            while (ReadNext(out c))
            {
                switch (c)
                {
                    case '-':
                        TokenizerState = HtmlTokenizerState.ScriptDataEscapedDash;
                        data.Append('-');
                        return;
                    case '<':
                        TokenizerState = HtmlTokenizerState.ScriptDataEscapedLessThan;
                        return;
                    case '\0':
                        c = '\uFFFD';
                        goto default;
                    default:
                        data.Append(c);
                        break;
                }
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            EmitScriptDataToken();

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
                case '\0':
                    c = '\uFFFD';
                    goto default;
                default:
                    TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                    data.Append(c);
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
                        //TODO :review here again
                        TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                        data.Append(c);
                        break;
                }
                //TODO :review here again
            } while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);
        }
        /// <summary>
        /// 8.2.4.25 Script data escaped less-than sign state
        /// </summary>
        void R25_ScriptDataEscapedLessThan()
        {


            char c;
            CharMode charMode;
            if (!Peek(out c, out charMode))
            { throw new System.Exception(); }

            switch (charMode)
            {
                case CharMode.UpperAsciiLetter:
                case CharMode.LowerAsciiLetter:
                    TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                    data.Append('<');
                    data.Append(c);
                    name.Append(c);
                    ReadNext();
                    break;
                case CharMode.Slash:
                    TokenizerState = HtmlTokenizerState.ScriptDataEndTagOpen;
                    name.Length = 0;
                    ReadNext();
                    break;
                default:
                    TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                    data.Append('<');
                    break;
            }

        }
        /// <summary>
        /// 8.2.4.26 Script data escaped end tag open state
        /// </summary>
        void R26_ScriptDataEscapedEndTagOpen()
        {


            data.Append("</");
            char c;
            CharMode charMode;
            if (!Peek(out c, out charMode))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitScriptDataToken();
                return;
            }
            switch (charMode)
            {
                //if (IsAsciiLetter(c))
                case CharMode.UpperAsciiLetter:
                case CharMode.LowerAsciiLetter:
                    TokenizerState = HtmlTokenizerState.ScriptDataEscapedEndTagName;
                    name.Append(c);
                    ReadNext();
                    break;
                default:
                    TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                    break;
            }
        }
        /// <summary>
        /// 8.2.4.27 Script data escaped end tag name state
        /// </summary>
        void R27_ScriptDataEscapedEndTagName()
        {
            char c;
            CharMode charMode;
            while (ReadNext(out c, out charMode))
            {
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
                            tag = CreateTagTokenFromNameBuffer(true);
                            return;
                        }

                        goto default;
                    case '/':
                        if (NameIs("script"))
                        {
                            TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                            tag = CreateTagTokenFromNameBuffer(true);
                            return;
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
                        switch (charMode)
                        {
                            default:
                                //if (!IsAsciiLetter(c))
                                TokenizerState = HtmlTokenizerState.ScriptData;
                                data.Append(c);
                                return;
                            case CharMode.LowerAsciiLetter:
                            case CharMode.UpperAsciiLetter:
                                name.Append(c);
                                break;
                        }
                        break;
                }
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            name.Length = 0;
            EmitScriptDataToken();
        }

        /// <summary>
        /// 8.2.4.28 Script data double escape start state
        /// </summary>
        void R28_ScriptDataDoubleEscapeStart()
        {

            char c;
            CharMode charMode;
            while (ReadNext(out c, out charMode))
            {
                data.Append(c);
                switch (charMode)
                {
                    case CharMode.LowerAsciiLetter:
                    case CharMode.UpperAsciiLetter:
                        name.Append(c);
                        break;

                    case CharMode.WhiteSpace:
                    case CharMode.NewLine:
                    case CharMode.Slash:
                    case CharMode.Gt:
                        if (NameIs("script"))
                            TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                        else
                            TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                        name.Length = 0;
                        return;
                    default:
                        //  if (!IsAsciiLetter(c))
                        TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
                        return;
                }
            }
            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            name.Length = 0;
            EmitScriptDataToken();
        }
        /// <summary>
        /// 8.2.4.29 Script data double escaped state
        /// </summary>
        void R29_ScriptDataDoubleEscaped()
        {
            char c;
            while (ReadNext(out c))
            {

                switch (c)
                {
                    case '-':
                        TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedDash;
                        data.Append('-');
                        return;
                    case '<':
                        TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
                        return;
                    case '\0':
                        c = '\uFFFD';
                        goto default;
                    default:
                        data.Append(c);
                        break;
                }
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            EmitScriptDataToken();

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
                case '\0':
                    c = '\uFFFD';
                    goto default;
                default:
                    TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                    data.Append(c);
                    break;
            }

        }
        /// <summary>
        /// 8.2.4.31 Script data double escaped dash dash state
        /// </summary>
        void R31_ScriptDataDoubleEscapedDashDash()
        {

            char c;
            while (ReadNext(out c))
            {
                data.Append(c);

                switch (c)
                {
                    case '-':
                        break;
                    case '<':
                        TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
                        return;
                    case '>':
                        TokenizerState = HtmlTokenizerState.ScriptData;
                        return;
                    default:
                        TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
                        return;
                }
            }


            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            EmitScriptDataToken();

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