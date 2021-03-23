using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MarkdownUI.WPF;
using SimpleLed;

namespace SyncStudio.Core.Helpers
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

        public static IEnumerable<Type> GetTypesWithInterface(this Assembly asm)
        {
            Type it = typeof(ISimpleLed);
            return asm.GetLoadableTypes().Where(it.IsAssignableFrom).ToList();
        }



        public static void LoadChildAssemblies(Assembly assembly, string basePath)
        {

            AssemblyName[] names = assembly.GetReferencedAssemblies();

            foreach (AssemblyName assemblyName in names)
            {
                try
                {
                    if (File.Exists(basePath + "\\" + assemblyName.Name + ".dll"))
                    {
                        Assembly temp = Assembly.Load(File.ReadAllBytes(basePath + "\\" + assemblyName.Name + ".dll"));
                        LoadChildAssemblies(temp, basePath);
                    }
                    else
                    {
                        Assembly.Load(assemblyName);
                    }

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args, string basePath)
        {
            string assemblyName = new AssemblyName(args.Name).Name;

            string dllName = assemblyName + ".dll";
            string dllFullPath = Path.Combine(basePath, dllName);

            if (File.Exists(dllFullPath))
            {
                return Assembly.Load(File.ReadAllBytes(dllFullPath));
            }

            return null;
        }

        public static ISimpleLed LoadDll(string basePath, string dllFileName)
        {
            ISimpleLed result = null;

            ResolveEventHandler delly = (sender, args) => CurrentDomainOnAssemblyResolve(sender, args, basePath);

            AppDomain.CurrentDomain.AssemblyResolve += delly;

            Assembly assembly = Assembly.Load(File.ReadAllBytes(basePath + "\\" + dllFileName));
            //Assembly assembly = Assembly.LoadFrom(file);
            Type[] typeroo = assembly.GetTypes();
            List<Type> pat2 = typeroo.Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass).ToList();

            List<Type> pat3 = pat2.Where(t => typeof(ISimpleLed).IsAssignableFrom(t)).ToList();

            foreach (Type loaderType in pat3)
            {
                if (Activator.CreateInstance(loaderType) is ISimpleLed slsDriver)
                {
                    if (slsDriver is ISimpleLedWithConfig slsWithConfig)
                    {
                      //  MarkdownUIBundle temp = slsWithConfig.GetCustomConfig(null);

                    }


                    LoadChildAssemblies(assembly, basePath);

                    result = slsDriver;
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve -= delly;

            return result;
        }
    }

}
