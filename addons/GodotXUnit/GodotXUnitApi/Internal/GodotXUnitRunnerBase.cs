using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Godot;
using Newtonsoft.Json;
using Xunit.Runners;
using Path = System.IO.Path;

namespace GodotXUnitApi.Internal
{
    public abstract partial class GodotXUnitRunnerBase : Node2D
    {
        /// <summary>
        /// Get the assembly name from the project settings.
        /// </summary>
        /// <returns>The name of the target assembly or null if the default is requested</returns>
        private String GetTargetAssemblyNameFromSettings()
        {
            if (!ProjectSettings.HasSetting(Consts.SETTING_TARGET_ASSEMBLY))
            {
                return null;
            }
            var targetProject = ProjectSettings.GetSetting(Consts.SETTING_TARGET_ASSEMBLY).ToString();
            if (string.IsNullOrEmpty(targetProject))
            {
                return null;
            }
            return targetProject;
        }

        private String GetAssemblyPath(String assemblyName)
        {
            var currentDir = System.IO.Directory.GetCurrentDirectory();
            return Path.Combine(currentDir, $".mono/build/bin/Debug/{assemblyName}.dll");
        }

        private String GetDefaultTargetAssemblyPath()
        {
            return GetAssemblyPath(Assembly.GetExecutingAssembly().GetName().Name);
        }

        private String GetTargetAssemblyPath(GodotXUnitSummary summary)
        {
            var assemblyName = GetTargetAssemblyNameFromSettings();
            if (assemblyName is null)
            {
                summary.AddDiagnostic("target assembly name is null");
                return GetDefaultTargetAssemblyPath();
            }
            if (assemblyName.Equals(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM_FLAG))
            {
                var customDll = ProjectSettings.HasSetting(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM)
                    ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM).ToString()
                    : "";
                if (string.IsNullOrEmpty(customDll))
                {
                    summary.AddDiagnostic("no custom dll assembly configured.");
                    GD.PrintErr("no custom dll assembly configured.");
                    return GetDefaultTargetAssemblyPath();
                }

                summary.AddDiagnostic($"attempting to load custom dll at: {customDll}");
                return customDll;
            }

            // find the project in the project list. if its not there, print error and leave
            var projectList = ProjectListing.GetProjectInfo();
            if (!projectList.ContainsKey(assemblyName))
            {
                var message = $"unable to find project {assemblyName}. expected values: {string.Join(", ", projectList.Keys)}";
                GD.PrintErr(message);
                summary.AddDiagnostic(message);
                return GetDefaultTargetAssemblyPath();
            }

            // finally, attempt to load project..
            var targetAssembly = GetAssemblyPath(assemblyName);
            return targetAssembly;
        }

