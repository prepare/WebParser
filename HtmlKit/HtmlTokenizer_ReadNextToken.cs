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
                    case HtmlTokenizerState.Data: 
                        R01_DataToken();
                        break;
                    case HtmlTokenizerState.CharacterReferenceInData: 
                        R02_CharacterReferenceInData();
                        break;
                    case HtmlTokenizerState.RcData: 
                        R03_RcData();
                        break;
                    case HtmlTokenizerState.CharacterReferenceInRcData: 
                        R04_CharacterReferenceInRcData();
                        break;
                    case HtmlTokenizerState.RawText: 
                        R05_RawText();
                        break;
                    case HtmlTokenizerState.ScriptData: 
                        R06_ScriptData();
                        break;
                    case HtmlTokenizerState.PlainText:                        
                        R07_PlainText();
                        break;
                    case HtmlTokenizerState.TagOpen:                        
                        R08_TagOpen();
                        break;
                    case HtmlTokenizerState.EndTagOpen:                        
                        R09_EndTagOpen();
                        break;
                    case HtmlTokenizerState.TagName:                        
                        R10_TagName();
                        break;
                    case HtmlTokenizerState.RcDataLessThan:                        
                        R11_RcDataLessThan();
                        break;
                    case HtmlTokenizerState.RcDataEndTagOpen:                        
                        R12_RcDataEndTagOpen();
                        break;
                    case HtmlTokenizerState.RcDataEndTagName:                        
                        R13_RcDataEndTagName();
                        break;
                    case HtmlTokenizerState.RawTextLessThan:                        
                        R14_RawTextLessThan();
                        break;
                    case HtmlTokenizerState.RawTextEndTagOpen:                        
                        R15_RawTextEndTagOpen();
                        break;
                    case HtmlTokenizerState.RawTextEndTagName:                        
                        R16_RawTextEndTagName();
                        break;
                    case HtmlTokenizerState.ScriptDataLessThan:                        
                        R17_ScriptDataLessThan();
                        break;
                    case HtmlTokenizerState.ScriptDataEndTagOpen:                        
                        R18_ScriptDataEndTagOpen();
                        break;
                    case HtmlTokenizerState.ScriptDataEndTagName:                        
                        R19_ScriptDataEndTagName();
                        break;
                    case HtmlTokenizerState.ScriptDataEscapeStart:                        
                        R20_ScriptDataEscapeStart();                        
                        break;
                    case HtmlTokenizerState.ScriptDataEscapeStartDash:                        
                        R21_ScriptDataEscapeStartDash();
                        break;
                    case HtmlTokenizerState.ScriptDataEscaped:                        
                        R22_ScriptDataEscaped();
                        break;
                    case HtmlTokenizerState.ScriptDataEscapedDash:                        
                        R23_ScriptDataEscapedDash();
                        break;
                    case HtmlTokenizerState.ScriptDataEscapedDashDash:                        
                        R24_ScriptDataEscapedDashDash();
                        break;
                    case HtmlTokenizerState.ScriptDataEscapedLessThan:                        
                        R25_ScriptDataEscapedLessThan();
                        break;
                    case HtmlTokenizerState.ScriptDataEscapedEndTagOpen:                        
                        R26_ScriptDataEscapedEndTagOpen();
                        break;
                    case HtmlTokenizerState.ScriptDataEscapedEndTagName:                        
                        R27_ScriptDataEscapedEndTagName();
                        break;
                    case HtmlTokenizerState.ScriptDataDoubleEscapeStart:                        
                        R28_ScriptDataDoubleEscapeStart();
                        break;
                    case HtmlTokenizerState.ScriptDataDoubleEscaped:                        
                        R29_ScriptDataDoubleEscaped();
                        break;
                    case HtmlTokenizerState.ScriptDataDoubleEscapedDash:                        
                        R30_ScriptDataDoubleEscapedDash();
                        break;
                    case HtmlTokenizerState.ScriptDataDoubleEscapedDashDash:
                        R31_ScriptDataDoubleEscapedDashDash();
                        break;
                    case HtmlTokenizerState.ScriptDataDoubleEscapedLessThan:                        
                        R32_ScriptDataDoubleEscapedLessThan();
                        break;
                    case HtmlTokenizerState.ScriptDataDoubleEscapeEnd:                        
                        R33_ScriptDataDoubleEscapeEnd();
                        break;
                    case HtmlTokenizerState.BeforeAttributeName:                        
                        R34_BeforeAttributeName();
                        break;
                    case HtmlTokenizerState.AttributeName:                        
                        R35_AttributeName();
                        break;
                    case HtmlTokenizerState.AfterAttributeName:                        
                        R36_AfterAttributeName();
                        break;
                    case HtmlTokenizerState.BeforeAttributeValue:                        
                        R37_BeforeAttributeValue();
                        break;
                    case HtmlTokenizerState.AttributeValueQuoted:                       
                        R38_39_AttributeValueQuoted();
                        break;
                    case HtmlTokenizerState.AttributeValueUnquoted:                        
                        R40_AttributeValueUnquoted();
                        break;
                    case HtmlTokenizerState.CharacterReferenceInAttributeValue:                        
                        R41_CharacterReferenceInAttributeValue();
                        break;
                    case HtmlTokenizerState.AfterAttributeValueQuoted:                        
                        R42_AfterAttributeValueQuoted();
                        break;
                    case HtmlTokenizerState.SelfClosingStartTag:                        
                        R43_SelfClosingStartTag();
                        break;
                    case HtmlTokenizerState.BogusComment:                        
                        R44_BogusComment();
                        break;
                    case HtmlTokenizerState.MarkupDeclarationOpen:                        
                        R45_MarkupDeclarationOpen();
                        break;
                    case HtmlTokenizerState.CommentStart:                        
                        R46_CommentStart();
                        break;
                    case HtmlTokenizerState.CommentStartDash:                        
                        R47_CommentStartDash();
                        break;
                    case HtmlTokenizerState.Comment:                        
                        R48_Comment();
                        break;
                    case HtmlTokenizerState.CommentEndDash:                        
                        R49_CommentEndDash();
                        break;
                    case HtmlTokenizerState.CommentEnd:                        
                        R50_CommentEnd();
                        break;
                    case HtmlTokenizerState.CommentEndBang:                        
                        R51_CommentEndBang();
                        break;
                    case HtmlTokenizerState.DocType:                        
                        R52_DocType();
                        break;
                    case HtmlTokenizerState.BeforeDocTypeName:                        
                        R53_BeforeDocTypeName();
                        break;
                    case HtmlTokenizerState.DocTypeName:                        
                        R54_DocTypeName();
                        break;
                    case HtmlTokenizerState.AfterDocTypeName:                        
                        R55_AfterDocTypeName();
                        break;
                    case HtmlTokenizerState.AfterDocTypePublicKeyword:                        
                        R56_AfterDocTypePublicKeyword();
                        break;
                    case HtmlTokenizerState.BeforeDocTypePublicIdentifier:                        
                        R57_BeforeDocTypePublicIdentifier();
                        break;
                    case HtmlTokenizerState.DocTypePublicIdentifierQuoted:                       
                        R58_59_DocTypePublicIdentifierQuoted();
                        break;
                    case HtmlTokenizerState.AfterDocTypePublicIdentifier:                        
                        R60_AfterDocTypePublicIdentifier();
                        break;
                    case HtmlTokenizerState.BetweenDocTypePublicAndSystemIdentifiers:  
                        R61_BetweenDocTypePublicAndSystemIdentifiers();
                        break;
                    case HtmlTokenizerState.AfterDocTypeSystemKeyword:                        
                        R62_AfterDocTypeSystemKeyword();
                        break;
                    case HtmlTokenizerState.BeforeDocTypeSystemIdentifier:                        
                        R63_BeforeDocTypeSystemIdentifier();
                        break;
                    case HtmlTokenizerState.DocTypeSystemIdentifierQuoted:              
                        R64_65_DocTypeSystemIdentifierQuoted();
                        break;
                    case HtmlTokenizerState.AfterDocTypeSystemIdentifier:                       
                        R66_AfterDocTypeSystemIdentifier();
                        break;
                    case HtmlTokenizerState.BogusDocType:                        
                        R67_BogusDocType();
                        break;
                    case HtmlTokenizerState.CDataSection:                        
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