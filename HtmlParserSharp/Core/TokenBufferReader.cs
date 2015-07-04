using System;
using System.Text;
using System.Collections.Generic;

using System.Diagnostics;
using HtmlParserSharp.Common;
namespace HtmlParserSharp.Core
{

    class TokenBufferReader
    {
        int index = -1;
        int totalLength = 0;
        char[] buffer;
        int lineNumber;
        int columnNumber;

        public TokenBufferReader(char[] buffer)
        {
            this.buffer = buffer;
            this.totalLength = buffer.Length;
            this.index = -1;

            this.columnNumber = 1;//init at column 1
            this.lineNumber = 1; //init at line1
        }
        public int CurrentIndex
        {
            get { return this.index; }
        }
        public bool ReadNext(out char c)
        {
            //1. move 
            if (index < totalLength)
            {
                c = buffer[++index];
                switch (c)
                {
                    case '\n':
                        //reset 
                        this.lineNumber++;
                        this.columnNumber = 1;//reset
                        break;
                    default:
                        this.columnNumber++;
                        break;
                }
                return true;
            }
            else
            {
                c = '\0';
                return false;
            }
        }
        /// <summary>
        /// read next and analyze char group
        /// </summary>
        /// <param name="c"></param>
        /// <param name="charMode"></param>
        /// <returns></returns>
        public bool ReadNext(out char c, out CharMode charMode)
        {
            //1. move 

            if (index < totalLength)
            {
                c = buffer[++index];
                switch (c)
                {
                    case '\n':

                        //reset 
                        charMode = CharMode.NewLine;
                        this.lineNumber++;
                        this.columnNumber = 1;//reset
                        return true;
                    case '\t':
                    case ' ':
                    case '\r':
                    case '\f':
                        this.columnNumber++;
                        charMode = CharMode.WhiteSpace;
                        return true;
                    case '!':
                        columnNumber++;
                        charMode = CharMode.Bang;
                        return true;
                    case '/':
                        columnNumber++;
                        charMode = CharMode.Slash;
                        return true;
                    case '?':
                        columnNumber++;
                        charMode = CharMode.Quest;
                        return true;
                    case '#':
                        columnNumber++;
                        charMode = CharMode.Sharp;
                        return true;
                    case '&':
                        columnNumber++;
                        charMode = CharMode.Ampersand;
                        return true;
                    case '"':
                        columnNumber++;
                        charMode = CharMode.DoubleQuote;
                        return true;
                    case '\'':
                        columnNumber++;
                        charMode = CharMode.Quote;
                        return true;
                    case '=':
                        columnNumber++;
                        charMode = CharMode.Assign;
                        return true;
                    case '>':
                        columnNumber++;
                        charMode = CharMode.Gt;
                        return true;
                    case '<':
                        columnNumber++;
                        charMode = CharMode.Lt;
                        return true;
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                        charMode = CharMode.LowerAsciiLetter;
                        columnNumber++;
                        return true;
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                        charMode = CharMode.UpperAsciiLetter;
                        columnNumber++;
                        return true;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        charMode = CharMode.Numeric;
                        columnNumber++;
                        return true;
                    default:
                        charMode = CharMode.Others;
                        columnNumber++;
                        return true;
                }
            }
            else
            {
                c = '\0';
                charMode = CharMode.Others;
                return false;
            }
        }
        public bool Peek(out char c)
        {
            if (index < totalLength)
            {
                c = buffer[index + 1];
                return true;
            }
            else
            {
                c = '\0';
                return false;
            }
        }
        public void StepBack()
        {
            this.index--;
        }
        public void StartCollect()
        {
            //this.cstart = this.index
        }
        public void SkipOneAndStartCollect()
        {
            //this.cstart = this.index
        }
    }

}