        protected virtual string GetTargetClass(GodotXUnitSummary summary)
        {
            return ProjectSettings.HasSetting(Consts.SETTING_TARGET_CLASS)
                ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_CLASS).AsString()
                : null;
        }

        protected virtual string GetTargetMethod(GodotXUnitSummary summary)
        {
            return ProjectSettings.HasSetting(Consts.SETTING_TARGET_METHOD)
                ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_METHOD).AsString()
                : null;
        }

        private ConcurrentQueue<Action<Node2D>> drawRequests = new ConcurrentQueue<Action<Node2D>>();

        [Signal]
        public delegate void OnProcessEventHandler();

        [Signal]
        public delegate void OnPhysicsProcessEventHandler();

        [Signal]
        public delegate void OnDrawRequestDoneEventHandler();

        public void RequestDraw(Action<Node2D> request)
        {
            drawRequests.Enqueue(request);
        }

        private AssemblyRunner runner;

        private GodotXUnitSummary summary;

        private MessageSender messages;

        public Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            GD.Print($"Resolving {args.Name}.");
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly is not null)
            {
                GD.Print($"Assembly resolution success, already loaded: {assembly.Location}");
                return assembly;
            }
            try
            {
                var shortName = args.Name.Split(",")[0];
                assembly = Assembly.LoadFile(GetAssemblyPath(shortName));
                GD.Print($"Assembly resolution success {args.Name} -> {assembly.Location}");
                return assembly;
            }
            catch (System.IO.FileNotFoundException e)
            {
                var msg = $"Assembly resolution failed for {args.Name}, requested by {args.RequestingAssembly?.FullName ?? "unknown assembly"}";
                GD.PrintErr(msg);
                GD.PushError(msg);
                return null;
            }
        }

        public override void _Ready()
        {
            GDU.Instance = this;
            GD.Print($"running tests in tree at: {GetPath()}");
            WorkFiles.CleanWorkDir();
            summary = new GodotXUnitSummary();
            messages = new MessageSender();
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            CreateRunner();
            if (runner == null)
            {
                messages.SendMessage(summary, "summary");
                WriteSummary(summary);
                GetTree().Quit(1);
                return;
            }
            runner.OnDiagnosticMessage = message =>
            {
                GD.PrintErr($"OnDiagnosticMessage: {message.Message}");
                summary.AddDiagnostic(message.Message);
            };
            runner.OnDiscoveryComplete = message =>
            {
                summary.testsDiscovered = message.TestCasesDiscovered;
                summary.testsExpectedToRun = message.TestCasesToRun;
                GD.Print($"discovery finished: found {message.TestCasesDiscovered}," +
                         $" running {message.TestCasesToRun}");
            };
            runner.OnErrorMessage = message =>
            {
                GD.PrintErr($"OnErrorMessage ({message.MesssageType}) {message.ExceptionType}: " +
                            $"{message.ExceptionMessage}\n{message.ExceptionStackTrace}");
                summary.diagnostics.Add(new GodotXUnitOtherDiagnostic
                {
                    message = message.ExceptionMessage,
                    exceptionType = message.ExceptionType,
                    exceptionStackTrace = message.ExceptionStackTrace
                });
            };
            runner.OnTestStarting = message =>
            {
                messages.SendMessage(new GodotXUnitTestStart()
                {
                    testCaseClass = message.TypeName,
                    testCaseName = message.MethodName
                }, "start");
            };
            runner.OnTestFailed = message =>
            {
                messages.SendMessage(summary.AddFailed(message), "failed");
                GD.Print($"  > OnTestFailed: {message.TestDisplayName} in {message.ExecutionTime}");
            };
            runner.OnTestPassed = message =>
            {
                messages.SendMessage(summary.AddPassed(message), "passed");
                GD.Print($"  > OnTestPassed: {message.TestDisplayName} in {message.ExecutionTime}");
            };
            runner.OnTestSkipped = message =>
            {
                messages.SendMessage(summary.AddSkipped(message), "skipped");
                GD.Print($"  > OnTestSkipped: {message.TestDisplayName}");
            };
            runner.OnExecutionComplete = message =>
            {
                messages.SendMessage(summary, "summary");
                WriteSummary(summary);
                GD.Print($"tests completed ({message.ExecutionTime}): {summary.completed}");
                GetTree().Quit(Mathf.Clamp(summary.failed.Count, 0, 20));
            };

            var targetMethod = GetTargetMethod(summary);
            if (!string.IsNullOrEmpty(targetMethod))
            {
                GD.Print($"targeting method for discovery: {targetMethod}");
                runner.TestCaseFilter = test => targetMethod.Equals(test.TestMethod.Method.Name);
            }

            // if its an empty string, then we need to set it to null because the runner only checks for null
            var targetClass = GetTargetClass(summary);
            if (string.IsNullOrEmpty(targetClass))
                targetClass = null;
            else
            {
                GD.Print($"targeting class for discovery: {targetClass}");
            }
            runner.Start(targetClass, null, null, null, null, false, null, null);
        }

        private void CreateRunner()
        {
            try
            {
                var assemblyPath = GetTargetAssemblyPath(summary);
                if (String.IsNullOrEmpty(assemblyPath))
                {
                    summary.AddDiagnostic(new Exception("no assembly returned for tests"));
                    return;
                }
                summary.AddDiagnostic($"Loading assembly at {assemblyPath}");
                runner = AssemblyRunner.WithoutAppDomain(assemblyPath);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"error while attempting to get test assembly: {ex}");
                summary.AddDiagnostic("error while attempting to get test assembly");
                summary.AddDiagnostic(ex);
            }
        }

        public override void _ExitTree()
        {
            GDU.Instance = null;
            runner?.Dispose();
            runner = null;
        }

        private void WriteSummary(GodotXUnitSummary testSummary)
        {
            var location = ProjectSettings.HasSetting(Consts.SETTING_RESULTS_SUMMARY)
                ? ProjectSettings.GetSetting(Consts.SETTING_RESULTS_SUMMARY).ToString()
                : Consts.SETTING_RESULTS_SUMMARY_DEF;

            try
            {
                var file = FileAccess.Open(location, FileAccess.ModeFlags.Write).ThrowIfNotOk();
                file.StoreString(JsonConvert.SerializeObject(testSummary, Formatting.Indented, WorkFiles.jsonSettings));
                file.Close();
            }
            catch (System.IO.IOException e)
            {
                GD.Print($"error returned for writing message at {location}: {e}");
            }
        }

        public override void _Process(double delta)
        {
            EmitSignal(nameof(OnProcess));
            QueueRedraw();
        }

        public override void _PhysicsProcess(double delta)
        {
            EmitSignal(nameof(OnPhysicsProcess));
        }

        public override void _Draw()
        {
            while (drawRequests.TryDequeue(out var request))
                request(this);
            EmitSignal(nameof(OnDrawRequestDone));
        }
    }
}