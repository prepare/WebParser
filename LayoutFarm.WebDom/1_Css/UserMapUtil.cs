﻿//BSD, 2014 WinterDev
using System;
using System.Collections.Generic;

using LayoutFarm.Css;
using LayoutFarm.HtmlBoxes;

namespace LayoutFarm.WebDom
{
    public enum WellKnownDomNodeName : byte
    {
        NotAssign, //extension , for anonymous element
        Unknown,
        //---------------- 
        [Map("html")]
        html,
        [Map("a")]
        a,
        [Map("area")]
        area,
        [Map("hr")]
        hr,
        [Map("br")]
        br,
        [Map("body")]
        body,
        [Map("style")]
        style,
        [Map("script")]
        script,
        [Map("img")]
        img,
        [Map("input")]
        input,
        [Map("canvas")]
        canvas,
        [Map("div")]
        div,
        [Map("span")]
        span,

        [Map("link")]
        link,
        [Map("p")]
        p,

        //----------------------------------
        [Map("table")]
        table,

        [Map("tr")]
        tr,//table-row

        [Map("tbody")]
        tbody,//table-row-group

        [Map("thead")]
        thead, //table-header-group
        //from css2.1 spec:
        //thead: like 'table-row-group' ,but for visual formatting.
        //the row group is always displayed before all other rows and row groups and
        //after any top captions...

        [Map("tfoot")]
        tfoot, //table-footer-group
        //css2.1: like 'table-row-group',but for visual formatting

        [Map("col")]
        col,//table-column, specifics that an element describes a column of cells

        [Map("colgroup")]
        colgroup,//table-column-group, specific that an element groups one or more columns;

        [Map("template")]
        template, //html5 template
        [Map("td")]
        td,//table-cell                
        [Map("th")]
        th,//table-cell

        [Map("caption")]
        caption,//table-caption element
        //----------------------------------------


        [Map("iframe")]
        iframe,


        //----------------------------------------
        [FeatureDeprecated("not support in Html5")]
        [Map("frame")]
        frame,
        [FeatureDeprecated("not support in Html5,Use Css instead")]
        [Map("font")]
        font,
        [FeatureDeprecated("not support in Html5,Use Css instead")]
        [Map("basefont")]
        basefont,

        [Map("base")]
        _base,

        [Map("meta")]
        meta,
        [Map("param")]
        _param,


        [Map("svg")]
        svg,
        [Map("rect")]
        svg_rect,
        [Map("circle")]
        svg_circle,
        [Map("ellipse")]
        svg_ellipse,
        [Map("polygon")]
        svg_polygon,
        [Map("polyline")]
        svg_polyline,

        [Map("defs")]
        svg_defs,
        [Map("linearGradient")]
        svg_linearGradient,
        [Map("stop")]
        svg_stop,

        [Map("path")]
        svg_path,
        [Map("image")]
        svg_image,
        [Map("g")]
        svg_g
    }


    public static class UserMapUtil
    {

        static readonly ValueMap<CssDisplay> _cssDisplayMap = new ValueMap<CssDisplay>();
        static readonly ValueMap<CssDirection> _cssDirectionMap = new ValueMap<CssDirection>();
        static readonly ValueMap<CssPosition> _cssPositionMap = new ValueMap<CssPosition>();
        static readonly ValueMap<CssWordBreak> _cssWordBreakMap = new ValueMap<CssWordBreak>();
        static readonly ValueMap<CssTextDecoration> _cssTextDecorationMap = new ValueMap<CssTextDecoration>();
        static readonly ValueMap<CssOverflow> _cssOverFlowMap = new ValueMap<CssOverflow>();
        static readonly ValueMap<CssTextAlign> _cssTextAlignMap = new ValueMap<CssTextAlign>();
        static readonly ValueMap<CssVerticalAlign> _cssVerticalAlignMap = new ValueMap<CssVerticalAlign>();
        static readonly ValueMap<CssVisibility> _cssVisibilityMap = new ValueMap<CssVisibility>();
        static readonly ValueMap<CssWhiteSpace> _cssWhitespaceMap = new ValueMap<CssWhiteSpace>();
        static readonly ValueMap<CssBorderCollapse> _cssCollapseBorderMap = new ValueMap<CssBorderCollapse>();
        static readonly ValueMap<CssBorderStyle> _cssBorderStyleMap = new ValueMap<CssBorderStyle>();
        static readonly ValueMap<CssEmptyCell> _cssEmptyCellMap = new ValueMap<CssEmptyCell>();
        static readonly ValueMap<CssFloat> _cssFloatMap = new ValueMap<CssFloat>();
        static readonly ValueMap<CssFontStyle> _cssFontStyleMap = new ValueMap<CssFontStyle>();
        static readonly ValueMap<CssFontVariant> _cssFontVariantMap = new ValueMap<CssFontVariant>();
        static readonly ValueMap<CssFontWeight> _cssFontWeightMap = new ValueMap<CssFontWeight>();
        static readonly ValueMap<CssListStylePosition> _cssListStylePositionMap = new ValueMap<CssListStylePosition>();
        static readonly ValueMap<CssListStyleType> _cssListStyleTypeMap = new ValueMap<CssListStyleType>();

        static readonly ValueMap<CssNamedBorderWidth> _cssNamedBorderWidthMap = new ValueMap<CssNamedBorderWidth>();
        static readonly ValueMap<CssBackgroundRepeat> _cssBackgroundRepeatMap = new ValueMap<CssBackgroundRepeat>();



        static readonly ValueMap<LayoutFarm.WebDom.WellknownCssPropertyName> _wellKnownCssPropNameMap = new ValueMap<WebDom.WellknownCssPropertyName>();
        static readonly ValueMap<WellKnownDomNodeName> _wellknownHtmlTagNameMap = new ValueMap<WellKnownDomNodeName>();

        static readonly ValueMap<CssBoxSizing> _cssBoxSizingMap = new ValueMap<CssBoxSizing>();



        static UserMapUtil()
        {

        }

        //-----------------------
        public static bool IsBorderStyle(string value)
        {
            return _cssBorderStyleMap.GetValueFromString(value, CssBorderStyle.Unknown) != CssBorderStyle.Unknown;
        }
        //thin,medium,thick border width
        public static bool IsNamedBorderWidth(string value)
        {
            return _cssNamedBorderWidthMap.GetValueFromString(value, CssNamedBorderWidth.Unknown) != CssNamedBorderWidth.Unknown;
        }
        public static bool IsFontVariant(string value)
        {
            return _cssFontVariantMap.GetValueFromString(value, CssFontVariant.Unknown) != CssFontVariant.Unknown;
        }
        public static bool IsFontStyle(string value)
        {
            return _cssFontStyleMap.GetValueFromString(value, CssFontStyle.Unknown) != CssFontStyle.Unknown;
        }
        public static bool IsFontWeight(string value)
        {
            return _cssFontWeightMap.GetValueFromString(value, CssFontWeight.Unknown) != CssFontWeight.Unknown;
        }

