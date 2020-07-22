using System;
using System.Collections.Generic;
using Godot;
using Xunit.Runners;

namespace GodotXUnitApi
{
    [Serializable]
    public class GodotXUnitSummary
    {
        public int testsDiscovered;
        public int testsExpectedToRun;
        public List<GodotXUnitTestResult> skipped = new List<GodotXUnitTestResult>();
        public List<GodotXUnitTestResult> passed = new List<GodotXUnitTestResult>();
        public List<GodotXUnitTestResult> failed = new List<GodotXUnitTestResult>();

        public int completed => passed.Count + failed.Count;

        public GodotXUnitTestResult AddSkipped(TestSkippedInfo message)
        {
            var result = new GodotXUnitTestResult
            {
                testCaseClass = message.TypeName,
                testCaseName = message.MethodName,
                result = "skipped"
            };
            skipped.Add(result);
            return result;
        }

        public GodotXUnitTestResult AddPassed(TestPassedInfo message)
        {
            var result = new GodotXUnitTestResult
            {
                testCaseClass = message.TypeName,
                testCaseName = message.MethodName,
                output = message.Output,
                time = (float) message.ExecutionTime,
                result = "passed"
            };
            passed.Add(result);
            return result;
        }

        public GodotXUnitTestResult AddFailed(TestFailedInfo message)
        {
            var result = new GodotXUnitTestResult
            {
                testCaseClass = message.TypeName,
                testCaseName = message.MethodName,
                output = message.Output,
                time = (float) message.ExecutionTime,
                result = "failed",
                exceptionType = message.ExceptionType,
                exceptionMessage = message.ExceptionMessage,
                exceptionStackTrace = message.ExceptionStackTrace,
            };
            failed.Add(result);
            return result;
        }
    }

    [Serializable]
    public class GodotXUnitTestResult
    {
        public string testCaseClass;
        public string testCaseName;
        public string output;
        public float time;
        public string result;
        public string exceptionType;
        public string exceptionMessage;
        public string exceptionStackTrace;
    }
}