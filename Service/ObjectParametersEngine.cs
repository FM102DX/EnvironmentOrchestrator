using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Service
{
    public static class ObjectParameters
    {
        public static void SetObjectParameter(object x, string name, object value)
        {

            FieldInfo[] newObjectFields = x.GetType().GetFields();
            PropertyInfo[] newObjectProperties = x.GetType().GetProperties();

            foreach (FieldInfo f0 in newObjectFields)
            {

                if (f0.Name.ToUpperInvariant() == name.ToUpperInvariant())
                {
                    try
                    {
                        value = ConvertedObjectValue(f0.FieldType.ToString(), value); //making sure value has suitable type for unboxing
                        f0.SetValue(x, value);
                    }
                    catch
                    {

                    }
                }
            }

            foreach (PropertyInfo f1 in newObjectProperties)
            {
                if (f1.Name.ToUpperInvariant() == name.ToUpperInvariant())
                {
                    if (IsItOnlyGetter(x, name)) return;
                    try
                    {
                        value = ConvertedObjectValue(f1.PropertyType.ToString(), value); //making sure value has suitable type for unboxing
                        f1.SetValue(x, value);
                    }
                    catch
                    {

                    }
                }
            }
        }
        public static ObjectParameter GetObjectParameterByName(object x, string name)
        {
            ObjectParameter op = new ObjectParameter();
            op.Name = name;

            FieldInfo[] newObjectFields = x.GetType().GetFields();
            PropertyInfo[] newObjectProperties = x.GetType().GetProperties();

            foreach (FieldInfo f0 in newObjectFields)
            {

                if (f0.Name.ToUpperInvariant() == name.ToUpperInvariant())
                {
                    op.Value = f0.GetValue(x);
                    return op;
                }
            }

            foreach (PropertyInfo f1 in newObjectProperties)
            {
                if (f1.Name.ToUpperInvariant() == name.ToUpperInvariant())
                {
                    op.Value = f1.GetValue(x);
                    return op;
                }
            }
            return null;
        }
        public static List<ObjectParameter> GetObjectParameters(object x)
        {
            List<ObjectParameter> rez = new List<ObjectParameter>();

            FieldInfo[] newObjectFields = x.GetType().GetFields();
            PropertyInfo[] newObjectProperties = x.GetType().GetProperties();

            foreach (FieldInfo f0 in newObjectFields)  //это поля реального объекта, который только что был создан и будет добавлен в коллекцию
            {
                ObjectParameter op = new ObjectParameter();
                op.Name = f0.Name;
                op.Value = f0.GetValue(x);
                rez.Add(op);

            }

            foreach (PropertyInfo f1 in newObjectProperties)  //это поля реального объекта, который только что был создан и будет добавлен в коллекцию
            {
                ObjectParameter op = new ObjectParameter();
                op.Name = f1.Name;
                op.Value = f1.GetValue(x);
                rez.Add(op);
            }

            return rez;
        }

        public class ObjectParameter
        {
            public string Name;
            public object Value;
            public string TypeStr;
        }

        public static bool IsItOnlyGetter(object x, string fieldName)
        {
            PropertyInfo[] newObjectProperties = x.GetType().GetProperties();

            foreach (PropertyInfo f1 in newObjectProperties)
            {
                if (f1.Name.ToUpperInvariant() == fieldName.ToUpperInvariant())
                {
                    //вот он нашел это поле
                    return (f1.CanRead && (!f1.CanWrite));
                }
            }
            return false;

        }

        public static object ConvertedObjectValue(string typeStr, object value)
        {
            //возвращает object - обертку исходя из того, какой тип передан в typeStr

            if (value == null) return null;

            if (value.GetType().ToString() == typeStr) { return value; }

            object rez = null;

            string _typeVar="";

            if (typeStr == typeof(System.String).FullName) _typeVar = "String";
            if (typeStr == typeof(System.Decimal).FullName || typeStr == typeof(System.Double).FullName) _typeVar = "DoubleDecimal";
            if (typeStr == typeof(System.Int16).FullName || typeStr == typeof(System.Int32).FullName) _typeVar = "Int";
            if (typeStr == typeof(System.Boolean).FullName) _typeVar = "Boolean";
            if (typeStr == typeof(System.DateTime).FullName) _typeVar = "DateTime";

            try
            {
                switch (_typeVar)
                {
                    case "String":
                        rez = Convert.ToString(value);
                        break;

                    case "DoubleDecimal":
                        rez = Convert.ToDouble(value);
                        break;

                    case "Int":
                        rez = Convert.ToInt32(value);
                        break;

                    case "Boolean":
                        rez = Convert.ToBoolean(value);
                        break;

                    case "DateTime":
                        rez = Convert.ToDateTime(value);
                        break;

                    default:
                        rez = value;
                        break;
                }
            }
            catch
            {
                //конвертация неуспешна
                return null;
            }
            return rez;
        }


    }
}