        //-----------------------

        public static CssBorderCollapse GetBorderCollapse(WebDom.CssCodeValueExpression value)
        {
            return (CssBorderCollapse)EvaluateIntPropertyValueFromString(
               _cssCollapseBorderMap,
               WebDom.CssValueEvaluatedAs.BorderCollapse,
               CssBorderCollapse.Separate,
               value);
        }

        public static string ToCssStringValue(this CssDisplay value)
        {
            return _cssDisplayMap.GetStringFromValue(value);
        }
        //-----------------------
        static int EvaluateIntPropertyValueFromString<T>(ValueMap<T> map,
            WebDom.CssValueEvaluatedAs
            evalAs,
            T defaultValue,
            WebDom.CssCodeValueExpression value)
            where T : struct
        {
            if (value.EvaluatedAs != evalAs)
            {
                T knownValue = map.GetValueFromString(value.GetTranslatedStringValue(), defaultValue);
                int result = Convert.ToInt32(knownValue);
                value.SetIntValue(result, evalAs);
                return (int)result;
            }
            else
            {
                return value.GetCacheIntValue();
            }
        }
        //-----------------------
        public static CssDisplay GetDisplayType(WebDom.CssCodeValueExpression value)
        {
            return (CssDisplay)EvaluateIntPropertyValueFromString(
                _cssDisplayMap,
                WebDom.CssValueEvaluatedAs.Display,
                CssDisplay.Inline,
                value);
        }
        public static CssBackgroundRepeat GetBackgroundRepeat(WebDom.CssCodeValueExpression value)
        {
            return (CssBackgroundRepeat)EvaluateIntPropertyValueFromString(
                _cssBackgroundRepeatMap,
                WebDom.CssValueEvaluatedAs.BackgroundRepeat,
                CssBackgroundRepeat.Repeat,
                value);
        }
        //----------------------
        public static string ToCssStringValue(this CssDirection value)
        {
            return _cssDirectionMap.GetStringFromValue(value);
        }
        public static CssDirection GetCssDirection(WebDom.CssCodeValueExpression value)
        {
            return (CssDirection)EvaluateIntPropertyValueFromString(
             _cssDirectionMap,
             WebDom.CssValueEvaluatedAs.Direction,
             CssDirection.Ltl,
             value);
        }
        //----------------------
        public static CssPosition GetCssPosition(WebDom.CssCodeValueExpression value)
        {
            return (CssPosition)EvaluateIntPropertyValueFromString(
             _cssPositionMap,
             WebDom.CssValueEvaluatedAs.Position,
             CssPosition.Static,
             value);

        }
        public static CssWordBreak GetWordBreak(WebDom.CssCodeValueExpression value)
        {

            return (CssWordBreak)EvaluateIntPropertyValueFromString(
             _cssWordBreakMap,
             WebDom.CssValueEvaluatedAs.WordBreak,
             CssWordBreak.Normal,
             value);
        }
        public static CssTextDecoration GetTextDecoration(WebDom.CssCodeValueExpression value)
        {
            return (CssTextDecoration)EvaluateIntPropertyValueFromString(
                _cssTextDecorationMap,
                WebDom.CssValueEvaluatedAs.TextDecoration,
                CssTextDecoration.NotAssign,
                value);

        }
        public static CssOverflow GetOverflow(WebDom.CssCodeValueExpression value)
        {
            return (CssOverflow)EvaluateIntPropertyValueFromString(
               _cssOverFlowMap,
               WebDom.CssValueEvaluatedAs.Overflow,
               CssOverflow.Visible,
               value);
        }
        public static CssTextAlign GetTextAlign(WebDom.CssCodeValueExpression value)
        {
            return (CssTextAlign)EvaluateIntPropertyValueFromString(
                _cssTextAlignMap,
                WebDom.CssValueEvaluatedAs.TextAlign,
                CssTextAlign.NotAssign,
                value);
        }
        public static CssBoxSizing GetBoxSizing(WebDom.CssCodeValueExpression value)
        {
            return (CssBoxSizing)EvaluateIntPropertyValueFromString(
                _cssBoxSizingMap,
                WebDom.CssValueEvaluatedAs.BoxSizing,
                CssBoxSizing.ContentBox,//default
                value);
        }
        public static CssVerticalAlign GetVerticalAlign(WebDom.CssCodeValueExpression value)
        {
            return (CssVerticalAlign)EvaluateIntPropertyValueFromString(
            _cssVerticalAlignMap,
            WebDom.CssValueEvaluatedAs.VerticalAlign,
            CssVerticalAlign.Baseline,
            value);
        }


        public static CssVisibility GetVisibility(WebDom.CssCodeValueExpression value)
        {
            return (CssVisibility)EvaluateIntPropertyValueFromString(
            _cssVisibilityMap,
            WebDom.CssValueEvaluatedAs.Visibility,
            CssVisibility.Visible,
            value);
        }
        public static CssWhiteSpace GetWhitespace(WebDom.CssCodeValueExpression value)
        {
            return (CssWhiteSpace)EvaluateIntPropertyValueFromString(
            _cssWhitespaceMap,
            WebDom.CssValueEvaluatedAs.WhiteSpace,
            CssWhiteSpace.Normal,
            value);
        }
        public static CssBorderStyle GetBorderStyle(WebDom.CssCodeValueExpression value)
        {
            return (CssBorderStyle)EvaluateIntPropertyValueFromString(
                   _cssBorderStyleMap,
                   WebDom.CssValueEvaluatedAs.BorderStyle,
                   CssBorderStyle.None,
                   value);
        }

        public static CssUnitOrNames GetCssUnit(string u)
        {
            switch (u)
            {
                case CssConstants.Em:

                    return CssUnitOrNames.Ems;
                case CssConstants.Ex:
                    return CssUnitOrNames.Ex;
                case CssConstants.Px:
                    return CssUnitOrNames.Pixels;
                case CssConstants.Mm:
                    return CssUnitOrNames.Milimeters;
                case CssConstants.Cm:
                    return CssUnitOrNames.Centimeters;
                case CssConstants.In:
                    return CssUnitOrNames.Inches;
                case CssConstants.Pt:
                    return CssUnitOrNames.Points;
                case CssConstants.Pc:
                    return CssUnitOrNames.Picas;
                case "%":
                    return CssUnitOrNames.Percent;
                default:
                    return CssUnitOrNames.Unknown;
            }
        }

