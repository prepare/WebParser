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
        /// 8.2.4.1 Data state
        /// </summary>
        void R01_DataToken()
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

            char c;
            CharMode charMode;
            if (!ReadNext(out c, out charMode))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                SetEmitToken(CreateDataToken("<"));
                return;
            }


            // Note: we save the data in case we hit a parse error and have to emit a data token
            data.Append('<');
            data.Append(c);

            switch (charMode)
            {
                case CharMode.Bang: TokenizerState = HtmlTokenizerState.MarkupDeclarationOpen; break;
                case CharMode.Quest: TokenizerState = HtmlTokenizerState.BogusComment; break;
                case CharMode.Slash: TokenizerState = HtmlTokenizerState.EndTagOpen; break;
                default:
                    TokenizerState = HtmlTokenizerState.Data;
                    return;
                case CharMode.UpperAsciiLetter:
                case CharMode.LowerAsciiLetter:
                    TokenizerState = HtmlTokenizerState.TagName;
                    isEndTag = false;
                    name.Append(c);
                    break;
            }
        }
        /// <summary>
        /// 8.2.4.9 End tag open state
        /// </summary>
        void R09_EndTagOpen()
        {

            char c;
            CharMode charMode;
            if (!ReadNext(out c, out charMode))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitDataToken();
                return;
            }

            // Note: we save the data in case we hit a parse error and have to emit a data token
            data.Append(c);


            switch (charMode)
            {
                default:
                    TokenizerState = HtmlTokenizerState.BogusComment;
                    return;
                case CharMode.Gt:// parse error
                    TokenizerState = HtmlTokenizerState.Data;
                    data.Length = 0;
                    break;
                case CharMode.UpperAsciiLetter:
                case CharMode.LowerAsciiLetter:
                    TokenizerState = HtmlTokenizerState.TagName;
                    isEndTag = true;
                    name.Append(c);
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
                char c;
                CharMode charMode;
                if (!ReadNext(out c, out charMode))
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    name.Length = 0;
                    EmitDataToken();
                    return;
                }
                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);

                switch (charMode)
                {
                    case CharMode.NewLine:
                    case CharMode.WhiteSpace:
                        TokenizerState = HtmlTokenizerState.BeforeAttributeName;
                        break;
                    case CharMode.Slash:
                        TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                        break;
                    case CharMode.Gt:
                        SetEmitToken(CreateTagTokenFromNameBuffer(isEndTag));
                        TokenizerState = HtmlTokenizerState.Data;
                        data.Length = 0;
                        return;
                    case CharMode.NullChar:
                        name.Append('\uFFFD');
                        break;
                    default:
                        name.Append(c);
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

            char c;
            if (!ReadNext(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitDataToken();
                return;
            }

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