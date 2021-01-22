using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SimpleLed;

namespace RGBSyncPlus.Helper
{
    public static class TypeLoaderExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static  IEnumerable<Type> GetTypesWithInterface(this Assembly asm)
        {
            var it = typeof(ISimpleLed);
            return asm.GetLoadableTypes().Where(it.IsAssignableFrom).ToList();
        }
    }
}
