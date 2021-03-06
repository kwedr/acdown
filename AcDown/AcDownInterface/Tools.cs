﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kaedei.AcDown.Interface
{
	/// <summary>
	/// 其他工具
	/// </summary>
	public static class Tools
	{
        static Tools()
		{
			IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
			//For Testing
			//IsRunningOnMono = true;
		}

		/// <summary>
		/// 获取一个值，指示程序当前是否运行在Mono环境中
		/// </summary>
		public static bool IsRunningOnMono { get; private set; }

		/// <summary>
		/// 无效字符过滤
		/// </summary>
		/// <param name="input">需要过滤的字符串</param>
		/// <param name="replace">替换为的字符串</param>
		/// <returns></returns>
		[DebuggerNonUserCode()]
		public static string InvalidCharacterFilter(string input, string replace)
		{
			if (replace == null)
				replace = "";
			foreach (var item in System.IO.Path.GetInvalidFileNameChars())
			{
				input = input.Replace(item.ToString(), replace);
			}
			foreach (var item in System.IO.Path.GetInvalidPathChars())
			{
				input = input.Replace(item.ToString(), replace);
			}
			return input;
		}

		/// <summary>
		/// 取得网络文件的后缀名
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetExtension(string url)
		{
			string r = new Regex(@"(?<ext>\.\w{3})\?").Match(url).Groups["ext"].ToString();
			if (r == ".hlv")
				r = ".flv";
			return r;
		}

		/// <summary>
		/// 将Unicode字符转换为String(转换类似于\u1234的字符串)
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string ReplaceUnicode2Str(string input)
		{
			Regex regex = new Regex("(?i)\\\\u[0-9a-f]{4}");
			MatchEvaluator matchAction = delegate(Match m)
			{
				string str = m.Groups[0].Value;
				byte[] bytes = new byte[2];
				bytes[1] = byte.Parse(int.Parse(str.Substring(2, 2), NumberStyles.HexNumber).ToString());
				bytes[0] = byte.Parse(int.Parse(str.Substring(4, 2), NumberStyles.HexNumber).ToString());
				return Encoding.Unicode.GetString(bytes);
			};
			return regex.Replace(input, matchAction);
		}

		/// <summary>
		/// Url编码(转换为%AF这样的字符)，默认使用UTF8编码
		/// </summary>
		/// <param name="str">待转换的字符串</param>
		/// <returns></returns>
		public static string UrlEncode(string str)
		{
			return UrlEncode(str, Encoding.UTF8);
		}

		/// <summary>
		/// Url编码(转换为%AF这样的字符)
		/// </summary>
		/// <param name="str">待转换的字符串</param>
		/// <param name="encoding">使用的编码</param>
		/// <returns></returns>
		public static string UrlEncode(string str, Encoding encoding)
		{
			StringBuilder sb = new StringBuilder();
			byte[] byStr = encoding.GetBytes(str);
			for (int i = 0; i < byStr.Length; i++)
			{
				sb.Append(@"%" + Convert.ToString(byStr[i], 16));
			}
			return (sb.ToString());
		}


		/// <summary>
		/// url解码（从%AF这样的字符转换）
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string UrlDecode(string input)
		{
			string result = "";

			for (int i = 0; i < input.Length; i++)
			{
				if (input.Substring(i, 1) == "%" && input.Substring((i + 3), 1) == "%")
				{
					string bstr1 = "0x" + input.Substring(i + 1, 1) + input.Substring(i + 2, 1);
					string bstr2 = "0x" + input.Substring(i + 4, 1) + input.Substring(i + 5, 1);

					result += encode(Convert.ToByte(bstr1, 16), Convert.ToByte(bstr2, 16));
					i += 5;
				}
				else
				{
					result += input.Substring(i, 1);
				}
			}

			return result;
		}


		private static string encode(byte b1, byte b2)
		{
			System.Text.Encoding ecode = System.Text.Encoding.GetEncoding("GB18030");
			Byte[] codeBytes = { b1, b2 };
			return ecode.GetChars(codeBytes)[0].ToString();
		}

		/// <summary>
		/// 从适用于URL的Base64编码字符串转换为普通字符串
		/// </summary>
		public static string FromBase64StringForUrl(string base64String)
		{
			string temp = base64String.Replace('.', '=').Replace('*', '+').Replace('-', '/');
			return Encoding.UTF8.GetString(Convert.FromBase64String(temp));
		}

		/// <summary>
		/// 从普通字符串转换为适用于URL的Base64编码字符串
		/// </summary>
		public static string ToBase64StringForUrl(string normalString)
		{
			string temp = Convert.ToBase64String(Encoding.UTF8.GetBytes(normalString));
			return temp.Replace('+', '*').Replace('/', '-').Replace('=', '.');
		}

		/// <summary>
		/// 算出一个字符串的MD5
		/// </summary>
		public static string GetStringHash(string content)
		{
			var sb = new StringBuilder(32);
			var md5 = new MD5CryptoServiceProvider();

			var fileContent = Encoding.UTF8.GetBytes(content);

			byte[] hash = md5.ComputeHash(fileContent);

			foreach (byte b in hash)
			{
				int i = Convert.ToInt32(b);
				int j = i >> 4;
				sb.Append(Convert.ToString(j, 16));
				j = ((i << 4) & 0x00ff) >> 4;
				sb.Append(Convert.ToString(j, 16));
			}
			return sb.ToString();
		}

        internal const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        internal const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        internal const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);
        public static string ToSimplified(string source)
        {
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, source, source.Length, target, source.Length);
            return target;
        }

        public static string ToTraditional(string source)
        {
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, source, source.Length, target, source.Length);
            return target;
        }

        public static string UTF8ToBig5(string source)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            return Encoding.GetEncoding(950).GetString(Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding(950), Encoding.Unicode.GetBytes(source)));
        }

        public static string UTF8ToGB2312(string source)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            return Encoding.GetEncoding(936).GetString(Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding(936), Encoding.Unicode.GetBytes(source)));
        }

        public static string GB2312ToBig5(string source)
        {
            System.Text.Encoding gb2312 = System.Text.Encoding.GetEncoding("gb2312");
            System.Text.Encoding big5 = System.Text.Encoding.GetEncoding("big5");
            byte[] bGb2312 = gb2312.GetBytes(source);
            byte[] bBig5 = System.Text.Encoding.Convert(gb2312, big5, bGb2312);
            return big5.GetString(bBig5);
        }
    }
}
