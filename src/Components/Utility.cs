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
using System.Web.UI;

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

		//注意：原有版本的实现会对value参数的分解之后进行去重，但是在某些场合是不能去重的，所以，现在取消去重的功能实现了。
		public static string ResolveCssClass(string value, Func<string> getCssClass)
		{
			if(string.IsNullOrWhiteSpace(value))
				return null;

			//使用 HashSet 将文本解析后的字符串数组元素进行去重处理
			//var parts = new HashSet<string>(value.ToLowerInvariant().Split(new[] { ' ', '\t', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

			//组合去重后的文本值
			//var resolvedValue = string.Join(" ", System.Linq.Enumerable.ToArray(parts));

			var parts = value.ToLowerInvariant().Split(new[] { ' ', '\t', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
			var resolvedValue = string.Join(" ", parts);

			if(value.Trim().StartsWith(":"))
			{
				var css = getCssClass();

				if(!string.IsNullOrWhiteSpace(css))
					return css.Trim() + " " + resolvedValue;
			}

			return resolvedValue;
		}

		public static IList<Control> GetVisibleChildren(System.Web.UI.Control container)
		{
			if(container == null)
				return new Control[0];

			var controls = new List<Control>();

			foreach(Control control in container.Controls)
			{
				var literal = control as LiteralControl;

				if(literal != null)
				{
					if(string.IsNullOrWhiteSpace(literal.Text))
						continue;
				}

				if(control.Visible)
					controls.Add(control);
			}

			return controls;
		}

		public static int GetVisibleChildrenCount(System.Web.UI.Control container)
		{
			if(container == null || (!container.HasControls()))
				return 0;

			int count = 0;

			foreach(System.Web.UI.Control control in container.Controls)
			{
				if(control.Visible)
					count++;
			}

			return count;
		}

		public static string GetNumberString(int number)
		{
			switch(number)
			{
				case 1:
					return "one";
				case 2:
					return "two";
				case 3:
					return "three";
				case 4:
					return "four";
				case 5:
					return "five";
				case 6:
					return "six";
				case 7:
					return "seven";
				case 8:
					return "eight";
				case 9:
					return "nine";
				case 10:
					return "ten";
				case 11:
					return "eleven";
				case 12:
					return "twelve";
				case 13:
					return "thirteen";
				case 14:
					return "fourteen";
				case 15:
					return "fifteen";
				case 16:
					return "sixteen";
			}

			return string.Empty;
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

		public static void GenerateParts(Control container, ICollection<ViewPart> parts)
		{
			if(parts == null || parts.Count < 1)
				return;

			foreach(var part in parts)
			{
				Literal wrapper = null;
				var css = Utility.ResolveCssClass(part.Alignment == HorizontalAlignment.Right ? ":right floated" : string.Empty, () => part.CssClass);

				if(string.IsNullOrWhiteSpace(part.NavigateUrl))
					wrapper = new Literal("span", css);
				else
				{
					wrapper = new Literal("a", css);
					wrapper.SetAttributeValue("href", part.NavigateUrl);
				}

				if(!string.IsNullOrWhiteSpace(part.Style))
					wrapper.SetAttributeValue("style", part.Style);

				if(part.Properties.Count > 0)
				{
					foreach(var property in part.Properties)
					{
						wrapper.SetAttributeValue(property.Name, property.Value);
					}
				}

				container.Controls.Add(wrapper);

				if(part.IconAlignment == HorizontalAlignment.Left)
				{
					if(!string.IsNullOrWhiteSpace(part.Icon))
						(wrapper ?? container).Controls.Add(new Literal("i", part.Icon + " icon"));

					(wrapper ?? container).Controls.Add(new Literal()
					{
						Text = part.Text,
					});
				}
				else
				{
					(wrapper ?? container).Controls.Add(new Literal()
					{
						Text = part.Text,
					});

					if(!string.IsNullOrWhiteSpace(part.Icon))
						(wrapper ?? container).Controls.Add(new Literal("i", part.Icon + " icon"));
				}
			}
		}
	}
}
