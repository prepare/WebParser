
using System.Text;
namespace HtmlKit
{

    partial class HtmlTokenizer
    {
        HtmlToken CreateCommentToken(string comment)
        {
            return tokenFactory.CreateCommentToken(comment);
        }
        HtmlToken CreateCommentToken(StringBuilder stbuilder)
        {
            return tokenFactory.CreateCommentToken(stbuilder.ToString());
        }
        HtmlToken CreateDataToken(string data)
        {
            return tokenFactory.CreateHtmlDataToken(data);
        }
        HtmlToken CreateDataToken(StringBuilder stbuilder)
        {
            return tokenFactory.CreateHtmlDataToken(stbuilder.ToString());
        }
        HtmlDocTypeToken CreateDocTypeToken(string name)
        {
            return (HtmlDocTypeToken)tokenFactory.CreateHtmlDocTypeToken(name);
        }
        HtmlDocTypeToken CreateDocTypeToken(StringBuilder stbuilder)
        {
            return (HtmlDocTypeToken)tokenFactory.CreateHtmlDocTypeToken(stbuilder.ToString());
        }
        HtmlTagToken CreateTagToken(string name, bool isEndTag)
        {
            return (HtmlTagToken)tokenFactory.CreateHtmlTagToken(name, isEndTag);
        }
        HtmlTagToken CreateTagToken(StringBuilder stbuilder, bool isEndTag)
        {
            return (HtmlTagToken)tokenFactory.CreateHtmlTagToken(stbuilder.ToString(), isEndTag);
        }
    }
}