        public static CssEmptyCell GetEmptyCell(WebDom.CssCodeValueExpression value)
        {

            return (CssEmptyCell)EvaluateIntPropertyValueFromString(
               _cssEmptyCellMap,
               WebDom.CssValueEvaluatedAs.EmptyCell,
               CssEmptyCell.Show, value);

        }

        public static string ToCssStringValue(this CssEmptyCell value)
        {
            return _cssEmptyCellMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssTextAlign value)
        {
            return _cssTextAlignMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssTextDecoration value)
        {
            return _cssTextDecorationMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssWordBreak value)
        {
            return _cssWordBreakMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssWhiteSpace value)
        {
            return _cssWhitespaceMap.GetStringFromValue(value);

        }
        public static string ToCssStringValue(this CssVisibility value)
        {
            return _cssVisibilityMap.GetStringFromValue(value);

        }
        public static string ToCssStringValue(this CssVerticalAlign value)
        {
            return _cssVerticalAlignMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssPosition value)
        {
            return _cssPositionMap.GetStringFromValue(value);
        }
        //public static CssLength SetLineHeight(this CssBox box, CssLength len)
        //{
        //    //2014,2015,
        //    //from www.w3c.org/wiki/Css/Properties/line-height

        //    //line height in <percentage> : 
        //    //The computed value if the property is percentage multiplied by the 
        //    //element's computed font size. 
        //    return CssLength.MakePixelLength(
        //         CssValueParser.ConvertToPx(len, box.GetEmHeight(), box));
        //}

        public static LayoutFarm.WebDom.WellknownCssPropertyName GetWellKnownPropName(string propertyName)
        {
            return _wellKnownCssPropNameMap.GetValueFromString(propertyName, WebDom.WellknownCssPropertyName.Unknown);
        }
        public static CssFloat GetFloat(WebDom.CssCodeValueExpression value)
        {
            return (CssFloat)EvaluateIntPropertyValueFromString(
                _cssFloatMap,
                WebDom.CssValueEvaluatedAs.Float,
                CssFloat.None,
                value);
        }

        public static string ToCssStringValue(this CssFloat cssFloat)
        {
            return _cssFloatMap.GetStringFromValue(cssFloat);
        }
        public static string ToCssStringValue(this CssBorderStyle borderStyle)
        {
            return _cssBorderStyleMap.GetStringFromValue(borderStyle);
        }
        public static string ToCssStringValue(this CssBorderCollapse borderCollapse)
        {
            return _cssCollapseBorderMap.GetStringFromValue(borderCollapse);
        }
        public static string ToCssStringValue(this CssBackgroundRepeat backgrounRepeat)
        {
            return _cssBackgroundRepeatMap.GetStringFromValue(backgrounRepeat);
        }
        public static string ToHexColor(this CssColor color)
        {
            return string.Concat("#", color.R.ToString("X"), color.G.ToString("X"), color.B.ToString("X"));
        }
        public static string ToCssStringValue(this CssFontStyle fontstyle)
        {
            return _cssFontStyleMap.GetStringFromValue(fontstyle);
        }
        public static CssFontStyle GetFontStyle(WebDom.CssCodeValueExpression value)
        {
            return (CssFontStyle)EvaluateIntPropertyValueFromString(
              _cssFontStyleMap,
              WebDom.CssValueEvaluatedAs.FontStyle,
              CssFontStyle.Normal,
              value);

        }
        public static string ToCssStringValue(this CssFontVariant fontVariant)
        {
            return _cssFontVariantMap.GetStringFromValue(fontVariant);
        }
        public static CssFontVariant GetFontVariant(WebDom.CssCodeValueExpression value)
        {
            return (CssFontVariant)EvaluateIntPropertyValueFromString(
              _cssFontVariantMap,
              WebDom.CssValueEvaluatedAs.FontVariant,
              CssFontVariant.Normal,
              value);
        }
        public static string ToFontSizeString(this CssLength length)
        {
            if (length.IsFontSizeName)
            {

                switch (length.UnitOrNames)
                {
                    case CssUnitOrNames.FONTSIZE_MEDIUM:
                        return CssConstants.Medium;
                    case CssUnitOrNames.FONTSIZE_SMALL:
                        return CssConstants.Small;
                    case CssUnitOrNames.FONTSIZE_X_SMALL:
                        return CssConstants.XSmall;
                    case CssUnitOrNames.FONTSIZE_XX_SMALL:
                        return CssConstants.XXSmall;
                    case CssUnitOrNames.FONTSIZE_LARGE:
                        return CssConstants.Large;
                    case CssUnitOrNames.FONTSIZE_X_LARGE:
                        return CssConstants.XLarge;
                    case CssUnitOrNames.FONTSIZE_XX_LARGE:
                        return CssConstants.XXLarge;
                    case CssUnitOrNames.FONTSIZE_LARGER:
                        return CssConstants.Larger;
                    case CssUnitOrNames.FONTSIZE_SMALLER:
                        return CssConstants.Smaller;
                    default:
                        return "";
                }
            }
            else
            {
                return length.ToString();
            }
        }


