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

#pragma warning disable 1591 // Missing XML comment
#pragma warning disable 1570 // XML comment on 'construct' has badly formed XML — 'reason'
#pragma warning disable 1587 // XML comment is not placed on a valid element

namespace HtmlParserSharp.Core
{
    public enum CommentLexerState
    {   
        s46_COMMENT_START_p = 32, //comment

        s47_COMMENT_START_DASH_p = 33, //comment

        s48_COMMENT_p = 34, //comment

        s49_COMMENT_END_DASH_p = 35, //comment

        s50_COMMENT_END_p = 36, //comment

        s51_COMMENT_END_BANG_p = 37, //comment

        MARKUP_DECLARATION_HYPHEN_p = 39, //comment

        BOGUS_COMMENT_HYPHEN_p = 64,//comment
    }
    class SubLexerComment : SubLexer
    {
        int index = 0;
        XmlViolationPolicy commentPolicy = XmlViolationPolicy.AlterInfoset;
        /*@Inline*/
        void AppendSecondHyphenToBogusComment()
        {
            // [NOCPP[
            switch (commentPolicy)
            {
                case XmlViolationPolicy.AlterInfoset:
                    // detachLongStrBuf();
                    AppendLongStrBuf(' ');
                    // FALLTHROUGH
                    goto case XmlViolationPolicy.Allow;
                case XmlViolationPolicy.Allow:
                    Warn("The document is not mappable to XML 1.0 due to two consecutive hyphens in a comment.");
                    // ]NOCPP]
                    AppendLongStrBuf('-');
                    // [NOCPP[
                    break;
                case XmlViolationPolicy.Fatal:
                    Fatal("The document is not mappable to XML 1.0 due to two consecutive hyphens in a comment.");
                    break;
            }
            // ]NOCPP]
        }

