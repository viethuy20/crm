using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;
using ServiceStack;

namespace PQT.Domain.Helpers
{
    public static class StringHelper
    {
        public static bool CaseInsensitiveEquals(this string str1, string str2)
        {
            if (String.IsNullOrWhiteSpace(str1) && String.IsNullOrWhiteSpace(str2))
                return true;

            return str1.Trim().Equals(str2.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool CaseInsensitiveContains(this IEnumerable<string> arr, string pattern)
        {
            return arr.Any(s => s.CaseInsensitiveEquals(pattern));
        }

        public static string[] Split(this string str, char separator, bool removeEmptyEntries)
        {
            StringSplitOptions options = removeEmptyEntries
                                             ? StringSplitOptions.RemoveEmptyEntries
                                             : StringSplitOptions.None;
            return str.Split(new[] { separator }, options);
        }

        public static string EncodeDecode(object _data, bool encode)
        {
            string data = _data.ToString();
            if (encode)
                try
                {
                    byte[] encDataByte = Encoding.UTF8.GetBytes(data);
                    string encodedData = Convert.ToBase64String(encDataByte);
                    return encodedData;
                }
                catch
                {
                    return "";
                }
            try
            {
                var encoder = new UTF8Encoding();
                Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecodeByte = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecodeByte, 0, todecodeByte.Length);
                var decodedChar = new char[charCount];
                utf8Decode.GetChars(todecodeByte, 0, todecodeByte.Length, decodedChar, 0);
                var result = new String(decodedChar);
                return result;
            }
            catch
            {
                return "";
            }
        }

        public static byte[] ConvertByte(string input)
        {
            return Encoding.ASCII.GetBytes(input);
        }

        public static string ConvertFromByte(byte[] input)
        {
            return Encoding.ASCII.GetString(input);
        }

        public static string SubString(string str, int index, int len)
        {
            int l = str.Length;
            int maxlen = index + len > l ? l - index : len;
            return str.Substring(index, maxlen);
        }

        public static string Ensure(object obj)
        {
            if (obj == null)
                return string.Empty;

            return Convert.ToString(obj);
        }

        public static decimal DecNormalize(decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }

        public static string NumNormalize(string s)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) return "";

            const string f = @"[^0-9.]?";
            string result = Regex.Replace(s, f, "");
            result = result.TrimStart('0', '.').TrimEnd('.');

            int pre = 0;
            if (result.Count(j => j == '.') > 1)
            {
                int i = result.IndexOf('.');
                result = Regex.Replace(result, @"[^0-9]?", "");
                result = result.Insert(i, ".").TrimEnd('0');
                pre = result.Length - 1 - i;
            }

            result = double.Parse(result).ToString("N" + pre);
            return result;
        }

        public static T Encrypt<T>(object _toEncrypt, bool isEncrypt)
        {
            string toEncrypt = _toEncrypt.ToString();
            const string securitykey = "3ecur1tyKey";
            if (isEncrypt)
            {
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

                var hashmd5 = new MD5CryptoServiceProvider();
                byte[] keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(securitykey));
                hashmd5.Clear();

                var tdes = new TripleDESCryptoServiceProvider
                {
                    Key = keyArray,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                ICryptoTransform cTransform = tdes.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                tdes.Clear();

                string value = Convert.ToBase64String(resultArray, 0, resultArray.Length);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else
            {
                byte[] toEncryptArray = Convert.FromBase64String(toEncrypt);

                var hashmd5 = new MD5CryptoServiceProvider();
                byte[] keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(securitykey));
                hashmd5.Clear();

                var tdes = new TripleDESCryptoServiceProvider
                {
                    Key = keyArray,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                tdes.Clear();

                string value = Encoding.UTF8.GetString(resultArray);
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }

        public static string InsertSpaceByUpperCase(string input)
        {
            string output = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (output.Length > 0 && Char.IsUpper(input[i]) &&
                    (!Char.IsUpper(input[i - 1]) ||
                     (i < input.Length - 1 && !Char.IsUpper(input[i + 1]))
                    ))
                {
                    output += " " + input[i];
                }
                else
                    output += input[i];
            }
            return output;
        }
        public static string UpperCaseBySpace(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            input = input.Trim();
            string output = input[0].ToString().ToUpper();
            for (int i = 1; i < input.Length; i++)
            {
                var letter = input[i];
                if (letter == ' ' && output.Length > 0)
                {
                    output += " " + input[i + 1].ToString().ToUpper();
                    i++;
                }
                else
                    output += letter;
            }
            return output;
        }
        public static string GetExcelConnection(string strFilePath)
        {
            string strConn;
            if (strFilePath.Substring(strFilePath.LastIndexOf('.')).ToLower() == ".xlsx")
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + strFilePath +
                          ";Extended Properties=\"Excel 12.0;IMEX=1\"";
            else
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strFilePath +
                          ";Extended Properties=\"Excel 8.0;IMEX=1\"";
            return strConn;
        }

        public static object ConvertYesNoToBool(string value)
        {
            if (value != null && value.Trim().ToLower() == "yes")
                return true;
            if (value != null && value.Trim().ToLower() == "no")
                return false;
            return null;
        }

        public static string RemoveSpecialCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            value = UpperCaseBySpace(value);
            var sb = new StringBuilder();
            foreach (char c in value)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public static string GetToken(string onOffSession, string apiPw, string apiUsername)
        {
            var currentDate = DateTime.Now.ToString("yyyyMMdd"); // YYYYMMDD Eg: 20160122
            return EncryptHelper.EncryptPassword(onOffSession + apiPw + currentDate + apiUsername);
        }


        public static string TimeAgo(this DateTime dateTime)
        {
            string result = string.Empty;
            var timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(10))
            {
                result = "just now";
            }
            else if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                result = string.Format("{0} seconds ago", timeSpan.Seconds);
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                result = timeSpan.Minutes > 1 ?
                    String.Format("{0} minutes ago", timeSpan.Minutes) :
                    "a minute ago";
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                result = timeSpan.Hours > 1 ?
                    String.Format("{0} hours ago", timeSpan.Hours) :
                    "an hour ago";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                result = timeSpan.Days > 1 ?
                    String.Format("{0} at {1}", dateTime.ToString("dd MMM"), dateTime.ToString("HH:mm")) :
                    "yesterday";
            }
            //else if (timeSpan <= TimeSpan.FromDays(365))
            //{
            //    result = timeSpan.Days > 30 ?
            //        String.Format("about {0} months ago", timeSpan.Days / 30) :
            //        "a month ago";
            //}
            else
            {
                result = String.Format("{0} at {1}", dateTime.ToString("dd MMM yyyy"), dateTime.ToString("HH:mm"));
            }

