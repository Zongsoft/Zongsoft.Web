/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace Zongsoft.Web
{
	public static class InputValidationUtility
	{
		#region 常量定义
		private static readonly Regex _regex = new Regex(@"(?'start'<\s*(?'tag'script|form|frameset)[^>]*?>)(?'content'.*?)(?'end'<\s*/\s*\k'tag'\s*>)", (RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.Singleline), TimeSpan.FromSeconds(5));
		private static readonly Regex _iframeRegex = new Regex(@"<iframe[^>]+src=[""']?(?'src'[^""']+)[""']?", (RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.Singleline), TimeSpan.FromSeconds(5));
		#endregion

		#region 公共方法
		public static string Detoxify(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return text;

			text = _regex.Replace(text, match =>
			{
				return
					EncodeHtmlTag(match.Groups["start"].Value) +
					match.Groups["content"].Value +
					EncodeHtmlTag(match.Groups["end"].Value);
			});

			text = _iframeRegex.Replace(text, match =>
			{
				var src = match.Groups["src"];

				if(src.Success && (!src.Value.Trim().StartsWith("/")))
					return match.Value.Substring(0, src.Index - match.Index) + "err-illegal-path.html?src=" + src.Value + match.Value.Substring(src.Index - match.Index + src.Length);

				return match.Value;
			});

			return text;
		}
		#endregion

		#region 私有方法
		private static string EncodeHtmlTag(string tag)
		{
			if(string.IsNullOrWhiteSpace(tag))
				return tag;

			return "&lt;" + tag.Substring(1, tag.Length - 2) + "&gt;";
		}
		#endregion
	}
}
