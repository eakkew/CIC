﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace CIC
{
    class Util
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetFilenameFromFilePath(string FilePath)
        {
            string sResult = "";
            string[] sTemp;

            if (FilePath.Substring(1, 1) != "\\")
            {
                if (System.IO.File.Exists(FilePath) == true)
                {
                    sTemp = FilePath.ToString().Split('\\');
                    if (sTemp.Length > 0)
                    {
                        sResult = sTemp[sTemp.Length - 1];
                    }
                }
            }
            else
            {
                //UNC
                sTemp = FilePath.ToString().Split('\\');
                if (sTemp.Length > 0)
                {
                    sResult = sTemp[sTemp.Length - 1];
                }
            }
            return sResult;
        }


        public static string GetFullyScreenUrl(NameValueCollection mDialerData)
        {
            string scope = "CIC::MainForm::GetFullyScreenUrl()::";
            //Tracing.TraceStatus(scope + "Starting.");
            string sRet = "";
            string[] QueryATTRValueList = Properties.Settings.Default.ScreenPop_FieldList_ATTR.Split(';');
            string[] sTemp;
            int i = 0;
            try
            {
                if (mDialerData == null)
                {
                    if (Properties.Settings.Default.StartupUrl != null)
                    {
                        if (Properties.Settings.Default.AutoCRMSignOn == true)
                        {
                            sRet = System.String.Format(Properties.Settings.Default.StartupUrl.ToString(), global::CIC.Program.mLoginParam.UserId, global::CIC.Program.mLoginParam.Password);
                        }
                        else
                        {
                            sRet = Properties.Settings.Default.StartupUrl.ToString();
                        }
                    }
                }
                else
                {
                    sTemp = new string[QueryATTRValueList.Length];
                    for (i = 0; i < QueryATTRValueList.Length; i++)
                    {
                        sTemp[i] = mDialerData[QueryATTRValueList[i]];

                    }
                    sRet = System.String.Format(Properties.Settings.Default.PopUrl.ToString(), sTemp);
                }
            }
            catch (System.Exception ex)
            {
                sRet = Properties.Settings.Default.StartupUrl.ToString();
                log.Error(scope + "Error info." + ex.Message);
            }
            return sRet;
        }

        public static bool form_validation_telephone_number(string number)
        {
            // TODO: make config file for regex to validate 4digit number, # and 4 digit number, and 9 or 10 digit
            Regex r = new Regex(@"^[\d]+\d$"); // extension      -
            Match m = r.Match(number);
            return m.Success;
        }
    }
}
