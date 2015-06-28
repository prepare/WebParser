﻿//
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
        /// 8.2.4.52 DOCTYPE state
        /// </summary>
        void R52_DocType()
        {
            int nc = Peek();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                doctype.ForceQuirksMode = true;
                token = doctype;
                doctype = null;
                data.Length = 0;
                name.Length = 0;
                return;
            }

            TokenizerState = HtmlTokenizerState.BeforeDocTypeName;
            c = (char)nc;
            token = null;

            switch (c)
            {
                case '\t':
                case '\r':
                case '\n':
                case '\f':
                case ' ':
                    data.Append(c);
                    Read();
                    break;
            }

            return;
        }
        /// <summary>
        /// 8.2.4.53 Before DOCTYPE name state
        /// </summary>
        void R53_BeforeDocTypeName()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
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
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.ForceQuirksMode = true;
                        token = doctype;
                        doctype = null;
                        data.Length = 0;
                        return;
                    case '\0':
                        TokenizerState = HtmlTokenizerState.DocTypeName;
                        name.Append('\uFFFD');
                        return;
                    default:
                        TokenizerState = HtmlTokenizerState.DocTypeName;
                        name.Append(c);
                        return;
                }
            } while (true);
        }

        /// <summary>
        /// 8.2.4.54 DOCTYPE name state
        /// </summary> 
        void R54_DocTypeName()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    doctype.Name = name.ToString();
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    data.Length = 0;
                    name.Length = 0;
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
                        TokenizerState = HtmlTokenizerState.AfterDocTypeName;
                        break;
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.Name = name.ToString();
                        token = doctype;
                        doctype = null;
                        data.Length = 0;
                        name.Length = 0;
                        return;
                    case '\0':
                        name.Append('\uFFFD');
                        break;
                    default:
                        name.Append(c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.DocTypeName);

            doctype.Name = name.ToString();
            name.Length = 0;            
        }
        /// <summary>
        /// 8.2.4.55 After DOCTYPE name state
        /// </summary>
        /// <returns></returns>
        void R55_AfterDocTypeName()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
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
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        token = doctype;
                        doctype = null;
                        data.Length = 0;
                        return;
                    default:
                        name.Append(c);
                        if (name.Length < 6)
                            break;

                        if (NameIs("public"))
                        {
                            TokenizerState = HtmlTokenizerState.AfterDocTypePublicKeyword;
                            doctype.PublicKeyword = name.ToString();
                        }
                        else if (NameIs("system"))
                        {
                            TokenizerState = HtmlTokenizerState.AfterDocTypeSystemKeyword;
                            doctype.SystemKeyword = name.ToString();
                        }
                        else
                        {
                            TokenizerState = HtmlTokenizerState.BogusDocType;
                        }

                        name.Length = 0;
                        return;
                }
            } while (true);
        }
        /// <summary>
        /// 8.2.4.56 After DOCTYPE public keyword state
        /// </summary>         
        void R56_AfterDocTypePublicKeyword()
        {
            int nc = Read();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                doctype.ForceQuirksMode = true;
                token = doctype;
                doctype = null;
                data.Length = 0;
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
                    TokenizerState = HtmlTokenizerState.BeforeDocTypePublicIdentifier;
                    break;
                case '"':
                case '\'': // parse error
                    TokenizerState = HtmlTokenizerState.DocTypePublicIdentifierQuoted;
                    doctype.PublicIdentifier = string.Empty;
                    quote = c;
                    break;
                case '>': // parse error
                    TokenizerState = HtmlTokenizerState.Data;
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
                    return;
                default: // parse error
                    TokenizerState = HtmlTokenizerState.BogusDocType;
                    doctype.ForceQuirksMode = true;
                    break;
            }

            token = null;             
        }
        /// <summary>
        /// 8.2.4.57 Before DOCTYPE public identifier state
        /// </summary>         
        void R57_BeforeDocTypePublicIdentifier()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
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
                        TokenizerState = HtmlTokenizerState.DocTypePublicIdentifierQuoted;
                        doctype.PublicIdentifier = string.Empty;
                        quote = c;
                        return;
                    case '>': // parse error
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.ForceQuirksMode = true;
                        token = doctype;
                        doctype = null;
                        data.Length = 0;
                        return;
                    default: // parse error
                        TokenizerState = HtmlTokenizerState.BogusDocType;
                        doctype.ForceQuirksMode = true;
                        return;
                }
            } while (true);
        }
        /// <summary>
        /// 8.2.4.58 DOCTYPE public identifier (double-quoted) state,
        /// 8.2.4.59 DOCTYPE public identifier (single-quoted) state
        /// </summary>
        void R58_59_DocTypePublicIdentifierQuoted()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    doctype.PublicIdentifier = name.ToString();
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
                    name.Length = 0;
                    return;
                }

                c = (char)nc;

                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);

                switch (c)
                {
                    case '\0': // parse error
                        name.Append('\uFFFD');
                        break;
                    case '>': // parse error
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.PublicIdentifier = name.ToString();
                        doctype.ForceQuirksMode = true;
                        token = doctype;
                        doctype = null;
                        data.Length = 0;
                        name.Length = 0;
                        return;
                    default:
                        if (c == quote)
                        {
                            TokenizerState = HtmlTokenizerState.AfterDocTypePublicIdentifier;
                            break;
                        }

                        name.Append(c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.DocTypePublicIdentifierQuoted);

            doctype.PublicIdentifier = name.ToString();
            name.Length = 0;


        }
        /// <summary>
        /// 8.2.4.60 After DOCTYPE public identifier state
        /// </summary>
        void R60_AfterDocTypePublicIdentifier()
        {
            int nc = Read();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                doctype.ForceQuirksMode = true;
                token = doctype;
                doctype = null;
                data.Length = 0;
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
                    TokenizerState = HtmlTokenizerState.BetweenDocTypePublicAndSystemIdentifiers;
                    break;
                case '>':
                    TokenizerState = HtmlTokenizerState.Data;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
                    return;
                case '"':
                case '\'': // parse error
                    TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
                    doctype.SystemIdentifier = string.Empty;
                    quote = c;
                    break;
                default: // parse error
                    TokenizerState = HtmlTokenizerState.BogusDocType;
                    doctype.ForceQuirksMode = true;
                    break;
            }

            token = null;


        }
        /// <summary>
        /// 8.2.4.61 Between DOCTYPE public and system identifiers state
        /// </summary>
        void R61_BetweenDocTypePublicAndSystemIdentifiers()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
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
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        token = doctype;
                        doctype = null;
                        data.Length = 0;
                        return;
                    case '"':
                    case '\'':
                        TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
                        doctype.SystemIdentifier = string.Empty;
                        quote = c;
                        return;
                    default: // parse error
                        TokenizerState = HtmlTokenizerState.BogusDocType;
                        doctype.ForceQuirksMode = true;
                        return;
                }
            } while (true);
        }

        /// <summary>
        /// 8.2.4.62 After DOCTYPE system keyword state
        /// </summary>
        void R62_AfterDocTypeSystemKeyword()
        {
            int nc = Read();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                doctype.ForceQuirksMode = true;
                token = doctype;
                doctype = null;
                data.Length = 0;
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
                    TokenizerState = HtmlTokenizerState.BeforeDocTypeSystemIdentifier;
                    break;
                case '"':
                case '\'': // parse error
                    TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
                    doctype.SystemIdentifier = string.Empty;
                    quote = c;
                    break;
                case '>': // parse error
                    TokenizerState = HtmlTokenizerState.Data;
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
                    return;
                default: // parse error
                    TokenizerState = HtmlTokenizerState.BogusDocType;
                    doctype.ForceQuirksMode = true;
                    break;
            }

            token = null;
        }
        /// <summary>
        /// 8.2.4.63 Before DOCTYPE system identifier state
        /// </summary>
        void R63_BeforeDocTypeSystemIdentifier()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
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
                        TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
                        doctype.SystemIdentifier = string.Empty;
                        quote = c;
                        return;
                    case '>': // parse error
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.ForceQuirksMode = true;
                        token = doctype;
                        doctype = null;
                        data.Length = 0;
                        return;
                    default: // parse error
                        TokenizerState = HtmlTokenizerState.BogusDocType;
                        doctype.ForceQuirksMode = true;
                        return;
                }
            } while (true);
        }
        /// <summary>
        ///8.2.4.64 DOCTYPE system identifier (double-quoted) state,
        ///8.2.4.65 DOCTYPE system identifier (single-quoted) state
        /// </summary>
        void R64_65_DocTypeSystemIdentifierQuoted()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    doctype.SystemIdentifier = name.ToString();
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
                    name.Length = 0;
                    return;
                }

                c = (char)nc;

                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);

                switch (c)
                {
                    case '\0': // parse error
                        name.Append('\uFFFD');
                        break;
                    case '>': // parse error
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.SystemIdentifier = name.ToString();
                        doctype.ForceQuirksMode = true;
                        token = doctype;
                        doctype = null;
                        data.Length = 0;
                        name.Length = 0;
                        return;
                    default:
                        if (c == quote)
                        {
                            TokenizerState = HtmlTokenizerState.AfterDocTypeSystemIdentifier;
                            break;
                        }

                        name.Append(c);
                        break;
                }
            } while (TokenizerState == HtmlTokenizerState.DocTypeSystemIdentifierQuoted);

            doctype.SystemIdentifier = name.ToString();
            name.Length = 0;


        }

        /// <summary>
        /// 8.2.4.66 After DOCTYPE system identifier state
        /// </summary>
        void R66_AfterDocTypeSystemIdentifier()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
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
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        token = doctype;
                        doctype = null;
                        data.Length = 0;
                        return;
                    default: // parse error
                        TokenizerState = HtmlTokenizerState.BogusDocType;
                        return;
                }
            } while (true);
        }
        /// <summary>
        /// 8.2.4.67 Bogus DOCTYPE state
        /// </summary>
        void R67_BogusDocType()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    doctype.ForceQuirksMode = true;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
                    return;
                }

                c = (char)nc;

                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);

                if (c == '>')
                {
                    TokenizerState = HtmlTokenizerState.Data;
                    token = doctype;
                    doctype = null;
                    data.Length = 0;
                    return;
                }
            } while (true);
        }


    }
}