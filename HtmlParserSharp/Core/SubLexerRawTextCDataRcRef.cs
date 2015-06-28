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
    public enum RawTextCDataRcRefState
    {
        s11_RAWTEXT_RCDATA_LESS_THAN_SIGN_p = 65, //rawtext 
        //TODO: 02_CharacterReferenceInData() 
        s03_RCDATA_p = 129, //rawtext 
        //TODO: R04_CharacterReferenceInRcData(); 
        s05_RAWTEXT_p = 3, //rawtext 
        s07_PLAINTEXT_p = 8,//rawtext 
        s68_CDATA_SECTION_p = 56, //rawtext
        CDATA_RSQB_p = 57,// rawtext
        CDATA_RSQB_RSQB_p = 58,//rawtext 
        PROCESSING_INSTRUCTION_p = 73, // rawtext
        PROCESSING_INSTRUCTION_QUESTION_MARK_p = 74// rawtext
    }
    class SubLexerRawTextCDataRcRef : SubLexer
    {   
        int index;
        bool endTag; //TODO: review shared endTag with other sublexer? 
        ElementName tagName; //TODO: review shared tagName with other sublexer? 
        char additional;
       
      
        RawTextCDataRcRefState EmitCurrentTagToken2(bool isSelfClosing)
        {
            throw new NotImplementedException();
        }
        void SaveStates(RawTextCDataRcRefState state, RawTextCDataRcRefState returnState)
        {
        }
        void StateLoop3_RawText_CData_RcRef(RawTextCDataRcRefState state, RawTextCDataRcRefState returnState)
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
                    case (RawTextCDataRcRefState)TokenizerState.CDATA_START_i:
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {

                                if (index < 6)
                                { // CDATA_LSQB.Length
                                    if (c == CDATA_LSQB[index])
                                    {
                                        AppendLongStrBuf(c);
                                    }
                                    else
                                    {
                                        ErrBogusComment();
                                        //state = Transition(state, Tokenizer.BOGUS_COMMENT, reconsume, pos);
                                        //state = TokenizerState.s44_BOGUS_COMMENT_i;
                                        SetInterLexerState(TokenizerState.s44_BOGUS_COMMENT_i);
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
                                    state = RawTextCDataRcRefState.s68_CDATA_SECTION_p;
                                    //reconsume = true;
                                    reader.StepBack();
                                    goto case RawTextCDataRcRefState.s68_CDATA_SECTION_p;
                                    //break; // FALL THROUGH goto continueStateloop;
                                }
                            }
                            //-------------------------------
                            //eof
                            goto breakStateloop;
                            //------------------------------------

                        }
                    // WARNING FALLTHRU case TokenizerState.TRANSITION: DON'T REORDER
                    case RawTextCDataRcRefState.s68_CDATA_SECTION_p:
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
                                        state = RawTextCDataRcRefState.CDATA_RSQB_p;
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
                            goto case RawTextCDataRcRefState.CDATA_RSQB_p;
                        }
                    // WARNING FALLTHRU case TokenizerState.TRANSITION: DON'T REORDER
                    case RawTextCDataRcRefState.CDATA_RSQB_p:
                        /*cdatarsqb:*/
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                switch (c)
                                {
                                    case ']':
                                        //state = Transition(state, Tokenizer.CDATA_RSQB_RSQB, reconsume, pos);
                                        state = RawTextCDataRcRefState.CDATA_RSQB_RSQB_p;
                                        goto breakCdatarsqb;
                                    default:
                                        TokenListener.Characters(RSQB_RSQB, 0, 1);
                                        reader.StartCollect();
                                        //state = Transition(state, Tokenizer.CDATA_SECTION, reconsume, pos);
                                        state = RawTextCDataRcRefState.s68_CDATA_SECTION_p;
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
                            goto case RawTextCDataRcRefState.CDATA_RSQB_RSQB_p;
                        }
                    // WARNING FALLTHRU case TokenizerState.TRANSITION: DON'T REORDER
                    case RawTextCDataRcRefState.CDATA_RSQB_RSQB_p:
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
                                    //state = TokenizerState.s01_DATA_i;
                                    SetInterLexerState(TokenizerState.s01_DATA_i);
                                    goto continueStateloop;
                                default:
                                    TokenListener.Characters(RSQB_RSQB, 0, 2);
                                    reader.StartCollect();
                                    //state = Transition(state, Tokenizer.CDATA_SECTION, reconsume, pos);
                                    state = RawTextCDataRcRefState.s68_CDATA_SECTION_p;

                                    reader.StepBack();
                                    //reconsume = true;
                                    goto continueStateloop;

                            }
                        }
                    // XXX reorder point
                    case RawTextCDataRcRefState.s07_PLAINTEXT_p:
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
                    case RawTextCDataRcRefState.s03_RCDATA_p:
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
                                        //state = TokenizerState.CONSUME_CHARACTER_REFERENCE_i;
                                        SetInterLexerState(TokenizerState.CONSUME_CHARACTER_REFERENCE_i);
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

                                        state = RawTextCDataRcRefState.s11_RAWTEXT_RCDATA_LESS_THAN_SIGN_p;
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
                    case RawTextCDataRcRefState.PROCESSING_INSTRUCTION_p:
                        //processinginstructionloop: 
                        {
                            char c;
                            while (reader.ReadNext(out c))
                            {
                                switch (c)
                                {
                                    case '?':
                                        //state = Transition(state,Tokenizer.PROCESSING_INSTRUCTION_QUESTION_MARK,reconsume, pos);
                                        state = RawTextCDataRcRefState.PROCESSING_INSTRUCTION_QUESTION_MARK_p;

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
                    case RawTextCDataRcRefState.PROCESSING_INSTRUCTION_QUESTION_MARK_p:
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
                                    //state = TokenizerState.s01_DATA_i;
                                    SetInterLexerState(TokenizerState.s01_DATA_i);
                                    continue;
                                default:
                                    //state = Transition(state,Tokenizer.PROCESSING_INSTRUCTION,reconsume, pos);

                                    state = RawTextCDataRcRefState.PROCESSING_INSTRUCTION_p;
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
            SaveStates(state, returnState);
            //stateSave = state;
            //returnStateSave = returnState;
        }

    }

}