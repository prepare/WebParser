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
        /// 8.2.4.44 Bogus comment state
        /// </summary>
        void ReadBogusComment()
        {
            int nc;
            char c;

            if (data.Length > 0)
            {
                c = data[data.Length - 1];
                data.Length = 1;
                data[0] = c;
            }

            do
            {
                if ((nc = Read()) == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    break;
                }

                if ((c = (char)nc) == '>')
                    break;

                data.Append(c == '\0' ? '\uFFFD' : c);
            } while (true);

            EmitCommentToken(data);
        }
        /// <summary>
        /// 8.2.4.45 Markup declaration open state
        /// </summary>
        void ReadMarkupDeclarationOpen()
        {
            int count = 0, nc;
            char c = '\0';

            while (count < 2)
            {
                if ((nc = Peek()) == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitDataToken(false);
                    return;
                }

                if ((c = (char)nc) != '-')
                    break;

                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);
                Read();
                count++;
            }

            token = null;

            if (count == 2)
            {
                TokenizerState = HtmlTokenizerState.CommentStart;
                name.Length = 0;
                return;
            }

            if (count == 1)
            {
                // parse error
                TokenizerState = HtmlTokenizerState.BogusComment;
                return;
            }

            if (c == 'D' || c == 'd')
            {
                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);
                name.Append(c);
                Read();
                count = 1;

                while (count < 7)
                {
                    if ((nc = Read()) == -1)
                    {
                        TokenizerState = HtmlTokenizerState.EndOfFile;
                        EmitDataToken(false);
                        return;
                    }

                    if (ToLower((c = (char)nc)) != DocType[count])
                        break;

                    // Note: we save the data in case we hit a parse error and have to emit a data token
                    data.Append(c);
                    name.Append(c);
                    count++;
                }

                if (count == 7)
                {
                    doctype = CreateDocTypeToken(name.ToString());
                    TokenizerState = HtmlTokenizerState.DocType;
                    name.Length = 0;
                    return;
                }

                name.Length = 0;
            }
            else if (c == '[')
            {
                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);
                Read();
                count = 1;

                while (count < 7)
                {
                    if ((nc = Read()) == -1)
                    {
                        TokenizerState = HtmlTokenizerState.EndOfFile;
                        EmitDataToken(false);
                        return;
                    }

                    if ((c = (char)nc) != CData[count])
                        break;

                    // Note: we save the data in case we hit a parse error and have to emit a data token
                    data.Append(c);
                    count++;
                }

                if (count == 7)
                {
                    TokenizerState = HtmlTokenizerState.CDataSection;
                    return;
                }
            }

            // parse error
            TokenizerState = HtmlTokenizerState.BogusComment;


        }
        /// <summary>
        /// 8.2.4.46 Comment start state
        /// </summary>
        void ReadCommentStart()
        {
            int nc = Read();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.Data;

                EmitCommentToken(string.Empty);
                return;
            }

            c = (char)nc;

            data.Append(c);

            switch (c)
            {
                case '-':
                    TokenizerState = HtmlTokenizerState.CommentStartDash;
                    break;
                case '>': // parse error
                    TokenizerState = HtmlTokenizerState.Data;
                    EmitCommentToken(string.Empty);
                    return;
                default: // parse error
                    TokenizerState = HtmlTokenizerState.Comment;
                    name.Append(c == '\0' ? '\uFFFD' : c);
                    break;
            }

            token = null;
        }
        /// <summary>
        /// 8.2.4.47 Comment start dash state
        /// </summary>
        void ReadCommentStartDash()
        {
            int nc = Read();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.Data;
                EmitCommentToken(name);
                return;
            }

            c = (char)nc;

            data.Append(c);

            switch (c)
            {
                case '-':
                    TokenizerState = HtmlTokenizerState.CommentEnd;
                    break;
                case '>': // parse error
                    TokenizerState = HtmlTokenizerState.Data;
                    EmitCommentToken(name);
                    return;
                default: // parse error
                    TokenizerState = HtmlTokenizerState.Comment;
                    name.Append('-');
                    name.Append(c == '\0' ? '\uFFFD' : c);
                    break;
            }

            token = null;
        }
        /// <summary>
        /// 8.2.4.48 Comment state
        /// </summary>
        void ReadComment()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitCommentToken(name);
                    return;
                }

                c = (char)nc;

                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);

                switch (c)
                {
                    case '-':
                        TokenizerState = HtmlTokenizerState.CommentEndDash;
                        return;
                    default:
                        name.Append(c == '\0' ? '\uFFFD' : c);
                        break;
                }
            } while (true);
        }

        // FIXME: this is exactly the same as ReadCommentStartDash
        /// <summary>
        /// 8.2.4.49 Comment end dash state
        /// </summary>
        void ReadCommentEndDash()
        {
            int nc = Read();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.Data;
                EmitCommentToken(name);
                return;
            }

            c = (char)nc;

            data.Append(c);

            switch (c)
            {
                case '-':
                    TokenizerState = HtmlTokenizerState.CommentEnd;
                    break;
                case '>': // parse error
                    TokenizerState = HtmlTokenizerState.Data;
                    EmitCommentToken(name);
                    return;
                default: // parse error
                    TokenizerState = HtmlTokenizerState.Comment;
                    name.Append('-');
                    name.Append(c == '\0' ? '\uFFFD' : c);
                    break;
            }

            token = null;


        }
        /// <summary>
        /// 8.2.4.50 Comment end state
        /// </summary>
        void ReadCommentEnd()
        {
            token = null;

            do
            {
                int nc = Read();
                char c;

                if (nc == -1)
                {
                    TokenizerState = HtmlTokenizerState.EndOfFile;
                    EmitCommentToken(name);
                    return;
                }

                c = (char)nc;

                // Note: we save the data in case we hit a parse error and have to emit a data token
                data.Append(c);

                switch (c)
                {
                    case '>':
                        TokenizerState = HtmlTokenizerState.Data;
                        EmitCommentToken(name);
                        return;
                    case '!': // parse error
                        TokenizerState = HtmlTokenizerState.CommentEndBang;
                        return;
                    case '-':
                        name.Append('-');
                        break;
                    default:
                        TokenizerState = HtmlTokenizerState.Comment;
                        name.Append(c == '\0' ? '\uFFFD' : c);
                        return;
                }
            } while (true);
        }
        /// <summary>
        /// 8.2.4.51 Comment end bang state
        /// </summary>
        void ReadCommentEndBang()
        {
            int nc = Read();
            char c;

            if (nc == -1)
            {
                TokenizerState = HtmlTokenizerState.EndOfFile;
                EmitCommentToken(name);
                return;
            }

            c = (char)nc;

            data.Append(c);

            switch (c)
            {
                case '-':
                    TokenizerState = HtmlTokenizerState.CommentEndDash;
                    name.Append("--!");
                    break;
                case '>':
                    TokenizerState = HtmlTokenizerState.Data;
                    EmitCommentToken(name);
                    return;
                default: // parse error
                    TokenizerState = HtmlTokenizerState.Comment;
                    name.Append("--!");
                    name.Append(c == '\0' ? '\uFFFD' : c);
                    break;
            }

            token = null;


        }

    }
}