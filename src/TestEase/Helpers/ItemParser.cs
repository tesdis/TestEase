using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using TestEase.LibrarItemDictionaries;

namespace TestEase.Helpers
{
    public static class ItemParser
    {
        private const string JsonStylePropertyPattern = @"                
                \s*
                ([A-Za-z0-9_]+)\s*                                      (?# The property name)
                :                                                       (?# Property name and value are separated by a colon)
                \s*((\d+\.?(\d+)?|'.*'|"".*""))         (?# The property value. It can be a string with quotes, a number with or without a decimal, or true/false, or null)
                ,?\s*                                                   (?# Multiple property name/value pairs can be separated by commas)                
            ";

        public static string Parse(string sqlCode, IDictionary<string, object> replacementValues,
            BaseItemDictionary itemDictionary)
        {
            return
                ProcessReplacementValues(
                    ProcessIncludeStatements(ProcessReplacementValues(sqlCode, replacementValues), itemDictionary), replacementValues);
        }

        private static string ProcessReplacementValues(string libraryItemText, IDictionary<string, object> replacements)
        {
            //Get the defaults if there are any
            var defaults = GetDefaultsReplacementValues(libraryItemText);
            var finalReplacementValues = defaults.Item1;
            libraryItemText = defaults.Item2;

            //Override the defaults with any that were actually passed in
            var overrideReplacementValues = replacements ?? new ExpandoObject();

            foreach (var key in overrideReplacementValues.Keys)
            {
                finalReplacementValues[key] = overrideReplacementValues[key];
            }

            var replacementRegex = new Regex(@"\{(\w+)\}");
            var replacementValuesInItemText = new HashSet<string>();

            var text = libraryItemText;
            libraryItemText = replacementRegex.Replace(libraryItemText, match =>
            {
                var key = "";
                var replacementValueName = match.Groups[1].Value;

                replacementValuesInItemText.Add(key);

                foreach (var replacementKvp in finalReplacementValues)
                {
                    if (replacementKvp.Key.ToLower() == replacementValueName.ToLower())
                    {
                        key = replacementKvp.Key;
                    }
                }

                if (key == "")
                {
                    throw new InvalidOperationException(
                        string.Format("A replacement value was not specified for {0}. \n\n {1}", replacementValueName,
                            text));
                }

                string newValue;

                try
                {
                    newValue = finalReplacementValues[key].ToString();
                }
                catch (Exception)
                {
                    throw new InvalidOperationException(
                        string.Format(@"Replacement value ""{0}"" could not be converted into a string. \n\n {1}",
                            replacementValueName, text));
                }

                return newValue;
            });

            return libraryItemText;
        }
        private static string ProcessIncludeStatements(string libraryItemText, BaseItemDictionary itemDictionary)
        {
            var includeStatementPattern = @"
                    INCLUDE 

                    \s+
                        ([A-Za-z0-9_\.]+)       (?# The name of the item from the Test Data Library. Can contain alpha-numeric characters, underscores, and periods.)
                    \s*

                    (                           (?# Can have a comma followed by JSON-style objects to specify params)
                        ,\s*
                        \[?                     (?# JSON-style array brackets are optional when specifying multiple JSON-style objects)
                            (                   (?# This group matches zero, one, or more JSON-style objects)
                                \s*
                                \{
                                    (
                                        " + JsonStylePropertyPattern + @"
                                    )+
                                \},?
                                \s*
                            )*                  
                        \]?
                    )?

                ";

            var includeStatementRegex = new Regex(includeStatementPattern,
                RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

            return includeStatementRegex.Replace(libraryItemText, includeStatementMatch =>
            {
                var includedLibraryItemName = includeStatementMatch.Groups[1].Value;
                string includeLibraryItem;

                try
                {
                    includeLibraryItem = itemDictionary[includedLibraryItemName].LibraryItemText;
                }
                catch (KeyNotFoundException)
                {
                    throw new InvalidOperationException(
                        $"The library item \"{includedLibraryItemName}\" that is part of an include statement was not found.\n\n{libraryItemText}");
                }

                string newIncludeLibraryItem;

                //Are there any JSON-style replacement values specified?
                if (includeStatementMatch.Groups[3].Captures.Count > 0)
                {
                    //JSON-style replacement values were specified. If more than one was specified,
                    //then each one is like a separate include statement.
                    var allIncludes = new StringBuilder();

                    foreach (Capture capture in includeStatementMatch.Groups[3].Captures)
                    {
                        allIncludes.AppendLine(ProcessReplacementValues(includeLibraryItem,
                            GetReplacementValuesFromJsonString(capture.Value)));
                    }

                    newIncludeLibraryItem = allIncludes.ToString();
                }
                else
                {
                    //Just a plain INCLUDE statement with no JSON-style replacement values. E.g.: INCLUDE Some.Stuff
                    newIncludeLibraryItem = ProcessReplacementValues(includeLibraryItem, null);
                }

                if (Debugger.IsAttached)
                {
                    newIncludeLibraryItem = "\r\n\r\n--INCLUDED from " + includedLibraryItemName + "\r\n\r\n" +
                                            newIncludeLibraryItem + "\r\n\r\n--END INCLUDE\r\n\r\n";
                }

                //Recursively process all include statements
                return ProcessIncludeStatements(newIncludeLibraryItem, itemDictionary);
            });
        }
        private static Tuple<IDictionary<string, object>, string> GetDefaultsReplacementValues(string libraryItemText)
        {
            IDictionary<string, object> replacementValues = new ExpandoObject();

            const string defaultsPattern = @"
                    DEFAULTS 
                    \s*:\s*
                    (
                        \{
                            (
                            " + JsonStylePropertyPattern + @"
                            )+
                        \}
                    )
                ";

            var newSqlScript = libraryItemText;

            var defaultsRegex = new Regex(defaultsPattern, RegexOptions.IgnorePatternWhitespace);

            if (!defaultsRegex.IsMatch(libraryItemText)) return Tuple.Create(replacementValues, newSqlScript);

            newSqlScript = defaultsRegex.Replace(libraryItemText, "");
            replacementValues = GetReplacementValuesFromJsonString(defaultsRegex.Match(libraryItemText).Groups[1].Value);

            return Tuple.Create(replacementValues, newSqlScript);
        }
        private static IDictionary<string, object> GetReplacementValuesFromJsonString(string replacementObjectJson)
        {
            var jsonObjectRegex = new Regex(JsonStylePropertyPattern, RegexOptions.IgnorePatternWhitespace);

            IDictionary<string, object> replacementValues = new ExpandoObject();

            foreach (Match match in jsonObjectRegex.Matches(replacementObjectJson))
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;

                object newValue = null;

                if ((value.StartsWith("'") || value.StartsWith("\"")) && (value.EndsWith("'") || value.EndsWith("\"")))
                {
                    newValue = value.Substring(1, value.Length - 2);
                }
                else
                {
                    if (bool.TryParse(value, out var boolVal))
                    {
                        newValue = boolVal;
                    }
                    else if (int.TryParse(value, out var intVal))
                    {
                        newValue = intVal;
                    }
                    else if (double.TryParse(value, out var doubleVal))
                    {
                        newValue = doubleVal;
                    }
                    else if (value == "null")
                    {
                    }
                }

                replacementValues[key] = newValue;
            }

            return replacementValues;
        }
    }
}