        public static LayoutFarm.Css.CssLength AsBorderLength(this WebDom.CssCodeValueExpression value)
        {
            if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.BorderLength)
            {
                switch (value.Hint)
                {
                    case WebDom.CssValueHint.Number:
                        {
                            if (value is WebDom.CssCodePrimitiveExpression)
                            {
                                WebDom.CssCodePrimitiveExpression prim = (WebDom.CssCodePrimitiveExpression)value;
                                CssLength len = new CssLength(value.AsNumber(), GetCssUnit(prim.Unit));
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.BorderLength);
                                return len;
                            }
                            else
                            {
                                CssLength len = CssLength.MakePixelLength(value.AsNumber());
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.BorderLength);
                                return len;
                            }
                        }
                    default:
                        {
                            CssLength len = MakeBorderLength(value.ToString());
                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.BorderLength);
                            return len;
                        }
                }
            }
            return value.GetCacheCssLength();
        }


        public static CssLength AsLength(this WebDom.CssCodeValueExpression value)
        {
            if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.Length)
            {
                //length from number 
                switch (value.Hint)
                {
                    case WebDom.CssValueHint.Number:
                        {
                            if (value is WebDom.CssCodePrimitiveExpression)
                            {
                                WebDom.CssCodePrimitiveExpression prim = (WebDom.CssCodePrimitiveExpression)value;
                                CssLength len = new CssLength(value.AsNumber(), GetCssUnit(prim.Unit));
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.Length);
                                return len;
                            }
                            else
                            {
                                CssLength len = CssLength.MakePixelLength(value.AsNumber());
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.Length);
                                return len;
                            }
                        }
                    default:
                        {

                            CssLength len = TranslateLength(value.ToString());
                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.Length);
                            return len;
                        }
                }
            }
            return value.GetCacheCssLength();
        }



        public static CssLength AsTranslatedLength(this WebDom.CssCodeValueExpression value)
        {
            if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.TranslatedLength)
            {

                switch (value.Hint)
                {
                    case WebDom.CssValueHint.Number:
                        {
                            if (value is WebDom.CssCodePrimitiveExpression)
                            {
                                WebDom.CssCodePrimitiveExpression prim = (WebDom.CssCodePrimitiveExpression)value;
                                CssLength len = new CssLength(value.AsNumber(), GetCssUnit(prim.Unit));
                                if (len.HasError)
                                {
                                    len = CssLength.MakePixelLength(0);
                                }
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.TranslatedLength);
                                return len;
                            }
                            else
                            {
                                CssLength len = CssLength.MakePixelLength(value.AsNumber());
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.TranslatedLength);
                                return len;
                            }

                        }
                    default:
                        {
                            CssLength len = TranslateLength(value.ToString());
                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.TranslatedLength);
                            return len;
                        }
                }
            }
            return value.GetCacheCssLength();
        }
        public static CssColor AsColor(WebDom.CssCodeValueExpression value)
        {
            if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.Color)
            {
                if (value is WebDom.CssCodeColor)
                {
                    return ((WebDom.CssCodeColor)value).ActualColor;
                }
                else
                {
                    CssColor actualColor = CssValueParser2.GetActualColor(value.GetTranslatedStringValue());
                    value.SetColorValue(actualColor);
                    return actualColor;
                }
            }
            return value.GetCacheColor();
        }


        public static CssFontWeight GetFontWeight(WebDom.CssCodeValueExpression value)
        {
            return (CssFontWeight)EvaluateIntPropertyValueFromString(
                _cssFontWeightMap,
                WebDom.CssValueEvaluatedAs.Float,
                CssFontWeight.NotAssign,
                value);
        }

        public static string ToCssStringValue(this CssFontWeight weight)
        {
            return _cssFontWeightMap.GetStringFromValue(weight);
        }

        public static string ToCssStringValue(this CssListStylePosition listStylePosition)
        {
            return _cssListStylePositionMap.GetStringFromValue(listStylePosition);
        }
        public static CssListStylePosition GetListStylePosition(WebDom.CssCodeValueExpression value)
        {
            return (CssListStylePosition)EvaluateIntPropertyValueFromString(
               _cssListStylePositionMap,
               WebDom.CssValueEvaluatedAs.ListStylePosition,
               CssListStylePosition.Outside,
               value);

        }

        public static string ToCssStringValue(this CssListStyleType listStyleType)
        {
            return _cssListStyleTypeMap.GetStringFromValue(listStyleType);
        }
        public static CssListStyleType GetListStyleType(WebDom.CssCodeValueExpression value)
        {
            return (CssListStyleType)EvaluateIntPropertyValueFromString(
            _cssListStyleTypeMap,
            WebDom.CssValueEvaluatedAs.ListStyleType,
            CssListStyleType.Disc,
            value);
        }


        public static WellKnownDomNodeName EvaluateTagName(string name)
        {
            return _wellknownHtmlTagNameMap.GetValueFromString(name, WellKnownDomNodeName.Unknown);
        }


        /// <summary>
        /// Converts an HTML length into a Css length
        /// </summary>
        /// <param name="htmlLength"></param>
        /// <returns></returns>
        public static CssLength TranslateLength(string htmlLength)
        {
            CssLength len = ParseGenericLength(htmlLength);
            if (len.HasError)
            {
                return CssLength.MakePixelLength(0);
            }
            return len;
        }
        internal static CssLength MakeBorderLength(string str)
        {
            switch (str)
            {
                case CssConstants.Medium:
                    return CssLength.Medium;
                case CssConstants.Thick:
                    return CssLength.Thick;
                case CssConstants.Thin:
                    return CssLength.Thin;
            }
            return ParseGenericLength(str);
        }

        public static CssLength ParseGenericLength(string lenValue)
        {
            float parsedNumber = 0f;

            switch (lenValue)
            {
                case null:
                case "":
                case "0":
                    //Return zero if no length specified, zero specified
                    return CssLength.ZeroNoUnit;
                case "auto":
                    return CssLength.AutoLength;
                case "normal":
                    return CssLength.NormalWordOrLine;
            }

            //then parse
            //If percentage, use ParseNumber
            if (lenValue.EndsWith("%"))
            {
                parsedNumber = float.Parse(lenValue.Substring(0, lenValue.Length - 1));
                return new CssLength(parsedNumber, CssUnitOrNames.Percent);
            }
            //If no units, has error
            if (lenValue.Length < 3)
            {

                if (float.TryParse(lenValue, out parsedNumber))
                {
                    return new CssLength(parsedNumber, CssUnitOrNames.Pixels);
                }
                else
                {
                    return new CssLength(parsedNumber, CssUnitOrNames.Unknown);
                }

            }
            //Get units of the length
            //TODO: Units behave different in paper and in screen! 
            CssUnitOrNames unit = GetCssUnit(lenValue.Substring(lenValue.Length - 2, 2));
            //parse number part
            if (unit == CssUnitOrNames.Unknown)
            {
                //check if all char is number?

                if (!float.TryParse(lenValue,
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.NumberFormatInfo.InvariantInfo, out parsedNumber))
                {
                    //make an error value
                    return new CssLength(0, CssUnitOrNames.Unknown);
                }
                else
                {
                    return new CssLength(parsedNumber, CssUnitOrNames.Pixels);
                }
            }
            else
            {
                string number_part = lenValue.Substring(0, lenValue.Length - 2);

                if (!float.TryParse(number_part,
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.NumberFormatInfo.InvariantInfo, out parsedNumber))
                {
                    //make an error value
                    return new CssLength(0, CssUnitOrNames.Unknown);
                }
                else
                {
                    return new CssLength(parsedNumber, unit);
                }
            }

        }
    }


    //public static class UserMapUtil
    //{

    //    static readonly ValueMap<CssDisplay> _cssDisplayMap = new ValueMap<CssDisplay>();
    //    static readonly ValueMap<CssDirection> _cssDirectionMap = new ValueMap<CssDirection>();
    //    static readonly ValueMap<CssPosition> _cssPositionMap = new ValueMap<CssPosition>();
    //    static readonly ValueMap<CssWordBreak> _cssWordBreakMap = new ValueMap<CssWordBreak>();
    //    static readonly ValueMap<CssTextDecoration> _cssTextDecorationMap = new ValueMap<CssTextDecoration>();
    //    static readonly ValueMap<CssOverflow> _cssOverFlowMap = new ValueMap<CssOverflow>();
    //    static readonly ValueMap<CssTextAlign> _cssTextAlignMap = new ValueMap<CssTextAlign>();
    //    static readonly ValueMap<CssVerticalAlign> _cssVerticalAlignMap = new ValueMap<CssVerticalAlign>();
    //    static readonly ValueMap<CssVisibility> _cssVisibilityMap = new ValueMap<CssVisibility>();
    //    static readonly ValueMap<CssWhiteSpace> _cssWhitespaceMap = new ValueMap<CssWhiteSpace>();
    //    static readonly ValueMap<CssBorderCollapse> _cssCollapseBorderMap = new ValueMap<CssBorderCollapse>();
    //    static readonly ValueMap<CssBorderStyle> _cssBorderStyleMap = new ValueMap<CssBorderStyle>();
    //    static readonly ValueMap<CssEmptyCell> _cssEmptyCellMap = new ValueMap<CssEmptyCell>();
    //    static readonly ValueMap<CssFloat> _cssFloatMap = new ValueMap<CssFloat>();
    //    static readonly ValueMap<CssFontStyle> _cssFontStyleMap = new ValueMap<CssFontStyle>();
    //    static readonly ValueMap<CssFontVariant> _cssFontVariantMap = new ValueMap<CssFontVariant>();
    //    static readonly ValueMap<CssFontWeight> _cssFontWeightMap = new ValueMap<CssFontWeight>();
    //    static readonly ValueMap<CssListStylePosition> _cssListStylePositionMap = new ValueMap<CssListStylePosition>();
    //    static readonly ValueMap<CssListStyleType> _cssListStyleTypeMap = new ValueMap<CssListStyleType>();

    //    static readonly ValueMap<CssNamedBorderWidth> _cssNamedBorderWidthMap = new ValueMap<CssNamedBorderWidth>();
    //    static readonly ValueMap<CssBackgroundRepeat> _cssBackgroundRepeatMap = new ValueMap<CssBackgroundRepeat>();



    //    static readonly ValueMap<LayoutFarm.WebDom.WellknownCssPropertyName> _wellKnownCssPropNameMap = new ValueMap<WebDom.WellknownCssPropertyName>();
    //    static readonly ValueMap<WellKnownDomNodeName> _wellknownHtmlTagNameMap = new ValueMap<WellKnownDomNodeName>();



    //    static UserMapUtil()
    //    {

    //    }

    //    //-----------------------
    //    public static bool IsBorderStyle(string value)
    //    {
    //        return _cssBorderStyleMap.GetValueFromString(value, CssBorderStyle.Unknown) != CssBorderStyle.Unknown;
    //    }
    //    //thin,medium,thick border width
    //    public static bool IsNamedBorderWidth(string value)
    //    {
    //        return _cssNamedBorderWidthMap.GetValueFromString(value, CssNamedBorderWidth.Unknown) != CssNamedBorderWidth.Unknown;
    //    }
    //    public static bool IsFontVariant(string value)
    //    {
    //        return _cssFontVariantMap.GetValueFromString(value, CssFontVariant.Unknown) != CssFontVariant.Unknown;
    //    }
    //    public static bool IsFontStyle(string value)
    //    {
    //        return _cssFontStyleMap.GetValueFromString(value, CssFontStyle.Unknown) != CssFontStyle.Unknown;
    //    }
    //    public static bool IsFontWeight(string value)
    //    {
    //        return _cssFontWeightMap.GetValueFromString(value, CssFontWeight.Unknown) != CssFontWeight.Unknown;
    //    }

    //    //-----------------------

    //    public static CssBorderCollapse GetBorderCollapse(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssBorderCollapse)EvaluateIntPropertyValueFromString(
    //           _cssCollapseBorderMap,
    //           WebDom.CssValueEvaluatedAs.BorderCollapse,
    //           CssBorderCollapse.Separate,
    //           value);
    //    }

    //    public static string ToCssStringValue(this CssDisplay value)
    //    {
    //        return _cssDisplayMap.GetStringFromValue(value);
    //    }
    //    //-----------------------
    //    static int EvaluateIntPropertyValueFromString<T>(ValueMap<T> map,
    //        WebDom.CssValueEvaluatedAs
    //        evalAs,
    //        T defaultValue,
    //        WebDom.CssCodeValueExpression value)
    //        where T : struct
    //    {
    //        if (value.EvaluatedAs != evalAs)
    //        {
    //            T knownValue = map.GetValueFromString(value.GetTranslatedStringValue(), defaultValue);
    //            int result = Convert.ToInt32(knownValue);
    //            value.SetIntValue(result, evalAs);
    //            return (int)result;
    //        }
    //        else
    //        {
    //            return value.GetCacheIntValue();
    //        }
    //    }
    //    //-----------------------
    //    public static CssDisplay GetDisplayType(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssDisplay)EvaluateIntPropertyValueFromString(
    //            _cssDisplayMap,
    //            WebDom.CssValueEvaluatedAs.Display,
    //            CssDisplay.Inline,
    //            value);
    //    }
    //    public static CssBackgroundRepeat GetBackgroundRepeat(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssBackgroundRepeat)EvaluateIntPropertyValueFromString(
    //            _cssBackgroundRepeatMap,
    //            WebDom.CssValueEvaluatedAs.BackgroundRepeat,
    //            CssBackgroundRepeat.Repeat,
    //            value);
    //    }
    //    //----------------------
    //    public static string ToCssStringValue(this CssDirection value)
    //    {
    //        return _cssDirectionMap.GetStringFromValue(value);
    //    }
    //    public static CssDirection GetCssDirection(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssDirection)EvaluateIntPropertyValueFromString(
    //         _cssDirectionMap,
    //         WebDom.CssValueEvaluatedAs.Direction,
    //         CssDirection.Ltl,
    //         value);
    //    }
    //    //----------------------
    //    public static CssPosition GetCssPosition(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssPosition)EvaluateIntPropertyValueFromString(
    //         _cssPositionMap,
    //         WebDom.CssValueEvaluatedAs.Position,
    //         CssPosition.Static,
    //         value);

    //    }
    //    public static CssWordBreak GetWordBreak(WebDom.CssCodeValueExpression value)
    //    {

    //        return (CssWordBreak)EvaluateIntPropertyValueFromString(
    //         _cssWordBreakMap,
    //         WebDom.CssValueEvaluatedAs.WordBreak,
    //         CssWordBreak.Normal,
    //         value);
    //    }
    //    public static CssTextDecoration GetTextDecoration(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssTextDecoration)EvaluateIntPropertyValueFromString(
    //            _cssTextDecorationMap,
    //            WebDom.CssValueEvaluatedAs.TextDecoration,
    //            CssTextDecoration.NotAssign,
    //            value);

    //    }
    //    public static CssOverflow GetOverflow(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssOverflow)EvaluateIntPropertyValueFromString(
    //           _cssOverFlowMap,
    //           WebDom.CssValueEvaluatedAs.Overflow,
    //           CssOverflow.Visible,
    //           value);
    //    }
    //    public static CssTextAlign GetTextAlign(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssTextAlign)EvaluateIntPropertyValueFromString(
    //            _cssTextAlignMap,
    //            WebDom.CssValueEvaluatedAs.TextAlign,
    //            CssTextAlign.NotAssign,
    //            value);
    //    }

    //    public static CssVerticalAlign GetVerticalAlign(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssVerticalAlign)EvaluateIntPropertyValueFromString(
    //        _cssVerticalAlignMap,
    //        WebDom.CssValueEvaluatedAs.VerticalAlign,
    //        CssVerticalAlign.Baseline,
    //        value);
    //    }


    //    public static CssVisibility GetVisibility(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssVisibility)EvaluateIntPropertyValueFromString(
    //        _cssVisibilityMap,
    //        WebDom.CssValueEvaluatedAs.Visibility,
    //        CssVisibility.Visible,
    //        value);
    //    }
    //    public static CssWhiteSpace GetWhitespace(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssWhiteSpace)EvaluateIntPropertyValueFromString(
    //        _cssWhitespaceMap,
    //        WebDom.CssValueEvaluatedAs.WhiteSpace,
    //        CssWhiteSpace.Normal,
    //        value);
    //    }
    //    public static CssBorderStyle GetBorderStyle(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssBorderStyle)EvaluateIntPropertyValueFromString(
    //               _cssBorderStyleMap,
    //               WebDom.CssValueEvaluatedAs.BorderStyle,
    //               CssBorderStyle.None,
    //               value);
    //    }
    //    //public static void SetBorderSpacing(this CssBox box, WebDom.CssCodeValueExpression value)
    //    //{
    //    //    WebDom.CssCodePrimitiveExpression primValue = value as WebDom.CssCodePrimitiveExpression;
    //    //    if (primValue == null)
    //    //    {
    //    //        //2 values?
    //    //        //box.BorderSpacingHorizontal = new CssLength(r[0].Value);
    //    //        //box.BorderSpacingVertical = new CssLength(r[1].Value);
    //    //        throw new NotSupportedException();
    //    //    }
    //    //    else
    //    //    {
    //    //        //primitive value 
    //    //        box.BorderSpacingHorizontal = box.BorderSpacingVertical = primValue.AsLength();
    //    //    }
    //    //}
    //    //public static void SetBorderSpacing(this BoxSpec box, WebDom.CssCodeValueExpression value)
    //    //{
    //    //    WebDom.CssCodePrimitiveExpression primValue = value as WebDom.CssCodePrimitiveExpression;
    //    //    if (primValue == null)
    //    //    {
    //    //        //2 values?
    //    //        //box.BorderSpacingHorizontal = new CssLength(r[0].Value);
    //    //        //box.BorderSpacingVertical = new CssLength(r[1].Value);
    //    //        throw new NotSupportedException();
    //    //    }
    //    //    else
    //    //    {
    //    //        //primitive value 
    //    //        box.BorderSpacingHorizontal = box.BorderSpacingVertical = primValue.AsLength();
    //    //    }
    //    //}
    //    //public static string GetCornerRadius(this CssBox box)
    //    //{
    //    //    System.Text.StringBuilder stbuilder = new System.Text.StringBuilder();
    //    //    stbuilder.Append(box.CornerNERadius);
    //    //    stbuilder.Append(' ');
    //    //    stbuilder.Append(box.CornerNWRadius);
    //    //    stbuilder.Append(' ');
    //    //    stbuilder.Append(box.CornerSERadius);
    //    //    stbuilder.Append(' ');
    //    //    stbuilder.Append(box.CornerSWRadius);
    //    //    return stbuilder.ToString();
    //    //}
    //    public static CssUnitOrNames GetCssUnit(string u)
    //    {
    //        switch (u)
    //        {
    //            case CssConstants.Em:

    //                return CssUnitOrNames.Ems;
    //            case CssConstants.Ex:
    //                return CssUnitOrNames.Ex;
    //            case CssConstants.Px:
    //                return CssUnitOrNames.Pixels;
    //            case CssConstants.Mm:
    //                return CssUnitOrNames.Milimeters;
    //            case CssConstants.Cm:
    //                return CssUnitOrNames.Centimeters;
    //            case CssConstants.In:
    //                return CssUnitOrNames.Inches;
    //            case CssConstants.Pt:
    //                return CssUnitOrNames.Points;
    //            case CssConstants.Pc:
    //                return CssUnitOrNames.Picas;
    //            default:
    //                return CssUnitOrNames.Unknown;
    //        }
    //    }

    //    public static CssEmptyCell GetEmptyCell(WebDom.CssCodeValueExpression value)
    //    {

    //        return (CssEmptyCell)EvaluateIntPropertyValueFromString(
    //           _cssEmptyCellMap,
    //           WebDom.CssValueEvaluatedAs.EmptyCell,
    //           CssEmptyCell.Show, value);

    //    }

    //    public static string ToCssStringValue(this CssEmptyCell value)
    //    {
    //        return _cssEmptyCellMap.GetStringFromValue(value);
    //    }
    //    public static string ToCssStringValue(this CssTextAlign value)
    //    {
    //        return _cssTextAlignMap.GetStringFromValue(value);
    //    }
    //    public static string ToCssStringValue(this CssTextDecoration value)
    //    {
    //        return _cssTextDecorationMap.GetStringFromValue(value);
    //    }
    //    public static string ToCssStringValue(this CssWordBreak value)
    //    {
    //        return _cssWordBreakMap.GetStringFromValue(value);
    //    }
    //    public static string ToCssStringValue(this CssWhiteSpace value)
    //    {
    //        return _cssWhitespaceMap.GetStringFromValue(value);

    //    }
    //    public static string ToCssStringValue(this CssVisibility value)
    //    {
    //        return _cssVisibilityMap.GetStringFromValue(value);

    //    }
    //    public static string ToCssStringValue(this CssVerticalAlign value)
    //    {
    //        return _cssVerticalAlignMap.GetStringFromValue(value);
    //    }
    //    public static string ToCssStringValue(this CssPosition value)
    //    {
    //        return _cssPositionMap.GetStringFromValue(value);
    //    }
    //    //public static CssLength SetLineHeight(this CssBox box, CssLength len)
    //    //{
    //    //    //2014,2015,
    //    //    //from www.w3c.org/wiki/Css/Properties/line-height

    //    //    //line height in <percentage> : 
    //    //    //The computed value if the property is percentage multiplied by the 
    //    //    //element's computed font size. 
    //    //    return CssLength.MakePixelLength(
    //    //         CssValueParser.ConvertToPx(len, box.GetEmHeight(), box));
    //    //}

    //    public static LayoutFarm.WebDom.WellknownCssPropertyName GetWellKnownPropName(string propertyName)
    //    {
    //        return _wellKnownCssPropNameMap.GetValueFromString(propertyName, WebDom.WellknownCssPropertyName.Unknown);
    //    }
    //    public static CssFloat GetFloat(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssFloat)EvaluateIntPropertyValueFromString(
    //            _cssFloatMap,
    //            WebDom.CssValueEvaluatedAs.Float,
    //            CssFloat.None,
    //            value);
    //    }

    //    public static string ToCssStringValue(this CssFloat cssFloat)
    //    {
    //        return _cssFloatMap.GetStringFromValue(cssFloat);
    //    }
    //    public static string ToCssStringValue(this CssBorderStyle borderStyle)
    //    {
    //        return _cssBorderStyleMap.GetStringFromValue(borderStyle);
    //    }
    //    public static string ToCssStringValue(this CssBorderCollapse borderCollapse)
    //    {
    //        return _cssCollapseBorderMap.GetStringFromValue(borderCollapse);
    //    }
    //    public static string ToCssStringValue(this CssBackgroundRepeat backgrounRepeat)
    //    {
    //        return _cssBackgroundRepeatMap.GetStringFromValue(backgrounRepeat);
    //    }
    //    public static string ToHexColor(this CssColor color)
    //    {
    //        return string.Concat("#", color.R.ToString("X"), color.G.ToString("X"), color.B.ToString("X"));
    //    }
    //    public static string ToCssStringValue(this CssFontStyle fontstyle)
    //    {
    //        return _cssFontStyleMap.GetStringFromValue(fontstyle);
    //    }
    //    public static CssFontStyle GetFontStyle(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssFontStyle)EvaluateIntPropertyValueFromString(
    //          _cssFontStyleMap,
    //          WebDom.CssValueEvaluatedAs.FontStyle,
    //          CssFontStyle.Normal,
    //          value);

    //    }
    //    public static string ToCssStringValue(this CssFontVariant fontVariant)
    //    {
    //        return _cssFontVariantMap.GetStringFromValue(fontVariant);
    //    }
    //    public static CssFontVariant GetFontVariant(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssFontVariant)EvaluateIntPropertyValueFromString(
    //          _cssFontVariantMap,
    //          WebDom.CssValueEvaluatedAs.FontVariant,
    //          CssFontVariant.Normal,
    //          value);
    //    }
    //    public static string ToFontSizeString(this CssLength length)
    //    {
    //        if (length.IsFontSizeName)
    //        {

    //            switch (length.UnitOrNames)
    //            {
    //                case CssUnitOrNames.FONTSIZE_MEDIUM:
    //                    return CssConstants.Medium;
    //                case CssUnitOrNames.FONTSIZE_SMALL:
    //                    return CssConstants.Small;
    //                case CssUnitOrNames.FONTSIZE_X_SMALL:
    //                    return CssConstants.XSmall;
    //                case CssUnitOrNames.FONTSIZE_XX_SMALL:
    //                    return CssConstants.XXSmall;
    //                case CssUnitOrNames.FONTSIZE_LARGE:
    //                    return CssConstants.Large;
    //                case CssUnitOrNames.FONTSIZE_X_LARGE:
    //                    return CssConstants.XLarge;
    //                case CssUnitOrNames.FONTSIZE_XX_LARGE:
    //                    return CssConstants.XXLarge;
    //                case CssUnitOrNames.FONTSIZE_LARGER:
    //                    return CssConstants.Larger;
    //                case CssUnitOrNames.FONTSIZE_SMALLER:
    //                    return CssConstants.Smaller;
    //                default:
    //                    return "";
    //            }
    //        }
    //        else
    //        {
    //            return length.ToString();
    //        }
    //    }


    //    public static LayoutFarm.Css.CssLength AsBorderLength(this WebDom.CssCodeValueExpression value)
    //    {
    //        if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.BorderLength)
    //        {
    //            switch (value.Hint)
    //            {
    //                case WebDom.CssValueHint.Number:
    //                    {
    //                        if (value is WebDom.CssCodePrimitiveExpression)
    //                        {
    //                            WebDom.CssCodePrimitiveExpression prim = (WebDom.CssCodePrimitiveExpression)value;
    //                            CssLength len = new CssLength(value.AsNumber(), GetCssUnit(prim.Unit));
    //                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.BorderLength);
    //                            return len;
    //                        }
    //                        else
    //                        {
    //                            CssLength len = CssLength.MakePixelLength(value.AsNumber());
    //                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.BorderLength);
    //                            return len;
    //                        }
    //                    }
    //                default:
    //                    {
    //                        CssLength len = MakeBorderLength(value.ToString());
    //                        value.SetCssLength(len, WebDom.CssValueEvaluatedAs.BorderLength);
    //                        return len;
    //                    }
    //            }
    //        }
    //        return value.GetCacheCssLength();
    //    }


    //    public static CssLength AsLength(this WebDom.CssCodeValueExpression value)
    //    {
    //        if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.Length)
    //        {
    //            //length from number 
    //            switch (value.Hint)
    //            {
    //                case WebDom.CssValueHint.Number:
    //                    {
    //                        if (value is WebDom.CssCodePrimitiveExpression)
    //                        {
    //                            WebDom.CssCodePrimitiveExpression prim = (WebDom.CssCodePrimitiveExpression)value;
    //                            CssLength len = new CssLength(value.AsNumber(), GetCssUnit(prim.Unit));
    //                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.Length);
    //                            return len;
    //                        }
    //                        else
    //                        {
    //                            CssLength len = CssLength.MakePixelLength(value.AsNumber());
    //                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.Length);
    //                            return len;
    //                        }
    //                    }
    //                default:
    //                    {

    //                        CssLength len = TranslateLength(value.ToString());
    //                        value.SetCssLength(len, WebDom.CssValueEvaluatedAs.Length);
    //                        return len;
    //                    }
    //            }
    //        }
    //        return value.GetCacheCssLength();
    //    }



    //    public static CssLength AsTranslatedLength(this WebDom.CssCodeValueExpression value)
    //    {
    //        if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.TranslatedLength)
    //        {

    //            switch (value.Hint)
    //            {
    //                case WebDom.CssValueHint.Number:
    //                    {
    //                        if (value is WebDom.CssCodePrimitiveExpression)
    //                        {
    //                            WebDom.CssCodePrimitiveExpression prim = (WebDom.CssCodePrimitiveExpression)value;
    //                            CssLength len = new CssLength(value.AsNumber(), GetCssUnit(prim.Unit));
    //                            if (len.HasError)
    //                            {
    //                                len = CssLength.MakePixelLength(0);
    //                            }
    //                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.TranslatedLength);
    //                            return len;
    //                        }
    //                        else
    //                        {
    //                            CssLength len = CssLength.MakePixelLength(value.AsNumber());
    //                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.TranslatedLength);
    //                            return len;
    //                        }

    //                    }
    //                default:
    //                    {
    //                        CssLength len = TranslateLength(value.ToString());
    //                        value.SetCssLength(len, WebDom.CssValueEvaluatedAs.TranslatedLength);
    //                        return len;
    //                    }
    //            }
    //        }
    //        return value.GetCacheCssLength();
    //    }
    //    public static CssColor AsColor(this WebDom.CssCodeValueExpression value)
    //    {
    //        if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.Color)
    //        {
    //            if (value is WebDom.CssCodeColor)
    //            {
    //                return ((WebDom.CssCodeColor)value).ActualColor;
    //            }
    //            else
    //            {
    //                CssColor actualColor = CssValueParser2.GetActualColor(value.GetTranslatedStringValue());
    //                value.SetColorValue(actualColor);
    //                return actualColor;
    //            }
    //        }
    //        return value.GetCacheColor();
    //    }


    //    public static CssFontWeight GetFontWeight(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssFontWeight)EvaluateIntPropertyValueFromString(
    //            _cssFontWeightMap,
    //            WebDom.CssValueEvaluatedAs.Float,
    //            CssFontWeight.NotAssign,
    //            value);
    //    }

    //    public static string ToCssStringValue(this CssFontWeight weight)
    //    {
    //        return _cssFontWeightMap.GetStringFromValue(weight);
    //    }

    //    public static string ToCssStringValue(this CssListStylePosition listStylePosition)
    //    {
    //        return _cssListStylePositionMap.GetStringFromValue(listStylePosition);
    //    }
    //    public static CssListStylePosition GetListStylePosition(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssListStylePosition)EvaluateIntPropertyValueFromString(
    //           _cssListStylePositionMap,
    //           WebDom.CssValueEvaluatedAs.ListStylePosition,
    //           CssListStylePosition.Outside,
    //           value);

    //    }

    //    public static string ToCssStringValue(this CssListStyleType listStyleType)
    //    {
    //        return _cssListStyleTypeMap.GetStringFromValue(listStyleType);
    //    }
    //    public static CssListStyleType GetListStyleType(WebDom.CssCodeValueExpression value)
    //    {
    //        return (CssListStyleType)EvaluateIntPropertyValueFromString(
    //        _cssListStyleTypeMap,
    //        WebDom.CssValueEvaluatedAs.ListStyleType,
    //        CssListStyleType.Disc,
    //        value);
    //    }


    //    public static WellKnownDomNodeName EvaluateTagName(string name)
    //    {
    //        return _wellknownHtmlTagNameMap.GetValueFromString(name, WellKnownDomNodeName.Unknown);
    //    }


    //    /// <summary>
    //    /// Converts an HTML length into a Css length
    //    /// </summary>
    //    /// <param name="htmlLength"></param>
    //    /// <returns></returns>
    //    public static CssLength TranslateLength(string htmlLength)
    //    {
    //        CssLength len = ParseGenericLength(htmlLength);
    //        if (len.HasError)
    //        {
    //            return CssLength.MakePixelLength(0);
    //        }
    //        return len;
    //    }
    //    internal static CssLength MakeBorderLength(string str)
    //    {
    //        switch (str)
    //        {
    //            case CssConstants.Medium:
    //                return CssLength.Medium;
    //            case CssConstants.Thick:
    //                return CssLength.Thick;
    //            case CssConstants.Thin:
    //                return CssLength.Thin;
    //        }
    //        return ParseGenericLength(str);
    //    }

    //    public static CssLength ParseGenericLength(string lenValue)
    //    {
    //        float parsedNumber = 0f;

    //        switch (lenValue)
    //        {
    //            case null:
    //            case "":
    //            case "0":
    //                //Return zero if no length specified, zero specified
    //                return CssLength.ZeroNoUnit;
    //            case "auto":
    //                return CssLength.AutoLength;
    //            case "normal":
    //                return CssLength.NormalWordOrLine;
    //        }

    //        //then parse
    //        //If percentage, use ParseNumber
    //        if (lenValue.EndsWith("%"))
    //        {
    //            parsedNumber = float.Parse(lenValue.Substring(0, lenValue.Length - 1));
    //            return new CssLength(parsedNumber, CssUnitOrNames.Percent);
    //        }
    //        //If no units, has error
    //        if (lenValue.Length < 3)
    //        {

    //            if (float.TryParse(lenValue, out parsedNumber))
    //            {
    //                return new CssLength(parsedNumber, CssUnitOrNames.Pixels);
    //            }
    //            else
    //            {
    //                return new CssLength(parsedNumber, CssUnitOrNames.Unknown);
    //            }

    //        }
    //        //Get units of the length
    //        //TODO: Units behave different in paper and in screen! 
    //        CssUnitOrNames unit = GetCssUnit(lenValue.Substring(lenValue.Length - 2, 2));
    //        //parse number part
    //        if (unit == CssUnitOrNames.Unknown)
    //        {
    //            //check if all char is number?

    //            if (!float.TryParse(lenValue,
    //                System.Globalization.NumberStyles.Number,
    //                System.Globalization.NumberFormatInfo.InvariantInfo, out parsedNumber))
    //            {
    //                //make an error value
    //                return new CssLength(0, CssUnitOrNames.Unknown);
    //            }
    //            else
    //            {
    //                return new CssLength(parsedNumber, CssUnitOrNames.Pixels);
    //            }

    //        }
    //        else
    //        {
    //            string number_part = lenValue.Substring(0, lenValue.Length - 2);

    //            if (!float.TryParse(number_part,
    //                System.Globalization.NumberStyles.Number,
    //                System.Globalization.NumberFormatInfo.InvariantInfo, out parsedNumber))
    //            {
    //                //make an error value
    //                return new CssLength(0, CssUnitOrNames.Unknown);
    //            }
    //            else
    //            {
    //                return new CssLength(parsedNumber, unit);
    //            }
    //        }

    //    }
    //}

}