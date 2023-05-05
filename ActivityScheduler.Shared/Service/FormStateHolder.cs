using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Shared.Service
{
    public class FormStateHolder
    {
        
        private List<FormState> _formStates = new List<FormState>();    
        
        public FormStateHolder() { }

        public FormState CreateFormState (string stateName)
        {
            FormState stt= new FormState(stateName, this);
            _formStates.Add(stt);
            return stt;
        }
        public void SetFormState(string stateName)
        {
            var stt = _formStates.FirstOrDefault(s => s.Name == stateName);
            if(stt == null) { return; }
            stt.Actions.ForEach(x=>x.Invoke());
        }


        public class FormState 
        {
            public FormStateHolder Parent { get; set; }

            public List<Action> Actions { get; set; } = new List<Action>();
            public FormState(string name, FormStateHolder parent)
            {
                Name = name;
                Parent = parent;
            }
            public string Name { get; set; }

            public FormState AddAction(Action action) { Actions.Add(action); return this; }
        }
    }
}
