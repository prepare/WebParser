/*
 * Copyright (c) 2005-2007 Henri Sivonen
 * Copyright (c) 2007-2010 Mozilla Foundation
 * Portions of comments Copyright 2004-2010 Apple Computer, Inc., Mozilla 
 * Foundation, and Opera Software ASA.
 * Copyright (c) 2012 Patrick Reisert
 * 
 * 2015, WinterDev
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

/*
 * The comments following this one that use the same comment syntax as this 
 * comment are quotes from the WHATWG HTML 5 spec as of 2 June 2007 
 * amended as of June 18 2008 and May 31 2010.
 * That document came with this statement:
 * © Copyright 2004-2010 Apple Computer, Inc., Mozilla Foundation, and 
 * Opera Software ASA. You are granted a license to use, reproduce and 
 * create derivative works of this document."
 */


using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using HtmlParserSharp.Common;

namespace HtmlParserSharp.Core
{
    partial class Tokenizer
    {
        TokenBufferReader reader = null;
        void FlushChars()
        {
            //if (pos > cstart)
            //{
            //    TokenListener.Characters(buf, cstart, pos - cstart);
            //}
            //cstart = int.MaxValue;
            if (reader.CollectionLength > 0)
            {
                TokenListener.Characters(reader.InternalBuffer,
                    reader.CollectionStart,
                    reader.CollectionLength);
            }
            reader.ResetMarker();
        }
        void EmitPlaintextReplacementCharacter()
        {
            throw new NotImplementedException();
        }
        void EmitReplacementCharacter()
        {
            FlushChars();
            TokenListener.ZeroOriginatingReplacementCharacter();
            
        }
        void EmitCarriageReturn()
        {
            throw new NotImplementedException();
        }
        void EmitDoctypeToken()
        {
            throw new NotImplementedException();
        }
        TokenizerState EmitCurrentTagToken(bool isSelfClosing)
        {
            //cstart = pos + 1;
            reader.ResetMarker();
            MaybeErrSlashInEndTag(isSelfClosing);
            stateSave = TokenizerState.s01_DATA;
            HtmlAttributes attrs = attributes ?? HtmlAttributes.EMPTY_ATTRIBUTES;

            if (endTag)
            {
                /*
                 * When an end tag token is emitted, the content model flag must be
                 * switched to the PCDATA state.
                 */
                MaybeErrAttributesOnEndTag(attrs);
                TokenListener.EndTag(tagName);
            }
            else
            {
                TokenListener.StartTag(tagName, attrs, isSelfClosing);
            }
            tagName = null;
            ResetAttributes();
            /*
             * The token handler may have called setStateAndEndTagExpectation
             * and changed stateSave since the start of this method.
             */
            return stateSave;
        }
        void EmitComment(int provisionalHyphens)
        {
            throw new NotImplementedException();
        }

    }
}