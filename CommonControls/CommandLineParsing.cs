using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Modbus.Common
{
    public static class CommandLineParsing
    {
        /// <summary>
        /// Calling to windoze api is more robust than using Environment.CommandLine from where some guys have noticed
        /// a few discrepancies
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern IntPtr GetCommandLineW(); 

        /// <summary>
        /// Parse command line, expecting the common following format <br/>
        /// --param1 value1 -param2 "value 2" --param3 "value \"3\"" --flag1 -flag2
        /// </summary>
        /// <returns>
        /// Retrieve arguments list under KV form e.g [param1] = value1, [param2] = value 2 [param3] = value "3", 
        /// [flag1] = {}
        /// </returns>
        public static List<KeyValuePair<string, string>> GetArguments()
        {
            string cmdline = Marshal.PtrToStringUni(GetCommandLineW());
            string args;

            //strip the invocation part
            //the invocation path may (or may not) be quoted ! if path does not contain spaces, it should not be quoted
            var exe = Environment.GetCommandLineArgs()[0]; // Command invocation part

            if(exe[0] != '"' && cmdline[0] == '"')
            {
                //exe path spec doesn't contain space, but the whole command line nearly always wraps it
                //(except when exe is a single verb on the command line ... to be confirmed)
                args = cmdline.Substring(exe.Length +3);
            }
            else
            {
                //if path contains spaces, it should be quoted in any cases (else something somewhere has been wrong)
                args = cmdline.Substring(exe.Length);
            }

            //no args
            if (string.IsNullOrWhiteSpace(args)) return new List<KeyValuePair<string, string>>();

            string pattern = @"-{1,2}(?'switch'[\w-]+)(?:\s*[""'](?'value'.*?)(?<!\\)[""']|\s*(?'value'[\S-[-]]*))?";
            var matches = Regex.Matches(args, pattern);

            if (matches.Count == 0 ) throw new InvalidOperationException($"'{args}' cannot be parsed into a valid command line");

            var result = new List<KeyValuePair<string, string>>(matches.Count);

            foreach (Match m in matches)
            {
                if (m.Success && m.Groups.Count >= 2)
                {
                    //expected 3 groups: with [1, switch] := parameter-name and [2, value] := parameter-value 

                    result.Add(new KeyValuePair<string, string>(m.Groups[1].Value, m.Groups[2].Value));
                }
            }

            return result;
        }
    }
}
