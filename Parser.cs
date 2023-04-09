using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace MsSqlLogParse
{
    public class Parser
    {
        #region Consts
        const string AllPattern = @"exec.*?sp_executesql.*?N'(?<sql>.*?)'.*?,.*?N'(?<paramdef>.*?)'.*?,.*?(?<paramval>.*)";
        const string InlistPattern = @"declare (?<param>@p\d+) dbo.(?<list>\w+)\s+(?<inserts>insert into.*?(?=(exec\s+|declare\s+)))";
        const string InlistInsPattern = @"insert into {0} values\((?<value>.*?(?=\)))\)";
        const char ParamDelim = ',';
        #endregion

        #region Attributes
        #endregion

        #region Constructor
        public Parser() 
        {
            
        }
        #endregion

        #region Public methods
        public string ParseClipboard()
        {
            string inputStr = Clipboard.GetText();
            if (inputStr.Length == 0)
                return null;

            Regex reMain = new Regex(AllPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            MatchCollection mcMain = reMain.Matches(inputStr);

            if (mcMain.Count == 0 || mcMain[0].Groups.Count < 4)
                return null;

            string inListSql = mcMain[0].Groups["inlist"].Value;
            string sSql = mcMain[0].Groups["sql"].Value;
            string sParamValStr = mcMain[0].Groups["paramval"].Value;
            string sParamNameStr = mcMain[0].Groups["paramdef"].Value;
            
            /* put parameter names to ParamNames */
            string[] ParamNames = sParamNameStr.Split(ParamDelim);
            Dictionary<int, string> dictParamNames = new Dictionary<int, string>();
            for (int i = 0; i < ParamNames.Length; i++)
            {
                /* Turn ParamNames to dictionary to sort order and keys further */
                dictParamNames.Add(i,ParamNames[i].Trim());
            }
            /* Sort by lenght of parameter name, and also make parameters without types */
            IEnumerable<KeyValuePair<int, string>> ParamNamesOrdered =
                dictParamNames.OrderByDescending(x => x.Value.IndexOf(" "))
                    .Select(y => new KeyValuePair<int, string>(y.Key, y.Value.Substring(0, y.Value.IndexOf(" "))));
            
            /* put parameter values to ParamVals */
            string[] ParamVals = sParamValStr.Split(ParamDelim);
            for (int i = 0; i < ParamVals.Length; i++)
            { ParamVals[i] = ParamVals[i].Trim(); }
            
            /* Put list parameters to ParamVals */
            if (inListSql.Length > 0)
            {
                Regex reInlist = new Regex(InlistPattern, RegexOptions.Singleline);
                MatchCollection mcInlist = reInlist.Matches(inputStr);
                if (mcInlist.Count > 0)
                {
                    foreach (Match mcInListItem in mcInlist)
                    {
                        string inListParam = mcInListItem.Groups["param"].Value;
                        string inListInserts = mcInListItem.Groups["inserts"].Value;
                        if (inListParam.Length > 0 && inListInserts.Length > 0)
                        {
                            string ptrn = String.Format(InlistInsPattern, inListParam);
                            Regex reInlistValues = new Regex(ptrn, RegexOptions.Singleline);
                            MatchCollection mcInlistValues = reInlistValues.Matches(inListInserts);
                            List<string> vals = new List<string>();
                            foreach (Match match in mcInlistValues)
                            {
                                string pVal = match.Groups["value"].Value;
                                if (pVal[0] == 'N' && pVal != "NULL")
                                {
                                    pVal = pVal.Substring(1);
                                }
                                vals.Add(pVal);
                            }
                            string paramval = string.Join(",", vals.ToArray());
                            for (int i = 0; i < ParamVals.Length; i++)
                            {
                                string val = ParamVals[i];
                                if (val == inListParam)
                                {
                                    ParamVals[i] = paramval;
                                    string inExpr = String.Format("(SELECT * FROM @P{0})", i + 1);
                                    sSql = sSql.Replace(inExpr, String.Format("({0})", paramval));
                                    break;
                                }
                            }
                        }
                    }

                }
            }
            
            
            /* Parameter values replacement */
            foreach (var paramName in ParamNamesOrdered)
            {
                /* try to find by name of parameter if parameters are named */
                string paramValueTrim = ParamVals.FirstOrDefault(x => x.Contains(paramName.Value+"="));
                if (paramValueTrim == null) // try to get by index
                    paramValueTrim = ParamVals[paramName.Key];
                else
                    paramValueTrim = paramValueTrim.Replace(paramName.Value + "=", "");
                
                paramValueTrim = paramValueTrim.Replace("N'", "'");
                sSql = sSql.Replace(paramName.Value, paramValueTrim);
            }

            /* Formatting */
            SqlBlock sqlObj = new SqlBlock(sSql);

            /* Set result back to clipboard */
            Clipboard.SetText(sqlObj.GetString());
            return null;
        }
        #endregion
        }
}
