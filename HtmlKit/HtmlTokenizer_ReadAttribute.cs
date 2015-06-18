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
        void ReadBeforeAttributeName()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    tag = null;

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
                    default:
                        TokenizerState = HtmlTokenizerState.AttributeName;
                        name.Append(c == '\0' ? '\uFFFD' : c);
                        return;
                }
            } while (true);
        }
        /// <summary>
        /// 8.2.4.35 Attribute name state
        /// </summary>
        void ReadAttributeName()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    name.Length = 0;
                    tag = null;
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
                        TokenizerState = HtmlTokenizerState.AfterAttributeName;
                        break;
                    case '/':
                        TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                        break;
                    case '=':
                        TokenizerState = HtmlTokenizerState.BeforeAttributeValue;
                        break;
                    case '>':
                        EmitTagAttribute();

                        EmitTagToken();
                        return;
                    default:
                        name.Append(c == '\0' ? '\uFFFD' : c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.AttributeName);

            EmitTagAttribute();
        }
        /// <summary>
        /// 8.2.4.36 After attribute name state
        /// </summary>
        void ReadAfterAttributeName()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    tag = null;

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
                    default:
                        TokenizerState = HtmlTokenizerState.AttributeName;
                        name.Append(c == '\0' ? '\uFFFD' : c);
                        return;
                }
            } while (true);
        }
        /// <summary>
        /// 8.2.4.37 Before attribute value state
        /// </summary>
        void ReadBeforeAttributeValue()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    tag = null;

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
                    default:
                        TokenizerState = HtmlTokenizerState.AttributeName;
                        name.Append(c == '\0' ? '\uFFFD' : c);
                        return;
                }
            } while (true);
        }
        /// <summary>
        /// 8.2.4.38 Attribute value (double-quoted) state,
        /// 8.2.4.39 Attribute value (single-quoted) state
        /// </summary>
        void ReadAttributeValueQuoted()
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
                    case '&':
                        TokenizerState = HtmlTokenizerState.CharacterReferenceInAttributeValue;
                        token = null;
                        return;
                    default:
                        if (c == quote)
                        {
                            TokenizerState = HtmlTokenizerState.AfterAttributeValueQuoted;
                            break;
                        }

                        name.Append(c == '\0' ? '\uFFFD' : c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.AttributeValueQuoted);

            attribute.Value = name.ToString();
            name.Length = 0;
            token = null;

            return;
        }
        /// <summary>
        /// 8.2.4.40 Attribute value (unquoted) state
        /// </summary>
        void ReadAttributeValueUnquoted()
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
                    case '&':
                        TokenizerState = HtmlTokenizerState.CharacterReferenceInAttributeValue;
                        token = null;
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
                            break;
                        }

                        name.Append(c == '\0' ? '\uFFFD' : c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.AttributeValueUnquoted);

            attribute.Value = name.ToString();
            name.Length = 0;
            token = null;


        }

        /// <summary>
        /// 8.2.4.41 Character reference in attribute value state
        /// </summary>
        void ReadCharacterReferenceInAttributeValue()
        {
            char additionalAllowedCharacter = quote == '\0' ? '>' : quote;
            int nc = Peek();
            bool consume;
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                data.Append('&');
                name.Length = 0;

                EmitDataToken(false);
                return;
            }

            c = (char)nc;
            token = null;

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
                        Read();

                        if ((nc = Peek()) == -1)
                        {
                            TokenizerState = HtmlTokenizerState.EndOfFile;
                            data.Append(entity.GetPushedInput());
                            entity.Reset();

                            EmitDataToken(false);
                            return;
                        }

                        c = (char)nc;
                    }

                    var pushed = entity.GetPushedInput();
                    string value;

                    if (c == '=' || IsAlphaNumeric(c))
                        value = pushed;
                    else
                        value = entity.GetValue();

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
                Read();


        }
        /// <summary>
        /// 8.2.4.42 After attribute value (quoted) state
        /// </summary>
        void ReadAfterAttributeValueQuoted()
        {
            int nc = Peek();
            bool consume;
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitDataToken(false);
                return;
            }

            c = (char)nc;
            token = null;

            switch (c)
            {
                case '\t':
                case '\r':
                case '\n':
                case '\f':
                case ' ':
                    TokenizerState = HtmlTokenizerState.BeforeAttributeName;
                    consume = true;
                    break;
                case '/':
                    TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
                    consume = true;
                    break;
                case '>':
                    EmitTagToken();
                    consume = true;
                    break;
                default:
                    TokenizerState = HtmlTokenizerState.BeforeAttributeName;
                    consume = false;
                    break;
            }

            if (consume)
                Read();
        }


    }
}