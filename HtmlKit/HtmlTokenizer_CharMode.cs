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

            Gt, // >
            Lt, // <
            Bang,// !
            Quest,//? 
            Slash, //  /
            Assign, // =


            Eof
        }
    }

}