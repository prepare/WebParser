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
    enum CharMode : byte
    {
        Others,
        Numeric,
        LowerAsciiLetter,
        UpperAsciiLetter,
        NewLine,
        WhiteSpace,
        NullChar,


        /// <summary>
        /// &gt;
        /// </summary>
        Gt,
        /// <summary>
        /// &lt;
        /// </summary>
        Lt,
        /// <summary>
        /// !
        /// </summary>
        Bang,
        /// <summary>
        /// ?
        /// </summary>
        Quest,
        /// <summary>
        /// /
        /// </summary>
        Slash,
        /// <summary>
        /// =
        /// </summary>
        Assign,

        Sharp,

        Ampersand,

        Quote,
        DoubleQuote,

        Eof
    }
    //with html5 state 
    //review missing state

    public enum InterLexerState : byte
    {

        s01_DATA_i = 128, //comment, doctype,cdata,tag   
        s45_MARKUP_DECLARATION_OPEN_i = 18, //tag ,comment,  
        MARKUP_DECLARATION_OCTYPE_i = 40, //doctype, comment

        //------------------
        CONSUME_CHARACTER_REFERENCE_i = 46, //tag,cdata ,enterpoint for character reference
        //------------------
        s44_BOGUS_COMMENT_i = 17,//doctype,cdata,tag,comment 
        //------------------
        //for doctype  
        NON_DATA_END_TAG_NAME_i = 38, //scriptdata, tag  
        CONSUME_NCR_i = 47, //ncr->numeric character reference, used by ncr,tag
        CDATA_START_i = 55,//comment,cdata 
    }

}