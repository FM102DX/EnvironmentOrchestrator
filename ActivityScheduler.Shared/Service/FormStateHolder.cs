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

        public class FormState 
        {
            public FormStateHolder Parent { get; set; }

            private List<Action> actions = new List<Action>();
            public FormState(string name, FormStateHolder parent)
            {
                Name = name;
                Parent = parent;
            }
            public string Name { get; set; }

            public FormState AddAction(Action action) { actions.Add(action); return this; }

        }
        
    }
}
