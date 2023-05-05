using ActivityScheduler.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Executors
{
    public class CheckExecutor<T> where T : class
    {
        //class to create and perfrom object checks in manager classes

        private List<Check> _items = new List<Check>();

        private Dictionary<string, object> _itemToControlBindings = new Dictionary<string, object>();
        private Dictionary<string, Action> _itemToActionBindings = new Dictionary<string, Action>();

        public CheckExecutor() { }
        public CheckExecutor<T> AddCheck(List<string> checkGroupNames, string checkName, Func<T, CommonOperationResult> funcDef) 
        { 
            _items.Add(new Check() { Group= checkGroupNames, Name= checkName, CheckFunction= funcDef });
            return this;
        }

        public CheckExecutor<T> BindControlToCheck(string groupNamePlusCheckName, object ctrl) 
        {
            if (!_itemToControlBindings.ContainsKey(groupNamePlusCheckName))
            {
                _itemToControlBindings.Add(groupNamePlusCheckName, ctrl);
            }
            return this; 
        }
        public CheckExecutor<T> BindActionToCheck(string groupNamePlusCheckName, Action action)
        {
            if (!_itemToActionBindings.ContainsKey(groupNamePlusCheckName))
            {
                _itemToActionBindings.Add(groupNamePlusCheckName, action);
            }
            return this;
        }

        public CommonOperationResult PerformCheck(string checkGroup, T t)
        {
            var checkItems = _items.Where(x => x.Group.Contains(checkGroup)).ToList();

            foreach (var item in checkItems)
            {
                var rez = item.CheckFunction.Invoke(t);

                if (!rez.Success)
                {
                    rez.ControlObject = _itemToControlBindings.FirstOrDefault(x=>x.Key==$"{checkGroup}{item.Name}").Value;
                    rez.StoredAction = _itemToActionBindings.FirstOrDefault(x => x.Key == $"{checkGroup}{item.Name}").Value;
                    return rez;
                }
            }
            return CommonOperationResult.SayOk();
        }

        public class Check
        {
            public List<string> Group { get; set; }
            public string Name { get; set; }
            public Func<T, CommonOperationResult> CheckFunction { get; set; }
        }
    }
}
