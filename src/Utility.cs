/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2016-2017 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Web.
 *
 * Zongsoft.Web is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Web is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Web; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Zongsoft.Web
{
	internal static class Utility
	{
		#region 私有变量
		/*
\s*
(?<part>
	(\*)|
	(\([^\(\)]+\))|
	([^-]+)
)
\s*-?
		 */
		private static readonly Regex _regex = new Regex(@"\s*(?<part>(\*)|(\([^\(\)]+\))|([^-]+))\s*-?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
		#endregion

		public static string RepairQueryString(string path, string queryString = null)
		{
			if(string.IsNullOrWhiteSpace(queryString))
				return path;

			queryString = queryString.Trim().TrimStart('?');

			if(string.IsNullOrWhiteSpace(path))
				return "/" + (string.IsNullOrWhiteSpace(queryString) ? string.Empty : "?" + queryString);

			var index = path.IndexOf('?');

			if(index > 0)
				return path + "&" + queryString;
			else
				return path + "?" + queryString;
		}

		public static string[] Slice(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return new string[0];

			var matches = _regex.Matches(text);
			var result = new string[matches.Count];

			for(var i = 0; i < matches.Count; i++)
			{
				if(matches[i].Success)
				{
					result[i] = matches[i].Groups["part"].Value;

					if(result[i] == "*")
						result[i] = string.Empty;
				}
				else
				{
					result[i] = string.Empty;
				}
			}

			return result;
		}
	}
}
