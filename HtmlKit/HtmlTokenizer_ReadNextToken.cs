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

using System.IO;
using System.Text;

namespace HtmlKit
{
    public class TokenizerEventArgs : System.EventArgs
    {
        public HtmlTokenKind TokenKind { get; internal set; }
        public int LineNumber { get; internal set; }
        public int ColumnNumber { get; internal set; }
        public string Data { get; set; }
        /// <summary>
        /// stop at this point, or not
        /// </summary>
        public bool Stop { get; set; }
    }

    public delegate void TokenizerEmit(TokenizerEventArgs e);


    partial class HtmlTokenizer
    {

        public event TokenizerEmit TokenEmit;
        public bool UseEventEmitterModel { get; set; }
        bool stopTokenizer;

        HtmlToken _nextEmitToken;
        HtmlToken nextEmitToken
        {
            get { return _nextEmitToken; }
        } 
        void SetEmitToken(HtmlToken token)
        {
            this._nextEmitToken = token;
        } 
        void ResetEmittingToken()
        {
            _nextEmitToken = null;
        }
        //---------------------------
        public bool ReadNextToken(out HtmlToken output)
        {  
            while (TokenizerState != HtmlTokenizerState.EndOfFile)
            {
                ResetEmittingToken(); //before each round we reset current token

                switch (TokenizerState)
                {
                    case HtmlTokenizerState.s01_Data: 
                        R01_Data();
                        break;
                    case HtmlTokenizerState.s02_CharacterReferenceInData: 
                        R02_CharacterReferenceInData();
                        break;
                    case HtmlTokenizerState.s03_RcData: 
                        R03_RcData();
                        break;
                    case HtmlTokenizerState.s04_CharacterReferenceInRcData: 
                        R04_CharacterReferenceInRcData();
                        break;
                    case HtmlTokenizerState.s05_RawText: 
                        R05_RawText();
                        break;
                    case HtmlTokenizerState.s06_ScriptData: 
                        R06_ScriptData();
                        break;
                    case HtmlTokenizerState.s07_PlainText:                        
                        R07_PlainText();
                        break;
                    case HtmlTokenizerState.s08_TagOpen:                        
                        R08_TagOpen();
                        break;
                    case HtmlTokenizerState.s09_EndTagOpen:                        
                        R09_EndTagOpen();
                        break;
                    case HtmlTokenizerState.s10_TagName:                        
                        R10_TagName();
                        break;
                    case HtmlTokenizerState.s11_RcDataLessThan:                        
                        R11_RcDataLessThan();
                        break;
                    case HtmlTokenizerState.s12_RcDataEndTagOpen:                        
                        R12_RcDataEndTagOpen();
                        break;
                    case HtmlTokenizerState.s13_RcDataEndTagName:                        
                        R13_RcDataEndTagName();
                        break;
                    case HtmlTokenizerState.s14_RawTextLessThan:                        
                        R14_RawTextLessThan();
                        break;
                    case HtmlTokenizerState.s15_RawTextEndTagOpen:                        
                        R15_RawTextEndTagOpen();
                        break;
                    case HtmlTokenizerState.s16_RawTextEndTagName:                        
                        R16_RawTextEndTagName();
                        break;
                    case HtmlTokenizerState.s17_ScriptDataLessThan:                        
                        R17_ScriptDataLessThan();
                        break;
                    case HtmlTokenizerState.s18_ScriptDataEndTagOpen:                        
                        R18_ScriptDataEndTagOpen();
                        break;
                    case HtmlTokenizerState.s19_ScriptDataEndTagName:                        
                        R19_ScriptDataEndTagName();
                        break;
                    case HtmlTokenizerState.s20_ScriptDataEscapeStart:                        
                        R20_ScriptDataEscapeStart();                        
                        break;
                    case HtmlTokenizerState.s21_ScriptDataEscapeStartDash:                        
                        R21_ScriptDataEscapeStartDash();
                        break;
                    case HtmlTokenizerState.s22_ScriptDataEscaped:                        
                        R22_ScriptDataEscaped();
                        break;
                    case HtmlTokenizerState.s23_ScriptDataEscapedDash:                        
                        R23_ScriptDataEscapedDash();
                        break;
                    case HtmlTokenizerState.s24_ScriptDataEscapedDashDash:                        
                        R24_ScriptDataEscapedDashDash();
                        break;
                    case HtmlTokenizerState.s25_ScriptDataEscapedLessThan:                        
                        R25_ScriptDataEscapedLessThan();
                        break;
                    case HtmlTokenizerState.s26_ScriptDataEscapedEndTagOpen:                        
                        R26_ScriptDataEscapedEndTagOpen();
                        break;
                    case HtmlTokenizerState.s27_ScriptDataEscapedEndTagName:                        
                        R27_ScriptDataEscapedEndTagName();
                        break;
                    case HtmlTokenizerState.s28_ScriptDataDoubleEscapeStart:                        
                        R28_ScriptDataDoubleEscapeStart();
                        break;
                    case HtmlTokenizerState.s29_ScriptDataDoubleEscaped:                        
                        R29_ScriptDataDoubleEscaped();
                        break;
                    case HtmlTokenizerState.s30_ScriptDataDoubleEscapedDash:                        
                        R30_ScriptDataDoubleEscapedDash();
                        break;
                    case HtmlTokenizerState.s31_ScriptDataDoubleEscapedDashDash:
                        R31_ScriptDataDoubleEscapedDashDash();
                        break;
                    case HtmlTokenizerState.s32_ScriptDataDoubleEscapedLessThan:                        
                        R32_ScriptDataDoubleEscapedLessThan();
                        break;
                    case HtmlTokenizerState.s33_ScriptDataDoubleEscapeEnd:                        
                        R33_ScriptDataDoubleEscapeEnd();
                        break;
                    case HtmlTokenizerState.s34_BeforeAttributeName:                        
                        R34_BeforeAttributeName();
                        break;
                    case HtmlTokenizerState.s35_AttributeName:                        
                        R35_AttributeName();
                        break;
                    case HtmlTokenizerState.s36_AfterAttributeName:                        
                        R36_AfterAttributeName();
                        break;
                    case HtmlTokenizerState.s37_BeforeAttributeValue:                        
                        R37_BeforeAttributeValue();
                        break;
                    case HtmlTokenizerState.s38_39_AttributeValueQuoted:                       
                        R38_39_AttributeValueQuoted();
                        break;
                    case HtmlTokenizerState.s40_AttributeValueUnquoted:                        
                        R40_AttributeValueUnquoted();
                        break;
                    case HtmlTokenizerState.s41_CharacterReferenceInAttributeValue:                        
                        R41_CharacterReferenceInAttributeValue();
                        break;
                    case HtmlTokenizerState.s42_AfterAttributeValueQuoted:                        
                        R42_AfterAttributeValueQuoted();
                        break;
                    case HtmlTokenizerState.s43_SelfClosingStartTag:                        
                        R43_SelfClosingStartTag();
                        break;
                    case HtmlTokenizerState.s44_BogusComment:                        
                        R44_BogusComment();
                        break;
                    case HtmlTokenizerState.s45_MarkupDeclarationOpen:                        
                        R45_MarkupDeclarationOpen();
                        break;
                    case HtmlTokenizerState.s46_CommentStart:                        
                        R46_CommentStart();
                        break;
                    case HtmlTokenizerState.s47_CommentStartDash:                        
                        R47_CommentStartDash();
                        break;
                    case HtmlTokenizerState.s48_Comment:                        
                        R48_Comment();
                        break;
                    case HtmlTokenizerState.s49_CommentEndDash:                        
                        R49_CommentEndDash();
                        break;
                    case HtmlTokenizerState.s50_CommentEnd:                        
                        R50_CommentEnd();
                        break;
                    case HtmlTokenizerState.s51_CommentEndBang:                        
                        R51_CommentEndBang();
                        break;
                    case HtmlTokenizerState.s52_DocType:                        
                        R52_DocType();
                        break;
                    case HtmlTokenizerState.s53_BeforeDocTypeName:                        
                        R53_BeforeDocTypeName();
                        break;
                    case HtmlTokenizerState.s54_DocTypeName:                        
                        R54_DocTypeName();
                        break;
                    case HtmlTokenizerState.s55_AfterDocTypeName:                        
                        R55_AfterDocTypeName();
                        break;
                    case HtmlTokenizerState.s56_AfterDocTypePublicKeyword:                        
                        R56_AfterDocTypePublicKeyword();
                        break;
                    case HtmlTokenizerState.s57_BeforeDocTypePublicIdentifier:                        
                        R57_BeforeDocTypePublicIdentifier();
                        break;
                    case HtmlTokenizerState.s58_59_DocTypePublicIdentifierQuoted:                       
                        R58_59_DocTypePublicIdentifierQuoted();
                        break;
                    case HtmlTokenizerState.s60_AfterDocTypePublicIdentifier:                        
                        R60_AfterDocTypePublicIdentifier();
                        break;
                    case HtmlTokenizerState.s61_BetweenDocTypePublicAndSystemIdentifiers:  
                        R61_BetweenDocTypePublicAndSystemIdentifiers();
                        break;
                    case HtmlTokenizerState.s62_AfterDocTypeSystemKeyword:                        
                        R62_AfterDocTypeSystemKeyword();
                        break;
                    case HtmlTokenizerState.s63_BeforeDocTypeSystemIdentifier:                        
                        R63_BeforeDocTypeSystemIdentifier();
                        break;
                    case HtmlTokenizerState.s64_65_DocTypeSystemIdentifierQuoted:              
                        R64_65_DocTypeSystemIdentifierQuoted();
                        break;
                    case HtmlTokenizerState.s66_AfterDocTypeSystemIdentifier:                       
                        R66_AfterDocTypeSystemIdentifier();
                        break;
                    case HtmlTokenizerState.s67_BogusDocType:                        
                        R67_BogusDocType();
                        break;
                    case HtmlTokenizerState.s68_CDataSection:                        
                        R68_CDataSection();
                        break;
                    case HtmlTokenizerState.EndOfFile:
                        output =null;
                        return false;
                }

                if ((output = nextEmitToken) != null)
                {
                    return true;//found next token 
                }
            }
            //3.
            output = null;
            return false;
        }
    }
}