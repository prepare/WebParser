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

 
using System.Text;

namespace HtmlKit
{
 
    partial class HtmlTokenizer
    {

        /// <summary>
        /// Create a DOCTYPE token.
        /// </summary>
        /// <remarks>
        /// Creates a DOCTYPE token.
        /// </remarks>
        /// <returns>The DOCTYPE token.</returns>
        protected virtual HtmlDocTypeToken CreateDocType()
        {
            return new HtmlDocTypeToken();
        }

        HtmlDocTypeToken CreateDocTypeToken(string rawTagName)
        {
            var token = CreateDocType();
            token.RawTagName = rawTagName;
            return token;
        }

        /// <summary>
        /// Create an HTML comment token.
        /// </summary>
        /// <remarks>
        /// Creates an HTML comment token.
        /// </remarks>
        /// <returns>The HTML comment token.</returns>
        /// <param name="comment">The comment.</param>
        protected virtual HtmlCommentToken CreateCommentToken(string comment)
        {
            return new HtmlCommentToken(comment);
        }

        /// <summary>
        /// Create an HTML character data token.
        /// </summary>
        /// <remarks>
        /// Creates an HTML character data token.
        /// </remarks>
        /// <returns>The HTML character data token.</returns>
        /// <param name="data">The character data.</param>
        protected virtual HtmlDataToken CreateDataToken(string data)
        {
            return new HtmlDataToken(data);
        }
        /// <summary>
        /// Create an HTML character data token.
        /// </summary>
        /// <remarks>
        /// Creates an HTML character data token.
        /// </remarks>
        /// <returns>The HTML character data token.</returns>
        /// <param name="data">The character data.</param>
        protected virtual HtmlCDataToken CreateCDataToken(string data)
        {
            return new HtmlCDataToken(data);
        }

        /// <summary>
        /// Create an HTML script data token.
        /// </summary>
        /// <remarks>
        /// Creates an HTML script data token.
        /// </remarks>
        /// <returns>The HTML script data token.</returns>
        /// <param name="data">The script data.</param>
        protected virtual HtmlScriptDataToken CreateScriptDataToken(string data)
        {
            return new HtmlScriptDataToken(data);
        }
        /// <summary>
        /// Create an HTML tag token.
        /// </summary>
        /// <remarks>
        /// Creates an HTML tag token.
        /// </remarks>
        /// <returns>The HTML tag token.</returns>
        /// <param name="name">The tag name.</param>
        /// <param name="isEndTag"><c>true</c> if the tag is an end tag; otherwise, <c>false</c>.</param>
        protected virtual HtmlTagToken CreateTagToken(string name, bool isEndTag = false)
        {
            return new HtmlTagToken(name, isEndTag);
        }

        /// <summary>
        /// Create an attribute.
        /// </summary>
        /// <remarks>
        /// Creates an attribute.
        /// </remarks>
        /// <returns>The attribute.</returns>
        /// <param name="name">THe attribute name.</param>
        protected virtual HtmlAttribute CreateAttribute(string name)
        {
            return new HtmlAttribute(name);
        }

        HtmlTagToken CreateTagTokenFromNameBuffer(bool isEndTag)
        {
            HtmlTagToken token = CreateTagToken(name.ToString(), isEndTag);
            //each time we create tag token, always clear name ***
            name.Length = 0;
            return token;
        }
        void EmitTagAttribute()
        {
            attribute = CreateAttribute(name.ToString());
            tag.Attributes.Add(attribute);
            name.Length = 0;
        }

        bool EmitCommentToken(string comment)
        {
            SetEmitToken(CreateCommentToken(comment));
            data.Length = 0;
            name.Length = 0;
            return true;
        }

        void EmitCommentToken(StringBuilder comment)
        {
            EmitCommentToken(comment.ToString());
        }
        void EmitDataToken(bool encodeEntities)
        {
            if (data.Length > 0)
            {
                var dataToken = CreateDataToken(data.ToString());
                dataToken.EncodeEntities = encodeEntities;                 
                SetEmitToken(dataToken);
                data.Length = 0; 
            } 
        }
        void EmitCDataToken()
        {
            if (data.Length > 0)
            {
                SetEmitToken(CreateCDataToken(data.ToString()));
                data.Length = 0; 
            } 
        }

        void EmitScriptDataToken()
        {
            if (data.Length > 0)
            {

                SetEmitToken(CreateScriptDataToken(data.ToString()));
                data.Length = 0; 
            } 
        }
        void EmitTagToken()
        {
            if (!tag.IsEndTag && !tag.IsEmptyElement)
            {
                switch (tag.Id)
                {
                    case HtmlTagId.Style:
                    case HtmlTagId.Xmp:
                    case HtmlTagId.IFrame:
                    case HtmlTagId.NoEmbed:
                    case HtmlTagId.NoFrames:
                        TokenizerState = HtmlTokenizerState.RawText;
                        activeTagName = tag.Name;
                        break;
                    case HtmlTagId.Title:
                    case HtmlTagId.TextArea:
                        TokenizerState = HtmlTokenizerState.RcData;
                        activeTagName = tag.Name;
                        break;
                    case HtmlTagId.PlainText:
                        TokenizerState = HtmlTokenizerState.PlainText;
                        break;
                    case HtmlTagId.Script:
                        TokenizerState = HtmlTokenizerState.ScriptData;
                        break;
                    case HtmlTagId.NoScript:
                        // TODO: only switch into the RawText state if scripting is enabled
                        TokenizerState = HtmlTokenizerState.RawText;
                        activeTagName = tag.Name;
                        break;
                    case HtmlTagId.Html:
                        TokenizerState = HtmlTokenizerState.Data;

                        for (int i = tag.Attributes.Count; i > 0; i--)
                        {
                             
                            var attr = tag.Attributes[i - 1];

                            if (attr.Id == HtmlAttributeId.XmlNS && attr.Value != null)
                            {
                                //TODO: and here i-1?
                                HtmlNamespace = tag.Attributes[i].Value.ToHtmlNamespace();
                                break;
                            }
                        }
                        break;
                    default:
                        TokenizerState = HtmlTokenizerState.Data;
                        break;
                }
            }
            else
            {
                TokenizerState = HtmlTokenizerState.Data;
            }
                         
            SetEmitToken(tag);
            data.Length = 0;
            tag = null;
        }

        

    }
}