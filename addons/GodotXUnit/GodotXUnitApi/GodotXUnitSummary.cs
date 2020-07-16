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
        
        public void AddSkipped(TestSkippedInfo message)
        {
            skipped.Add(new GodotXUnitTestResult
            {
                testCaseClass = message.TypeName,
                testCaseName = message.MethodName,
                result = "skipped"
            }); 
        }

        public void AddPassed(TestPassedInfo message)
        {
            passed.Add(new GodotXUnitTestResult
            {
                testCaseClass = message.TypeName,
                testCaseName = message.MethodName,
                output = message.Output,
                time = (float) message.ExecutionTime,
                result = "passed"
            });
        }

        public void AddFailed(TestFailedInfo message)
        {
            failed.Add(new GodotXUnitTestResult
            {
                testCaseClass = message.TypeName,
                testCaseName = message.MethodName,
                output = message.Output,
                time = (float) message.ExecutionTime,
                result = "failed",
                exceptionType = message.ExceptionType,
                exceptionMessage = message.ExceptionMessage,
                exceptionStackTrace = message.ExceptionStackTrace,
            });
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