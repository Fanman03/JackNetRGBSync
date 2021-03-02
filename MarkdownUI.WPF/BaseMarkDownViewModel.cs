using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownUI.WPF
{
    public class MarkDownViewModel
    {
        //should be string,generic or string,object, lets use strings for now
        internal Dictionary<string, string> bindings = new Dictionary<string, string>();
        internal Dictionary<string, List<Action<string>>> updateUIBindings = new Dictionary<string, List<Action<string>>>();

        //should be generic, lets use strings for now
        public bool Set(ref string backingProperty, string newValue, [CallerMemberName] string callerMemberName = "")
        {
            if (backingProperty == null && newValue == null)
            {
                //do nothing
                return false;
            }

            if ((backingProperty == null && newValue != null) || (backingProperty != null && newValue == null) || !backingProperty.Equals(newValue))
            {
                if (bindings.ContainsKey(callerMemberName))
                {
                    bindings.Remove(callerMemberName);
                }

                bindings.Add(callerMemberName, newValue);
                backingProperty = newValue;

                if (updateUIBindings.ContainsKey(callerMemberName))
                {
                    foreach (var inv in updateUIBindings[callerMemberName])
                    {
                        inv?.Invoke(newValue);
                    }
                }

            }
            return false;
        }
    }
}
