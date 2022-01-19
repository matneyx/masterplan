using System;
using System.IO;

namespace Masterplan.Tools
{
    /// <summary>
    ///     Class containing static methods and properties used for diagnostic logging.
    /// </summary>
    public class LogSystem
    {
        /// <summary>
        ///     Gets or sets the path of the current logfile.
        ///     If this is null or empty, no logfile is defined.
        /// </summary>
        public static string LogFile { get; set; } = "";

        /// <summary>
        ///     Gets or sets a value indicating the current level of indentation.
        /// </summary>
        public static int Indent { get; set; }

        /// <summary>
        ///     Sends a message to the console (and also to the logfile if one is defined).
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        public static void Trace(string message)
        {
            try
            {
                // Indent the message
                var str = "";
                for (var a = 0; a < Indent; ++a)
                    str += "\t";
                str += message + Environment.NewLine;

                // Write the message
                Console.Write(str);
                if (LogFile != null && LogFile != "")
                    try
                    {
                        var line = DateTime.Now + "\t" + str;
                        File.AppendAllText(LogFile, line);
                    }
                    catch
                    {
                    }
            }
            catch
            {
            }
        }

        /// <summary>
        ///     Traces an object to the console (and also to the logfile if one is defined).
        /// </summary>
        /// <param name="obj">The object to be traced.</param>
        public static void Trace(object obj)
        {
            try
            {
                Trace(obj.ToString());
            }
            catch
            {
            }
        }

        /// <summary>
        ///     Traces an exception (the exception message, stack trace and inner exceptions) to the console (and also to the
        ///     logfile if one is defined).
        /// </summary>
        /// <param name="ex">The exception to be traced.</param>
        public static void Trace(Exception ex)
        {
            try
            {
                Trace(ex.Message);
                Trace(ex.StackTrace);

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;

                    Indent += 1;
                    Trace(ex);
                    Indent -= 1;
                }
            }
            catch
            {
            }
        }
    }
}
