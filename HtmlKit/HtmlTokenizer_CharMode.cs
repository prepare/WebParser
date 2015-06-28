//MIT 2015,WinterDev

namespace HtmlKit
{
    partial class HtmlTokenizer
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


            Eof
        }
    }

}