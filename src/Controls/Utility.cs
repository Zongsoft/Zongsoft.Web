/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2013 Zongsoft Corporation <http://www.zongsoft.com>
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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Zongsoft.Web.Controls
{
	internal static class Utility
	{
		public static readonly string EmptyLink = "javascript:;";

		public static Unit GetWidth(Unit width, double totalWeight)
		{
			if(width.Type == UnitType.None)
			{
				if(totalWeight == 0)
					return Unit.Parse("100%");
				else
					return Unit.Parse(string.Format("{0}%", Math.Round(width.Value / totalWeight * 100, 4)));
			}

			return width;
		}

		public static string GetDataValue(object dataItem, string names)
		{
			if(dataItem == null)
				return string.Empty;

			if(string.IsNullOrWhiteSpace(names))
				return string.Empty;

			string result = string.Empty;
			string[] keys = names.Split(',');

			foreach(string key in keys)
			{
				if(string.IsNullOrWhiteSpace(key))
					continue;

				result += string.Format("{0},", BindingUtility.GetBindingValue(key.Trim(), dataItem, null));
			}

			if(string.IsNullOrWhiteSpace(result))
				return string.Empty;
			else
				return result.TrimEnd(',');
		}

		public static string CompressText(string text, int maxLength)
		{
			if(maxLength == 0)
				return text;

			string result = string.Empty;

			int count = 0;
			foreach(char c in text)
			{
				if(count >= maxLength)
				{
					result += "…";
					break;
				}

				if(IsChinese(c))
				{
					result += c.ToString();
					count += 2;
				}
				else
				{
					result += c.ToString();
					count += 1;
				}
			}

			return result;
		}

		public static string GetCssClass(string cssClass, string value)
		{
			if(string.IsNullOrWhiteSpace(value))
				return cssClass;

			if(string.IsNullOrWhiteSpace(cssClass))
				return value;

			return cssClass + " " + value;
		}

		public static bool IsChinese(char c)
		{
			byte[] b = System.Text.Encoding.GetEncoding("gb2312").GetBytes(c.ToString());
			return b.Length >= 2;
		}

		public static bool IsEmpty(IEnumerable enumerable)
		{
			if(enumerable == null)
				throw new ArgumentNullException("enumerable");

			return !enumerable.GetEnumerator().MoveNext();
		}
	}
}
