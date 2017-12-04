using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using Microsoft.CodeAnalysis.Text;

namespace KotBot.Modules
{
    public class ModuleWrap
    {
        public AppDomain Domain {get; set;}
        public Assembly Assembly { get; set; }
        public ModuleInfo Info { get; set; }
    }


    public static class ModuleLoader
    {
        public static Dictionary<string, ModuleWrap> loadedModules = new Dictionary<string, ModuleWrap>(); // todo wrap?
        public static Dictionary<string, bool> loadingModule = new Dictionary<string, bool>();
        public static bool LoadAllModules()
        {
            foreach(var domain in loadedModules)
            {
                AppDomain.Unload(domain.Value.Domain);
            }
            loadedModules = new Dictionary<string, ModuleWrap>();
            JArray modules = ModuleConfig.GetValue<JArray>(null, "loadedModules", new JArray());
            foreach(JToken module in modules)
            {
                if (!Load(module.ToString())) return false; 
            }
            return true;
        }
        private static List<SyntaxTree> loadSyntaxTrees(string folder)
        {
            List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
            if (!Directory.Exists(folder))
                return syntaxTrees;
            foreach (string filename in Directory.GetFiles(folder))
            {
                if(filename.EndsWith(".cs"))
                {
                    string text = File.ReadAllText(filename);
                    using (var stream = File.OpenRead(filename))
                    {
                        SyntaxTree tree = CSharpSyntaxTree.ParseText(SourceText.From(stream), path: filename);
                        syntaxTrees.Add(tree);
                    }
                }
            }
            foreach (string directory in Directory.GetDirectories($"{folder}/"))
            {
                syntaxTrees.AddRange(loadSyntaxTrees($"{directory}"));
            }
            return syntaxTrees;
        }
        private static readonly IEnumerable<string> DefaultNamespaces = new[]
        {
            "System",
            "System.IO",
            "System.Net",
            "System.Linq",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Collections.Generic",
            "System.Collections.Immutable",
            "System.Collections",
            "KotBot",
            "KotBot.Modules",
            "KotBot.BotManager",
            "Newtonsoft.Json.Linq",
            "Newtonsoft.Json",
            "Discord",
            "Discord.WebSocket",
            "Discord.Net",
            "Discord.Rest",
            "Discord.Net.WebSockets",
            "Discord.Net.Rest",
            "System.IO",
            "System.Interactive.Async"
        };

        private static bool DeleteWithWait(string filename, int timeoutMs = 1000)
        {
            var time = Stopwatch.StartNew();
            while (time.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    File.Delete(filename);
                    return true;
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException e)
                {
                    // access error
                    if (e.HResult != -2147024864)
                        return false;
                }
            }
            return false;
        }


