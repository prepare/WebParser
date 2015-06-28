﻿//BSD  2015,2014 ,WinterDev

using System;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.WebLexer;

namespace LayoutFarm.WebDom.Parser
{
    public enum HtmlLexerEvent
    {
        VisitOpenAngle,        //  <a
        VisitOpenSlashAngle,   //  </a
        VisitCloseAngle,       //  a>
        VisitCloseSlashAngle,  //  />        
        VisitAttrAssign,      //=

        VisitOpenAngleExclimation, //<! eg. document node <!doctype

        OpenComment,           //  <!--
        CloseComment,          //  -->

        OpenProcessInstruction,  //  <?
        CloseProcessInstruction, //  ?>

        NodeNameOrAttribute,
        NodeNamePrefix,
        NodeNameLocal,
        Attribute,
        AttributeNameLocal,
        AttributeNamePrefix,
        AttributeValueAsLiteralString,

        SwitchToContentPart,
        FromContentPart,
        CommentContent
    }

    enum HtmlLexState
    {
        Init,
        AfterOpenAngle


    }

    public delegate void HtmlLexerEventHandler(HtmlLexerEvent lexEvent, int startIndex, int len);

    public enum LexerEngine
    {
        MyLexer,
        HtmlKit
    }

    public abstract partial class HtmlLexer
    {
        public event HtmlLexerEventHandler LexStateChanged;

        protected void RaiseStateChanged(HtmlLexerEvent lexEvent, int startIndex, int len)
        {
            LexStateChanged(lexEvent, startIndex, len);
        }
        public virtual void Analyze(TextSnapshot textSnapshot) { }
        public virtual void BeginLex()
        {

        }
        public virtual void EndLex()
        {

        }
        public static HtmlLexer CreateLexer(LexerEngine lexEngine)
        {   
            switch (lexEngine)
            {
                case LexerEngine.HtmlKit:
                    return new MyHtmlLexer();
                default:
                    return new MyHtmlLexer();
            }
        }
    }



}