            return result;
        }
        public static int CountWeekends(DateTime startDate, DateTime endDate)
        {
            TimeSpan diff = endDate - startDate;
            int days = diff.Days;
            var count = 0;
            for (var i = 0; i <= days; i++)
            {
                var testDate = startDate.AddDays(i);
                switch (testDate.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        count++;
                        break;
                }
            }
            return count;
        }


        public static string NumberToWords(this decimal number)
        {
            string input = number.ToString();
            return ConvertToWords(input);
        }
        private static String ones(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = "";
            switch (_Number)
            {

                case 1:
                    name = "One";
                    break;
                case 2:
                    name = "Two";
                    break;
                case 3:
                    name = "Three";
                    break;
                case 4:
                    name = "Four";
                    break;
                case 5:
                    name = "Five";
                    break;
                case 6:
                    name = "Six";
                    break;
                case 7:
                    name = "Seven";
                    break;
                case 8:
                    name = "Eight";
                    break;
                case 9:
                    name = "Nine";
                    break;
            }
            return name;
        }
        private static String tens(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = null;
            switch (_Number)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (_Number > 0)
                    {
                        name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));
                    }
                    break;
            }
            return name;
        }
        private static String ConvertWholeNumber(String Number)
        {
            string word = "";
            try
            {
                bool beginsZero = false;//tests for 0XX  
                bool isDone = false;//test if already translated  
                double dblAmt = (Convert.ToDouble(Number));
                //if ((dblAmt > 0) && number.StartsWith("0"))  
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric  
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0;//store digit grouping  
                    String place = "";//digit grouping name:hundres,thousand,etc...  
                    switch (numDigits)
                    {
                        case 1://ones' range  

                            word = ones(Number);
                            isDone = true;
                            break;
                        case 2://tens' range  
                            word = tens(Number);
                            isDone = true;
                            break;
                        case 3://hundreds' range  
                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";
                            break;
                        case 4://thousands' range  
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " Thousand ";
                            break;
                        case 7://millions' range  
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " Million ";
                            break;
                        case 10://Billions's range  
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = " Billion ";
                            break;
                        //add extra case options for anything above Billion...  
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)  
                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                        {
                            try
                            {
                                word = ConvertWholeNumber(Number.Substring(0, pos)) + place + ConvertWholeNumber(Number.Substring(pos));
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(Number.Substring(0, pos)) + ConvertWholeNumber(Number.Substring(pos));
                        }

                        //check for trailing zeros  
                        //if (beginsZero) word = " and " + word.Trim();  
                    }
                    //ignore digit grouping names  
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch { }
            return word.Trim();
        }
        private static String ConvertToWords(String numb)
        {
            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
            String endStr = "Only";
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = "And Cents";// just to separate whole numbers from points/cents  
                        //endStr = "Paisa " + endStr;//Cents  
                        pointStr = ConvertDecimals(points);
                    }
                }
                val = String.Format("{0} {1}{2} {3}", ConvertWholeNumber(wholeNo).Trim(), andStr, pointStr, endStr);
            }
            catch { }
            return val;
        }
        private static String ConvertDecimals(String number)
        {
            String cd = "", digit = "", engOne = "";
            for (int i = 0; i < number.Length; i++)
            {
                digit = number[i].ToString();
                if (digit.Equals("0"))
                {
                    engOne = "Zero";
                }
                else
                {
                    engOne = ones(digit);
                }
                cd += " " + engOne;
            }
            return cd;
        }
    }

}
