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
    partial class Tokenizer
    {
        void StateLoop3_RawText_CData_RcRef(TokenizerState state, TokenizerState returnState)
        {

            /*
             * Idioms used in this code:
             * 
             * 
             * Consuming the next input character
             * 
             * To consume the next input character, the code does this: if (++pos ==
             * endPos) { goto breakStateloop; } c = buf[pos];
             * 
             * 
             * Staying in a state
             * 
             * When there's a state that the tokenizer may stay in over multiple
             * input characters, the state has a wrapper |for(;;)| loop and staying
             * in the state continues the loop.
             * 
             * 
             * Switching to another state
             * 
             * To switch to another state, the code sets the state variable to the
             * magic number of the new state. Then it either continues stateloop or
             * breaks out of the state's own wrapper loop if the target state is
             * right after the current state in source order. (This is a partial
             * workaround for Java's lack of goto.)
             * 
             * 
             * Reconsume support
             * 
             * The spec sometimes says that an input character is reconsumed in
             * another state. If a state can ever be entered so that an input
             * character can be reconsumed in it, the state's code starts with an
             * |if (reconsume)| that sets reconsume to false and skips over the
             * normal code for consuming a new character.
             * 
             * To reconsume the current character in another state, the code sets
             * |reconsume| to true and then switches to the other state.
             * 
             * 
             * Emitting character tokens
             * 
             * This method emits character tokens lazily. Whenever a new range of
             * character tokens starts, the field cstart must be set to the start
             * index of the range. The flushChars() method must be called at the end
             * of a range to flush it.
             * 
             * 
             * U+0000 handling
             * 
             * The various states have to handle the replacement of U+0000 with
             * U+FFFD. However, if U+0000 would be reconsumed in another state, the
             * replacement doesn't need to happen, because it's handled by the
             * reconsuming state.
             * 
             * 
             * LF handling
             * 
             * Every state needs to increment the line number upon LF unless the LF
             * gets reconsumed by another state which increments the line number.
             * 
             * 
             * CR handling
             * 
             * Every state needs to handle CR unless the CR gets reconsumed and is
             * handled by the reconsuming state. The CR needs to be handled as if it
             * were and LF, the lastCR field must be set to true and then this
             * method must return. The IO driver will then swallow the next
             * character if it is an LF to coalesce CRLF.
             */

            /*
             * As there is no support for labeled loops in C#, instead of break <loop>;
             * the port uses goto break<loop>; and a label after the loop.
             * Instead of continue <loop>; it uses goto continue<loop>; and a label
             * at the beginning or end of the loop (which doesn't matter in for(;;) loops)
             */

            /*stateloop:*/
            for (; ; )
            {
                 
                //*************
            continueStateloop:
                //*************

                switch (state)
                {   
                    // XXX reorder point
                    case TokenizerState.CDATA_START:
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                if (index < 6)
                                { // CDATA_LSQB.Length
                                    if (c == Tokenizer.CDATA_LSQB[index])
                                    {
                                        AppendLongStrBuf(c);
                                    }
                                    else
                                    {
                                        ErrBogusComment();
                                        //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);
                                        state = TokenizerState.s44_BOGUS_COMMENT;
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                    }
                                    index++;
                                    continue;
                                }
                                else
                                {
                                    reader.StartCollect(); // start coalescing
                                    //state = Transition(state, Tokenizer.CDATA_SECTION, reconsume, pos);
                                    state = TokenizerState.s68_CDATA_SECTION;
                                    //reconsume = true;
                                    reader.StepBack();
                                    goto case TokenizerState.s68_CDATA_SECTION;
                                    //break; // FALL THROUGH goto continueStateloop;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                            //------------------------------------

                        }
                    // WARNING FALLTHRU case TokenizerState.TRANSITION: DON'T REORDER
                    case TokenizerState.s68_CDATA_SECTION:
                        /*cdatasectionloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                switch (c)
                                {
                                    case ']':
                                        FlushChars();
                                        //state = Transition(state, Tokenizer.CDATA_RSQB, reconsume, pos);
                                        state = TokenizerState.CDATA_RSQB;
                                        goto breakCdatasectionloop; // FALL THROUGH
                                    case '\u0000':
                                        EmitReplacementCharacter();
                                        continue;
                                    case '\r':
                                        EmitCarriageReturn();
                                        goto breakStateloop;
                                    case '\n':
                                    default:
                                        continue;
                                }
                            }
                            goto breakStateloop;
                        //------------------------------------
                        breakCdatasectionloop:
                            goto case TokenizerState.CDATA_RSQB;
                        }
                    // WARNING FALLTHRU case TokenizerState.TRANSITION: DON'T REORDER
                    case TokenizerState.CDATA_RSQB:
                        /*cdatarsqb:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                switch (c)
                                {
                                    case ']':
                                        //state = Transition(state, Tokenizer.CDATA_RSQB_RSQB, reconsume, pos);
                                        state = TokenizerState.CDATA_RSQB_RSQB;
                                        goto breakCdatarsqb;
                                    default:
                                        TokenListener.Characters(Tokenizer.RSQB_RSQB, 0, 1);
                                        reader.StartCollect();
                                        //state = Transition(state, Tokenizer.CDATA_SECTION, reconsume, pos);
                                        state = TokenizerState.s68_CDATA_SECTION;
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                        //------------------------------------ 
                        breakCdatarsqb:
                            goto case TokenizerState.CDATA_RSQB_RSQB;
                        }
                    // WARNING FALLTHRU case TokenizerState.TRANSITION: DON'T REORDER
                    case TokenizerState.CDATA_RSQB_RSQB:
                        {
                            char c;
                            if (!reader.ReadNext(out c))
                            {
                                goto breakStateloop;
                            }
                            switch (c)
                            {
                                case '>':
                                    //cstart = pos + 1;
                                    reader.SkipOneAndStartCollect();
                                    //state = Transition(state, Tokenizer.DATA, reconsume, pos);
                                    state = TokenizerState.s01_DATA;
                                    goto continueStateloop;
                                default:
                                    TokenListener.Characters(Tokenizer.RSQB_RSQB, 0, 2);
                                    reader.StartCollect();
                                    //state = Transition(state, Tokenizer.CDATA_SECTION, reconsume, pos);
                                    state = TokenizerState.s68_CDATA_SECTION;
                                    reader.StepBack();
                                    //reconsume = true;
                                    goto continueStateloop;

                            }
                        }
                    // XXX reorder point
                    case TokenizerState.s39_ATTRIBUTE_VALUE_SINGLE_QUOTED:
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
                                        state = TokenizerState.s42__AFTER_ATTRIBUTE_VALUE_QUOTED;
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
                                        state = TokenizerState.CONSUME_CHARACTER_REFERENCE;
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
                            goto case TokenizerState.CONSUME_CHARACTER_REFERENCE;
                        }
                    // FALLTHRU DON'T REORDER
                    case TokenizerState.CONSUME_CHARACTER_REFERENCE:
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
                                    state = TokenizerState.CONSUME_NCR;
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
                                        firstCharKey = c - 'a' + 26;
                                    }
                                    else if (c >= 'A' && c <= 'Z')
                                    {
                                        firstCharKey = c - 'A';
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
                                    state = TokenizerState.CHARACTER_REFERENCE_HILO_LOOKUP;

                                    // FALL THROUGH goto continueStateloop;
                                    break;
                            }
                            //------------------------------------
                            goto case TokenizerState.CHARACTER_REFERENCE_HILO_LOOKUP;
                        }
                    // WARNING FALLTHRU case TokenizerState.TRANSITION: DON'T REORDER
                    case TokenizerState.CHARACTER_REFERENCE_HILO_LOOKUP:
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
                            state = TokenizerState.CHARACTER_REFERENCE_TAIL;
                            // FALL THROUGH goto continueStateloop;
                            goto case TokenizerState.CHARACTER_REFERENCE_TAIL;
                        }
                    case TokenizerState.CHARACTER_REFERENCE_TAIL:
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
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        }

                    // XXX reorder point
                    case TokenizerState.CONSUME_NCR:
                        {
                            char c;
                            if (!reader.ReadNext(out c))
                            {
                                //------------------------------------
                                //eof
                                goto breakStateloop;
                            }

                            prevValue = -1;
                            value = 0;
                            seenDigits = false;
                            /*
                             * The behavior further depends on the character after the
                             * U+0023 NUMBER SIGN:
                             */
                            switch (c)
                            {
                                case 'x':
                                case 'X':

                                    /*
                                     * U+0078 LATIN SMALL LETTER X U+0058 LATIN CAPITAL
                                     * LETTER X Consume the X.
                                     * 
                                     * Follow the steps below, but using the range of
                                     * characters U+0030 DIGIT ZERO through to U+0039
                                     * DIGIT NINE, U+0061 LATIN SMALL LETTER A through
                                     * to U+0066 LATIN SMALL LETTER F, and U+0041 LATIN
                                     * CAPITAL LETTER A, through to U+0046 LATIN CAPITAL
                                     * LETTER F (in other words, 0-9, A-F, a-f).
                                     * 
                                     * When it comes to interpreting the number,
                                     * interpret it as a hexadecimal number.
                                     */
                                    AppendStrBuf(c);
                                    //state = Transition(state, Tokenizer.HEX_NCR_LOOP, reconsume, pos);
                                    state = TokenizerState.HEX_NCR_LOOP;

                                    goto continueStateloop;
                                default:
                                    /*
                                     * Anything else Follow the steps below, but using
                                     * the range of characters U+0030 DIGIT ZERO through
                                     * to U+0039 DIGIT NINE (i.e. just 0-9).
                                     * 
                                     * When it comes to interpreting the number,
                                     * interpret it as a decimal number.
                                     */
                                    //state = Transition(state, Tokenizer.DECIMAL_NRC_LOOP, reconsume, pos);
                                    state = TokenizerState.DECIMAL_NRC_LOOP;
                                    //reconsume = true;
                                    reader.StepBack();
                                    // FALL THROUGH goto continueStateloop;
                                    break;
                            }
                            //------------------------------------
                            // WARNING FALLTHRU case TokenizerState.TRANSITION: DON'T REORDER
                            goto case TokenizerState.DECIMAL_NRC_LOOP;
                        }
                    case TokenizerState.DECIMAL_NRC_LOOP:
                        /*decimalloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                // Deal with overflow gracefully
                                if (value < prevValue)
                                {
                                    value = 0x110000; // Value above Unicode range but
                                    // within int
                                    // range
                                }
                                prevValue = value;
                                /*
                                 * Consume as many characters as match the range of
                                 * characters given above.
                                 */
                                if (c >= '0' && c <= '9')
                                {
                                    seenDigits = true;
                                    value *= 10;
                                    value += c - '0';
                                    continue;
                                }
                                else if (c == ';')
                                {
                                    if (seenDigits)
                                    {
                                        //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                        if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                        {
                                            reader.SkipOneAndStartCollect();
                                        }
                                        //state = Transition(state, Tokenizer.HANDLE_NCR_VALUE, reconsume, pos);
                                        state = TokenizerState.HANDLE_NCR_VALUE;

                                        // FALL THROUGH goto continueStateloop;
                                        goto breakDecimalloop;
                                    }
                                    else
                                    {
                                        ErrNoDigitsInNCR();
                                        AppendStrBuf(';');
                                        EmitOrAppendStrBuf(returnState);
                                        //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                        if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                        {
                                            reader.SkipOneAndStartCollect();
                                        }
                                        //state = Transition(state, returnState, reconsume, pos);
                                        state = returnState;

                                        goto continueStateloop;
                                    }
                                }
                                else
                                {
                                    /*
                                     * If no characters match the range, then don't
                                     * consume any characters (and unconsume the U+0023
                                     * NUMBER SIGN character and, if appropriate, the X
                                     * character). This is a parse error; nothing is
                                     * returned.
                                     * 
                                     * Otherwise, if the next character is a U+003B
                                     * SEMICOLON, consume that too. If it isn't, there
                                     * is a parse error.
                                     */
                                    if (!seenDigits)
                                    {
                                        ErrNoDigitsInNCR();
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
                                        ErrCharRefLacksSemicolon();
                                        //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                        if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                        {
                                            reader.StartCollect();
                                        }
                                        //state = Transition(state, Tokenizer.HANDLE_NCR_VALUE, reconsume, pos);
                                        state = TokenizerState.HANDLE_NCR_VALUE;
                                        //reconsume = true;
                                        reader.StepBack();
                                        // FALL THROUGH goto continueStateloop;
                                        goto breakDecimalloop;
                                    }
                                }
                            }

                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        //-------------------------------------
                        breakDecimalloop:
                            goto case TokenizerState.HANDLE_NCR_VALUE;
                        }
                    // WARNING FALLTHRU case TokenizerState.TRANSITION: DON'T REORDER
                    case TokenizerState.HANDLE_NCR_VALUE:
                        {
                            // WARNING previous state sets reconsume
                            // XXX inline this case TokenizerState.if the method size can take it
                            HandleNcrValue(returnState);
                            //state = Transition(state, returnState, reconsume, pos);
                            state = returnState;

                            goto continueStateloop;
                        }
                    // XXX reorder point
                    case TokenizerState.HEX_NCR_LOOP:
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                // Deal with overflow gracefully
                                if (value < prevValue)
                                {
                                    value = 0x110000; // Value above Unicode range but
                                    // within int
                                    // range
                                }
                                prevValue = value;
                                /*
                                 * Consume as many characters as match the range of
                                 * characters given above.
                                 */
                                if (c >= '0' && c <= '9')
                                {
                                    seenDigits = true;
                                    value *= 16;
                                    value += c - '0';
                                    continue;
                                }
                                else if (c >= 'A' && c <= 'F')
                                {
                                    seenDigits = true;
                                    value *= 16;
                                    value += c - 'A' + 10;
                                    continue;
                                }
                                else if (c >= 'a' && c <= 'f')
                                {
                                    seenDigits = true;
                                    value *= 16;
                                    value += c - 'a' + 10;
                                    continue;
                                }
                                else if (c == ';')
                                {
                                    if (seenDigits)
                                    {
                                        //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                        if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                        {
                                            reader.SkipOneAndStartCollect();
                                        }
                                        //state = Transition(state, Tokenizer.HANDLE_NCR_VALUE, reconsume, pos);
                                        state = TokenizerState.HANDLE_NCR_VALUE;
                                        goto continueStateloop;
                                    }
                                    else
                                    {
                                        ErrNoDigitsInNCR();
                                        AppendStrBuf(';');
                                        EmitOrAppendStrBuf(returnState);
                                        //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                        if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                        {
                                            reader.SkipOneAndStartCollect();
                                        }
                                        //state = Transition(state, returnState, reconsume, pos);
                                        state = returnState;
                                        goto continueStateloop;
                                    }
                                }
                                else
                                {
                                    /*
                                     * If no characters match the range, then don't
                                     * consume any characters (and unconsume the U+0023
                                     * NUMBER SIGN character and, if appropriate, the X
                                     * character). This is a parse error; nothing is
                                     * returned.
                                     * 
                                     * Otherwise, if the next character is a U+003B
                                     * SEMICOLON, consume that too. If it isn't, there
                                     * is a parse error.
                                     */
                                    if (!seenDigits)
                                    {
                                        ErrNoDigitsInNCR();
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
                                        ErrCharRefLacksSemicolon();
                                        //if ((returnState & DATA_AND_RCDATA_MASK) == 0)
                                        if (((byte)returnState & DATA_AND_RCDATA_MASK) != 0)
                                        {
                                            reader.StartCollect();
                                        }
                                        //state = Transition(state, Tokenizer.HANDLE_NCR_VALUE, reconsume, pos);
                                        state = TokenizerState.HANDLE_NCR_VALUE;
                                        //reconsume = true;
                                        reader.StepBack();
                                        goto continueStateloop;
                                    }
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        }
                    // XXX reorder point
                    case TokenizerState.s07_PLAINTEXT:
                        /*plaintextloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '\u0000':
                                        EmitPlaintextReplacementCharacter();
                                        continue;
                                    case '\r':
                                        EmitCarriageReturn();
                                        goto breakStateloop;
                                    case '\n':
                                    default:
                                        /*
                                         * Anything else Emit the current input
                                         * character as a character token. Stay in the
                                         * RAWTEXT state.
                                         */
                                        continue;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        } 
                    // XXX reorder point
                    case TokenizerState.s03_RCDATA:
                        /*rcdataloop:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                switch (c)
                                {
                                    case '&':
                                        /*
                                         * U+0026 AMPERSAND (&) Switch to the character
                                         * reference in RCDATA state.
                                         */
                                        //FlushChars(buf, pos);
                                        FlushChars();
                                        ClearStrBufAndAppend(c);
                                        additional = '\u0000';
                                        returnState = state;
                                        //state = Transition(state, Tokenizer.CONSUME_CHARACTER_REFERENCE, reconsume, pos);
                                        state = TokenizerState.CONSUME_CHARACTER_REFERENCE;
                                        goto continueStateloop;
                                    case '<':
                                        /*
                                         * U+003C LESS-THAN SIGN (<) Switch to the
                                         * RCDATA less-than sign state.
                                         */
                                        //FlushChars(buf, pos);
                                        FlushChars();
                                        returnState = state;
                                        //state = Transition(state, Tokenizer.RAWTEXT_RCDATA_LESS_THAN_SIGN, reconsume, pos);
                                        state = TokenizerState.s11_RAWTEXT_RCDATA_LESS_THAN_SIGN;
                                        goto continueStateloop;
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
                                         * character token. Stay in the RCDATA state.
                                         */
                                        continue;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        }
                    // XXX reorder point
                    case TokenizerState.s05_RAWTEXT:
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
                                        state = TokenizerState.s11_RAWTEXT_RCDATA_LESS_THAN_SIGN;
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
                            goto case TokenizerState.s11_RAWTEXT_RCDATA_LESS_THAN_SIGN;
                        }
                    // XXX fallthru don't reorder
                    case TokenizerState.s11_RAWTEXT_RCDATA_LESS_THAN_SIGN:
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
                                        state = TokenizerState.NON_DATA_END_TAG_NAME;
                                        goto breakRawtextrcdatalessthansignloop;
                                    // FALL THRU goto continueStateloop;
                                    default:
                                        /*
                                         * Otherwise, emit a U+003C LESS-THAN SIGN
                                         * character token
                                         */
                                        TokenListener.Characters(Tokenizer.LT_GT, 0, 1);
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
                            goto case TokenizerState.NON_DATA_END_TAG_NAME;
                        }
                    // XXX fall thru. don't reorder.
                    case TokenizerState.NON_DATA_END_TAG_NAME:
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
                                        TokenListener.Characters(Tokenizer.LT_SOLIDUS,
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
                                            state = TokenizerState.s34_BEFORE_ATTRIBUTE_NAME;

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
                                            state = TokenizerState.s34_BEFORE_ATTRIBUTE_NAME;
                                            goto continueStateloop;
                                        case '/':
                                            /*
                                             * U+002F SOLIDUS (/) If the current end tag
                                             * token is an appropriate end tag token,
                                             * then switch to the self-closing start tag
                                             * state.
                                             */
                                            //state = Transition(state, Tokenizer.SELF_CLOSING_START_TAG, reconsume, pos);
                                            state = TokenizerState.s43_SELF_CLOSING_START_TAG;
                                            goto continueStateloop;
                                        case '>':
                                            /*
                                             * U+003E GREATER-THAN SIGN (>) If the
                                             * current end tag token is an appropriate
                                             * end tag token, then emit the current tag
                                             * token and switch to the data state.
                                             */
                                            //state = Transition(state, EmitCurrentTagToken(false, pos), reconsume, pos);
                                            state = EmitCurrentTagToken(false);
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
                    case TokenizerState.PROCESSING_INSTRUCTION:
                        //processinginstructionloop: 
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                switch (c)
                                {
                                    case '?':
                                        //state = Transition(state,Tokenizer.PROCESSING_INSTRUCTION_QUESTION_MARK,reconsume, pos);
                                        state = TokenizerState.PROCESSING_INSTRUCTION_QUESTION_MARK;

                                        break;
                                    // continue stateloop;
                                    default:
                                        continue;
                                }
                            }
                            //------------------------------------
                            //eof
                            goto breakStateloop;
                        }
                    //breakProcessingInstructionLoop: 
                    case TokenizerState.PROCESSING_INSTRUCTION_QUESTION_MARK:
                        {
                            char c;
                            if (!reader.ReadNext(out c))
                            {
                                goto breakStateloop;

                            }

                            switch (c)
                            {
                                case '>':
                                    //state = Transition(state, Tokenizer.DATA,reconsume, pos);
                                    state = TokenizerState.s01_DATA;
                                    continue;
                                default:
                                    //state = Transition(state,Tokenizer.PROCESSING_INSTRUCTION,reconsume, pos);
                                    state = TokenizerState.PROCESSING_INSTRUCTION;
                                    continue;
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
            stateSave = state;
            returnStateSave = returnState;
        }

    }

}