        public static bool Load(string module)
        {
            if (loadingModule.ContainsKey(module) && loadingModule[module]) return false;
            Log.Print($"Loading module {module}");

            loadingModule[module] = true;
            AppDomain testDomain = AppDomain.CurrentDomain;
            string[] dirs = Directory.GetDirectories($"csharp/");
            if (!Directory.GetDirectories($"csharp/").Contains<string>($"csharp/{module}"))
                return false;
            if (!Directory.Exists($"csharp/{module}"))
                return false;

            DateTime start = DateTime.Now;
            try
            {
                var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
                List<MetadataReference> defaultReferences = new List<MetadataReference>(new[]
                {
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(typeof(System.Collections.Immutable.ImmutableArray).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(KotBot.BotManager.User).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.Linq.JToken).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.AsyncCallback).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.IO.BinaryReader).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Web.HttpUtility).Assembly.Location),
                });
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach(var loadedAssemblies in assemblies)
                {
                    if (loadedAssemblies.FullName.Contains("Anonymous"))
                        continue;
                    string location = loadedAssemblies.Location;
                    if(!string.IsNullOrWhiteSpace(location))
                     defaultReferences.Add(MetadataReference.CreateFromFile(location));
                }
                foreach(var dlls in Directory.GetFiles($"{AppDomain.CurrentDomain.BaseDirectory}/lib"))
                {
                    if(dlls.EndsWith(".dll"))
                    {
                        try
                        {
                            System.Reflection.AssemblyName testAssembly =
                                System.Reflection.AssemblyName.GetAssemblyName(dlls);
                            defaultReferences.Add(MetadataReference.CreateFromFile(dlls));

                        }
                        catch (Exception) {

                        }
                    }
                }
                string moduleBin = $"{AppDomain.CurrentDomain.BaseDirectory}/csharp/{module}/bin";
                if(Directory.Exists(moduleBin))
                {
                    foreach (var dlls in Directory.GetFiles(moduleBin))
                    {
                        if (dlls.EndsWith(".dll"))
                        {
                            try
                            {
                                System.Reflection.AssemblyName testAssembly =
                                    System.Reflection.AssemblyName.GetAssemblyName(dlls);
                                defaultReferences.Add(MetadataReference.CreateFromFile(dlls));
                                
                            }
                            catch (Exception) { }
                        }   
                    }
                }
                
                List<string> newList = new List<string>(DefaultNamespaces);
                List<SyntaxTree> syntaxTrees = loadSyntaxTrees($"csharp/{module}");
                CSharpCompilationOptions defaultCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                            .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Release)
                            .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);

                Compilation compillation = CSharpCompilation.Create(module,
                    options: defaultCompilationOptions,
                    syntaxTrees: syntaxTrees.ToArray<SyntaxTree>(),
                    references: defaultReferences
                );
                byte[] compiledAssembly;
                byte[] compiledSymbols;
                using (var symbols = new MemoryStream())
                {
                    using (var output = new MemoryStream())
                    {
                        EmitResult result = compillation.Emit(output, symbols);

                        if (!result.Success)
                        {
                            Log.Error($"Module {module} failed to load with errors:");
                            foreach(var error in result.Diagnostics)
                            {
                                if(error.IsWarningAsError || error.Severity == DiagnosticSeverity.Error)
                                {
                                    Log.Error($"{error.GetMessage()} : {error.Location.ToString()}");
                                }
                            }
                            return false;
                        }
                        compiledAssembly = output.ToArray();
                        compiledSymbols = symbols.ToArray();
                    }
                }
                
                var assembly = Assembly.Load(compiledAssembly, compiledSymbols);

                var pluginType = assembly.GetType("Plugin.Plugin");
                var methods = pluginType.GetMethods();
                var methodInfo = pluginType.GetMethod("Main", BindingFlags.Static | BindingFlags.Public);
                var outMain = methodInfo.Invoke(null, new[] { new string[] { module } });
                ModuleCommands.RegisterAssembliesComamnds(module, assembly);

                if (loadedModules.ContainsKey(module))
                {
                    ModuleWrap wrap = loadedModules[module];
                    Assembly oldAssembly = wrap.Assembly;
                    var oldPluginType = oldAssembly.GetType("Plugin.Plugin");
                    var oldMethods = oldPluginType.GetMethods();
                    var oldMethodInfo = oldPluginType.GetMethod("Close", BindingFlags.Static | BindingFlags.Public);
                    var oldOutMain = oldMethodInfo.Invoke(null, null);
                    loadedModules.Remove(module);
                }
                ModuleInfo info = (ModuleInfo)methodInfo.GetCustomAttribute(typeof(ModuleInfo));
                loadedModules[module] = new ModuleWrap { Domain = null, Assembly = assembly, Info = info };
            }
            catch(Exception compillationFailed)
            {
                Log.Error($"Module {module} failed to load {compillationFailed.Message}", $"{compillationFailed.StackTrace}");
                return false;
            }
            Log.Print($"Module {module} loaded successfully took {(DateTime.Now - start).TotalSeconds}s");
            loadingModule[module] = false;
            ModuleCommunications.OnModuleLoaded(module, loadedModules[module]);
            return true;

        }
    }
}
