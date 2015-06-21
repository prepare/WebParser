//
// HtmlTokenizerState.cs
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

namespace HtmlKit {
	/// <summary>
	/// The HTML tokenizer state.
	/// </summary>
	/// <remarks>
	/// The HTML tokenizer state.
	/// </remarks>
	public enum HtmlTokenizerState {
		s01_Data,
		s02_CharacterReferenceInData,
		s03_RcData,
		s04_CharacterReferenceInRcData,
		s05_RawText,
		s06_ScriptData,
		s07_PlainText,
		s08_TagOpen,
		s09_EndTagOpen,
		s10_TagName,
		s11_RcDataLessThan,
		s12_RcDataEndTagOpen,
		s13_RcDataEndTagName,
		s14_RawTextLessThan,
		s15_RawTextEndTagOpen,
		s16_RawTextEndTagName,
		s17_ScriptDataLessThan,
		s18_ScriptDataEndTagOpen,
		s19_ScriptDataEndTagName,
		s20_ScriptDataEscapeStart,
		s21_ScriptDataEscapeStartDash,
		s22_ScriptDataEscaped,
		s23_ScriptDataEscapedDash,
		s24_ScriptDataEscapedDashDash,
		s25_ScriptDataEscapedLessThan,
		s26_ScriptDataEscapedEndTagOpen,
		s27_ScriptDataEscapedEndTagName,
		s28_ScriptDataDoubleEscapeStart,
		s29_ScriptDataDoubleEscaped,
		s30_ScriptDataDoubleEscapedDash,
		s31_ScriptDataDoubleEscapedDashDash,
		s32_ScriptDataDoubleEscapedLessThan,
		s33_ScriptDataDoubleEscapeEnd,
		s34_BeforeAttributeName,
		s35_AttributeName,
		s36_AfterAttributeName,
		s37_BeforeAttributeValue,
		s38_39_AttributeValueQuoted,
		s40_AttributeValueUnquoted,
		s41_CharacterReferenceInAttributeValue,
		s42_AfterAttributeValueQuoted,
		s43_SelfClosingStartTag,
		s44_BogusComment,
		s45_MarkupDeclarationOpen,
		s46_CommentStart,
		s47_CommentStartDash,
		s48_Comment,
		s49_CommentEndDash,
		s50_CommentEnd,
		s51_CommentEndBang,
		s52_DocType,
		s53_BeforeDocTypeName,
		s54_DocTypeName,
		s55_AfterDocTypeName,
		s56_AfterDocTypePublicKeyword,
		s57_BeforeDocTypePublicIdentifier,
		s58_59_DocTypePublicIdentifierQuoted,
		s60_AfterDocTypePublicIdentifier,
		s61_BetweenDocTypePublicAndSystemIdentifiers,
		s62_AfterDocTypeSystemKeyword,
		s63_BeforeDocTypeSystemIdentifier,
		s64_65_DocTypeSystemIdentifierQuoted,
		s66_AfterDocTypeSystemIdentifier,
		s67_BogusDocType,
		s68_CDataSection,
		EndOfFile
	}
}
