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
        /// 8.2.4.34 Before attribute name state
        /// </summary>
        void R34_BeforeAttributeName()
        {
            char c;
            while (ReadNext(out c))
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
                        break;
                    case '/':
                        TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                        return;
                    case '>':
                        EmitTagToken();
                        return;
                    case '"':
                    case '\'':
                    case '<':
                    case '=':
                        // parse error
                        goto default;
                    case '\0':
                        c = '\uFFFD';
                        goto default;
                    default:
                        TokenizerState = HtmlTokenizerState.AttributeName;
                        name.Append(c);
                        return;
                }
            }
            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            tag = null;
            EmitDataToken();
        }
        /// <summary>
        /// 8.2.4.35 Attribute name state
        /// </summary>
        void R35_AttributeName()
        {
            char c;
            while (ReadNext(out c))
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
                        TokenizerState = HtmlTokenizerState.AfterAttributeName;
                        EmitTagAttribute();
                        return;
                    case '/':
                        TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                        EmitTagAttribute();
                        return;
                    case '=':
                        TokenizerState = HtmlTokenizerState.BeforeAttributeValue;
                        EmitTagAttribute();
                        return;
                    case '>':
                        EmitTagAttribute();
                        EmitTagToken();
                        return;
                    case '\0':
                        c = '\uFFFD';
                        goto default;
                    default:
                        name.Append(c);
                        break;
                }

            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            name.Length = 0;
            tag = null;
            EmitDataToken();
        }
        /// <summary>
        /// 8.2.4.36 After attribute name state
        /// </summary>
        void R36_AfterAttributeName()
        {
            char c;
            while (ReadNext(out c))
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
                        break;
                    case '/':
                        TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                        return;
                    case '=':
                        TokenizerState = HtmlTokenizerState.BeforeAttributeValue;
                        return;
                    case '>':
                        EmitTagToken();
                        return;
                    case '"':
                    case '\'':
                    case '<':
                        // parse error
                        goto default;
                    case '\0':
                        c = '\uFFFD';
                        goto default;
                    default:
                        TokenizerState = HtmlTokenizerState.AttributeName;
                        name.Append(c);
                        return;
                }
            }
            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            tag = null;
            EmitDataToken();
        }
        /// <summary>
        /// 8.2.4.37 Before attribute value state
        /// </summary>
        void R37_BeforeAttributeValue()
        {
            char c;
            while (ReadNext(out c))
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
                        break;
                    case '"':
                    case '\'':
                        TokenizerState = HtmlTokenizerState.AttributeValueQuoted;
                        quote = c;
                        return;
                    case '&':
                        TokenizerState = HtmlTokenizerState.AttributeValueUnquoted;
                        return;
                    case '/':
                        TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                        return;
                    case '>':
                        EmitTagToken();
                        return;
                    case '<':
                    case '=':
                    case '`':
                        // parse error
                        goto default;
                    case '\0':
                        c = '\uFFFD';
                        goto default;
                    default:
                        TokenizerState = HtmlTokenizerState.AttributeName;
                        name.Append(c);
                        return;
                }
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            tag = null;
            EmitDataToken();
        }
        /// <summary>
        /// 8.2.4.38 Attribute value (double-quoted) state,
        /// 8.2.4.39 Attribute value (single-quoted) state
        /// </summary>
        void R38_39_AttributeValueQuoted()
        {
            char c;
            while (ReadNext(out c))
            {
                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);
                switch (c)
                {
                    case '&':
                        TokenizerState = HtmlTokenizerState.CharacterReferenceInAttributeValue;
                        return;
                    default:
                        if (c == quote)
                        {
                            TokenizerState = HtmlTokenizerState.AfterAttributeValueQuoted;
                            attribute.Value = ClearNameBuffer();
                            return;
                        }
                        else
                        {
                            name.Append(c == '\0' ? '\uFFFD' : c);
                        }
                        break;
                }
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            name.Length = 0;
            EmitDataToken();
        }
        /// <summary>
        /// 8.2.4.40 Attribute value (unquoted) state
        /// </summary>
        void R40_AttributeValueUnquoted()
        {
            char c;
            while(ReadNext(out c))
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
                        TokenizerState = HtmlTokenizerState.BeforeAttributeName;
                        attribute.Value = ClearNameBuffer();
                        return;
                    case '&':
                        TokenizerState = HtmlTokenizerState.CharacterReferenceInAttributeValue;
                        return;
                    case '>':
                        EmitTagToken();
                        return;
                    case '\'':
                    case '<':
                    case '=':
                    case '`':
                        // parse error
                        goto default;
                    default:
                        if (c == quote)
                        {
                            TokenizerState = HtmlTokenizerState.AfterAttributeValueQuoted;
                            attribute.Value = ClearNameBuffer();
                            return;
                        }
                        else
                        {
                            name.Append(c == '\0' ? '\uFFFD' : c);
                        }
                        break;
                } 
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            name.Length = 0;
            EmitDataToken(); 
        }

        /// <summary>
        /// 8.2.4.41 Character reference in attribute value state
        /// </summary>
        void R41_CharacterReferenceInAttributeValue()
        {
            char additionalAllowedCharacter = quote == '\0' ? '>' : quote;
            char c;
            CharMode charMode;
            if (!Peek(out c, out charMode))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                data.Append('&');
                name.Length = 0;
                EmitDataToken();
                return;
            }

            bool consume;

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
                    data.Append('&');
                    name.Append('&');
                    consume = false;
                    break;
                default:
                    if (c == additionalAllowedCharacter)
                    {
                        // this is not a character reference, nothing is consumed
                        data.Append('&');
                        name.Append('&');
                        consume = false;
                        break;
                    }

                    entity.Push('&');
                    while (entity.Push(c))
                    {
                        ReadNext();
                        if (!Peek(out c, out charMode))
                        {
                            TokenizerState = HtmlTokenizerState.EndOfFile;
                            data.Append(entity.GetPushedInput());
                            entity.Reset();
                            EmitDataToken();
                            return;
                        }
                    }
                    var pushed = entity.GetPushedInput();
                    string value;

                    switch (charMode)
                    {
                        default:
                            value = entity.GetValue();
                            break;
                        case CharMode.Assign:
                        case CharMode.LowerAsciiLetter:
                        case CharMode.UpperAsciiLetter:
                        case CharMode.Numeric:
                            value = pushed;
                            break;
                    }
                    data.Append(pushed);
                    name.Append(value);
                    consume = c == ';';
                    entity.Reset();
                    break;
            }

            if (quote == '\0')
                TokenizerState = HtmlTokenizerState.AttributeValueUnquoted;
            else
                TokenizerState = HtmlTokenizerState.AttributeValueQuoted;

            if (consume)
                ReadNext();


        }
        /// <summary>
        /// 8.2.4.42 After attribute value (quoted) state
        /// </summary>
        void R42_AfterAttributeValueQuoted()
        {
            char c;
            if (!Peek(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitDataToken();
                return;
            }

            switch (c)
            {
                case '\t':
                case '\r':
                case '\n':
                case '\f':
                case ' ':
                    TokenizerState = HtmlTokenizerState.BeforeAttributeName;
                    ReadNext(); //consume
                    return;
                case '/':
                    TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                    ReadNext();//consume
                    return;
                case '>':
                    EmitTagToken();
                    ReadNext();//consume
                    return;
                default:
                    TokenizerState = HtmlTokenizerState.BeforeAttributeName;
                    break;
            }
        }


    }
}