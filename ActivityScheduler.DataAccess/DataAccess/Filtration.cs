using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.DataAccess
{
    public class Filtration
    {
        private string getWhereConditionFromFilter(Lib.Filter filter)
        {

            //это условие where для sql - запросов, сгенерированное из фильтра с синтаксисом mysql
            //тут опять надо разбирать этот filtering expression

            //ну то есть берешь fe и начинаешь его парсить как при проверке
            // Regex regex;
            // MatchCollection matches;
            // string rgx;

            if (filter == null) return "";

            string rez = filter.filteringExpression;
            //rgx = @"([^0-9]+[0-9]{1,2}[^0-9]+)|(^[0-9]{1,2}[^0-9]+)|([^0-9]+[0-9]{1,2}$)|(^[0-9]{1,2}$)";
            //regex = new Regex(rgx, RegexOptions.IgnoreCase);
            string oldVal;
            string newVal;

            rez = filter.filteringExpression;

            foreach (Lib.Filter.FilteringRule fr in filter.filteringRuleList)
            {
                oldVal = "B" + fr.ruleOrder.ToString() + "E";
                newVal = " " + fr.fieldInfoObject.parent.tableName + "." + getFilteringRuleWhereExpr(fr) + " ";
                rez = rez.Replace(oldVal, newVal);

                //rgx = @"([^0-9]+" + s + "+[^0-9]+)|(" + s + "+[^0-9]+)|([^0-9]+" + s + "+$)|(^" + s + "+$)";
                //regex = new Regex(rgx, RegexOptions.IgnoreCase);

                //matches = regex.Matches(filteringExpression);
                /*
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                        Console.WriteLine("REGEX_MATCH_" + match.Value);
                }
                */
                // rez = regex.Replace(rez, fr.whereExpression);
            }

            // Logger.log("DB", "Returned WhrerExpr =" + rez);
            return rez;
        }

        private string getFilteringRuleWhereExpr(Lib.Filter.FilteringRule tmp)
        {
            //выдает выражение фильтрации применительно к конкретной базе, в аднном случае MySql
            // дело в том, что применительно к конкретной базе выражения имеют разный синтакс, напр, field like 'la'


            string s = "";

            if (tmp != null)
            {
                bool itsBool = (tmp.fieldInfoObject.fieldType == Lib.FieldTypeEnum.Bool);
                bool itsTime = tmp.fieldInfoObject.fieldType == Lib.FieldTypeEnum.Time;
                bool itsDate = tmp.fieldInfoObject.fieldType == Lib.FieldTypeEnum.Date;
                bool itsDateTime = (itsTime || itsDate);
                bool itsNumeric = (tmp.fieldInfoObject.fieldType == Lib.FieldTypeEnum.Int || tmp.fieldInfoObject.fieldType == Lib.FieldTypeEnum.Double);
                bool itsString = (tmp.fieldInfoObject.fieldType == Lib.FieldTypeEnum.String || tmp.fieldInfoObject.fieldType == Lib.FieldTypeEnum.Memo);

                switch (tmp.filtrationOperator)
                {
                    case Lib.RIFDC_DataCompareOperatorEnum.contains:
                        //тут только строки
                        s = tmp.fieldInfoObject.fieldDbName + " like " + "'*" + tmp.filtrationValue + "*'";
                        break;

                    case Lib.RIFDC_DataCompareOperatorEnum.equal:

                        if (itsString) { s = tmp.fieldInfoObject.fieldDbName + " = " + "'" + tmp.filtrationValue + "'"; }
                        if (itsDate) { s = tmp.fieldInfoObject.fieldDbName + " = " + "#" + replaceDateSeparator(tmp.filtrationValue) + "#"; }
                        if (itsNumeric || itsBool) { s = tmp.fieldInfoObject.fieldDbName + " = " + tmp.filtrationValue; }
                        break;
                    case Lib.RIFDC_DataCompareOperatorEnum.greater:
                    case Lib.RIFDC_DataCompareOperatorEnum.greaterEqual:
                    case Lib.RIFDC_DataCompareOperatorEnum.lower:
                    case Lib.RIFDC_DataCompareOperatorEnum.lowerEqual:
                        if (itsNumeric || itsBool) { s = tmp.fieldInfoObject.fieldDbName + " " + getCompareOperatorName(tmp.filtrationOperator) + " " + tmp.filtrationValue; }
                        if (itsDate) { s = tmp.fieldInfoObject.fieldDbName + " " + getCompareOperatorName(tmp.filtrationOperator) + " #" + replaceDateSeparator(tmp.filtrationValue) + "#"; }
                        if (itsTime) { s = "/класс под ACCESS пока время не обрабатывает/"; }

                        break;

                    case Lib.RIFDC_DataCompareOperatorEnum.notEuqal:
                        if (itsString) { s = tmp.fieldInfoObject.fieldDbName + " <> " + "'" + tmp.filtrationValue + "'"; }
                        if (itsDate) { s = tmp.fieldInfoObject.fieldDbName + " <> " + "#" + replaceDateSeparator(tmp.filtrationValue) + "#"; }
                        if (itsNumeric || itsBool) { s = tmp.fieldInfoObject.fieldDbName + " <> " + tmp.filtrationValue; }

                        break;
                }
            }

            return s;
        }
        private string replaceDateSeparator(string source)
        {
            source = source.Replace('.', '/');
            return source;
        }

        private string getCompareOperatorName(Lib.RIFDC_DataCompareOperatorEnum op)
        {
            //
            switch (op)
            {
                case Lib.RIFDC_DataCompareOperatorEnum.equal:
                    return "=";

                case Lib.RIFDC_DataCompareOperatorEnum.greater:
                    return ">";

                case Lib.RIFDC_DataCompareOperatorEnum.greaterEqual:
                    return ">=";

                case Lib.RIFDC_DataCompareOperatorEnum.lower:
                    return "<";
                case Lib.RIFDC_DataCompareOperatorEnum.lowerEqual:
                    return "<>"; // ACCESS не равно выглядить так

                default: return "";
            }
        }

    }
}
