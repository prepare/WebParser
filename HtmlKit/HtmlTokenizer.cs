﻿//
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

namespace HtmlKit {
	public class HtmlTokenizer
	{
		const string AlphaChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		const string HexAlphabet = "0123456789ABCDEF";
		const string Numeric = "0123456789";
		const string DocType = "doctype";
		const string CData = "[CDATA[";

		readonly HtmlEntityDecoder entity = new HtmlEntityDecoder ();
		readonly StringBuilder data = new StringBuilder ();
		readonly StringBuilder name = new StringBuilder ();
		HtmlDocTypeToken doctype;
		HtmlAttribute attribute;
		HtmlTagToken tag;
		bool isEndTag;
		char quote;

		TextReader text;

		public HtmlTokenizer (TextReader reader)
		{
			text = reader;
		}

		/// <summary>
		/// Get the current state of the tokenizer.
		/// </summary>
		/// <remarks>
		/// Gets the current state of the tokenizer.
		/// </remarks>
		/// <value>The current state of the tokenizer.</value>
		public HtmlTokenizerState TokenizerState {
			get; private set;
		}

		static bool IsAlphaNumeric (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
		}

		static bool IsAsciiUpperLetter (char c)
		{
			return c >= 'A' && c <= 'Z';
		}

		static bool IsAsciiLowerLetter (char c)
		{
			return c >= 'a' && c <= 'z';
		}

		static bool IsAsciiLetter (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
		}

		static char ToLower (char c)
		{
			return (c >= 'A' && c <= 'Z') ? (char) (c + 0x20) : c;
		}

		void ClearBuffers ()
		{
			data.Clear ();
			name.Clear ();
		}

		bool ReadDataToken (out HtmlToken token)
		{
			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					break;
				}

				c = (char) nc;

				switch (c) {
				case '&':
					TokenizerState = HtmlTokenizerState.CharacterReferenceInData;
					token = null;
					return false;
				case '<':
					TokenizerState = HtmlTokenizerState.TagOpen;
					break;
				//case 0: // parse error, but emit it anyway
				default:
					data.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.Data);

			if (data.Length > 0) {
				token = new HtmlDataToken (data.ToString ());
				data.Clear ();
				return true;
			}

			token = null;

			return false;
		}

		bool ReadCharacterReferenceInData (out HtmlToken token)
		{
			int nc = text.Peek ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlDataToken (data + "&");
				return true;
			}

			c = (char) nc;
			token = null;

			switch (c) {
			case '\t': case '\n': case '\f': case ' ': case '<': case '&':
				// no character is consumed, emit '&'
				TokenizerState = HtmlTokenizerState.Data;
				data.Append ('&');
				return false;
			default:
//				if (nc == additionalAllowedCharacter) {
//					TokenizerState = HtmlTokenizerState.Data;
//					data.Append ('&');
//					return false;
//				}
				break;
			}

			entity.Push ('&');

			while (entity.Push (c)) {
				text.Read ();

				if ((nc = text.Peek ()) == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data + entity.GetValue ());
					entity.Reset ();
					data.Clear ();
					return true;
				}

