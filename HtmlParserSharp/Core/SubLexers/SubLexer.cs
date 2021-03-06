﻿/*
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


    abstract class SubLexer
    {
        protected bool shouldSuspend;
        protected InterLexerState stateSave;
        protected InterLexerState returnStateSave;
        public event EventHandler<ParserErrorEventArgs> ErrorEvent;
        protected const byte DATA_AND_RCDATA_MASK = (byte)0xF0;
       
        /// <summary>
        /// UTF-16 code unit array containing less than and greater than for emitting
        /// those characters on certain parse errors.
        /// </summary>
        protected static readonly char[] LT_GT = { '<', '>' };

       
        /// <summary>
        /// UTF-16 code unit array containing ]] for emitting those characters on
        /// state transitions.
        /// </summary>
        protected static readonly char[] RSQB_RSQB = { ']', ']' }; 
        /// <summary>
        /// "CDATA[" as <code>char[]</code>
        /// </summary>
        protected static readonly char[] CDATA_LSQB = "CDATA[".ToCharArray();

       
         

        /// <summary>
        ///  Buffer for short identifiers.
        /// </summary>
        protected StringBuilder strBuffer = new StringBuilder();
        /// <summary>
        /// buffer for long string
        /// </summary>
        protected StringBuilder longStrBuffer = new StringBuilder();

        protected TokenBufferReader reader = null;
        /// <summary>
        /// Emits the smaller buffer as character tokens.
        /// </summary>
        protected void EmitStrBuf()
        {

            int j = this.strBuffer.Length;
            if (j > 0)
            {
                TokenListener.Characters(CopyFromStringBuiler(strBuffer, 0, j));
            }
        }
        /// <summary>
        /// Append the contents of the smaller buffer to the larger one.
        /// </summary>
        protected void AppendStrBufToLongStrBuf()
        {
            /*@Inline*/

            AppendLongStrBuf(this.strBuffer);
        }
        protected void EmitOrAppendStrBuf(InterLexerState returnState)
        {
            //if ((returnState & DATA_AND_RCDATA_MASK) != 0)
            if (((byte)returnState & DATA_AND_RCDATA_MASK) == 0)
            {
                AppendStrBufToLongStrBuf();
            }
            else
            {
                EmitStrBuf();
            }
        }
        protected static char[] CopyFromStringBuiler(StringBuilder stBuilder, int start, int len)
        {
            char[] copyBuff = new char[len];
            stBuilder.CopyTo(start, copyBuff, 0, len);
            return copyBuff;
        }
        protected void SilentCarriageReturn()
        {
            throw new NotImplementedException();
        }
        protected void FlushChars()
        {
            //if (pos > cstart)
            //{
            //    TokenListener.Characters(buf, cstart, pos - cstart);
            //}
            //cstart = int.MaxValue;
            throw new NotImplementedException();
        }
        protected void EmitPlaintextReplacementCharacter()
        {
            throw new NotImplementedException();
        }
        protected void EmitReplacementCharacter()
        {
            throw new NotImplementedException();
        }
        protected void EmitCarriageReturn()
        {
            throw new NotImplementedException();
        }
        protected void EmitDoctypeToken()
        {
            throw new NotImplementedException();
        }
        protected InterLexerState EmitCurrentTagToken(bool isSelfClosing)
        {
            throw new NotImplementedException();
        }
        protected void EmitComment(int provisionalHyphens)
        {
            throw new NotImplementedException();
        }
        /*@Inline*/
        protected void ClearLongStrBuf()
        {

            throw new NotImplementedException();
        }

        /*@Inline*/
        protected void ClearLongStrBufAndAppend(char c)
        {
            throw new NotImplementedException();
        }
        public ITokenListener TokenListener { get; set; }
        /*@Inline*/
        protected void AppendLongStrBufLineFeed()
        {
            throw new NotImplementedException();
            //SilentLineFeed();
            //AppendLongStrBuf('\n');
        }

        /*@Inline*/
        protected void AppendLongStrBufCarriageReturn()
        {
            throw new NotImplementedException();
            //SilentCarriageReturn();
            //AppendLongStrBuf('\n');
        }
        /**
        * Appends to the larger buffer.
        * 
        * @param c
        *            the UTF-16 code unit to append
        */
        protected void AppendLongStrBuf(char c)
        {
            this.longStrBuffer.Append(c);
        }
        protected void AppendLongStrBuf(StringBuilder stBuilder)
        {
            this.longStrBuffer.Append(stBuilder.ToString());
        }
        /// <summary>
        /// Appends to the smaller buffer.
        /// </summary>
        /// <param name="c"></param>
        protected void AppendStrBuf(char c)
        {
            this.strBuffer.Append(c);
        }
        /*@Inline*/
        protected void ClearStrBufAndAppend(char c)
        {
            this.strBuffer.Length = 0;
            this.strBuffer.Append(c);

        }

        /*@Inline*/
        protected void ClearStrBuf()
        {
            this.strBuffer.Length = 0;
        }


        protected void ErrGarbageAfterLtSlash()
        {
        }

        protected void ErrLtSlashGt()
        {
        }

        protected void ErrWarnLtSlashInRcdata()
        {
        }

        protected void ErrHtml4LtSlashInRcdata(char folded)
        {
        }

        protected void ErrCharRefLacksSemicolon()
        {
        }

        protected void ErrNoDigitsInNCR()
        {
        }

        protected void ErrGtInSystemId()
        {
        }

        protected void ErrGtInPublicId()
        {
        }

        protected void ErrNamelessDoctype()
        {
        }

        protected void ErrConsecutiveHyphens()
        {
        }

        protected void ErrPrematureEndOfComment()
        {
        }

        protected void ErrBogusComment()
        {
        }

        protected void ErrUnquotedAttributeValOrNull(char c)
        {
        }

        protected void ErrSlashNotFollowedByGt()
        {
        }

        protected void ErrHtml4XmlVoidSyntax()
        {
        }

        protected void ErrNoSpaceBetweenAttributes()
        {
        }

        protected void ErrHtml4NonNameInUnquotedAttribute(char c)
        {
        }

        protected void ErrLtOrEqualsOrGraveInUnquotedAttributeOrNull(char c)
        {
        }

        protected void ErrAttributeValueMissing()
        {
        }

        protected void ErrBadCharBeforeAttributeNameOrNull(char c)
        {
        }

        protected void ErrEqualsSignBeforeAttributeName()
        {
        }

        protected void ErrBadCharAfterLt(char c)
        {
        }

        
        protected void ErrUnescapedAmpersandInterpretedAsCharacterReference()
        {
        }

        protected void ErrNotSemicolonTerminated()
        {
        }

        protected void ErrNoNamedCharacterMatch()
        {
        }

        protected void ErrQuoteBeforeAttributeName(char c)
        {
        }

        protected void ErrQuoteOrLtInAttributeNameOrNull(char c)
        {
        }

        protected void ErrExpectedPublicId()
        {
        }

        protected void ErrBogusDoctype()
        {
        }

        protected void MaybeWarnPrivateUseAstral()
        {
        }

        protected void MaybeWarnPrivateUse(char ch)
        {
        }

        protected void MaybeErrAttributesOnEndTag(HtmlAttributes attrs)
        {
        }

        protected void MaybeErrSlashInEndTag(bool selfClosing)
        {
        }

        protected char ErrNcrNonCharacter(char ch)
        {
            return ch;
        }

        protected void ErrAstralNonCharacter(int ch)
        {
        }

        protected void ErrNcrSurrogate()
        {
        }

        protected char ErrNcrControlChar(char ch)
        {
            return ch;
        }

        protected void ErrNcrCr()
        {
        }

        protected void ErrNcrInC1Range()
        {
        }

        protected void ErrEofInPublicId()
        {
        }

        protected void ErrEofInComment()
        {
        }

        protected void ErrEofInDoctype()
        {
        }

        protected void ErrEofInAttributeValue()
        {
        }

        protected void ErrEofInAttributeName()
        {
        }

        protected void ErrEofWithoutGt()
        {
        }

        protected void ErrEofInTagName()
        {
        }

        protected void ErrEofInEndTag()
        {
        }

        protected void ErrEofAfterLt()
        {
        }

        protected void ErrNcrOutOfRange()
        {
        }

        protected void ErrNcrUnassigned()
        {
        }

        protected void ErrDuplicateAttribute()
        {
        }

        protected void ErrEofInSystemId()
        {
        }

        protected void ErrExpectedSystemId()
        {
        }

        protected void ErrMissingSpaceBeforeDoctypeName()
        {
        }

        protected void ErrHyphenHyphenBang()
        {
        }

        protected void ErrNcrControlChar()
        {
        }

        protected void ErrNcrZero()
        {
        }

        protected void ErrNoSpaceBetweenDoctypeSystemKeywordAndQuote()
        {
        }

        protected void ErrNoSpaceBetweenPublicAndSystemIds()
        {
        }

        protected void ErrNoSpaceBetweenDoctypePublicKeywordAndQuote()
        {
        }

        protected void NoteAttributeWithoutValue()
        {
        }

        protected void NoteUnquotedAttributeValue()
        {
        }

        protected void StartErrorReporting()
        {

        }
        public void Err(string message)
        {
            if (ErrorEvent == null)
            {
                return;
            }
            ErrorEvent(this, new ParserErrorEventArgs(message, false));
        }
        public void Warn(string message)
        {
            if (ErrorEvent == null)
            {
                return;
            }
            ErrorEvent(this, new ParserErrorEventArgs(message, true));
        } 
        /**
         * Reports an condition that would make the infoset incompatible with XML
         * 1.0 as fatal.
         * 
         * @param message
         *            the message
         * @throws SAXException
         * @throws SAXParseException
         */
        public void Fatal(string message)
        {
            /*SAXParseException spe = new SAXParseException(message, this);
            if (errorHandler != null) {
                errorHandler.fatalError(spe);
            }
            throw spe;*/
            throw new Exception(message); // TODO
        }
        protected void SetInterLexerState(InterLexerState interLexerState)
        {

        }
         
    }
}