        // [NOCPP[
        void MaybeAppendSpaceToBogusComment()
        {
            switch (commentPolicy)
            {
                case XmlViolationPolicy.AlterInfoset:
                    // detachLongStrBuf();
                    AppendLongStrBuf(' ');
                    // FALLTHROUGH
                    goto case XmlViolationPolicy.Allow;
                case XmlViolationPolicy.Allow:
                    Warn("The document is not mappable to XML 1.0 due to a trailing hyphen in a comment.");
                    break;
                case XmlViolationPolicy.Fatal:
                    Fatal("The document is not mappable to XML 1.0 due to a trailing hyphen in a comment.");
                    break;
            }
        }
        void EmitComment(int provisionalHyphens)
        {
            throw new NotImplementedException();
        }
        /*@Inline*/
        void AdjustDoubleHyphenAndAppendToLongStrBufCarriageReturn()
        {
            SilentCarriageReturn();
            AdjustDoubleHyphenAndAppendToLongStrBufAndErr('\n');
        }
        /*@Inline*/
        void AdjustDoubleHyphenAndAppendToLongStrBufLineFeed()
        {
            //SilentLineFeed();
            AdjustDoubleHyphenAndAppendToLongStrBufAndErr('\n');
        }
        /*@Inline*/
        void AdjustDoubleHyphenAndAppendToLongStrBufAndErr(char c)
        {
            ErrConsecutiveHyphens();
            // [NOCPP[
            switch (commentPolicy)
            {
                case XmlViolationPolicy.AlterInfoset:
                    // detachLongStrBuf();
                    longStrBuffer.Length--;
                    AppendLongStrBuf(' ');
                    AppendLongStrBuf('-');

                    // FALLTHROUGH
                    goto case XmlViolationPolicy.Allow;
                case XmlViolationPolicy.Allow:
                    Warn("The document is not mappable to XML 1.0 due to two consecutive hyphens in a comment.");
                    // ]NOCPP]
                    AppendLongStrBuf(c);
                    // [NOCPP[
                    break;
                case XmlViolationPolicy.Fatal:
                    Fatal("The document is not mappable to XML 1.0 due to two consecutive hyphens in a comment.");
                    break;
            }
            // ]NOCPP]
        }
        void StateLoop3_Comment(CommentLexerState state, CommentLexerState returnState)
        {

            for (; ; )
            {
            //*************
            continueStateloop:
                //************* 
                switch (state)
                {
                    case (CommentLexerState)InterLexerState.s45_MARKUP_DECLARATION_OPEN_i:
                        /*markupdeclarationopenloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                /*
                                 * If the next two characters are both U+002D
                                 * HYPHEN-MINUS characters (-), consume those two
                                 * characters, create a comment token whose data is the
                                 * empty string, and switch to the comment start state.
                                 * 
                                 * Otherwise, if the next seven characters are an ASCII
                                 * case-insensitive match for the word "DOCTYPE", then
                                 * consume those characters and switch to the DOCTYPE
                                 * state.
                                 * 
                                 * Otherwise, if the insertion mode is
                                 * "in foreign content" and the current node is not an
                                 * element in the HTML namespace and the next seven
                                 * characters are an case-sensitive match for the string
                                 * "[CDATA[" (the five uppercase TokenizerState.letters "CDATA" with a
                                 * U+005B LEFT SQUARE BRACKET character before and
                                 * after), then consume those characters and switch to
                                 * the CDATA section state.
                                 * 
                                 * Otherwise, is is a parse error. Switch to the bogus
                                 * comment state. The next character that is consumed,
                                 * if any, is the first character that will be in the
                                 * comment.
                                 */
                                switch (c)
                                {
                                    case '-':
                                        ClearLongStrBufAndAppend(c);
                                        //state = Transition(state, Tokenizer.MARKUP_DECLARATION_HYPHEN, reconsume, pos);
                                        state = CommentLexerState.MARKUP_DECLARATION_HYPHEN_p;
                                        goto breakMarkupdeclarationopenloop;
                                    // goto continueStateloop;
                                    case 'd':
                                    case 'D':
                                        ClearLongStrBufAndAppend(c);
                                        index = 0;
                                        //state = Transition(state, Tokenizer.MARKUP_DECLARATION_OCTYPE, reconsume, pos);

                                        //state = TokenizerState.MARKUP_DECLARATION_OCTYPE_i;

                                        SetInterLexerState(InterLexerState.MARKUP_DECLARATION_OCTYPE_i);

                                        goto continueStateloop;
                                    case '[':
                                        if (TokenListener.IsCDataSectionAllowed)
                                        {
                                            ClearLongStrBufAndAppend(c);
                                            index = 0;
                                            //state = Transition(state, Tokenizer.CDATA_START, reconsume, pos);
                                            //state = TokenizerState.CDATA_START_i;
                                            SetInterLexerState(InterLexerState.CDATA_START_i);
                                            goto continueStateloop;
                                        }
                                        else
                                        {
                                            // else fall through
                                            goto default;
                                        }
                                    default:
                                        ErrBogusComment();
                                        ClearLongStrBuf();
                                        //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);

                                        SetInterLexerState(InterLexerState.s44_BOGUS_COMMENT_i);
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakMarkupdeclarationopenloop:
                            goto case CommentLexerState.MARKUP_DECLARATION_HYPHEN_p;
                        }
                    // FALLTHRU DON'T REORDER
                    case CommentLexerState.MARKUP_DECLARATION_HYPHEN_p:
                        /*markupdeclarationhyphenloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                switch (c)
                                {
                                    case '\u0000':
                                        goto breakStateloop;
                                    case '-':
                                        ClearLongStrBuf();
                                        //state = Transition(state, Tokenizer.COMMENT_START, reconsume, pos);
                                        state = CommentLexerState.s46_COMMENT_START_p;
                                        goto breakMarkupdeclarationhyphenloop;
                                    // goto continueStateloop;
                                    default:
                                        ErrBogusComment();
                                        //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);
                                        SetInterLexerState(InterLexerState.s44_BOGUS_COMMENT_i);
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------

                        breakMarkupdeclarationhyphenloop:
                            goto case CommentLexerState.s46_COMMENT_START_p;
                        }
                    // FALLTHRU DON'T REORDER
                    case CommentLexerState.s46_COMMENT_START_p:
                        /*commentstartloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '-':
                                        /*
                                         * U+002D HYPHEN-MINUS (-) Switch to the comment
                                         * start dash state.
                                         */
                                        AppendLongStrBuf(c);
                                        //state = Transition(state, Tokenizer.COMMENT_START_DASH, reconsume, pos);
                                        state = CommentLexerState.s47_COMMENT_START_DASH_p;
                                        goto continueStateloop;
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Parse error.
                                         */
                                        ErrPrematureEndOfComment();
                                        /* Emit the comment token. */
                                        EmitComment(0);
                                        /*
                                         * Switch to the data state.
                                         */
                                        //state = Transition(state, Tokenizer.DATA, reconsume, pos);

                                        SetInterLexerState(InterLexerState.s01_DATA_i);
                                        goto continueStateloop;
                                    case '\r':
                                        AppendLongStrBufCarriageReturn();
                                        // state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                        state = CommentLexerState.s48_COMMENT_p;
                                        goto breakStateloop;
                                    case '\n':
                                        AppendLongStrBufLineFeed();
                                        //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                        state = CommentLexerState.s48_COMMENT_p;

                                        goto breakCommentstartloop;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        // fall thru
                                        goto default;
                                    default:
                                        /*
                                         * Anything else Append the input character to
                                         * the comment token's data.
                                         */
                                        AppendLongStrBuf(c);
                                        /*
                                         * Switch to the comment state.
                                         */
                                        //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                        state = CommentLexerState.s48_COMMENT_p;

                                        goto breakCommentstartloop;
                                    // goto continueStateloop;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakCommentstartloop:
                            goto case CommentLexerState.s48_COMMENT_p;
                        }
                    // FALLTHRU DON'T REORDER
                    case CommentLexerState.s48_COMMENT_p:
                        /*commentloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '-':
                                        /*
                                         * U+002D HYPHEN-MINUS (-) Switch to the comment
                                         * end dash state
                                         */
                                        AppendLongStrBuf(c);
                                        //state = Transition(state, Tokenizer.COMMENT_END_DASH, reconsume, pos);
                                        state = CommentLexerState.s49_COMMENT_END_DASH_p;
                                        goto breakCommentloop;
                                    // goto continueStateloop;
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
                                         * Anything else Append the input character to
                                         * the comment token's data.
                                         */
                                        AppendLongStrBuf(c);
                                        /*
                                         * Stay in the comment state.
                                         */
                                        continue;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakCommentloop:
                            goto case CommentLexerState.s49_COMMENT_END_DASH_p;
                        }
                    // FALLTHRU DON'T REORDER
                    case CommentLexerState.s49_COMMENT_END_DASH_p:
                        /*commentenddashloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '-':
                                        /*
                                         * U+002D HYPHEN-MINUS (-) Switch to the comment
                                         * end state
                                         */
                                        AppendLongStrBuf(c);
                                        //state = Transition(state, Tokenizer.COMMENT_END, reconsume, pos);
                                        state = CommentLexerState.s50_COMMENT_END_p;
                                        goto breakCommentenddashloop;
                                    // goto continueStateloop;
                                    case '\r':
                                        AppendLongStrBufCarriageReturn();
                                        //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                        state = CommentLexerState.s48_COMMENT_p;
                                        goto breakStateloop;
                                    case '\n':
                                        AppendLongStrBufLineFeed();
                                        //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                        state = CommentLexerState.s48_COMMENT_p;
                                        goto continueStateloop;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        goto default;
                                    // fall thru
                                    default:
                                        /*
                                         * Anything else Append a U+002D HYPHEN-MINUS
                                         * (-) character and the input character to the
                                         * comment token's data.
                                         */
                                        AppendLongStrBuf(c);
                                        /*
                                         * Switch to the comment state.
                                         */
                                        //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                        state = CommentLexerState.s48_COMMENT_p;
                                        goto continueStateloop;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakCommentenddashloop:
                            goto case CommentLexerState.s50_COMMENT_END_p;
                        }
                    // FALLTHRU DON'T REORDER
                    case CommentLexerState.s50_COMMENT_END_p:
                        /*commentendloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Emit the comment
                                         * token.
                                         */
                                        EmitComment(2);
                                        /*
                                         * Switch to the data state.
                                         */
                                        //state = Transition(state, Tokenizer.DATA, reconsume, pos);
                                        //state = TokenizerState.s01_DATA_i;
                                        SetInterLexerState(InterLexerState.s01_DATA_i);
                                        goto continueStateloop;
                                    case '-':
                                        /* U+002D HYPHEN-MINUS (-) Parse error. */
                                        /*
                                         * Append a U+002D HYPHEN-MINUS (-) character to
                                         * the comment token's data.
                                         */
                                        AdjustDoubleHyphenAndAppendToLongStrBufAndErr(c);
                                        /*
                                         * Stay in the comment end state.
                                         */
                                        continue;
                                    case '\r':
                                        AdjustDoubleHyphenAndAppendToLongStrBufCarriageReturn();
                                        //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                        state = CommentLexerState.s48_COMMENT_p;
                                        goto breakStateloop;
                                    case '\n':
                                        AdjustDoubleHyphenAndAppendToLongStrBufLineFeed();
                                        //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                        state = CommentLexerState.s48_COMMENT_p;
                                        goto continueStateloop;
                                    case '!':
                                        ErrHyphenHyphenBang();
                                        AppendLongStrBuf(c);
                                        //state = Transition(state, Tokenizer.COMMENT_END_BANG, reconsume, pos);
                                        state = CommentLexerState.s51_COMMENT_END_BANG_p;
                                        goto continueStateloop;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        // fall thru
                                        goto default;
                                    default:
                                        /*
                                         * Append two U+002D HYPHEN-MINUS (-) characters
                                         * and the input character to the comment
                                         * token's data.
                                         */
                                        AdjustDoubleHyphenAndAppendToLongStrBufAndErr(c);
                                        /*
                                         * Switch to the comment state.
                                         */
                                        //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                        state = CommentLexerState.s48_COMMENT_p;
                                        goto continueStateloop;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                        }
                    // XXX reorder point
                    case CommentLexerState.s51_COMMENT_END_BANG_p:
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                switch (c)
                                {
                                    case '>':
                                        /*
                                         * U+003E GREATER-THAN SIGN (>) Emit the comment
                                         * token.
                                         */
                                        EmitComment(3);
                                        /*
                                         * Switch to the data state.
                                         */
                                        //state = Transition(state, Tokenizer.DATA, reconsume, pos);
                                        //state = TokenizerState.s01_DATA_i;
                                        SetInterLexerState(InterLexerState.s01_DATA_i);
                                        goto continueStateloop;
                                    case '-':
                                        /*
                                         * Append two U+002D HYPHEN-MINUS (-) characters
                                         * and a U+0021 EXCLAMATION MARK (!) character
                                         * to the comment token's data.
                                         */
                                        AppendLongStrBuf(c);
                                        /*
                                         * Switch to the comment end dash state.
                                         */
                                        //state = Transition(state, Tokenizer.COMMENT_END_DASH, reconsume, pos);
                                        state = CommentLexerState.s49_COMMENT_END_DASH_p;
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
                                         * Anything else Append two U+002D HYPHEN-MINUS
                                         * (-) characters, a U+0021 EXCLAMATION MARK (!)
                                         * character, and the input character to the
                                         * comment token's data. Switch to the comment
                                         * state.
                                         */
                                        AppendLongStrBuf(c);
                                        /*
                                         * Switch to the comment state.
                                         */
                                        //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                        state = CommentLexerState.s48_COMMENT_p;
                                        goto continueStateloop;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                        }
                    // XXX reorder point
                    case CommentLexerState.s47_COMMENT_START_DASH_p:
                        {
                            char c;
                            if (!reader.ReadNext(out c))
                            {
                                //-------------------------------
                                //eof
                                goto breakStateloop;
                            }
                            //----------------------

                            switch (c)
                            {
                                case '-':
                                    /*
                                     * U+002D HYPHEN-MINUS (-) Switch to the comment end
                                     * state
                                     */
                                    AppendLongStrBuf(c);
                                    //state = Transition(state, Tokenizer.COMMENT_END, reconsume, pos);
                                    state = CommentLexerState.s50_COMMENT_END_p;
                                    goto continueStateloop;
                                case '>':
                                    ErrPrematureEndOfComment();
                                    /* Emit the comment token. */
                                    EmitComment(1);
                                    /*
                                     * Switch to the data state.
                                     */
                                    //state = Transition(state, Tokenizer.DATA, reconsume, pos);

                                    SetInterLexerState(InterLexerState.s01_DATA_i);
                                    goto continueStateloop;
                                case '\r':
                                    AppendLongStrBufCarriageReturn();
                                    //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                    state = CommentLexerState.s48_COMMENT_p;
                                    goto breakStateloop;
                                case '\n':
                                    AppendLongStrBufLineFeed();
                                    //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                    state = CommentLexerState.s48_COMMENT_p;
                                    goto continueStateloop;
                                case '\u0000':
                                    c = '\uFFFD';
                                    // fall thru
                                    goto default;
                                default:
                                    /*
                                     * Append a U+002D HYPHEN-MINUS character (-) and
                                     * the current input character to the comment
                                     * token's data.
                                     */
                                    AppendLongStrBuf(c);
                                    /*
                                     * Switch to the comment state.
                                     */
                                    //state = Transition(state, Tokenizer.COMMENT, reconsume, pos);
                                    state = CommentLexerState.s48_COMMENT_p;
                                    goto continueStateloop;
                            }
                        }
                    // XXX reorder point
                    // BEGIN HOTSPOT WORKAROUND
                    case (CommentLexerState)InterLexerState.s44_BOGUS_COMMENT_i:
                        /*boguscommentloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                /*
                                 * Consume every character up to and including the first
                                 * U+003E GREATER-THAN SIGN character (>) or the end of
                                 * the file (EOF), whichever comes first. Emit a comment
                                 * token whose data is the concatenation of all the
                                 * characters starting from and including the character
                                 * that caused the state machine to switch into the
                                 * bogus comment state, up to and including the
                                 * character immediately before the last consumed
                                 * character (i.e. up to the character just before the
                                 * U+003E or EOF character). (If the comment was started
                                 * by the end of the file (EOF), the token is empty.)
                                 * 
                                 * Switch to the data state.
                                 * 
                                 * If the end of the file was reached, reconsume the EOF
                                 * character.
                                 */
                                switch (c)
                                {
                                    case '>':
                                        EmitComment(0);
                                        //state = Transition(state, Tokenizer.DATA, reconsume, pos);

                                        SetInterLexerState(InterLexerState.s01_DATA_i);
                                        goto continueStateloop;
                                    case '-':
                                        AppendLongStrBuf(c);
                                        //state = Transition(state, Tokenizer.BOGUS_COMMENT_HYPHEN, reconsume, pos);
                                        state = CommentLexerState.BOGUS_COMMENT_HYPHEN_p;
                                        goto breakBoguscommentloop;
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
                                        AppendLongStrBuf(c);
                                        continue;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------
                        breakBoguscommentloop:
                            goto case CommentLexerState.BOGUS_COMMENT_HYPHEN_p;
                        }
                    // FALLTHRU DON'T REORDER
                    case CommentLexerState.BOGUS_COMMENT_HYPHEN_p:
                        /*boguscommenthyphenloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '>':
                                        // [NOCPP[
                                        MaybeAppendSpaceToBogusComment();
                                        // ]NOCPP]
                                        EmitComment(0);
                                        //state = Transition(state, Tokenizer.DATA, reconsume, pos);
                                        //state = TokenizerState.s01_DATA_i;
                                        SetInterLexerState(InterLexerState.s01_DATA_i);
                                        goto continueStateloop;
                                    case '-':
                                        AppendSecondHyphenToBogusComment();
                                        goto continueBoguscommenthyphenloop;
                                    case '\r':
                                        AppendLongStrBufCarriageReturn();
                                        //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);
                                        //state = TokenizerState.s44_BOGUS_COMMENT_i;
                                        SetInterLexerState(InterLexerState.s44_BOGUS_COMMENT_i);
                                        goto breakStateloop;
                                    case '\n':
                                        AppendLongStrBufLineFeed();
                                        //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);
                                        //state = TokenizerState.s44_BOGUS_COMMENT_i;
                                        SetInterLexerState(InterLexerState.s44_BOGUS_COMMENT_i);
                                        goto continueStateloop;
                                    case '\u0000':
                                        c = '\uFFFD';
                                        // fall thru
                                        goto default;
                                    default:
                                        AppendLongStrBuf(c);
                                        //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);
                                        //state = TokenizerState.s44_BOGUS_COMMENT_i;
                                        SetInterLexerState(InterLexerState.s44_BOGUS_COMMENT_i);
                                        goto continueStateloop;
                                }
                            //------------------------------------
                            continueBoguscommenthyphenloop:
                                continue;
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
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
        void SaveStates(CommentLexerState saveState, CommentLexerState returnStateSave)
        {

        }
    }

}