using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownUI.WPF
{
    public class VirtualProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Name}:{Value}";
        }
    }

    public class VirtualCollectionItem
    {
        public string Name { get; set; }
        public List<VirtualProperty> Props { get; set; } = new List<VirtualProperty>();
        public List<VirtualViewModel> Items { get; set; } = new List<VirtualViewModel>();
    }
    public class VirtualViewModel
    {
        public string Name { get; set; }
        public List<VirtualProperty> Props { get; set; } = new List<VirtualProperty>();
        public List<VirtualCollectionItem> Collections { get; set; } = new List<VirtualCollectionItem>();
    }
    public static class VMHelpers
    {
        public static VirtualViewModel CreateVirtualViewModel(Type vmType, string name = "base")
        {
            PropertyInfo[] props = vmType.GetProperties();

            foreach (var prop in props)
            {
                Console.WriteLine("{0}={1}", prop.Name, prop.PropertyType);
            }

            VirtualViewModel vvm = new VirtualViewModel();
            vvm.Name = name;
            foreach (var prop in props)
            {
                if (prop.PropertyType.FullName == "System.String")
                {
                    var virtualProperty = new VirtualProperty
                    {
                        Name = prop.Name
                    };

                    vvm.Props.Add(virtualProperty);
                }
                else
                {

                    Type type = prop.PropertyType;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type itemType = type.GetGenericArguments()[0]; // use this...


                        var evm = CreateVirtualViewModel(itemType, prop.Name);
                        VirtualCollectionItem vci = new VirtualCollectionItem
                        {
                            Name = prop.Name,
                            Props = evm.Props,
                            Items = new List<VirtualViewModel>()
                        };
                        vvm.Collections.Add(vci);
                    }
                }
            }

            return vvm;
        }
    }

    public class MarkDownViewModel
    {
        public VirtualViewModel GetVirtualViewModel()
        {
            VirtualViewModel vvm = VMHelpers.CreateVirtualViewModel(this.GetType());


            foreach (VirtualProperty virtualProperty in vvm.Props)
            {
                object thing = this.GetType().GetProperty(virtualProperty.Name).GetValue(this);
                virtualProperty.Value = thing.ToString();

            }

            foreach (VirtualCollectionItem virtualCollectionItem in vvm.Collections)
            {
                object thing = this.GetType().GetProperty(virtualCollectionItem.Name).GetValue(this);
                IEnumerable<object> iThing = thing as IEnumerable<object>;

                virtualCollectionItem.Items.Clear();
                foreach (object markDownViewModel in iThing)
                {
                    VirtualViewModel item = new VirtualViewModel();
                    foreach (VirtualProperty virtualProperty in virtualCollectionItem.Props)
                    {
                        var prp = markDownViewModel.GetType().GetProperty(virtualProperty.Name);
                        object newthing = prp.GetValue(markDownViewModel);

                        item.Props.Add(new VirtualProperty
                        {
                            Name = virtualProperty.Name,
                            Value = newthing.ToString()
                        });

                    }
                    virtualCollectionItem.Items.Add(item);
                }
            }

            return vvm;
        }
        //should be string,generic or string,object, lets use strings for now
        public Dictionary<string, string> bindings = new Dictionary<string, string>();
        public Dictionary<string, List<Action<string>>> updateUIBindings = new Dictionary<string, List<Action<string>>>();

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
