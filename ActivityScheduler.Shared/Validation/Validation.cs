using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ActivityScheduler.Shared.Validation
{
    public static class Validation
    {
        
        public static CommonOperationResult CheckIf6DigitTrasactionNumberIsCorrect(string source)
        {
            Regex regex;
            source = StrRemoveArrSymbols(source, @"\|/^");
            // regex = new Regex("[^a-zA-Zа-яА-Я0-9() +-_:;!?@#.*]", RegexOptions.IgnoreCase);
            //source = regex.Replace(source, "");
            regex = new Regex("[^0-9]", RegexOptions.IgnoreCase);
            var m = regex.Matches(source);
            if (m.Count>0)
            {
                return CommonOperationResult.SayFail("Only digits allowed in transaction, batch and activity numbers");
            }
            if (source.Length != 6) 
            {
                return CommonOperationResult.SayFail("Numbers has to be 6-digit length");
            }
            return CommonOperationResult.SayOk();
        }

        public static CommonOperationResult CheckIfTransactionOrBatchNameIsCorrect(string source)
        {
            Regex regex;
            source = StrRemoveArrSymbols(source, @"\|/^");
            // regex = new Regex("[^a-zA-Zа-яА-Я0-9() +-_:;!?@#.*]", RegexOptions.IgnoreCase);
            //source = regex.Replace(source, "");
            regex = new Regex("[^a-zA-Z0-9- .]", RegexOptions.IgnoreCase);
            var m = regex.Matches(source);
            if (m.Count > 0)
            {
                return CommonOperationResult.SayFail("Please enter name like 'This is-name.'");
            }
            if (source.Length<6) 
            {
                return CommonOperationResult.SayFail("Transaction, batch and activity names must be more that 5 digits length");
            }

            return CommonOperationResult.SayOk();
        }

        public static CommonOperationResult CheckActivityId(string source)
        {
            Regex regex;
            source = StrRemoveArrSymbols(source, @"\|/^");
            // regex = new Regex("[^a-zA-Zа-яА-Я0-9() +-_:;!?@#.*]", RegexOptions.IgnoreCase);
            //source = regex.Replace(source, "");
            regex = new Regex("[^0-9]", RegexOptions.IgnoreCase);
            var m = regex.Matches(source);
            if (m.Count > 0)
            {
                return CommonOperationResult.SayFail("Only digits allowed in transaction, batch and activity numbers");
            }
            if (source.Length >4)
            {
                return CommonOperationResult.SayFail("Activity Id should be not more than 4 digits length, e.g. 10, 100, 120, ... 900, 950");
            }
            return CommonOperationResult.SayOk();
        }

        public static string StrRemoveArrSymbols(string source, string symbols)
        {
            string text = "";
            for (int i = 0; i < source.Length; i++)
            {
                if (!symbols.Contains(source[i]))
                {
                    text += source[i];
                }
            }

            return text;
        }
    }
}