				c = (char) nc;
			}

			TokenizerState = HtmlTokenizerState.Data;

			data.Append (entity.GetValue ());
			entity.Reset ();

			if (c == ';') {
				// consume the ';'
				text.Read ();
			}

			return false;
		}

		bool ReadScriptData (out HtmlToken token)
		{
			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					break;
				}

				c = (char) nc;

				switch (c) {
				case '<':
					TokenizerState = HtmlTokenizerState.ScriptDataLessThan;
					break;
				default:
					data.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptData);

			if (data.Length > 0) {
				token = new HtmlDataToken (data.ToString ());
				data.Clear ();
				return true;
			}

			token = null;

			return false;
		}

		bool ReadTagOpen (out HtmlToken token)
		{
			int nc = text.Read ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlDataToken ("<");
				return true;
			}

			token = null;

			c = (char) nc;

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append ('<');
			data.Append (c);

			switch ((c = (char) nc)) {
			case '!': TokenizerState = HtmlTokenizerState.MarkupDeclarationOpen; break;
			case '?': TokenizerState = HtmlTokenizerState.BogusComment; break;
			case '/': TokenizerState = HtmlTokenizerState.EndTagOpen; break;
			default:
				c = ToLower (c);

				if (c >= 'a' && c <= 'z') {
					TokenizerState = HtmlTokenizerState.TagName;
					isEndTag = false;
					name.Append (c);
				} else {
					TokenizerState = HtmlTokenizerState.Data;
					return false;
				}
				break;
			}

			return false;
		}

		bool ReadEndTagOpen (out HtmlToken token)
		{
			int nc = text.Read ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlDataToken (data.ToString ());
				data.Clear ();
				return true;
			}

			c = (char) nc;
			token = null;

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append (c);

			switch (c) {
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				data.Clear ();
				break;
			default:
				c = ToLower (c);

				if (c >= 'a' && c <= 'z') {
					TokenizerState = HtmlTokenizerState.TagName;
					isEndTag = true;
					name.Append (c);
				} else {
					TokenizerState = HtmlTokenizerState.BogusComment;
					return false;
				}
				break;
			}

			return false;
		}

		bool ReadTagName (out HtmlToken token)
		{
			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					ClearBuffers ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					TokenizerState = HtmlTokenizerState.BeforeAttributeName;
					break;
				case '/':
					TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
					break;
				case '>':
					token = new HtmlTagToken (name.ToString (), isEndTag);
					TokenizerState = HtmlTokenizerState.Data;
					data.Clear ();
					name.Clear ();
					return true;
				default:
					name.Append (c == '\0' ? '\uFFFD' : ToLower (c));
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.TagName);

			tag = new HtmlTagToken (name.ToString (), isEndTag);
			name.Clear ();
			token = null;

			return false;
		}

		bool ReadScriptDataLessThan (out HtmlToken token)
		{
			int nc = text.Peek ();

			switch ((char) nc) {
			case '/':
				TokenizerState = HtmlTokenizerState.ScriptDataEndTagOpen;
				name.Clear ();
				text.Read ();
				break;
			case '!':
				TokenizerState = HtmlTokenizerState.ScriptDataEscapeStart;
				data.Append ("<!");
				text.Read ();
				break;
			default:
				TokenizerState = HtmlTokenizerState.ScriptData;
				data.Append ('<');
				break;
			}

			token = null;

			return false;
		}

		bool ReadScriptDataEndTagOpen (out HtmlToken token)
		{
			int nc = text.Peek ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlDataToken ("</");
				data.Clear ();
				return true;
			}

			c = (char) nc;

			if (IsAsciiLetter (c)) {
				TokenizerState = HtmlTokenizerState.ScriptDataEndTagName;
				name.Append (ToLower (c));
				text.Read ();
			} else {
				TokenizerState = HtmlTokenizerState.ScriptData;
				data.Append ("</");
			}

			token = null;

			return false;
		}

		bool ReadScriptDataEndTagName (out HtmlToken token)
		{
			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					data.Clear ();
					name.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					// FIXME: compare to the active tag name
					if (name.ToString () == "script") {
						TokenizerState = HtmlTokenizerState.BeforeAttributeName;
						break;
					}

					goto default;
				case '/':
					// FIXME: compare to the active tag name
					if (name.ToString () == "script") {
						TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
						break;
					}
					goto default;
				case '>':
					// FIXME: compare to the active tag name
					if (name.ToString () == "script") {
						token = new HtmlTagToken (name.ToString (), isEndTag);
						TokenizerState = HtmlTokenizerState.Data;
						data.Clear ();
						name.Clear ();
						return true;
					}
					goto default;
				default:
					name.Append (c == '\0' ? '\uFFFD' : ToLower (c));
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEndTagName);

			tag = new HtmlTagToken (name.ToString (), isEndTag);
			name.Clear ();
			token = null;

			return false;
		}

		bool ReadScriptDataEscapeStart (out HtmlToken token)
		{
			int nc = text.Peek ();

			if (nc == '-') {
				TokenizerState = HtmlTokenizerState.ScriptDataEscapeStartDash;
				text.Read ();
			} else {
				TokenizerState = HtmlTokenizerState.ScriptData;
			}

			token = null;

			return false;
		}

		bool ReadScriptDataEscapeStartDash (out HtmlToken token)
		{
			int nc = text.Peek ();

			if (nc == '-') {
				TokenizerState = HtmlTokenizerState.ScriptDataEscapedDashDash;
				text.Read ();
			} else {
				TokenizerState = HtmlTokenizerState.ScriptData;
			}

			token = null;

			return false;
		}

		bool ReadScriptDataEscaped (out HtmlToken token)
		{
			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					data.Clear ();
					return true;
				}

				c = (char) nc;

				switch (c) {
				case '-':
					TokenizerState = HtmlTokenizerState.ScriptDataEscapedDash;
					data.Append ('-');
					break;
				case '<':
					TokenizerState = HtmlTokenizerState.ScriptDataEscapedLessThan;
					break;
				default:
					data.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);

			token = null;

			return false;
		}

		bool ReadScriptDataEscapedDash (out HtmlToken token)
		{
			int nc = text.Peek ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlDataToken (data.ToString ());
				data.Clear ();
				return true;
			}

			switch ((c = (char) nc)) {
			case '-':
				TokenizerState = HtmlTokenizerState.ScriptDataEscapedDashDash;
				data.Append ('-');
				break;
			case '<':
				TokenizerState = HtmlTokenizerState.ScriptDataEscapedLessThan;
				break;
			default:
				TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
				data.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			token = null;

			return false;
		}

		bool ReadScriptDataEscapedDashDash (out HtmlToken token)
		{
			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					data.Clear ();
					return true;
				}

				c = (char) nc;

				switch (c) {
				case '-':
					TokenizerState = HtmlTokenizerState.ScriptDataEscapedDash;
					data.Append ('-');
					break;
				case '<':
					TokenizerState = HtmlTokenizerState.ScriptDataEscapedLessThan;
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.ScriptData;
					data.Append ('>');
					break;
				default:
					TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
					data.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);

			token = null;

			return false;
		}

		bool ReadScriptDataEscapedLessThan (out HtmlToken token)
		{
			int nc = text.Peek ();
			char c = (char) nc;

			if (c == '/') {
				TokenizerState = HtmlTokenizerState.ScriptDataEndTagOpen;
				name.Clear ();
				text.Read ();
			} else if (IsAsciiLetter (c)) {
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
				name.Append (ToLower (c));
				data.Append ('<');
				data.Append (c);
				text.Read ();
			} else {
				TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
				data.Append ('<');
			}

			token = null;

			return false;
		}

		bool ReadScriptDataEscapedEndTagOpen (out HtmlToken token)
		{
			int nc = text.Peek ();
			char c;

			data.Append ("</");

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlDataToken (data.ToString ());
				data.Clear ();
				return true;
			}

			c = (char) nc;

			if (IsAsciiLetter (c)) {
				TokenizerState = HtmlTokenizerState.ScriptDataEscapedEndTagName;
				name.Append (ToLower (c));
				text.Read ();
			} else {
				TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
			}

			token = null;

			return false;
		}

		bool ReadScriptDataEscapedEndTagName (out HtmlToken token)
		{
			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					data.Clear ();
					name.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					// FIXME: compare to the active tag name
					if (name.ToString () == "script") {
						TokenizerState = HtmlTokenizerState.BeforeAttributeName;
						break;
					}

					goto default;
				case '/':
					// FIXME: compare to the active tag name
					if (name.ToString () == "script") {
						TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
						break;
					}
					goto default;
				case '>':
					// FIXME: compare to the active tag name
					if (name.ToString () == "script") {
						token = new HtmlTagToken (name.ToString (), isEndTag);
						TokenizerState = HtmlTokenizerState.Data;
						data.Clear ();
						name.Clear ();
						return true;
					}
					goto default;
				default:
					if (!IsAsciiLetter (c)) {
						TokenizerState = HtmlTokenizerState.ScriptData;
						data.Append (c);
						token = null;
						return false;
					}

					name.Append (ToLower (c));
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEscapedEndTagName);

			tag = new HtmlTagToken (name.ToString (), isEndTag);
			name.Clear ();
			token = null;

			return false;
		}

		bool ReadScriptDataDoubleEscapeStart (out HtmlToken token)
		{
			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					data.Clear ();
					name.Clear ();
					return true;
				}

				c = (char) nc;

				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ': case '/': case '>':
					if (name.ToString () == "script")
						TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
					else
						TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
					name.Clear ();
					break;
				default:
					if (!IsAsciiLetter (c))
						TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
					else
						name.Append (ToLower (c));
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataDoubleEscapeStart);

			token = null;

			return false;
		}

		bool ReadScriptDataDoubleEscaped (out HtmlToken token)
		{
			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					data.Clear ();
					return true;
				}

				c = (char) nc;

				switch (c) {
				case '-':
					TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedDash;
					data.Append ('-');
					break;
				case '<':
					TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
					break;
				default:
					data.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);

			token = null;

			return false;
		}

		bool ReadScriptDataDoubleEscapedDash (out HtmlToken token)
		{
			int nc = text.Peek ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlDataToken (data.ToString ());
				data.Clear ();
				return true;
			}

			switch ((c = (char) nc)) {
			case '-':
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedDashDash;
				data.Append ('-');
				break;
			case '<':
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
				break;
			default:
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
				data.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			token = null;

			return false;
		}

		bool ReadScriptDataDoubleEscapedDashDash (out HtmlToken token)
		{
			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					data.Clear ();
					return true;
				}

				c = (char) nc;

				switch (c) {
				case '-':
					data.Append ('-');
					break;
				case '<':
					TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapedLessThan;
					data.Append ('<');
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.ScriptData;
					data.Append ('>');
					break;
				default:
					TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
					data.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataEscaped);

			token = null;

			return false;
		}

		bool ReadScriptDataDoubleEscapedLessThan (out HtmlToken token)
		{
			int nc = text.Peek ();

			if (nc == '/') {
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscapeEnd;
				data.Append ('/');
				text.Read ();
			} else {
				TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
			}

			token = null;

			return false;
		}

		bool ReadScriptDataDoubleEscapeEnd (out HtmlToken token)
		{
			do {
				int nc = text.Peek ();
				char c = (char) nc;

				switch (c) {
				case '\t': case '\n': case '\f': case ' ': case '/': case '>':
					if (name.ToString () == "script")
						TokenizerState = HtmlTokenizerState.ScriptDataEscaped;
					else
						TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
					data.Append (c);
					text.Read ();
					break;
				default:
					if (!IsAsciiLetter (c)) {
						TokenizerState = HtmlTokenizerState.ScriptDataDoubleEscaped;
					} else {
						name.Append (ToLower (c));
						data.Append (c);
						text.Read ();
					}
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.ScriptDataDoubleEscapeEnd);

			token = null;

			return false;
		}

		bool ReadBeforeAttributeName (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					ClearBuffers ();
					tag = null;
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					break;
				case '/':
					TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
					return false;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					data.Clear ();
					token = tag;
					tag = null;
					return true;
				case '"': case '\'': case '<': case '=':
					// parse error
					goto default;
				default:
					TokenizerState = HtmlTokenizerState.AttributeName;
					name.Append (c == '\0' ? '\uFFFD' : ToLower (c));
					return false;
				}
			} while (true);
		}

		bool ReadAttributeName (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					ClearBuffers ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					TokenizerState = HtmlTokenizerState.AfterAttributeName;
					break;
				case '/':
					TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
					break;
				case '=':
					TokenizerState = HtmlTokenizerState.BeforeAttributeValue;
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					data.Clear ();
					token = tag;
					break;
				default:
					name.Append (c == '\0' ? '\uFFFD' : ToLower (c));
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.AttributeName);

			attribute = new HtmlAttribute (name.ToString ());
			tag.Attributes.Add (attribute);
			name.Clear ();

			return token != null;
		}

		bool ReadAfterAttributeName (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					ClearBuffers ();
					tag = null;
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					break;
				case '/':
					TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
					return false;
				case '=':
					TokenizerState = HtmlTokenizerState.BeforeAttributeValue;
					return false;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					data.Clear ();
					token = tag;
					tag = null;
					return true;
				case '"': case '\'': case '<':
					// parse error
					goto default;
				default:
					TokenizerState = HtmlTokenizerState.AttributeName;
					name.Append (c == '\0' ? '\uFFFD' : ToLower (c));
					return false;
				}
			} while (true);
		}

		bool ReadBeforeAttributeValue (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					ClearBuffers ();
					tag = null;
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					break;
				case '"': case '\'': TokenizerState = HtmlTokenizerState.AttributeValueQuoted; quote = c; return false;
				case '&': TokenizerState = HtmlTokenizerState.AttributeValueUnquoted; return false;
				case '/':
					TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
					return false;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					data.Clear ();
					token = tag;
					tag = null;
					return true;
				case '<': case '=': case '`':
					// parse error
					goto default;
				default:
					TokenizerState = HtmlTokenizerState.AttributeName;
					name.Append (c == '\0' ? '\uFFFD' : ToLower (c));
					return false;
				}
			} while (true);
		}

		bool ReadAttributeValueQuoted (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					ClearBuffers ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '&':
					// ReadCharacterReference (true);
					break;
				default:
					if (c == quote) {
						TokenizerState = HtmlTokenizerState.AfterAttributeValueQuoted;
						break;
					}

					name.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.AttributeValueQuoted);

			attribute.Value = name.ToString ();
			name.Clear ();

			return false;
		}

		bool ReadAttributeValueUnquoted (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					ClearBuffers ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					TokenizerState = HtmlTokenizerState.BeforeAttributeName;
					break;
				case '&':
					// ReadCharacterReference (true);
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					data.Clear ();
					token = tag;
					tag = null;
					return true;
				case '\'': case '<': case '=': case '`':
					// parse error
					goto default;
				default:
					if (c == quote) {
						TokenizerState = HtmlTokenizerState.AfterAttributeValueQuoted;
						break;
					}

					name.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.AttributeValueUnquoted);

			attribute.Value = name.ToString ();
			name.Clear ();

			return false;
		}

		bool ReadCharacterReferenceInAttributeValue (out HtmlToken token)
		{
			int nc = text.Peek ();
			bool consume;
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlDataToken (data + "&");
				data.Clear ();
				name.Clear ();
				return true;
			}

			c = (char) nc;
			token = null;

			switch (c) {
			case '\t': case '\n': case '\f': case ' ': case '<': case '&':
				// no character is consumed, emit '&'
				data.Append ('&');
				name.Append ('&');
				consume = false;
				break;
			default:
				//if (nc == additionalAllowedCharacter) {
				//	data.Append ('&');
				//	consume = false;
				//	break;
				//}

				entity.Push ('&');

				while (entity.Push (c)) {
					text.Read ();

					if ((nc = text.Peek ()) == -1) {
						TokenizerState = HtmlTokenizerState.EndOfFile;
						token = new HtmlDataToken (data + "&" + entity.GetValue ());
						entity.Reset ();
						data.Clear ();
						return true;
					}

					c = (char) nc;
				}

				data.Append (entity.GetValue ());
				consume = c == ';' || c == '=';
				entity.Reset ();
				break;
			}

			if (quote == '\0')
				TokenizerState = HtmlTokenizerState.AttributeValueUnquoted;
			else
				TokenizerState = HtmlTokenizerState.AttributeValueQuoted;

			if (consume)
				text.Read ();

			return false;
		}

		bool ReadAfterAttributeValueQuoted (out HtmlToken token)
		{
			int nc = text.Peek ();
			bool consume;
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlDataToken (data.ToString ());
				ClearBuffers ();
				return true;
			}

			c = (char) nc;
			token = null;

			switch (c) {
			case '\t': case '\n': case '\f': case ' ':
				TokenizerState = HtmlTokenizerState.BeforeAttributeName;
				consume = true;
				break;
			case '/':
				TokenizerState = HtmlTokenizerState.SelfClosingStartTag;
				consume = true;
				break;
			case '>':
				TokenizerState = HtmlTokenizerState.Data;
				consume = true;
				data.Clear ();
				token = tag;
				tag = null;
				break;
			default:
				TokenizerState = HtmlTokenizerState.BeforeAttributeName;
				consume = false;
				break;
			}

			if (consume)
				text.Read ();

			return token != null;
		}

		bool ReadSelfClosingStartTag (out HtmlToken token)
		{
			int nc = text.Read ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlDataToken (data.ToString ());
				ClearBuffers ();
				return true;
			}

			c = (char) nc;

			if (c == '>') {
				TokenizerState = HtmlTokenizerState.Data;
				tag.IsEmptyElement = true;
				data.Clear ();
				token = tag;
				return true;
			}

			// parse error
			TokenizerState = HtmlTokenizerState.BeforeAttributeName;

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append (c);

			token = null;

			return false;
		}

		bool ReadBogusComment (out HtmlToken token)
		{
			int nc;
			char c;

			if (data.Length > 0) {
				c = data[data.Length - 1];
				data.Clear ();
				data.Append (c);
			}

			do {
				if ((nc = text.Read ()) == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					break;
				}

				if ((c = (char) nc) == '>')
					break;

				data.Append (c == '\0' ? '\uFFFD' : c);
			} while (true);

			token = new HtmlCommentToken (data.ToString ());
			data.Clear ();

			return true;
		}

		bool ReadMarkupDeclarationOpen (out HtmlToken token)
		{
			int count = 0, nc;
			char c = '\0';

			while (count < 2) {
				if ((nc = text.Peek ()) == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlDataToken (data.ToString ());
					data.Clear ();
					return true;
				}

				if ((c = (char) nc) != '-')
					break;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);
				text.Read ();
				count++;
			}

			token = null;

			if (count == 2) {
				TokenizerState = HtmlTokenizerState.CommentStart;
				name.Clear ();
				return false;
			}

			if (count == 1) {
				// parse error
				TokenizerState = HtmlTokenizerState.BogusComment;
				return false;
			}

			if (c == 'D' || c == 'd') {
				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);
				text.Read ();
				count = 1;

				while (count < 7) {
					if ((nc = text.Read ()) == -1) {
						TokenizerState = HtmlTokenizerState.EndOfFile;
						token = new HtmlDataToken (data.ToString ());
						data.Clear ();
						return true;
					}

					if (ToLower ((c = (char) nc)) != DocType[count])
						break;

					// Note: we save the data in case we hit a parse error and have to emit a data token
					data.Append (c);
					count++;
				}

				if (count == 7) {
					TokenizerState = HtmlTokenizerState.DocType;
					return false;
				}
			} else if (c == '[') {
				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);
				text.Read ();
				count = 1;

				while (count < 7) {
					if ((nc = text.Read ()) == -1) {
						TokenizerState = HtmlTokenizerState.EndOfFile;
						token = new HtmlDataToken (data.ToString ());
						data.Clear ();
						return true;
					}

					if ((c = (char) nc) != CData[count])
						break;

					// Note: we save the data in case we hit a parse error and have to emit a data token
					data.Append (c);
					count++;
				}

				if (count == 7) {
					TokenizerState = HtmlTokenizerState.CDataSection;
					return false;
				}
			}

			// parse error
			TokenizerState = HtmlTokenizerState.BogusComment;

			return false;
		}

		bool ReadCommentStart (out HtmlToken token)
		{
			int nc = text.Read ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.Data;
				token = new HtmlCommentToken (string.Empty);
				return true;
			}

			c = (char) nc;

			data.Append (c);

			switch (c) {
			case '-':
				TokenizerState = HtmlTokenizerState.CommentStartDash;
				break;
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				token = new HtmlCommentToken (string.Empty);
				data.Clear ();
				return true;
			default: // parse error
				TokenizerState = HtmlTokenizerState.Comment;
				name.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			token = null;

			return false;
		}

		bool ReadCommentStartDash (out HtmlToken token)
		{
			int nc = text.Read ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.Data;
				token = new HtmlCommentToken (name.ToString ());
				name.Clear ();
				return true;
			}

			c = (char) nc;

			data.Append (c);

			switch (c) {
			case '-':
				TokenizerState = HtmlTokenizerState.CommentEnd;
				break;
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				token = new HtmlCommentToken (name.ToString ());
				data.Clear ();
				name.Clear ();
				return true;
			default: // parse error
				TokenizerState = HtmlTokenizerState.Comment;
				name.Append ('-');
				name.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			token = null;

			return false;
		}

		bool ReadComment (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlCommentToken (name.ToString ());
					ClearBuffers ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '-':
					TokenizerState = HtmlTokenizerState.CommentEndDash;
					return false;
				default:
					name.Append (c == '\0' ? '\uFFFD' : c);
					break;
				}
			} while (true);
		}

		// FIXME: this is exactly the same as ReadCommentStartDash
		bool ReadCommentEndDash (out HtmlToken token)
		{
			int nc = text.Read ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.Data;
				token = new HtmlCommentToken (name.ToString ());
				name.Clear ();
				return true;
			}

			c = (char) nc;

			data.Append (c);

			switch (c) {
			case '-':
				TokenizerState = HtmlTokenizerState.CommentEnd;
				break;
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				token = new HtmlCommentToken (name.ToString ());
				data.Clear ();
				name.Clear ();
				return true;
			default: // parse error
				TokenizerState = HtmlTokenizerState.Comment;
				name.Append ('-');
				name.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			token = null;

			return false;
		}

		bool ReadCommentEnd (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					token = new HtmlCommentToken (name.ToString ());
					ClearBuffers ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					token = new HtmlCommentToken (name.ToString ());
					data.Clear ();
					name.Clear ();
					return true;
				case '!': // parse error
					TokenizerState = HtmlTokenizerState.CommentEndBang;
					return false;
				case '-':
					name.Append ('-');
					break;
				default:
					TokenizerState = HtmlTokenizerState.Comment;
					name.Append (c == '\0' ? '\uFFFD' : c);
					return false;
				}
			} while (true);
		}

		bool ReadCommentEndBang (out HtmlToken token)
		{
			int nc = text.Read ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				token = new HtmlCommentToken (name.ToString ());
				data.Clear ();
				name.Clear ();
				return true;
			}

			c = (char) nc;

			data.Append (c);

			switch (c) {
			case '-':
				TokenizerState = HtmlTokenizerState.CommentEndDash;
				name.Append ("--!");
				break;
			case '>':
				TokenizerState = HtmlTokenizerState.Data;
				token = new HtmlCommentToken (name.ToString ());
				data.Clear ();
				name.Clear ();
				return true;
			default: // parse error
				TokenizerState = HtmlTokenizerState.Comment;
				name.Append ("--!");
				name.Append (c == '\0' ? '\uFFFD' : c);
				break;
			}

			token = null;

			return false;
		}

		bool ReadDocType (out HtmlToken token)
		{
			int nc = text.Peek ();
			char c;

			if (nc == -1) {
				token = new HtmlDocTypeToken { ForceQuirksMode = true };
				TokenizerState = HtmlTokenizerState.EndOfFile;
				data.Clear ();
				return true;
			}

			TokenizerState = HtmlTokenizerState.BeforeDocTypeName;
			c = (char) nc;
			token = null;

			switch (c) {
			case '\t': case '\n': case '\f': case ' ':
				data.Append (c);
				text.Read ();
				break;
			}

			return false;
		}

		bool ReadBeforeDocTypeName (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					token = new HtmlDocTypeToken { ForceQuirksMode = true };
					TokenizerState = HtmlTokenizerState.EndOfFile;
					data.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					break;
				case '>':
					token = new HtmlDocTypeToken { ForceQuirksMode = true };
					TokenizerState = HtmlTokenizerState.Data;
					data.Clear ();
					return true;
				case '\0':
					TokenizerState = HtmlTokenizerState.DocTypeName;
					doctype = new HtmlDocTypeToken ();
					name.Append ('\uFFFD');
					return false;
				default:
					TokenizerState = HtmlTokenizerState.DocTypeName;
					doctype = new HtmlDocTypeToken ();
					name.Append (ToLower (c));
					return false;
				}
			} while (true);
		}

		bool ReadDocTypeName (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.Name = name.ToString ();
					doctype.ForceQuirksMode = true;
					token = doctype;
					data.Clear ();
					name.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					TokenizerState = HtmlTokenizerState.AfterDocTypeName;
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					doctype.Name = name.ToString ();
					token = doctype;
					doctype = null;
					data.Clear ();
					name.Clear ();
					return true;
				case '\0':
					name.Append ('\uFFFD');
					break;
				default:
					name.Append (ToLower (c));
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.DocTypeName);

			doctype.Name = name.ToString ();
			name.Clear ();

			return false;
		}

		bool ReadAfterDocTypeName (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				default:
					name.Append (ToLower (c));
					if (name.Length < 6)
						break;

					switch (name.ToString ()) {
					case "public": TokenizerState = HtmlTokenizerState.AfterDocTypePublicKeyword; break;
					case "system": TokenizerState = HtmlTokenizerState.AfterDocTypeSystemKeyword; break;
					default: TokenizerState = HtmlTokenizerState.BogusDocType; break;
					}

					name.Clear ();
					return false;
				}
			} while (true);
		}

		public bool ReadAfterDocTypePublicKeyword (out HtmlToken token)
		{
			int nc = text.Read ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				doctype.ForceQuirksMode = true;
				token = doctype;
				doctype = null;
				data.Clear ();
				return true;
			}

			c = (char) nc;

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append (c);

			switch (c) {
			case '\t': case '\n': case '\f': case ' ':
				TokenizerState = HtmlTokenizerState.BeforeDocTypePublicIdentifier;
				break;
			case '"': case '\'': // parse error
				TokenizerState = HtmlTokenizerState.DocTypePublicIdentifierQuoted;
				doctype.PublicIdentifier = string.Empty;
				quote = c;
				break;
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				doctype.ForceQuirksMode = true;
				token = doctype;
				doctype = null;
				data.Clear ();
				return true;
			default: // parse error
				TokenizerState = HtmlTokenizerState.BogusDocType;
				doctype.ForceQuirksMode = true;
				break;
			}

			token = null;

			return false;
		}

		public bool ReadBeforeDocTypePublicIdentifier (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					break;
				case '"': case '\'':
					TokenizerState = HtmlTokenizerState.DocTypePublicIdentifierQuoted;
					doctype.PublicIdentifier = string.Empty;
					quote = c;
					return false;
				case '>': // parse error
					TokenizerState = HtmlTokenizerState.Data;
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				default: // parse error
					TokenizerState = HtmlTokenizerState.BogusDocType;
					doctype.ForceQuirksMode = true;
					return false;
				}
			} while (true);
		}

		bool ReadDocTypePublicIdentifierQuoted (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.PublicIdentifier = name.ToString ();
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					name.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\0': // parse error
					name.Append ('\uFFFD');
					break;
				case '>': // parse error
					TokenizerState = HtmlTokenizerState.Data;
					doctype.PublicIdentifier = name.ToString ();
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					name.Clear ();
					return true;
				default:
					if (c == quote) {
						TokenizerState = HtmlTokenizerState.AfterDocTypePublicIdentifier;
						break;
					}

					name.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.DocTypePublicIdentifierQuoted);

			doctype.PublicIdentifier = name.ToString ();
			name.Clear ();

			return false;
		}

		public bool ReadAfterDocTypePublicIdentifier (out HtmlToken token)
		{
			int nc = text.Read ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				doctype.ForceQuirksMode = true;
				token = doctype;
				doctype = null;
				data.Clear ();
				return true;
			}

			c = (char) nc;

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append (c);

			switch (c) {
			case '\t': case '\n': case '\f': case ' ':
				TokenizerState = HtmlTokenizerState.BetweenDocTypePublicAndSystemIdentifiers;
				break;
			case '>':
				TokenizerState = HtmlTokenizerState.Data;
				token = doctype;
				doctype = null;
				data.Clear ();
				return true;
			case '"': case '\'': // parse error
				TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
				doctype.SystemIdentifier = string.Empty;
				quote = c;
				break;
			default: // parse error
				TokenizerState = HtmlTokenizerState.BogusDocType;
				doctype.ForceQuirksMode = true;
				break;
			}

			token = null;

			return false;
		}

		bool ReadBetweenDocTypePublicAndSystemIdentifiers (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				case '"': case '\'':
					TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
					doctype.SystemIdentifier = string.Empty;
					quote = c;
					return false;
				default: // parse error
					TokenizerState = HtmlTokenizerState.BogusDocType;
					doctype.ForceQuirksMode = true;
					return false;
				}
			} while (true);
		}

		bool ReadAfterDocTypeSystemKeyword (out HtmlToken token)
		{
			int nc = text.Read ();
			char c;

			if (nc == -1) {
				TokenizerState = HtmlTokenizerState.EndOfFile;
				doctype.ForceQuirksMode = true;
				token = doctype;
				doctype = null;
				data.Clear ();
				return true;
			}

			c = (char) nc;

			// Note: we save the data in case we hit a parse error and have to emit a data token
			data.Append (c);

			switch (c) {
			case '\t': case '\n': case '\f': case ' ':
				TokenizerState = HtmlTokenizerState.BeforeDocTypeSystemIdentifier;
				break;
			case '"': case '\'': // parse error
				TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
				doctype.SystemIdentifier = string.Empty;
				quote = c;
				break;
			case '>': // parse error
				TokenizerState = HtmlTokenizerState.Data;
				doctype.ForceQuirksMode = true;
				token = doctype;
				doctype = null;
				data.Clear ();
				return true;
			default: // parse error
				TokenizerState = HtmlTokenizerState.BogusDocType;
				doctype.ForceQuirksMode = true;
				break;
			}

			token = null;

			return false;
		}

		bool ReadBeforeDocTypeSystemIdentifier (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					break;
				case '"': case '\'':
					TokenizerState = HtmlTokenizerState.DocTypeSystemIdentifierQuoted;
					doctype.SystemIdentifier = string.Empty;
					quote = c;
					return false;
				case '>': // parse error
					TokenizerState = HtmlTokenizerState.Data;
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				default: // parse error
					TokenizerState = HtmlTokenizerState.BogusDocType;
					doctype.ForceQuirksMode = true;
					return false;
				}
			} while (true);
		}

		bool ReadDocTypeSystemIdentifierQuoted (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.SystemIdentifier = name.ToString ();
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					name.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\0': // parse error
					name.Append ('\uFFFD');
					break;
				case '>': // parse error
					TokenizerState = HtmlTokenizerState.Data;
					doctype.SystemIdentifier = name.ToString ();
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					name.Clear ();
					return true;
				default:
					if (c == quote) {
						TokenizerState = HtmlTokenizerState.AfterDocTypeSystemIdentifier;
						break;
					}

					name.Append (c);
					break;
				}
			} while (TokenizerState == HtmlTokenizerState.DocTypeSystemIdentifierQuoted);

			doctype.SystemIdentifier = name.ToString ();
			name.Clear ();

			return false;
		}

		public bool ReadAfterDocTypeSystemIdentifier (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				switch (c) {
				case '\t': case '\n': case '\f': case ' ':
					break;
				case '>':
					TokenizerState = HtmlTokenizerState.Data;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				default: // parse error
					TokenizerState = HtmlTokenizerState.BogusDocType;
					return false;
				}
			} while (true);
		}

		bool ReadBogusDocType (out HtmlToken token)
		{
			token = null;

			do {
				int nc = text.Read ();
				char c;

				if (nc == -1) {
					TokenizerState = HtmlTokenizerState.EndOfFile;
					doctype.ForceQuirksMode = true;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				}

				c = (char) nc;

				// Note: we save the data in case we hit a parse error and have to emit a data token
				data.Append (c);

				if (c == '>') {
					TokenizerState = HtmlTokenizerState.Data;
					token = doctype;
					doctype = null;
					data.Clear ();
					return true;
				}
			} while (true);
		}

		public bool ReadNextToken (out HtmlToken token)
		{
			do {
				switch (TokenizerState) {
				case HtmlTokenizerState.Data:
					if (ReadDataToken (out token))
						return true;
					break;
				case HtmlTokenizerState.CharacterReferenceInData:
					if (ReadCharacterReferenceInData (out token))
						return true;
					break;
				case HtmlTokenizerState.RcData:
					break;
				case HtmlTokenizerState.CharacterReferenceInRcData:
					break;
				case HtmlTokenizerState.RawText:
					break;
				case HtmlTokenizerState.ScriptData:
					if (ReadScriptData (out token))
						return true;
					break;
				case HtmlTokenizerState.PlainText:
					break;
				case HtmlTokenizerState.TagOpen:
					if (ReadTagOpen (out token))
						return true;
					break;
				case HtmlTokenizerState.EndTagOpen:
					if (ReadEndTagOpen (out token))
						return true;
					break;
				case HtmlTokenizerState.TagName:
					if (ReadTagName (out token))
						return true;
					break;
				case HtmlTokenizerState.RcDataLessThan:
					break;
				case HtmlTokenizerState.RcDataEndTagOpen:
					break;
				case HtmlTokenizerState.RcDataEndTagName:
					break;
				case HtmlTokenizerState.RawTextLessThan:
					break;
				case HtmlTokenizerState.RawTextEndTagOpen:
					break;
				case HtmlTokenizerState.RawTextEndTagName:
					break;
				case HtmlTokenizerState.ScriptDataLessThan:
					if (ReadScriptDataLessThan (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataEndTagOpen:
					if (ReadScriptDataEndTagOpen (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataEndTagName:
					if (ReadScriptDataEndTagName (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataEscapeStart:
					if (ReadScriptDataEscapeStart (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataEscapeStartDash:
					if (ReadScriptDataEscapeStartDash (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataEscaped:
					if (ReadScriptDataEscaped (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataEscapedDash:
					if (ReadScriptDataEscapedDash (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataEscapedDashDash:
					if (ReadScriptDataEscapedDashDash (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataEscapedLessThan:
					if (ReadScriptDataEscapedLessThan (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataEscapedEndTagOpen:
					if (ReadScriptDataEscapedEndTagOpen (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataEscapedEndTagName:
					if (ReadScriptDataEscapedEndTagName (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscapeStart:
					if (ReadScriptDataDoubleEscapeStart (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscaped:
					if (ReadScriptDataDoubleEscaped (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscapedDash:
					if (ReadScriptDataDoubleEscapedDash (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscapedDashDash:
					if (ReadScriptDataDoubleEscapedDashDash (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscapedLessThan:
					if (ReadScriptDataDoubleEscapedLessThan (out token))
						return true;
					break;
				case HtmlTokenizerState.ScriptDataDoubleEscapeEnd:
					if (ReadScriptDataDoubleEscapeEnd (out token))
						return true;
					break;
				case HtmlTokenizerState.BeforeAttributeName:
					if (ReadBeforeAttributeName (out token))
						return true;
					break;
				case HtmlTokenizerState.AttributeName:
					if (ReadAttributeName (out token))
						return true;
					break;
				case HtmlTokenizerState.AfterAttributeName:
					if (ReadAfterAttributeName (out token))
						return true;
					break;
				case HtmlTokenizerState.BeforeAttributeValue:
					if (ReadBeforeAttributeValue (out token))
						return true;
					break;
				case HtmlTokenizerState.AttributeValueQuoted:
					if (ReadAttributeValueQuoted (out token))
						return true;
					break;
				case HtmlTokenizerState.AttributeValueUnquoted:
					if (ReadAttributeValueUnquoted (out token))
						return true;
					break;
				case HtmlTokenizerState.CharacterReferenceInAttributeValue:
					if (ReadCharacterReferenceInAttributeValue (out token))
						return true;
					break;
				case HtmlTokenizerState.AfterAttributeValueQuoted:
					if (ReadAfterAttributeValueQuoted (out token))
						return true;
					break;
				case HtmlTokenizerState.SelfClosingStartTag:
					if (ReadSelfClosingStartTag (out token))
						return true;
					break;
				case HtmlTokenizerState.BogusComment:
					if (ReadBogusComment (out token))
						return true;
					break;
				case HtmlTokenizerState.MarkupDeclarationOpen:
					if (ReadMarkupDeclarationOpen (out token))
						return true;
					break;
				case HtmlTokenizerState.CommentStart:
					if (ReadCommentStart (out token))
						return true;
					break;
				case HtmlTokenizerState.CommentStartDash:
					if (ReadCommentStartDash (out token))
						return true;
					break;
				case HtmlTokenizerState.Comment:
					if (ReadComment (out token))
						return true;
					break;
				case HtmlTokenizerState.CommentEndDash:
					if (ReadCommentEndDash (out token))
						return true;
					break;
				case HtmlTokenizerState.CommentEnd:
					if (ReadCommentEnd (out token))
						return true;
					break;
				case HtmlTokenizerState.CommentEndBang:
					if (ReadCommentEndBang (out token))
						return true;
					break;
				case HtmlTokenizerState.DocType:
					if (ReadDocType (out token))
						return true;
					break;
				case HtmlTokenizerState.BeforeDocTypeName:
					if (ReadBeforeDocTypeName (out token))
						return true;
					break;
				case HtmlTokenizerState.DocTypeName:
					if (ReadDocTypeName (out token))
						return true;
					break;
				case HtmlTokenizerState.AfterDocTypeName:
					if (ReadAfterDocTypeName (out token))
						return true;
					break;
				case HtmlTokenizerState.AfterDocTypePublicKeyword:
					if (ReadAfterDocTypePublicKeyword (out token))
						return true;
					break;
				case HtmlTokenizerState.BeforeDocTypePublicIdentifier:
					if (ReadBeforeDocTypePublicIdentifier (out token))
						return true;
					break;
				case HtmlTokenizerState.DocTypePublicIdentifierQuoted:
					if (ReadDocTypePublicIdentifierQuoted (out token))
						return true;
					break;
				case HtmlTokenizerState.AfterDocTypePublicIdentifier:
					if (ReadAfterDocTypePublicIdentifier (out token))
						return true;
					break;
				case HtmlTokenizerState.BetweenDocTypePublicAndSystemIdentifiers:
					if (ReadBetweenDocTypePublicAndSystemIdentifiers (out token))
						return true;
					break;
				case HtmlTokenizerState.AfterDocTypeSystemKeyword:
					if (ReadAfterDocTypeSystemKeyword (out token))
						return true;
					break;
				case HtmlTokenizerState.BeforeDocTypeSystemIdentifier:
					if (ReadBeforeDocTypeSystemIdentifier (out token))
						return true;
					break;
				case HtmlTokenizerState.DocTypeSystemIdentifierQuoted:
					if (ReadDocTypeSystemIdentifierQuoted (out token))
						return true;
					break;
				case HtmlTokenizerState.AfterDocTypeSystemIdentifier:
					if (ReadAfterDocTypeSystemIdentifier (out token))
						return true;
					break;
				case HtmlTokenizerState.BogusDocType:
					if (ReadBogusDocType (out token))
						return true;
					break;
				case HtmlTokenizerState.CDataSection:
					// TODO
					break;
				case HtmlTokenizerState.EndOfFile:
					token = null;
					return false;
				}
			} while (true);
		}
	}
}
