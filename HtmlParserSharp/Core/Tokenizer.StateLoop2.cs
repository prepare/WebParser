﻿/*
 * Copyright (c) 2005-2007 Henri Sivonen
 * Copyright (c) 2007-2010 Mozilla Foundation
 * Portions of comments Copyright 2004-2010 Apple Computer, Inc., Mozilla 
 * Foundation, and Opera Software ASA.
 * Copyright (c) 2012 Patrick Reisert
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
    partial class Tokenizer
    {
        int StateLoop2(TokenizerState state, char c,
           int pos, char[] buf, bool reconsume, TokenizerState returnState,
           int endPos)
        {

            TextSnapshotReader textSnapshot = new TextSnapshotReader(buf, pos, endPos - pos);
            do
            {
                //switch by state 
                switch (state)
                {
                    case TokenizerState.DATA:
                        {
                            //data state
                            MyParseDataState(textSnapshot, ref state);

                        } break;
                    case TokenizerState.TAG_OPEN:
                        {
                            //just after open tag <
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
                                state = TokenizerState.TAG_NAME;
                                /*
                                 * (Don't emit the token yet; further details will
                                 * be filled in before it is emitted.)
                                 */
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
                                state = TokenizerState.TAG_NAME;
                                /*
                                 * (Don't emit the token yet; further details will
                                 * be filled in before it is emitted.)
                                 */
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
                                    state = TokenizerState.MARKUP_DECLARATION_OPEN;
                                    break;
                                case '/':
                                    /*
                                     * U+002F SOLIDUS (/) Switch to the close tag
                                     * open state.
                                     */
                                    //state = Transition(state, Tokenizer.CLOSE_TAG_OPEN, reconsume, pos);
                                    state = TokenizerState.CLOSE_TAG_OPEN;
                                    break;
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
                                    state = TokenizerState.BOGUS_COMMENT;
                                    break;
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
                                    cstart = pos + 1;
                                    //state = Transition(state, Tokenizer.DATA, reconsume, pos);
                                    state = TokenizerState.DATA;
                                    break;
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
                                    cstart = pos;
                                    //state = Transition(state, Tokenizer.DATA, reconsume, pos);
                                    state = TokenizerState.DATA;
                                    reconsume = true;
                                    break;
                            }
                        } break;
                    case TokenizerState.TAG_NAME:
                        {
                            MyParseTagName(textSnapshot, ref state);
                        } break;
                    case TokenizerState.BEFORE_ATTRIBUTE_NAME:
                        {
                            MyParseBeforeAttribute(textSnapshot, ref state);

                        } break;
                    case TokenizerState.SELF_CLOSING_START_TAG:
                        {

                        } break;
                    case TokenizerState.ATTRIBUTE_NAME:
                        {

                        } break;
                }

            } while (textSnapshot.ReadNext(out c));
            return 0;
        }

        void MyParseDataState(TextSnapshotReader textSnapshot, ref TokenizerState state)
        {
            char c = textSnapshot.CurrentChar;
            do
            {
                switch (c)
                {
                    case '&':
                        /*
                         * U+0026 AMPERSAND (&) Switch to the character
                         * reference in data state.
                         */
                        FlushChars(textSnapshot);
                        ClearStrBufAndAppend(c);

                        SetAdditionalAndRememberAmpersandLocation('\u0000');
                        //returnState = state;
                        //state = Transition(state, Tokenizer.CONSUME_CHARACTER_REFERENCE, reconsume, pos);
                        state = TokenizerState.CONSUME_CHARACTER_REFERENCE;

                        //return;
                        //parse character reference ***
                        //TODO:
                        MyParseCharacterReference(textSnapshot, ref state);
                        return;
                    case '<':
                        /*
                         * U+003C LESS-THAN SIGN (<) Switch to the tag
                         * open state.
                         */
                        FlushChars(textSnapshot);
                        state = TokenizerState.TAG_OPEN;
                        //goto breakDataloop; // FALL THROUGH continue
                        return;
                    // stateloop;
                    case '\u0000':
                        EmitReplacementCharacter(textSnapshot.InteralBuff, textSnapshot.Position);
                        continue;
                    case '\r':
                        EmitCarriageReturn(textSnapshot.InteralBuff, textSnapshot.Position);
                        return;
                    case '\n':
                        SilentLineFeed();
                        goto default;
                    default:
                        /*
                         * Anything else Emit the input character as a
                         * character token.
                         * 
                         * Stay in the data state.
                         */
                        continue;
                }
            } while (textSnapshot.ReadNext(out c));
        }


        void MyParseCharacterReference(TextSnapshotReader textSnapshot, ref TokenizerState state)
        {
            throw new MyNotImplementException();
            char c = textSnapshot.CurrentChar;
            do
            {


            } while (textSnapshot.ReadNext(out c));
        }

        void MyParseTagName(TextSnapshotReader textSnapshot, ref TokenizerState state)
        {
            char c = textSnapshot.CurrentChar;
            do
            {
                switch (c)
                {
                    case '\r':
                        SilentCarriageReturn();
                        StrBufToElementNameString();
                        //state = Transition(state, Tokenizer.BEFORE_ATTRIBUTE_NAME, reconsume, pos);
                        state = TokenizerState.BEFORE_ATTRIBUTE_NAME;
                        return;
                    case '\n':
                        SilentLineFeed();
                        goto case ' ';
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
                        state = TokenizerState.BEFORE_ATTRIBUTE_NAME;
                        return;
                    // goto continueStateloop;
                    case '/':
                        /*
                         * U+002F SOLIDUS (/) Switch to the self-closing
                         * start tag state.
                         */
                        StrBufToElementNameString();
                        //state = Transition(state, Tokenizer.SELF_CLOSING_START_TAG, reconsume, pos);
                        state = TokenizerState.SELF_CLOSING_START_TAG;
                        return;
                    case '>':
                        /*
                         * U+003E GREATER-THAN SIGN (>) Emit the current
                         * tag token.
                         */
                        StrBufToElementNameString();
                        //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                        state = EmitCurrentTagToken(false, textSnapshot.Position);
                        if (shouldSuspend)
                        {
                            return;
                        }
                        /*
                         * Switch to the data state.
                         */
                        return;
                    case '\u0000':
                        c = '\uFFFD';
                        goto default;
                    // fall thru
                    default:
                        {
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
                        } break;
                }

            } while (textSnapshot.ReadNext(out c));
        }

        void MyParseBeforeAttribute(TextSnapshotReader textSnapshot, ref TokenizerState state)
        {
            char c = textSnapshot.CurrentChar;
            do
            {
                switch (c)
                {

                    case '\r':
                        SilentCarriageReturn();
                        break;
                    case '\n':
                        SilentLineFeed();
                        // fall thru
                        goto case ' ';
                    case ' ':
                    case '\t':
                    case '\u000C':
                        /*
                         * U+0009 CHARACTER TABULATION U+000A LINE FEED
                         * (LF) U+000C FORM FEED (FF) U+0020 SPACE Stay
                         * in the before attribute name state.
                         */
                        break;
                    case '/':
                        /*
                         * U+002F SOLIDUS (/) Switch to the self-closing
                         * start tag state.
                         */
                        //state = Transition(state, Tokenizer.SELF_CLOSING_START_TAG, reconsume, pos);
                        state = TokenizerState.SELF_CLOSING_START_TAG;
                        return;
                    case '>':
                        /*
                         * U+003E GREATER-THAN SIGN (>) Emit the current
                         * tag token.
                         */
                        //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                        state = EmitCurrentTagToken(false, textSnapshot.Position);
                        if (shouldSuspend)
                        {
                            return;
                        }
                        /*
                         * Switch to the data state.
                         */
                        return;
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
                        {
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
                            state = TokenizerState.ATTRIBUTE_NAME;

                        } return;
                }
            } while (textSnapshot.ReadNext(out c));


        }

        //TODO: remove  MyNotImplementException

        class MyNotImplementException : Exception { }
    }
}