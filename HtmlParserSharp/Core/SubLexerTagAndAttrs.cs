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

    public enum SubLexerTagState
    {
        s08_TAG_OPEN_p = 9, //tag

        s09_CLOSE_TAG_OPEN_p = 10, //tag 
        s10_TAG_NAME_p = 11, //tag 

        s34_BEFORE_ATTRIBUTE_NAME_p = 12, //rawtext, tag
        s35_ATTRIBUTE_NAME_p = 13, //tag

        s36_AFTER_ATTRIBUTE_NAME_p = 14, //tag

        s37_BEFORE_ATTRIBUTE_VALUE_p = 15, //tag

        s38_ATTRIBUTE_VALUE_DOUBLE_QUOTED_p = 5, //tag

        s39_ATTRIBUTE_VALUE_SINGLE_QUOTED_p = 6, //tag 
        s40_ATTRIBUTE_VALUE_UNQUOTED_p = 7, //tag 
        //TODO: R41_CharacterReferenceInAttributeValue() 
        s42__AFTER_ATTRIBUTE_VALUE_QUOTED_p = 16, //tag
        CHARACTER_REFERENCE_HILO_LOOKUP_p = 53,//tag,
        CHARACTER_REFERENCE_TAIL_p = 48, //tag
    }
    class SubLexerTagAndAttr : SubLexer
    {
        ElementName endTagExpectation = null;
        /// <summary>
        /// UTF-16 code unit array containing less than and solidus for emitting
        /// those characters on certain parse errors.
        /// </summary>
        static readonly char[] LT_SOLIDUS = { '<', '/' };
        char[] endTagExpectationAsArray; // not @Auto! 
        int entCol;
        int hi;
        int lo;
        int candidate;
        bool endTag;
        int strBufMark;
        int index = 0;
        char firstCharKey;

        bool newAttributesEachTime;
        bool metaBoundaryPassed;
        bool html4ModeCompatibleWithXhtml1Schemata;
        /// <summary>
        /// true when HTML4-specific additional errors are requested
        /// </summary>
        bool html4;

        /**
       * The attribute holder.
       */
        HtmlAttributes attributes;
        int mappingLangToXmlLang;
        int cstart;
        /**
         * The current tag token name.
         */
        ElementName tagName = null;
        XmlViolationPolicy namePolicy = XmlViolationPolicy.AlterInfoset;
        XmlViolationPolicy xmlnsPolicy = XmlViolationPolicy.AlterInfoset;
        /// <summary>
        /// The current attribute name.
        /// </summary>
        AttributeName attributeName = null;
        char additional;

        void ErrProcessingInstruction()
        {
        }
        void ErrLtGt()
        {
        }

        void SetAdditionalAndRememberAmpersandLocation(char add)
        {
            additional = add;
            // [NOCPP[
            //ampersandLocation = new Location(this.LineNumber, this.ColumnNumber);
            // ]NOCPP]
        }
        void EmitOrAppendTwo(char[] val, SubLexerTagState returnState)
        {
            throw new NotSupportedException();
            //TODO: review here=>   use != or == ?
            //if ((returnState & DATA_AND_RCDATA_MASK) != 0)
            //if (((byte)returnState & DATA_AND_RCDATA_MASK) == 0)
            //{
            //    AppendLongStrBuf(val[0]);
            //    AppendLongStrBuf(val[1]);
            //}
            //else
            //{
            //    TokenListener.Characters(val, 0, 2);
            //}
        }
        void EmitOrAppendOne(char[] val, SubLexerTagState returnState)
        {
            throw new NotSupportedException();
            //if (((byte)returnState & DATA_AND_RCDATA_MASK) == 0)
            //{
            //    AppendLongStrBuf(val[0]);
            //}
            //else
            //{
            //    TokenListener.Characters(val, 0, 1);
            //}
        }
        void EmitOrAppendStrBuf(SubLexerTagState state)
        {

            throw new NotSupportedException();
        }
        SubLexerTagState EmitCurrentTagToken2(bool isSelfClosingTag)
        {
            throw new NotSupportedException();
        }
        private void ResetAttributes()
        {
            // [NOCPP[
            if (newAttributesEachTime)
            {
                // ]NOCPP]
                attributes = null;
                // [NOCPP[
            }
            else
            {
                attributes.Clear(mappingLangToXmlLang);
            }
            // ]NOCPP]
        }
        void StrBufToElementNameString()
        {
            // if (strBufOffset != -1) {
            // return ElementName.elementNameByBuffer(buf, strBufOffset, strBufLen);
            // } else {

            tagName = ElementName.ElementNameByBuffer(CopyFromStringBuiler(strBuffer, 0, this.strBuffer.Length));
            // }
        }


        TokenizerState EmitCurrentTagToken(bool selfClosing, int pos)
        {
            cstart = pos + 1;
            MaybeErrSlashInEndTag(selfClosing);
            stateSave = TokenizerState.s01_DATA_i;
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
                TokenListener.StartTag(tagName, attrs, selfClosing);
            }
            tagName = null;
            ResetAttributes();
            /*
             * The token handler may have called setStateAndEndTagExpectation
             * and changed stateSave since the start of this method.
             */
            return stateSave;
        }
        void EmitOrAppendTwo(char[] val, TokenizerState returnState)
        {
            //TODO: review here=>   use != or == ?
            //if ((returnState & DATA_AND_RCDATA_MASK) != 0)
            if (((byte)returnState & DATA_AND_RCDATA_MASK) == 0)
            {
                AppendLongStrBuf(val[0]);
                AppendLongStrBuf(val[1]);
            }
            else
            {
                TokenListener.Characters(val, 0, 2);
            }
        }

        void EmitOrAppendOne(char[] val, TokenizerState returnState)
        {
            if (((byte)returnState & DATA_AND_RCDATA_MASK) == 0)
            {
                AppendLongStrBuf(val[0]);
            }
            else
            {
                TokenListener.Characters(val, 0, 1);
            }
        }

        void AttributeNameComplete()
        {


            // if (strBufOffset != -1) {
            // attributeName = AttributeName.nameByBuffer(buf, strBufOffset,
            // strBufLen, namePolicy != XmlViolationPolicy.ALLOW);
            // } else {
            char[] copyBuffer = CopyFromStringBuiler(this.strBuffer, 0, this.strBuffer.Length);
            attributeName = AttributeName.NameByBuffer(copyBuffer, 0, copyBuffer.Length
                    , namePolicy != XmlViolationPolicy.Allow
                    );
            // }

            if (attributes == null)
            {
                attributes = new HtmlAttributes(mappingLangToXmlLang);
            }

            /*
             * When the user agent leaves the attribute name state (and before
             * emitting the tag token, if appropriate), the complete attribute's
             * name must be compared to the other attributes on the same token; if
             * there is already an attribute on the token with the exact same name,
             * then this is a parse error and the new attribute must be dropped,
             * along with the value that gets associated with it (if any).
             */
            if (attributes.Contains(attributeName))
            {
                ErrDuplicateAttribute();
                attributeName = null;
            }
        }

        void AddAttributeWithoutValue()
        {

            //NoteAttributeWithoutValue();

            // [NOCPP[
            if (metaBoundaryPassed && AttributeName.CHARSET == attributeName
                    && ElementName.META == tagName)
            {
                Err("A \u201Ccharset\u201D attribute on a \u201Cmeta\u201D element found after the first 512 bytes.");
            }
            // ]NOCPP]
            if (attributeName != null)
            {
                //TODO: not need to validate this on lexer
                //consider validate on the listenser side

                if (html4)
                {
                    if (attributeName.IsBoolean)
                    {
                        if (html4ModeCompatibleWithXhtml1Schemata)
                        {
                            attributes.AddAttribute(attributeName,
                                    attributeName.GetLocal(AttributeName.HTML),
                                    xmlnsPolicy);
                        }
                        else
                        {
                            attributes.AddAttribute(attributeName, "", xmlnsPolicy);
                        }
                    }
                    else
                    {
                        if (AttributeName.BORDER != attributeName)
                        {
                            Err("Attribute value omitted for a non-bool attribute. (HTML4-only error.)");
                            attributes.AddAttribute(attributeName, "", xmlnsPolicy);
                        }
                    }
                }
                else
                {
                    if (AttributeName.SRC == attributeName
                            || AttributeName.HREF == attributeName)
                    {
                        Warn("Attribute \u201C"
                                + attributeName.GetLocal(AttributeName.HTML)
                                + "\u201D without an explicit value seen. The attribute may be dropped by IE7.");
                    }
                    attributes.AddAttribute(attributeName,
                            String.Empty
                            , xmlnsPolicy
                    );
                }
                attributeName = null; // attributeName has been adopted by the
                // |attributes| object
            }
        }
        string LongStrBufToString()
        {
            return this.longStrBuffer.ToString();
        }
        void AddAttributeWithValue()
        {

            // [NOCPP[
            if (metaBoundaryPassed && ElementName.META == tagName
                    && AttributeName.CHARSET == attributeName)
            {
                Err("A \u201Ccharset\u201D attribute on a \u201Cmeta\u201D element found after the first 512 bytes.");
            }
            // ]NOCPP]
            if (attributeName != null)
            {
                String val = LongStrBufToString(); // Ownership transferred to
                // HtmlAttributes

                // [NOCPP[
                if (!endTag && html4 && html4ModeCompatibleWithXhtml1Schemata
                        && attributeName.IsCaseFolded)
                {
                    val = NewAsciiLowerCaseStringFromString(val);
                }
                // ]NOCPP]
                attributes.AddAttribute(attributeName, val
                    // [NOCPP[
                        , xmlnsPolicy
                    // ]NOCPP]
                );
                attributeName = null; // attributeName has been adopted by the
                // |attributes| object
            }
        }
        static String NewAsciiLowerCaseStringFromString(String str)
        {
            if (str == null)
            {
                return null;
            }
            char[] buf = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c >= 'A' && c <= 'Z')
                {
                    c += (char)0x20;
                }
                buf[i] = c;
            }
            return new String(buf);
        }
        public TokenizerState OutputState
        {
            get { return this.stateSave; }
        }
        public void StateLoop3_Tag(SubLexerTagState state, SubLexerTagState returnState)
        {


            char c2;
            CharMode charMode;
            while (reader.ReadNext(out c2, out charMode))
            {
                //read one char
                //find mode
                switch (state)
                {
                    case SubLexerTagState.s08_TAG_OPEN_p:
                        {
                            switch (c2)
                            {
                                case '!':
                                    {
                                    } break;
                                case '?':
                                    {
                                    } break;
                                case '/':
                                    {
                                    } break;
                                default:
                                    {
                                    } break;
                            }
                        } break;
                }
            }


            for (; ; )
            {
            //*************
            continueStateloop:
                //************* 
                switch (state)
                {

                    case SubLexerTagState.s08_TAG_OPEN_p:
                        /*tagopenloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                /*
                                 * The behavior of this state depends on the content
                                 * model flag.
                                 */

                                /*
                                 * If the content model flag is set to the PCDATA state
                                 * Consume the next input character:
                                 */
                                if (c >= 'A' && c <= 'Z')
                                {
                                    /*
                                     * U+0041 LATIN CAPITAL LETTER A through to U+005A
                                     * LATIN CAPITAL LETTER Z Create a new start tag
                                     * token,
                                     */
                                    endTag = false;
                                    /*
                                     * set its tag name to the lowercase TokenizerState.version of the
                                     * input character (add 0x0020 to the character's
                                     * code point),
                                     */
                                    ClearStrBufAndAppend((char)(c + 0x20));
                                    /* then switch to the tag name state. */
                                    //state = Transition(state, Tokenizer.TAG_NAME, reconsume, pos);
                                    state = SubLexerTagState.s10_TAG_NAME_p;
                                    /*
                                     * (Don't emit the token yet; further details will
                                     * be filled in before it is emitted.)
                                     */
                                    goto breakTagopenloop;
                                    // goto continueStateloop;
                                }
                                else if (c >= 'a' && c <= 'z')
                                {
                                    /*
                                     * U+0061 LATIN SMALL LETTER A through to U+007A
                                     * LATIN SMALL LETTER Z Create a new start tag
                                     * token,
                                     */
                                    endTag = false;
                                    /*
                                     * set its tag name to the input character,
                                     */
                                    ClearStrBufAndAppend(c);
                                    /* then switch to the tag name state. */
                                    //state = Transition(state, Tokenizer.TAG_NAME, reconsume, pos);
                                    state = SubLexerTagState.s10_TAG_NAME_p;
                                    /*
                                     * (Don't emit the token yet; further details will
                                     * be filled in before it is emitted.)
                                     */
                                    goto breakTagopenloop;
                                    // goto continueStateloop;
                                }
                                switch (c)
                                {
                                    case '!':
                                        /*
                                         * U+0021 EXCLAMATION MARK (!) Switch to the
                                         * markup declaration open state.
                                         */
                                        //state = Transition(state, Tokenizer.MARKUP_DECLARATION_OPEN, reconsume, pos);
                                        //   state = SubLexerTagState.s45_MARKUP_DECLARATION_OPEN_i;
                                        SetInterLexerState(TokenizerState.s45_MARKUP_DECLARATION_OPEN_i);
                                        goto continueStateloop;
                                    case '/':
                                        /*
                                         * U+002F SOLIDUS (/) Switch to the close tag
                                         * open state.
                                         */
                                        //state = Transition(state, Tokenizer.CLOSE_TAG_OPEN, reconsume, pos);
                                        state = SubLexerTagState.s09_CLOSE_TAG_OPEN_p;
                                        goto continueStateloop;
                                    case '?':
                                        /*
                                         * U+003F QUESTION MARK (?) Parse error.
                                         */
                                        ErrProcessingInstruction();
                                        /*
                                         * Switch to the bogus comment state.
                                         */
                                        ClearLongStrBufAndAppend(c);
                                        //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);
                                        //state = TokenizerState.s44_BOGUS_COMMENT_i;
                                        SetInterLexerState(TokenizerState.s44_BOGUS_COMMENT_i);
                                        goto continueStateloop;
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Parse error.
                                         */
                                        ErrLtGt();
                                        /*
                                         * Emit a U+003C LESS-THAN SIGN character token
                                         * and a U+003E GREATER-THAN SIGN character
                                         * token.
                                         */
                                        TokenListener.Characters(LT_GT, 0, 2);
                                        /* Switch to the data state. */
                                        //cstart = pos + 1;
                                        reader.SkipOneAndStartCollect();
                                        //state = Transition(state, Tokenizer.DATA, reconsume, pos);
                                        //state = TokenizerState.s01_DATA_i;
                                        SetInterLexerState(TokenizerState.s01_DATA_i);
                                        goto continueStateloop;
                                    default:
                                        /*
                                         * Anything else Parse error.
                                         */
                                        ErrBadCharAfterLt(c);
                                        /*
                                         * Emit a U+003C LESS-THAN SIGN character token
                                         */
                                        TokenListener.Characters(LT_GT, 0, 1);
                                        /*
                                         * and reconsume the current input character in
                                         * the data state.
                                         */
                                        reader.StartCollect();
                                        //state = Transition(state, Tokenizer.DATA, reconsume, pos);
                                        SetInterLexerState(TokenizerState.s01_DATA_i);
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------

                        breakTagopenloop:
                            goto case SubLexerTagState.s10_TAG_NAME_p;
                        }
                    //  FALL THROUGH DON'T REORDER
                    case SubLexerTagState.s10_TAG_NAME_p:
                        /*tagnameloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                /*
                                 * Consume the next input character:
                                 */
                                switch (c)
                                {
                                    case '\r':
                                        SilentCarriageReturn();
                                        StrBufToElementNameString();
                                        //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                                        state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                        
                                        goto breakStateloop;
                                    case '\n':
                                    case ' ':
                                    case '\t':
                                    case '\u000C':
                                        /*
                                         * U+0009 CHARACTER TABULATION U+000A LINE FEED
                                         * (LF) U+000C FORM FEED (FF) U+0020 SPACE
                                         * Switch to the before attribute name state.
                                         */
                                        StrBufToElementNameString();
                                        //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                                        state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                        goto breakTagnameloop;
                                    // goto continueStateloop;
                                    case '/':
                                        /*
                                         * U+002F SOLIDUS (/) Switch to the self-closing
                                         * start tag state.
                                         */
                                        StrBufToElementNameString();
                                        //state = Transition(state, Tokenizer.SELF_CLOSING_START_TAG, reconsume, pos);
                                        //state = TokenizerState.s43_SELF_CLOSING_START_TAG_i;
                                        SetInterLexerState(TokenizerState.s43_SELF_CLOSING_START_TAG_i);
                                        goto continueStateloop;
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Emit the current
                                         * tag token.
                                         */
                                        StrBufToElementNameString();
                                        //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                                        state = EmitCurrentTagToken2(false);
                                        if (shouldSuspend)
                                        {
                                            goto breakStateloop;
                                        }
                                        /*
                                         * Switch to the data state.
                                         */
                                        goto continueStateloop;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        goto default;
                                    // fall thru
                                    default:
                                        if (c >= 'A' && c <= 'Z')
                                        {
                                            /*
                                             * U+0041 LATIN CAPITAL LETTER A through to
                                             * U+005A LATIN CAPITAL LETTER Z Append the
                                             * lowercase TokenizerState.version of the current input
                                             * character (add 0x0020 to the character's
                                             * code point) to the current tag token's
                                             * tag name.
                                             */
                                            c += (char)0x20;
                                        }
                                        /*
                                         * Anything else Append the current input
                                         * character to the current tag token's tag
                                         * name.
                                         */
                                        AppendStrBuf(c);
                                        /*
                                         * Stay in the tag name state.
                                         */
                                        continue;
                                }
                            }

                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakTagnameloop:
                            goto case SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                        }
                    // FALLTHRU DON'T REORDER
                    case SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p:
                        /*beforeattributenameloop:*/
                        {

                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '\r':
                                        SilentCarriageReturn();
                                        goto breakStateloop;
                                    case '\n':
                                    case ' ':
                                    case '\t':
                                    case '\u000C':
                                        /*
                                         * U+0009 CHARACTER TABULATION U+000A LINE FEED
                                         * (LF) U+000C FORM FEED (FF) U+0020 SPACE Stay
                                         * in the before attribute name state.
                                         */
                                        continue;
                                    case '/':
                                        /*
                                         * U+002F SOLIDUS (/) Switch to the self-closing
                                         * start tag state.
                                         */
                                        //state = Transition(state, Tokenizer.SELF_CLOSING_START_TAG, reconsume, pos);
                                        //state = TokenizerState.s43_SELF_CLOSING_START_TAG_i;
                                        SetInterLexerState(TokenizerState.s43_SELF_CLOSING_START_TAG_i);
                                        goto continueStateloop;
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Emit the current
                                         * tag token.
                                         */
                                        //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                                        state = EmitCurrentTagToken2(false);
                                        if (shouldSuspend)
                                        {
                                            goto breakStateloop;
                                        }
                                        /*
                                         * Switch to the data state.
                                         */
                                        goto continueStateloop;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        // fall thru
                                        goto case '\"';
                                    case '\"':
                                    case '\'':
                                    case '<':
                                    case '=':
                                        /*
                                         * U+0022 QUOTATION MARK (") U+0027 APOSTROPHE
                                         * (') U+003C LESS-THAN SIGN (<) U+003D EQUALS
                                         * SIGN (=) Parse error.
                                         */
                                        ErrBadCharBeforeAttributeNameOrNull(c);
                                        /*
                                         * Treat it as per the "anything else" entry
                                         * below.
                                         */
                                        goto default;
                                    default:
                                        /*
                                         * Anything else Start a new attribute in the
                                         * current tag token.
                                         */
                                        if (c >= 'A' && c <= 'Z')
                                        {
                                            /*
                                             * U+0041 LATIN CAPITAL LETTER A through to
                                             * U+005A LATIN CAPITAL LETTER Z Set that
                                             * attribute's name to the lowercase TokenizerState.version
                                             * of the current input character (add
                                             * 0x0020 to the character's code point)
                                             */
                                            c += (char)0x20;
                                        }
                                        /*
                                         * Set that attribute's name to the current
                                         * input character,
                                         */
                                        ClearStrBufAndAppend(c);
                                        /*
                                         * and its value to the empty string.
                                         */
                                        // Will do later.
                                        /*
                                         * Switch to the attribute name state.
                                         */
                                        //state = Transition(state, Tokenizer.ATTRIBUTE_NAME, reconsume, pos);
                                        state = SubLexerTagState.s35_ATTRIBUTE_NAME_p;

                                        goto breakBeforeattributenameloop;
                                    // goto continueStateloop;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakBeforeattributenameloop:
                            goto case SubLexerTagState.s35_ATTRIBUTE_NAME_p;
                        }
                    // FALLTHRU DON'T REORDER
                    case SubLexerTagState.s35_ATTRIBUTE_NAME_p:
                        /*attributenameloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                /*
                                 * Consume the next input character:
                                 */
                                switch (c)
                                {
                                    case '\r':
                                        SilentCarriageReturn();
                                        AttributeNameComplete();
                                        //state = Transition(state, Tokenizer.AFTER_ATTRIBUTE_NAME, reconsume, pos);
                                        state = SubLexerTagState.s36_AFTER_ATTRIBUTE_NAME_p;
                                        goto breakStateloop;
                                    case '\n':
                                    case ' ':
                                    case '\t':
                                    case '\u000C':
                                        /*
                                         * U+0009 CHARACTER TABULATION U+000A LINE FEED
                                         * (LF) U+000C FORM FEED (FF) U+0020 SPACE
                                         * Switch to the after attribute name state.
                                         */
                                        AttributeNameComplete();
                                        //state = Transition(state, Tokenizer.AFTER_ATTRIBUTE_NAME, reconsume, pos);
                                        state = SubLexerTagState.s36_AFTER_ATTRIBUTE_NAME_p;
                                        goto continueStateloop;
                                    case '/':
                                        /*
                                         * U+002F SOLIDUS (/) Switch to the self-closing
                                         * start tag state.
                                         */
                                        AttributeNameComplete();
                                        AddAttributeWithoutValue();
                                        //state = Transition(state, Tokenizer.SELF_CLOSING_START_TAG, reconsume, pos);
                                        //state = TokenizerState.s43_SELF_CLOSING_START_TAG_i;
                                        SetInterLexerState(TokenizerState.s43_SELF_CLOSING_START_TAG_i);
                                        goto continueStateloop;
                                    case '=':
                                        /*
                                         * U+003D EQUALS SIGN (=) Switch to the before
                                         * attribute value state.
                                         */
                                        AttributeNameComplete();
                                        //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_VALUE, reconsume, pos);
                                        state = SubLexerTagState.s37_BEFORE_ATTRIBUTE_VALUE_p;
                                        goto breakAttributenameloop;
                                    // goto continueStateloop;
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Emit the current
                                         * tag token.
                                         */
                                        AttributeNameComplete();
                                        AddAttributeWithoutValue();
                                        //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                                        state = EmitCurrentTagToken2(false);
                                        if (shouldSuspend)
                                        {
                                            goto breakStateloop;
                                        }
                                        /*
                                         * Switch to the data state.
                                         */
                                        goto continueStateloop;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        // fall thru
                                        goto case '\"';
                                    case '\"':
                                    case '\'':
                                    case '<':
                                        /*
                                         * U+0022 QUOTATION MARK (") U+0027 APOSTROPHE
                                         * (') U+003C LESS-THAN SIGN (<) Parse error.
                                         */
                                        ErrQuoteOrLtInAttributeNameOrNull(c);
                                        /*
                                         * Treat it as per the "anything else" entry
                                         * below.
                                         */
                                        goto default;
                                    default:
                                        if (c >= 'A' && c <= 'Z')
                                        {
                                            /*
                                             * U+0041 LATIN CAPITAL LETTER A through to
                                             * U+005A LATIN CAPITAL LETTER Z Append the
                                             * lowercase TokenizerState.version of the current input
                                             * character (add 0x0020 to the character's
                                             * code point) to the current attribute's
                                             * name.
                                             */
                                            c += (char)0x20;
                                        }
                                        /*
                                         * Anything else Append the current input
                                         * character to the current attribute's name.
                                         */
                                        AppendStrBuf(c);
                                        /*
                                         * Stay in the attribute name state.
                                         */
                                        continue;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakAttributenameloop:
                            goto case SubLexerTagState.s37_BEFORE_ATTRIBUTE_VALUE_p;
                        }
                    // FALLTHRU DON'T REORDER
                    case SubLexerTagState.s37_BEFORE_ATTRIBUTE_VALUE_p:
                        /*beforeattributevalueloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '\r':
                                        SilentCarriageReturn();
                                        goto breakStateloop;
                                    case '\n':
                                    case ' ':
                                    case '\t':
                                    case '\u000C':
                                        /*
                                         * U+0009 CHARACTER TABULATION U+000A LINE FEED
                                         * (LF) U+000C FORM FEED (FF) U+0020 SPACE Stay
                                         * in the before attribute value state.
                                         */
                                        continue;
                                    case '"':
                                        /*
                                         * U+0022 QUOTATION MARK (") Switch to the
                                         * attribute value (double-quoted) state.
                                         */
                                        ClearLongStrBuf();
                                        //state = Transition(state, Tokenizer.ATTRIBUTE_VALUE_DOUBLE_QUOTED, reconsume, pos);
                                        state = SubLexerTagState.s38_ATTRIBUTE_VALUE_DOUBLE_QUOTED_p;

                                        goto breakBeforeattributevalueloop;
                                    // goto continueStateloop;
                                    case '&':
                                        /*
                                         * U+0026 AMPERSAND (&) Switch to the attribute
                                         * value (unquoted) state and reconsume this
                                         * input character.
                                         */
                                        ClearLongStrBuf();
                                        //state = Transition(state, Tokenizer.ATTRIBUTE_VALUE_UNQUOTED, reconsume, pos);
                                        state = SubLexerTagState.s40_ATTRIBUTE_VALUE_UNQUOTED_p;
                                        NoteUnquotedAttributeValue();
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                    case '\'':
                                        /*
                                         * U+0027 APOSTROPHE (') Switch to the attribute
                                         * value (single-quoted) state.
                                         */
                                        ClearLongStrBuf();
                                        //state = Transition(state, Tokenizer.ATTRIBUTE_VALUE_SINGLE_QUOTED, reconsume, pos);
                                        state = SubLexerTagState.s39_ATTRIBUTE_VALUE_SINGLE_QUOTED_p;
                                        goto continueStateloop;
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Parse error.
                                         */
                                        ErrAttributeValueMissing();
                                        /*
                                         * Emit the current tag token.
                                         */
                                        AddAttributeWithoutValue();
                                        //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                                        state = EmitCurrentTagToken2(false);
                                        if (shouldSuspend)
                                        {
                                            goto breakStateloop;
                                        }
                                        /*
                                         * Switch to the data state.
                                         */
                                        goto continueStateloop;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        // fall thru
                                        goto case '<';
                                    case '<':
                                    case '=':
                                    case '`':
                                        /*
                                         * U+003C LESS-THAN SIGN (<) U+003D EQUALS SIGN
                                         * (=) U+0060 GRAVE ACCENT (`)
                                         */
                                        ErrLtOrEqualsOrGraveInUnquotedAttributeOrNull(c);
                                        /*
                                         * Treat it as per the "anything else" entry
                                         * below.
                                         */
                                        goto default;
                                    default:
                                        // [NOCPP[
                                        ErrHtml4NonNameInUnquotedAttribute(c);
                                        // ]NOCPP]
                                        /*
                                         * Anything else Append the current input
                                         * character to the current attribute's value.
                                         */
                                        ClearLongStrBufAndAppend(c);
                                        /*
                                         * Switch to the attribute value (unquoted)
                                         * state.
                                         */

                                        //state = Transition(state, Tokenizer.ATTRIBUTE_VALUE_UNQUOTED, reconsume, pos);
                                        state = SubLexerTagState.s40_ATTRIBUTE_VALUE_UNQUOTED_p;

                                        NoteUnquotedAttributeValue();
                                        goto continueStateloop;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakBeforeattributevalueloop:
                            goto case SubLexerTagState.s38_ATTRIBUTE_VALUE_DOUBLE_QUOTED_p;
                        }

                    // FALLTHRU DON'T REORDER
                    case SubLexerTagState.s38_ATTRIBUTE_VALUE_DOUBLE_QUOTED_p:
                        /*attributevaluedoublequotedloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '"':
                                        /*
                                         * U+0022 QUOTATION MARK (") Switch to the after
                                         * attribute value (quoted) state.
                                         */
                                        AddAttributeWithValue();

                                        //state = Transition(state, Tokenizer.AFTER_ATTRIBUTE_VALUE_QUOTED, reconsume, pos);
                                        state = SubLexerTagState.s42__AFTER_ATTRIBUTE_VALUE_QUOTED_p;
                                        goto breakAttributevaluedoublequotedloop;
                                    // goto continueStateloop;
                                    case '&':
                                        /*
                                         * U+0026 AMPERSAND (&) Switch to the character
                                         * reference in attribute value state, with the
                                         * additional allowed character being U+0022
                                         * QUOTATION MARK (").
                                         */
                                        ClearStrBufAndAppend(c);
                                        SetAdditionalAndRememberAmpersandLocation('\"');
                                        returnState = state;
                                        //state = Transition(state, Tokenizer.CONSUME_CHARACTER_REFERENCE, reconsume, pos);
                                        //state = TokenizerState.CONSUME_CHARACTER_REFERENCE_i;
                                        SetInterLexerState(TokenizerState.CONSUME_CHARACTER_REFERENCE_i);
                                        goto continueStateloop;
                                    case '\r':
                                        AppendLongStrBufCarriageReturn();
                                        goto breakStateloop;
                                    case '\n':
                                        AppendLongStrBufLineFeed();
                                        continue;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        // fall thru
                                        goto default;
                                    default:
                                        /*
                                         * Anything else Append the current input
                                         * character to the current attribute's value.
                                         */
                                        AppendLongStrBuf(c);
                                        /*
                                         * Stay in the attribute value (double-quoted)
                                         * state.
                                         */
                                        continue;
                                }

                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakAttributevaluedoublequotedloop:
                            goto case SubLexerTagState.s42__AFTER_ATTRIBUTE_VALUE_QUOTED_p;
                        }
                    // FALLTHRU DON'T REORDER
                    case SubLexerTagState.s42__AFTER_ATTRIBUTE_VALUE_QUOTED_p:
                        /*afterattributevaluequotedloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '\r':
                                        SilentCarriageReturn();
                                        //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                                        //state = TokenizerState.s34_BEFORE_ATTRIBUTE_NAME_i;
                                        state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                        goto breakStateloop;
                                    case '\n':
                                    case ' ':
                                    case '\t':
                                    case '\u000C':
                                        /*
                                         * U+0009 CHARACTER TABULATION U+000A LINE FEED
                                         * (LF) U+000C FORM FEED (FF) U+0020 SPACE
                                         * Switch to the before attribute name state.
                                         */
                                        //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                                        //state = TokenizerState.s34_BEFORE_ATTRIBUTE_NAME_i;
                                        state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                        goto continueStateloop;
                                    case '/':
                                        /*
                                         * U+002F SOLIDUS (/) Switch to the self-closing
                                         * start tag state.
                                         */
                                        //state = Transition(state, Tokenizer.SELF_CLOSING_START_TAG, reconsume, pos);
                                        //state = TokenizerState.s43_SELF_CLOSING_START_TAG_i;
                                        state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                        goto breakAfterattributevaluequotedloop;
                                    // goto continueStateloop;
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Emit the current
                                         * tag token.
                                         */
                                        //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                                        state = EmitCurrentTagToken2(false);
                                        if (shouldSuspend)
                                        {
                                            goto breakStateloop;
                                        }
                                        /*
                                         * Switch to the data state.
                                         */
                                        goto continueStateloop;
                                    default:
                                        /*
                                         * Anything else Parse error.
                                         */
                                        ErrNoSpaceBetweenAttributes();
                                        /*
                                         * Reconsume the character in the before
                                         * attribute name state.
                                         */
                                        //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                                        //state = TokenizerState.s34_BEFORE_ATTRIBUTE_NAME_i;
                                        state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakAfterattributevaluequotedloop:
                            goto case (SubLexerTagState)TokenizerState.s43_SELF_CLOSING_START_TAG_i;
                        }
                    // FALLTHRU DON'T REORDER
                    case (SubLexerTagState)TokenizerState.s43_SELF_CLOSING_START_TAG_i:
                        {

                            char c;
                            if (!reader.ReadNext(out c))
                            {
                                goto breakStateloop;
                            }
                            //---------------------------------
                            /*
                             * Consume the next input character:
                             */
                            switch (c)
                            {
                                case '>':
                                    /*
                                     * U+003E GREATER-THAN SIGN (>) Set the self-closing
                                     * flag of the current tag token. Emit the current
                                     * tag token.
                                     */
                                    // [NOCPP[
                                    ErrHtml4XmlVoidSyntax();
                                    // ]NOCPP]
                                    //state = Transition(state, EmitCurrentTagToken(true, pos), reconsume, pos);
                                    state = EmitCurrentTagToken2(true);
                                    if (shouldSuspend)
                                    {
                                        goto breakStateloop;
                                    }
                                    /*
                                     * Switch to the data state.
                                     */
                                    goto continueStateloop;
                                default:
                                    /* Anything else Parse error. */
                                    ErrSlashNotFollowedByGt();
                                    /*
                                     * Reconsume the character in the before attribute
                                     * name state.
                                     */
                                    //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                                    //state = TokenizerState.s34_BEFORE_ATTRIBUTE_NAME_i;

                                    state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                    reader.StepBack();
                                    //reconsume = true;
                                    goto continueStateloop;
                            }
                        }

                    // XXX reorder point
                    case SubLexerTagState.s40_ATTRIBUTE_VALUE_UNQUOTED_p:
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '\r':
                                        SilentCarriageReturn();
                                        AddAttributeWithValue();
                                        //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                                        //state = TokenizerState.s34_BEFORE_ATTRIBUTE_NAME_i;
                                        state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                        goto breakStateloop;
                                    case '\n':
                                    case ' ':
                                    case '\t':
                                    case '\u000C':
                                        /*
                                         * U+0009 CHARACTER TABULATION U+000A LINE FEED
                                         * (LF) U+000C FORM FEED (FF) U+0020 SPACE
                                         * Switch to the before attribute name state.
                                         */
                                        AddAttributeWithValue();
                                        //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                                        //state = TokenizerState.s34_BEFORE_ATTRIBUTE_NAME_i;
                                        state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                        goto continueStateloop;
                                    case '&':
                                        /*
                                         * U+0026 AMPERSAND (&) Switch to the character
                                         * reference in attribute value state, with the
                                         * additional allowed character being U+003E
                                         * GREATER-THAN SIGN (>)
                                         */
                                        ClearStrBufAndAppend(c);
                                        SetAdditionalAndRememberAmpersandLocation('>');
                                        returnState = state;
                                        //state = Transition(state, Tokenizer.CONSUME_CHARACTER_REFERENCE, reconsume, pos);
                                        //state = TokenizerState.CONSUME_CHARACTER_REFERENCE_i;
                                        SetInterLexerState(TokenizerState.CONSUME_CHARACTER_REFERENCE_i);
                                        goto continueStateloop;
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Emit the current
                                         * tag token.
                                         */
                                        AddAttributeWithValue();
                                        //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                                        state = EmitCurrentTagToken2(false);
                                        if (shouldSuspend)
                                        {
                                            goto breakStateloop;
                                        }
                                        /*
                                         * Switch to the data state.
                                         */
                                        goto continueStateloop;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        goto case '<';
                                    // fall thru
                                    case '<':
                                    case '\"':
                                    case '\'':
                                    case '=':
                                    case '`':
                                        /*
                                         * U+0022 QUOTATION MARK (") U+0027 APOSTROPHE
                                         * (') U+003C LESS-THAN SIGN (<) U+003D EQUALS
                                         * SIGN (=) U+0060 GRAVE ACCENT (`) Parse error.
                                         */
                                        ErrUnquotedAttributeValOrNull(c);
                                        /*
                                         * Treat it as per the "anything else" entry
                                         * below.
                                         */
                                        // fall through
                                        goto default;
                                    default:
                                        // [NOCPP]
                                        ErrHtml4NonNameInUnquotedAttribute(c);
                                        // ]NOCPP]
                                        /*
                                         * Anything else Append the current input
                                         * character to the current attribute's value.
                                         */
                                        AppendLongStrBuf(c);
                                        /*
                                         * Stay in the attribute value (unquoted) state.
                                         */
                                        continue;
                                }
                            }
                            //-------------------------------
                            goto breakStateloop;
                        }
                    // XXX reorder point
                    case SubLexerTagState.s36_AFTER_ATTRIBUTE_NAME_p:
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                switch (c)
                                {
                                    case '\r':
                                        SilentCarriageReturn();
                                        goto breakStateloop;
                                    case '\n':
                                    case ' ':
                                    case '\t':
                                    case '\u000C':
                                        /*
                                         * U+0009 CHARACTER TABULATION U+000A LINE FEED
                                         * (LF) U+000C FORM FEED (FF) U+0020 SPACE Stay
                                         * in the after attribute name state.
                                         */
                                        continue;
                                    case '/':
                                        /*
                                         * U+002F SOLIDUS (/) Switch to the self-closing
                                         * start tag state.
                                         */
                                        AddAttributeWithoutValue();
                                        //state = Transition(state, Tokenizer.SELF_CLOSING_START_TAG, reconsume, pos);
                                        //state = TokenizerState.s43_SELF_CLOSING_START_TAG_i;
                                        SetInterLexerState(TokenizerState.s43_SELF_CLOSING_START_TAG_i);
                                        goto continueStateloop;
                                    case '=':
                                        /*
                                         * U+003D EQUALS SIGN (=) Switch to the before
                                         * attribute value state.
                                         */
                                        //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_VALUE, reconsume, pos);
                                        state = SubLexerTagState.s37_BEFORE_ATTRIBUTE_VALUE_p;
                                        goto continueStateloop;
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Emit the current
                                         * tag token.
                                         */
                                        AddAttributeWithoutValue();
                                        //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                                        state = EmitCurrentTagToken2(false);
                                        if (shouldSuspend)
                                        {
                                            goto breakStateloop;
                                        }
                                        /*
                                         * Switch to the data state.
                                         */
                                        goto continueStateloop;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        goto case '\"';
                                    // fall thru
                                    case '\"':
                                    case '\'':
                                    case '<':
                                        ErrQuoteOrLtInAttributeNameOrNull(c);
                                        /*
                                         * Treat it as per the "anything else" entry
                                         * below.
                                         */
                                        goto default;
                                    default:
                                        AddAttributeWithoutValue();
                                        /*
                                         * Anything else Start a new attribute in the
                                         * current tag token.
                                         */
                                        if (c >= 'A' && c <= 'Z')
                                        {
                                            /*
                                             * U+0041 LATIN CAPITAL LETTER A through to
                                             * U+005A LATIN CAPITAL LETTER Z Set that
                                             * attribute's name to the lowercase TokenizerState.version
                                             * of the current input character (add
                                             * 0x0020 to the character's code point)
                                             */
                                            c += (char)0x20;
                                        }
                                        /*
                                         * Set that attribute's name to the current
                                         * input character,
                                         */
                                        ClearStrBufAndAppend(c);
                                        /*
                                         * and its value to the empty string.
                                         */
                                        // Will do later.
                                        /*
                                         * Switch to the attribute name state.
                                         */
                                        //state = Transition(state, Tokenizer.ATTRIBUTE_NAME, reconsume, pos);
                                        state = SubLexerTagState.s35_ATTRIBUTE_NAME_p;
                                        goto continueStateloop;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                        }

                    // XXX reorder point
                    case SubLexerTagState.s39_ATTRIBUTE_VALUE_SINGLE_QUOTED_p:
                        /*attributevaluesinglequotedloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                /*
                                 * Consume the next input character:
                                 */
                                switch (c)
                                {
                                    case '\'':
                                        /*
                                         * U+0027 APOSTROPHE (') Switch to the after
                                         * attribute value (quoted) state.
                                         */
                                        AddAttributeWithValue();

                                        //state = Transition(state, Tokenizer.AFTER_ATTRIBUTE_VALUE_QUOTED, reconsume, pos);
                                        state = SubLexerTagState.s42__AFTER_ATTRIBUTE_VALUE_QUOTED_p;
                                        goto continueStateloop;
                                    case '&':
                                        /*
                                         * U+0026 AMPERSAND (&) Switch to the character
                                         * reference in attribute value state, with the
                                         * + additional allowed character being U+0027
                                         * APOSTROPHE (').
                                         */
                                        ClearStrBufAndAppend(c);
                                        SetAdditionalAndRememberAmpersandLocation('\'');
                                        returnState = state;
                                        //state = Transition(state, Tokenizer.CONSUME_CHARACTER_REFERENCE, reconsume, pos);
                                        //state = TokenizerState.CONSUME_CHARACTER_REFERENCE_i;
                                        SetInterLexerState(TokenizerState.CONSUME_CHARACTER_REFERENCE_i);
                                        goto breakAttributevaluesinglequotedloop;
                                    // goto continueStateloop;
                                    case '\r':
                                        AppendLongStrBufCarriageReturn();
                                        goto breakStateloop;
                                    case '\n':
                                        AppendLongStrBufLineFeed();
                                        continue;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        goto default;
                                    // fall thru
                                    default:
                                        /*
                                         * Anything else Append the current input
                                         * character to the current attribute's value.
                                         */
                                        AppendLongStrBuf(c);
                                        /*
                                         * Stay in the attribute value (double-quoted)
                                         * state.
                                         */
                                        continue;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakAttributevaluesinglequotedloop:
                            goto case (SubLexerTagState)TokenizerState.CONSUME_CHARACTER_REFERENCE_i;
                        }
                    // FALLTHRU DON'T REORDER 
                    case (SubLexerTagState)TokenizerState.CONSUME_CHARACTER_REFERENCE_i:
                        {
                            char c;
                            if (!reader.ReadNext(out c))
                            {
                                //------------------------------------
                                //eof
                                goto breakStateloop;
                            }
                            if (c == '\u0000')
                            {
                                goto breakStateloop;
                            }
                            /*
                             * Unlike the definition is the spec, this state does not
                             * return a value and never requires the caller to
                             * backtrack. This state takes care of emitting characters
                             * or appending to the current attribute value. It also
                             * takes care of that in the case TokenizerState.when consuming the
                             * character reference fails.
                             */
                            /*
                             * This section defines how to consume a character
                             * reference. This definition is used when parsing character
                             * references in text and in attributes.
                             * 
                             * The behavior depends on the identity of the next
                             * character (the one immediately after the U+0026 AMPERSAND
                             * character):
                             */
                            switch (c)
                            {
                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r': // we'll reconsume!
                                case '\u000C':
                                case '<':
                                case '&':
                                    EmitOrAppendStrBuf(returnState);
                                    //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                    if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                    {
                                        reader.StartCollect();
                                    }
                                    //state = Transition(state, returnState, reconsume, pos);
                                    state = returnState;
                                    //reconsume = true;
                                    reader.StepBack();
                                    goto continueStateloop;
                                case '#':
                                    /*
                                     * U+0023 NUMBER SIGN (#) Consume the U+0023 NUMBER
                                     * SIGN.
                                     */
                                    AppendStrBuf('#');
                                    //state = Transition(state, Tokenizer.CONSUME_NCR, reconsume, pos);
                                    //state = TokenizerState.CONSUME_NCR_i;
                                    SetInterLexerState(TokenizerState.CONSUME_NCR_i);
                                    goto continueStateloop;
                                default:
                                    if (c == additional)
                                    {
                                        EmitOrAppendStrBuf(returnState);
                                        //state = Transition(state, returnState, reconsume, pos);
                                        state = returnState;
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                    }
                                    if (c >= 'a' && c <= 'z')
                                    {
                                        firstCharKey = (char)(c - 'a' + 26);
                                    }
                                    else if (c >= 'A' && c <= 'Z')
                                    {
                                        firstCharKey = (char)(c - 'A');
                                    }
                                    else
                                    {
                                        // No match
                                        /*
                                         * If no match can be made, then this is a parse
                                         * error.
                                         */
                                        ErrNoNamedCharacterMatch();
                                        EmitOrAppendStrBuf(returnState);
                                        //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                        if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                        {
                                            reader.StartCollect();
                                        }
                                        //state = Transition(state, returnState, reconsume, pos);
                                        state = returnState;
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                    }
                                    // Didn't fail yet
                                    AppendStrBuf(c);

                                    //state = Transition(state, Tokenizer.CHARACTER_REFERENCE_HILO_LOOKUP, reconsume, pos);
                                    state = SubLexerTagState.CHARACTER_REFERENCE_HILO_LOOKUP_p;

                                    // FALL THROUGH goto continueStateloop;
                                    break;
                            }
                            //------------------------------------
                            goto case SubLexerTagState.CHARACTER_REFERENCE_HILO_LOOKUP_p;
                        }
                    // WARNING FALLTHRU case TokenizerState.TRANSITION: DON'T REORDER
                    case SubLexerTagState.CHARACTER_REFERENCE_HILO_LOOKUP_p:
                        {
                            char c;
                            if (reader.ReadNext(out c))
                            {
                                //------------------------------------
                                //eof
                                goto breakStateloop;
                            }

                            if (c == '\u0000')
                            {
                                goto breakStateloop;
                            }
                            /*
                             * The data structure is as follows:
                             * 
                             * HILO_ACCEL is a two-dimensional int array whose major
                             * index corresponds to the second character of the
                             * character reference (code point as index) and the
                             * minor index corresponds to the first character of the
                             * character reference (packed so that A-Z runs from 0
                             * to 25 and a-z runs from 26 to 51). This layout makes
                             * it easier to use the sparseness of the data structure
                             * to omit parts of it: The second dimension of the
                             * table is null when no character reference starts with
                             * the character corresponding to that row.
                             * 
                             * The int value HILO_ACCEL (by these indeces) is zero
                             * if there exists no character reference starting with
                             * that two-letter prefix. Otherwise, the value is an
                             * int that packs two shorts so that the higher short is
                             * the index of the highest character reference name
                             * with that prefix in NAMES and the lower short
                             * corresponds to the index of the lowest character
                             * reference name with that prefix. (It happens that the
                             * first two character reference names share their
                             * prefix so the packed int cannot be 0 by packing the
                             * two shorts.)
                             * 
                             * NAMES is an array of byte arrays where each byte
                             * array encodes the name of a character references as
                             * ASCII. The names omit the first two letters of the
                             * name. (Since storing the first two letters would be
                             * redundant with the data contained in HILO_ACCEL.) The
                             * entries are lexically sorted.
                             * 
                             * For a given index in NAMES, the same index in VALUES
                             * contains the corresponding expansion as an array of
                             * two UTF-16 code units (either the character and
                             * U+0000 or a suggogate pair).
                             */
                            int hilo = 0;
                            if (c <= 'z')
                            {
                                int[] row = NamedCharactersAccel.HILO_ACCEL[c];
                                if (row != null)
                                {
                                    hilo = row[firstCharKey];
                                }
                            }
                            if (hilo == 0)
                            {
                                /*
                                 * If no match can be made, then this is a parse
                                 * error.
                                 */
                                ErrNoNamedCharacterMatch();
                                EmitOrAppendStrBuf(returnState);
                                //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                {
                                    reader.StartCollect();
                                }
                                //state = Transition(state, returnState, reconsume, pos);
                                state = returnState;
                                //reconsume = true;
                                reader.StepBack();
                                goto continueStateloop;
                            }
                            // Didn't fail yet
                            AppendStrBuf(c);
                            lo = hilo & 0xFFFF;
                            hi = hilo >> 16;
                            entCol = -1;
                            candidate = -1;
                            strBufMark = 0;
                            //state = Transition(state, Tokenizer.CHARACTER_REFERENCE_TAIL, reconsume, pos);
                            state = SubLexerTagState.CHARACTER_REFERENCE_TAIL_p;
                            // FALL THROUGH goto continueStateloop;
                            goto case SubLexerTagState.CHARACTER_REFERENCE_TAIL_p;
                        }
                    case SubLexerTagState.CHARACTER_REFERENCE_TAIL_p:
                        /*outer:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                if (c == '\u0000')
                                {
                                    goto breakStateloop;
                                }
                                entCol++;
                                /*
                                 * Consume the maximum number of characters possible,
                                 * with the consumed characters matching one of the
                                 * identifiers in the first column of the named
                                 * character references table (in a case-sensitive
                                 * manner).
                                 */
                                /*loloop:*/
                                for (; ; )
                                {
                                    if (hi < lo)
                                    {
                                        goto breakOuter;
                                    }
                                    if (entCol == NamedCharacters.NAMES[lo].Length)
                                    {
                                        candidate = lo;
                                        strBufMark = this.strBuffer.Length;
                                        lo++;
                                    }
                                    else if (entCol > NamedCharacters.NAMES[lo].Length)
                                    {
                                        goto breakOuter;
                                    }
                                    else if (c > NamedCharacters.NAMES[lo][entCol])
                                    {
                                        lo++;
                                    }
                                    else
                                    {
                                        goto breakLoloop;
                                    }
                                }
                            breakLoloop:

                                /*hiloop:*/
                                for (; ; )
                                {
                                    if (hi < lo)
                                    {
                                        goto breakOuter;
                                    }
                                    if (entCol == NamedCharacters.NAMES[hi].Length)
                                    {
                                        goto breakHiloop;
                                    }
                                    if (entCol > NamedCharacters.NAMES[hi].Length)
                                    {
                                        goto breakOuter;
                                    }
                                    else if (c < NamedCharacters.NAMES[hi][entCol])
                                    {
                                        hi--;
                                    }
                                    else
                                    {
                                        goto breakHiloop;
                                    }
                                }

                            breakHiloop:

                                if (hi < lo)
                                {
                                    goto breakOuter;
                                }
                                AppendStrBuf(c);
                                continue;
                            }

                        breakOuter:

                            if (candidate == -1)
                            {
                                // reconsume deals with CR, LF or nul
                                /*
                                 * If no match can be made, then this is a parse error.
                                 */
                                ErrNoNamedCharacterMatch();
                                EmitOrAppendStrBuf(returnState);
                                //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                {
                                    reader.StartCollect();
                                }
                                //state = Transition(state, returnState, reconsume, pos);
                                state = returnState;
                                //reconsume = true;
                                reader.StepBack();
                                goto continueStateloop;
                            }
                            else
                            {
                                // c can't be CR, LF or nul if we got here
                                string candidateName = NamedCharacters.NAMES[candidate];
                                if (candidateName.Length == 0
                                        || candidateName[candidateName.Length - 1] != ';')
                                {
                                    /*
                                     * If the last character matched is not a U+003B
                                     * SEMICOLON (;), there is a parse error.
                                     */
                                    //if ((returnState & DATA_AND_RCDATA_MASK) != 0)
                                    if (((byte)returnState & DATA_AND_RCDATA_MASK) == 0)
                                    {
                                        /*
                                         * If the entity is being consumed as part of an
                                         * attribute, and the last character matched is
                                         * not a U+003B SEMICOLON (;),
                                         */
                                        char ch;
                                        if (strBufMark == this.strBuffer.Length)
                                        {
                                            ch = c;
                                        }
                                        else
                                        {
                                            // if (strBufOffset != -1) {
                                            // ch = buf[strBufOffset + strBufMark];
                                            // } else {
                                            ch = this.strBuffer[strBufMark];
                                            // }
                                        }
                                        if (ch == '=' || (ch >= '0' && ch <= '9')
                                                || (ch >= 'A' && ch <= 'Z')
                                                || (ch >= 'a' && ch <= 'z'))
                                        {
                                            /*
                                             * and the next character is either a U+003D
                                             * EQUALS SIGN character (=) or in the range
                                             * U+0030 DIGIT ZERO to U+0039 DIGIT NINE,
                                             * U+0041 LATIN CAPITAL LETTER A to U+005A
                                             * LATIN CAPITAL LETTER Z, or U+0061 LATIN
                                             * SMALL LETTER A to U+007A LATIN SMALL
                                             * LETTER Z, then, for historical reasons,
                                             * all the characters that were matched
                                             * after the U+0026 AMPERSAND (&) must be
                                             * unconsumed, and nothing is returned.
                                             */
                                            ErrNoNamedCharacterMatch();
                                            AppendStrBufToLongStrBuf();
                                            //state = Transition(state, returnState, reconsume, pos);
                                            state = returnState;
                                            //reconsume = true;
                                            reader.StepBack();
                                            goto continueStateloop;
                                        }
                                    }
                                    //if ((returnState & DATA_AND_RCDATA_MASK) != 0)
                                    if (((byte)returnState & DATA_AND_RCDATA_MASK) == 0)
                                    {
                                        ErrUnescapedAmpersandInterpretedAsCharacterReference();
                                    }
                                    else
                                    {
                                        ErrNotSemicolonTerminated();
                                    }
                                }

                                /*
                                 * Otherwise, return a character token for the character
                                 * corresponding to the entity name (as given by the
                                 * second column of the named character references
                                 * table).
                                 */

                                char[] val = NamedCharacters.VALUES[candidate];
                                if (val.Length == 1)
                                {
                                    EmitOrAppendOne(val, returnState);
                                }
                                else
                                {
                                    EmitOrAppendTwo(val, returnState);
                                }
                                // this is so complicated!
                                if (strBufMark < this.strBuffer.Length)
                                {
                                    // if (strBufOffset != -1) {
                                    // if ((returnState & (~1)) != 0) {
                                    // for (int i = strBufMark; i < strBufLen; i++) {
                                    // appendLongStrBuf(buf[strBufOffset + i]);
                                    // }
                                    // } else {
                                    // tokenHandler.Characters(buf, strBufOffset
                                    // + strBufMark, strBufLen
                                    // - strBufMark);
                                    // }
                                    // } else {
                                    //if ((returnState & DATA_AND_RCDATA_MASK) != 0)
                                    if (((byte)returnState & DATA_AND_RCDATA_MASK) == 0)
                                    {
                                        int j = this.strBuffer.Length;
                                        for (int i = strBufMark; i < j; i++)
                                        {
                                            AppendLongStrBuf(strBuffer[i]);
                                        }
                                    }
                                    else
                                    {
                                        TokenListener.Characters(CopyFromStringBuiler(this.strBuffer, strBufMark, this.strBuffer.Length - strBufMark));
                                    }
                                    // }
                                }
                                //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                {
                                    reader.StartCollect();
                                }
                                //state = Transition(state, returnState, reconsume, pos);
                                state = returnState;
                                //reconsume = true;
                                reader.StepBack();
                                goto continueStateloop;
                                /*
                                 * If the markup contains I'm &notit; I tell you, the
                                 * entity is parsed as "not", as in, I'm Â¬it; I tell
                                 * you. But if the markup was I'm &notin; I tell you,
                                 * the entity would be parsed as "notin;", resulting in
                                 * I'm âˆ‰ I tell you.
                                 */
                            }

                        }
                    // XXX reorder point
                    case (SubLexerTagState)RawTextCDataRcRefState.s05_RAWTEXT_p:
                        /*rawtextloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '<':
                                        /*
                                         * U+003C LESS-THAN SIGN (<) Switch to the
                                         * RAWTEXT less-than sign state.
                                         */
                                        FlushChars();

                                        returnState = state;
                                        //state = Transition(state, Tokenizer.RAWTEXT_RCDATA_LESS_THAN_SIGN, reconsume, pos);
                                        state = (SubLexerTagState)RawTextCDataRcRefState.s11_RAWTEXT_RCDATA_LESS_THAN_SIGN_p;
                                        goto breakRawtextloop;
                                    // FALL THRU goto continueStateloop;
                                    case '\u0000':
                                        EmitReplacementCharacter();
                                        continue;
                                    case '\r':
                                        EmitCarriageReturn();
                                        goto breakStateloop;
                                    case '\n':
                                    default:
                                        /*
                                         * Emit the current input character as a
                                         * character token. Stay in the RAWTEXT state.
                                         */
                                        continue;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakRawtextloop:
                            goto case (SubLexerTagState)RawTextCDataRcRefState.s11_RAWTEXT_RCDATA_LESS_THAN_SIGN_p;
                        }
                    // XXX fallthru don't reorder
                    case (SubLexerTagState)RawTextCDataRcRefState.s11_RAWTEXT_RCDATA_LESS_THAN_SIGN_p:
                        /*rawtextrcdatalessthansignloop:*/
                        {

                            char c;
                            while (reader.ReadNext(out c))
                            {
                                switch (c)
                                {
                                    case '/':
                                        /*
                                         * U+002F SOLIDUS (/) Set the temporary buffer
                                         * to the empty string. Switch to the script
                                         * data end tag open state.
                                         */
                                        index = 0;
                                        ClearStrBuf();
                                        //state = Transition(state, Tokenizer.NON_DATA_END_TAG_NAME, reconsume, pos);
                                        //state = TokenizerState.NON_DATA_END_TAG_NAME_i;
                                        SetInterLexerState(TokenizerState.NON_DATA_END_TAG_NAME_i);
                                        goto breakRawtextrcdatalessthansignloop;
                                    // FALL THRU goto continueStateloop;
                                    default:
                                        /*
                                         * Otherwise, emit a U+003C LESS-THAN SIGN
                                         * character token
                                         */
                                        TokenListener.Characters(LT_GT, 0, 1);
                                        /*
                                         * and reconsume the current input character in
                                         * the data state.
                                         */
                                        reader.StartCollect();
                                        //state = Transition(state, returnState, reconsume, pos);
                                        state = returnState;
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakRawtextrcdatalessthansignloop:
                            goto case (RawTextCDataRcRefState)TokenizerState.NON_DATA_END_TAG_NAME_i;
                        }
                    // XXX fall thru. don't reorder.
                    case (SubLexerTagState)TokenizerState.NON_DATA_END_TAG_NAME_i:
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                /*
                                 * ASSERT! when entering this state, set index to 0 and
                                 * call clearStrBuf() assert (contentModelElement !=
                                 * null); Let's implement the above without lookahead.
                                 * strBuf is the 'temporary buffer'.
                                 */
                                if (index < endTagExpectationAsArray.Length)
                                {
                                    char e = endTagExpectationAsArray[index];
                                    char folded = c;
                                    if (c >= 'A' && c <= 'Z')
                                    {
                                        folded += (char)0x20;
                                    }
                                    if (folded != e)
                                    {

                                        ErrHtml4LtSlashInRcdata(folded);
                                        TokenListener.Characters(LT_SOLIDUS,
                                                0, 2);
                                        EmitStrBuf();
                                        reader.StartCollect();
                                        //state = Transition(state, returnState, reconsume, pos);
                                        state = returnState;
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                    }
                                    AppendStrBuf(c);
                                    index++;
                                    continue;
                                }
                                else
                                {
                                    endTag = true;
                                    // XXX replace contentModelElement with different
                                    // type
                                    tagName = endTagExpectation;
                                    switch (c)
                                    {
                                        case '\r':
                                            SilentCarriageReturn();
                                            //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                                            //state = TokenizerState.s34_BEFORE_ATTRIBUTE_NAME_i;
                                            state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                            goto breakStateloop;
                                        case '\n':
                                        case ' ':
                                        case '\t':
                                        case '\u000C':
                                            /*
                                             * U+0009 CHARACTER TABULATION U+000A LINE
                                             * FEED (LF) U+000C FORM FEED (FF) U+0020
                                             * SPACE If the current end tag token is an
                                             * appropriate end tag token, then switch to
                                             * the before attribute name state.
                                             */
                                            //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                                            //state = TokenizerState.s34_BEFORE_ATTRIBUTE_NAME_i;
                                            state = SubLexerTagState.s34_BEFORE_ATTRIBUTE_NAME_p;
                                            goto continueStateloop;
                                        case '/':
                                            /*
                                             * U+002F SOLIDUS (/) If the current end tag
                                             * token is an appropriate end tag token,
                                             * then switch to the self-closing start tag
                                             * state.
                                             */
                                            //state = Transition(state, Tokenizer.SELF_CLOSING_START_TAG, reconsume, pos);
                                            //state = TokenizerState.s43_SELF_CLOSING_START_TAG_i;
                                            SetInterLexerState(TokenizerState.s43_SELF_CLOSING_START_TAG_i);
                                            goto continueStateloop;
                                        case '>':
                                            /*
                                             * U+003E GREATER-THAN SIGN (>) If the
                                             * current end tag token is an appropriate
                                             * end tag token, then emit the current tag
                                             * token and switch to the data state.
                                             */
                                            //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                                            state = EmitCurrentTagToken2(false);
                                            if (shouldSuspend)
                                            {
                                                goto breakStateloop;
                                            }
                                            goto continueStateloop;
                                        default:
                                            /*
                                             * Emit a U+003C LESS-THAN SIGN character
                                             * token, a U+002F SOLIDUS character token,
                                             * a character token for each of the
                                             * characters in the temporary buffer (in
                                             * the order they were added to the buffer),
                                             * and reconsume the current input character
                                             * in the RAWTEXT state.
                                             */
                                            // [NOCPP[
                                            ErrWarnLtSlashInRcdata();
                                            // ]NOCPP]
                                            TokenListener.Characters(LT_SOLIDUS, 0, 2);
                                            EmitStrBuf();
                                            if (c == '\u0000')
                                            {
                                                EmitReplacementCharacter();
                                            }
                                            else
                                            {
                                                reader.StartCollect(); // don't drop the
                                                // character
                                            }
                                            //state = Transition(state, returnState, reconsume, pos);
                                            state = returnState;
                                            goto continueStateloop;
                                    }
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        }
                    // XXX reorder point
                    case SubLexerTagState.s09_CLOSE_TAG_OPEN_p:
                        {
                            char c;
                            if (!reader.ReadNext(out c))
                            {
                                //------------------------------------
                                //eof
                                goto breakStateloop;
                            }

                            /*
                             * Otherwise, if the content model flag is set to the PCDATA
                             * state, or if the next few characters do match that tag
                             * name, consume the next input character:
                             */
                            switch (c)
                            {
                                case '>':
                                    /* U+003E GREATER-THAN SIGN (>) Parse error. */
                                    ErrLtSlashGt();
                                    /*
                                     * Switch to the data state.
                                     */
                                    reader.SkipOneAndStartCollect();
                                    //state = Transition(state, Tokenizer.DATA, reconsume, pos);
                                    //state = TokenizerState.s01_DATA_i;
                                    SetInterLexerState(TokenizerState.s01_DATA_i);
                                    goto continueStateloop;
                                case '\r':
                                    SilentCarriageReturn();
                                    /* Anything else Parse error. */
                                    ErrGarbageAfterLtSlash();
                                    /*
                                     * Switch to the bogus comment state.
                                     */
                                    ClearLongStrBufAndAppend('\n');
                                    //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);
                                    //state = TokenizerState.s44_BOGUS_COMMENT_i;
                                    SetInterLexerState(TokenizerState.s44_BOGUS_COMMENT_i);
                                    goto breakStateloop;
                                case '\n':
                                    /* Anything else Parse error. */
                                    ErrGarbageAfterLtSlash();
                                    /*
                                     * Switch to the bogus comment state.
                                     */
                                    ClearLongStrBufAndAppend('\n');
                                    //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);
                                    //state = TokenizerState.s44_BOGUS_COMMENT_i;
                                    SetInterLexerState(TokenizerState.s44_BOGUS_COMMENT_i);
                                    goto continueStateloop;
                                case '\u0000':
                                    c = '\uFFFD';
                                    // fall thru
                                    goto default;
                                default:
                                    if (c >= 'A' && c <= 'Z')
                                    {
                                        c += (char)0x20;
                                    }
                                    if (c >= 'a' && c <= 'z')
                                    {
                                        /*
                                         * U+0061 LATIN SMALL LETTER A through to U+007A
                                         * LATIN SMALL LETTER Z Create a new end tag
                                         * token,
                                         */
                                        endTag = true;
                                        /*
                                         * set its tag name to the input character,
                                         */
                                        ClearStrBufAndAppend(c);
                                        /*
                                         * then switch to the tag name state. (Don't
                                         * emit the token yet; further details will be
                                         * filled in before it is emitted.)
                                         */
                                        //state = Transition(state, Tokenizer.TAG_NAME, reconsume, pos);
                                        state = SubLexerTagState.s10_TAG_NAME_p;
                                        goto continueStateloop;
                                    }
                                    else
                                    {
                                        /* Anything else Parse error. */
                                        ErrGarbageAfterLtSlash();
                                        /*
                                         * Switch to the bogus comment state.
                                         */
                                        ClearLongStrBufAndAppend(c);
                                        //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);
                                        //state = TokenizerState.s44_BOGUS_COMMENT_i;
                                        SetInterLexerState(TokenizerState.s44_BOGUS_COMMENT_i);
                                        goto continueStateloop;
                                    }
                            }
                        }
                    // END HOTSPOT WORKAROUND
                }
            } // stateloop

         breakStateloop:
            //FlushChars(buf, pos);
            FlushChars();
            /*
             * if (prevCR && pos != endPos) { // why is this needed? pos--; col--; }
             */
            // Save locals
            //stateSave = state;
            //returnStateSave = returnState;
            SaveStates(state, returnState);
        }

        void SaveStates(SubLexerTagState state, SubLexerTagState returnState)
        {

        }
    }
}