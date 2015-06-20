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
        /// 8.2.4.52 DOCTYPE state
        /// </summary>
        void R52_DocType()
        {

            char c;
            if (!Peek(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                doctype.ForceQuirksMode = true;

                EmitAndClearDocTypeToken();
                return;
            }

            TokenizerState = HtmlTokenizerState.BeforeDocTypeName;

            switch (c)
            {
                case '\t':
                case '\r':
                case '\n':
                case '\f':
                case ' ':
                    data.Append(c);
                    ReadNext();
                    break;
            }
        }
        /// <summary>
        /// 8.2.4.53 Before DOCTYPE name state
        /// </summary>
        void R53_BeforeDocTypeName()
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
                        break;
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.ForceQuirksMode = true;
                        EmitAndClearDocTypeToken();
                        return;
                    case '\0':
                        c = '\uFFFD';
                        goto default;
                    default:
                        TokenizerState = HtmlTokenizerState.DocTypeName;
                        name.Append(c);
                        return;
                } 
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            doctype.ForceQuirksMode = true;
            EmitAndClearDocTypeToken(); 
            
        }

        /// <summary>
        /// 8.2.4.54 DOCTYPE name state
        /// </summary> 
        void R54_DocTypeName()
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
                        TokenizerState = HtmlTokenizerState.AfterDocTypeName;
                        doctype.Name = ClearNameBuffer();
                        return;
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.Name = ClearNameBuffer();
                        EmitAndClearDocTypeToken();
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
            doctype.Name = name.ToString();
            doctype.ForceQuirksMode = true;
            EmitAndClearDocTypeToken();             
        }
        /// <summary>
        /// 8.2.4.55 After DOCTYPE name state
        /// </summary>
        /// <returns></returns>
        void R55_AfterDocTypeName()
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
                        break;
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        EmitAndClearDocTypeToken();
                        return;
                    default:
                        name.Append(c);
                        if (name.Length < 6)
                            continue; //read next
                        //-------------------
                        if (NameIs("public"))
                        {
                            TokenizerState = HtmlTokenizerState.AfterDocTypePublicKeyword;
                            doctype.PublicKeyword = ClearNameBuffer();
                        }
                        else if (NameIs("system"))
                        {
                            TokenizerState = HtmlTokenizerState.AfterDocTypeSystemKeyword;
                            doctype.SystemKeyword = ClearNameBuffer();
                        }
                        else
                        {
                            TokenizerState = HtmlTokenizerState.BogusDocType;
                            name.Length = 0;
                        }                        
                        return;
                } 
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            doctype.ForceQuirksMode = true;
            EmitAndClearDocTypeToken();
             
        }
        /// <summary>
        /// 8.2.4.56 After DOCTYPE public keyword state
        /// </summary>         
        void R56_AfterDocTypePublicKeyword()
        {

            char c;
            if (!ReadNext(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                doctype.ForceQuirksMode = true;
                EmitAndClearDocTypeToken();
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
                    EmitAndClearDocTypeToken();
                    return;
                default: // parse error
                    TokenizerState = HtmlTokenizerState.BogusDocType;
                    doctype.ForceQuirksMode = true;
                    break;
            }
        }
        /// <summary>
        /// 8.2.4.57 Before DOCTYPE public identifier state
        /// </summary>         
        void R57_BeforeDocTypePublicIdentifier()
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
                        TokenizerState = HtmlTokenizerState.DocTypePublicIdentifierQuoted;
                        doctype.PublicIdentifier = string.Empty;
                        quote = c;
                        return;
                    case '>': // parse error
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.ForceQuirksMode = true;
                        EmitAndClearDocTypeToken();
                        return;
                    default: // parse error
                        TokenizerState = HtmlTokenizerState.BogusDocType;
                        doctype.ForceQuirksMode = true;
                        return;
                }

            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            doctype.ForceQuirksMode = true;
            EmitAndClearDocTypeToken(); 
        }
        /// <summary>
        /// 8.2.4.58 DOCTYPE public identifier (double-quoted) state,
        /// 8.2.4.59 DOCTYPE public identifier (single-quoted) state
        /// </summary>
        void R58_59_DocTypePublicIdentifierQuoted()
        {
            char c;
            while(ReadNext(out c))
            {
                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);

                switch (c)
                {
                    case '\0': // parse error
                        name.Append('\uFFFD');
                        break;
                    case '>': // parse error
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.PublicIdentifier = ClearNameBuffer();
                        doctype.ForceQuirksMode = true;
                        EmitAndClearDocTypeToken();
                        return;
                    default:
                        if (c == quote)
                        {
                            TokenizerState = HtmlTokenizerState.AfterDocTypePublicIdentifier;
                            doctype.PublicIdentifier = ClearNameBuffer();
                            return;
                        }
                        else
                        {
                            name.Append(c);
                        }
                        break;
                }
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            doctype.PublicIdentifier = ClearNameBuffer();
            doctype.ForceQuirksMode = true;
            EmitAndClearDocTypeToken();
             
        }
        /// <summary>
        /// 8.2.4.60 After DOCTYPE public identifier state
        /// </summary>
        void R60_AfterDocTypePublicIdentifier()
        {

            char c;
            if (!ReadNext(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                doctype.ForceQuirksMode = true;
                EmitAndClearDocTypeToken();
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
                    TokenizerState = HtmlTokenizerState.BetweenDocTypePublicAndSystemIdentifiers;
                    break;
                case '>':
                    TokenizerState = HtmlTokenizerState.Data;
                    EmitAndClearDocTypeToken();
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
        }
        /// <summary>
        /// 8.2.4.61 Between DOCTYPE public and system identifiers state
        /// </summary>
        void R61_BetweenDocTypePublicAndSystemIdentifiers()
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
                        break;
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        EmitAndClearDocTypeToken();
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
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            doctype.ForceQuirksMode = true;
            EmitAndClearDocTypeToken(); 
        }

        /// <summary>
        /// 8.2.4.62 After DOCTYPE system keyword state
        /// </summary>
        void R62_AfterDocTypeSystemKeyword()
        {

            char c;
            if (!ReadNext(out c))
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                doctype.ForceQuirksMode = true;
                EmitAndClearDocTypeToken();
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

                    EmitAndClearDocTypeToken();
                    return;
                default: // parse error
                    TokenizerState = HtmlTokenizerState.BogusDocType;
                    doctype.ForceQuirksMode = true;
                    break;
            }
        }
        /// <summary>
        /// 8.2.4.63 Before DOCTYPE system identifier state
        /// </summary>
        void R63_BeforeDocTypeSystemIdentifier()
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
                        EmitAndClearDocTypeToken();
                        return;
                    default: // parse error
                        TokenizerState = HtmlTokenizerState.BogusDocType;
                        doctype.ForceQuirksMode = true;
                        return;
                } 
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            doctype.ForceQuirksMode = true;
            EmitAndClearDocTypeToken();
             
        }
        /// <summary>
        ///8.2.4.64 DOCTYPE system identifier (double-quoted) state,
        ///8.2.4.65 DOCTYPE system identifier (single-quoted) state
        /// </summary>
        void R64_65_DocTypeSystemIdentifierQuoted()
        {
            char c;
            while (ReadNext(out c))
            {
                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);

                switch (c)
                {
                    case '\0': // parse error
                        name.Append('\uFFFD');
                        break; //break switch

                    case '>': // parse error
                        TokenizerState = HtmlTokenizerState.Data;
                        doctype.SystemIdentifier = ClearNameBuffer();
                        doctype.ForceQuirksMode = true;
                        EmitAndClearDocTypeToken();
                        return;
                    default:
                        if (c == quote)
                        {
                            TokenizerState = HtmlTokenizerState.AfterDocTypeSystemIdentifier;
                            doctype.SystemIdentifier = ClearNameBuffer();
                            return;
                        }
                        else
                        {
                            name.Append(c);
                        }
                        break;
                }
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            doctype.SystemIdentifier = ClearNameBuffer();
            doctype.ForceQuirksMode = true;
            EmitAndClearDocTypeToken();
             
        }

        /// <summary>
        /// 8.2.4.66 After DOCTYPE system identifier state
        /// </summary>
        void R66_AfterDocTypeSystemIdentifier()
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
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        EmitAndClearDocTypeToken();
                        return;
                    default: // parse error
                        TokenizerState = HtmlTokenizerState.BogusDocType;
                        return;
                }
            }

            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            doctype.ForceQuirksMode = true;
            EmitAndClearDocTypeToken();

        }
        /// <summary>
        /// 8.2.4.67 Bogus DOCTYPE state
        /// </summary>
        void R67_BogusDocType()
        {
            char c;
            while (ReadNext(out c))
            {
                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);
                if (c == '>')
                {
                    TokenizerState = HtmlTokenizerState.Data;
                    EmitAndClearDocTypeToken();
                    return;
                }
            }
            //eof
            TokenizerState = HtmlTokenizerState.EndOfFile;
            doctype.ForceQuirksMode = true;
            EmitAndClearDocTypeToken();
        }

        void EmitAndClearDocTypeToken()
        {
            SetEmitToken(doctype);
            data.Length = 0;
            doctype = null;
        }
    }
}