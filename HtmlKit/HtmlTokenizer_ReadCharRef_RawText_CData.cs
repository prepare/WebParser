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





        void ReadGenericRawTextLessThan(HtmlTokenizerState rawText, HtmlTokenizerState rawTextEndTagOpen)
        {
    

            char c;
            if(!Peek(out c))
            {
                //?
                throw new System.NotSupportedException();
            }

            data.Append('<');
            switch (c)
            {
                case '/':
                    TokenizerState = rawTextEndTagOpen;
                    data.Append('/');
                    name.Length = 0;
                    ReadNext();
                    break;
                default:
                    TokenizerState = rawText;
                    break;
            }

        }

        void ReadGenericRawTextEndTagOpen(bool decoded, HtmlTokenizerState rawText, HtmlTokenizerState rawTextEndTagName)
        {
             
            char c;
            if(!Peek(out c))             
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitDataToken(decoded);
                return;
            }
             
            if (IsAsciiLetter(c))
            {
                TokenizerState = rawTextEndTagName;
                name.Append(c);
                data.Append(c);
                ReadNext();
            }
            else
            {
                TokenizerState = rawText;
            }
        }

        void ReadGenericRawTextEndTagName(bool decoded, HtmlTokenizerState rawText)
        {
            var current = TokenizerState;

            do
            {

                char c;
                if (!ReadNext(out c))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    name.Length = 0; 
                    EmitDataToken(decoded);
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
                        if (NameIs(activeTagName))
                        {
                            TokenizerState = HtmlTokenizerState.BeforeAttributeName;
                            break;
                        }

                        goto default;
                    case '/':
                        if (NameIs(activeTagName))
                        {
                            TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                            break;
                        }
                        goto default;
                    case '>':
                        if (NameIs(activeTagName))
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
                            TokenizerState = rawText;
                            return;
                        }

                        name.Append(c == '\0' ? '\uFFFD' : c);
                        break;
                }
            } while (TokenizerState == current);

            tag = CreateTagTokenFromNameBuffer(true);
        }

        /// <summary>
        /// 8.2.4.2 Character reference in data state
        /// </summary>
        void R02_CharacterReferenceInData()
        {
            ReadCharacterReference(HtmlTokenizerState.Data);
        }
        /// <summary>
        /// 8.2.4.3 RCDATA state
        /// </summary>
        void R03_RcData()
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
                    case '&':
                        if (DecodeCharacterReferences)
                        {
                            TokenizerState = HtmlTokenizerState.CharacterReferenceInRcData;
                            return;
                        }

                        goto default;
                    case '<':
                        TokenizerState = HtmlTokenizerState.RcDataLessThan;
                        EmitDataToken(DecodeCharacterReferences);
                        return;
                    default:
                        data.Append(c == '\0' ? '\uFFFD' : c);

                        // Note: we emit at 1024 characters simply to avoid
                        // consuming too much memory.
                        if (data.Length >= 1024)
                        {
                            EmitDataToken(DecodeCharacterReferences);
                        }

                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.RcData);

            if (data.Length > 0)
            {
                EmitDataToken(DecodeCharacterReferences);
                return;
            }
        }

        /// <summary>
        /// 8.2.4.4 Character reference in RCDATA state
        /// </summary>
        void R04_CharacterReferenceInRcData()
        {
            ReadCharacterReference(HtmlTokenizerState.RcData);

        }


        /// <summary>
        /// 8.2.4.5 RAWTEXT state
        /// </summary>
        void R05_RawText()
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
                        TokenizerState = HtmlTokenizerState.RawTextLessThan;
                        EmitDataToken();
                        return;
                    default:
                        data.Append(c == '\0' ? '\uFFFD' : c);

                        // Note: we emit at 1024 characters simply to avoid
                        // consuming too much memory.
                        if (data.Length >= 1024)
                        {
                            EmitDataToken();
                            return;
                        }

                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.RawText);

            if (data.Length > 0)
            {
                EmitDataToken();
            }
        }


        /// <summary>
        /// 8.2.4.7 PLAINTEXT state
        /// </summary>
        void R07_PlainText()
        {

            char c;
            while (ReadNext(out c))
            {
                data.Append(c == '\0' ? '\uFFFD' : c);
                // Note: we emit at 1024 characters simply to avoid
                // consuming too much memory.
                if (data.Length >= 1024)
                {
                    EmitDataToken();
                    return;
                }
            }

            TokenizerState = HtmlTokenizerState.EndOfFile;

            EmitDataToken();
        }

        void ReadCharacterReference(HtmlTokenizerState next)
        {
             
            char c;
            if(!Peek(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                data.Append('&');
                EmitDataToken(true);
                return;
            } 
            switch (c)
            {
                case '\t':
                case '\r':
                case '\n':
                case '\f':
                case ' ':
                case '<':
                case '&':
                    // no character is consumed, emit '&'
                    TokenizerState = next;
                    data.Append('&');
                    return;
            }

            entity.Push('&');

            while (entity.Push(c))
            {
                ReadNext();

                if (!Peek(out c))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    data.Append(entity.GetPushedInput());
                    entity.Reset();

                    EmitDataToken(true);
                    return;
                } 
            }

            TokenizerState = next;

            data.Append(entity.GetValue());
            entity.Reset();

            if (c == ';')
            {
                // consume the ';'
                ReadNext();
            }


        }
        /// <summary>
        /// 8.2.4.14 RAWTEXT less-than sign state
        /// </summary>
        void R14_RawTextLessThan()
        {
            ReadGenericRawTextLessThan(HtmlTokenizerState.RawText, HtmlTokenizerState.RawTextEndTagOpen);
        }
        /// <summary>
        /// 8.2.4.15 RAWTEXT end tag open state
        /// </summary>
        void R15_RawTextEndTagOpen()
        {
            ReadGenericRawTextEndTagOpen(false, HtmlTokenizerState.RawText, HtmlTokenizerState.RawTextEndTagName);
        }
        /// <summary>
        /// 8.2.4.16 RAWTEXT end tag name state
        /// </summary>
        void R16_RawTextEndTagName()
        {
            ReadGenericRawTextEndTagName(false, HtmlTokenizerState.RawText);
        }


        /// <summary>
        /// 8.2.4.11 RCDATA less-than sign state
        /// </summary>
        void R11_RcDataLessThan()
        {
            ReadGenericRawTextLessThan(HtmlTokenizerState.RcData, HtmlTokenizerState.RcDataEndTagOpen);
        }
        /// <summary>
        /// 8.2.4.12 RCDATA end tag open state
        /// </summary>
        void R12_RcDataEndTagOpen()
        {
            ReadGenericRawTextEndTagOpen(DecodeCharacterReferences, HtmlTokenizerState.RcData, HtmlTokenizerState.RcDataEndTagName);
        }
        /// <summary>
        /// 8.2.4.13 RCDATA end tag name state
        /// </summary>
        void R13_RcDataEndTagName()
        {
            ReadGenericRawTextEndTagName(DecodeCharacterReferences, HtmlTokenizerState.RcData);
        }


        /// <summary>
        /// 8.2.4.68 CDATA section state
        /// </summary>
        void R68_CDataSection()
        {

            char c;
            while (ReadNext(out c))
            {


                if (cdataIndex >= 3)
                {
                    data.Append(cdata[0]);
                    cdata[0] = cdata[1];
                    cdata[1] = cdata[2];
                    cdata[2] = c;

                    if (cdata[0] == ']' && cdata[1] == ']' && cdata[2] == '>')
                    {
                        TokenizerState = HtmlTokenizerState.Data;
                        cdataIndex = 0;

                        EmitCDataToken();
                        return;
                    }
                }
                else
                {
                    cdata[cdataIndex++] = c;
                } 
            }

            TokenizerState = HtmlTokenizerState.EndOfFile;

            for (int i = 0; i < cdataIndex; i++)
                data.Append(cdata[i]);

            cdataIndex = 0;

            EmitCDataToken();
            return;
        }

    }
}