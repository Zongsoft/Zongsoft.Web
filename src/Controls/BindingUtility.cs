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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Zongsoft.Web.Controls
{
	public delegate string BindingFormatter(object value, string formatString);

	public static class BindingUtility
	{
		#region 静态变量
		private static readonly Regex BindingExpressionRegex = new Regex(@"(?<binding>\$\{(?<path>[\w\[\]\s'"".-]+)(:(?<format>.+))?\})", (RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled));
		private static readonly Regex BindingPartsRegex = new Regex(@"(?<name>\w+)(\[(?<quote>['""]?)(?<index>[^'""]+)\k<quote>\])?", (RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled));
		#endregion

		#region 公共方法
		public static BindingDescription[] GetBindings(string bindingText)
		{
			var matchs = BindingExpressionRegex.Matches(bindingText);
			var bindings = new List<BindingDescription>(matchs.Count);

			if(matchs.Count > 0)
			{
				foreach(Match match in matchs)
				{
					bindings.Add(new BindingDescription(match.Groups["path"].Value, match.Groups["format"].Value, match.Index, match.Length));
				}
			}

			return bindings.ToArray();
		}

		public static object GetBindingValue(string bindingText, object bindingSource, bool isResolvedText = false)
		{
			return GetBindingValue(bindingText, bindingSource, (Type)null, isResolvedText);
		}

		public static object GetBindingValue(string bindingText, object bindingSource, Type valueType, bool isResolvedText = false)
		{
			if(string.IsNullOrEmpty(bindingText))
				return Zongsoft.Common.Convert.ConvertValue(bindingText, valueType);

			BindingDescription[] bindings = GetBindings(bindingText);

			if(bindings.Length > 1)
				throw new ArgumentException();

			object memberValue = null;

			if(bindings.Length == 0)
			{
				if(!isResolvedText)
					return Zongsoft.Common.Convert.ConvertValue(bindingText, valueType);

				memberValue = GetMemberValue(bindingSource, bindingText, false);

				if(memberValue == null)
					memberValue = bindingText;

				return Zongsoft.Common.Convert.ConvertValue(memberValue, valueType);
			}

			var text = string.Empty;
			var position = 0;

			foreach(var binding in bindings)
			{
				if(binding.Index > 0)
					text += bindingText.Substring(position, binding.Index - position);

				memberValue = GetMemberValue(bindingSource, binding.BindingPath, true);

				if(memberValue != null)
				{
					if(string.IsNullOrWhiteSpace(binding.BindingFormat))
						memberValue = Zongsoft.Common.Convert.ConvertValue(memberValue, valueType);
					else
						memberValue = string.Format("{0:" + binding.BindingFormat + "}", memberValue);
				}

				text += memberValue;
				position = binding.Index + binding.Length;
			}

			if(valueType == typeof(string))
				return text;

			return memberValue;

			//memberValue = GetMemberValue(bindingSource, bindings[0].BindingPath, true);

			//if(memberValue == null)
			//	return null;

			//if(string.IsNullOrWhiteSpace(bindings[0].BindingFormat))
			//	return Zongsoft.Common.Convert.ConvertValue(memberValue, valueType);
			//else
			//	return string.Format("{0:" + bindings[0].BindingFormat + "}", memberValue);
		}

		public static string FormatBindingValue(string bindingText, object bindingSource, bool isResolvedText = false)
		{
			return FormatBindingValue(bindingText, bindingSource, (IFormatProvider)null, isResolvedText);
		}

		public static string FormatBindingValue(string bindingText, object bindingSource, IFormatProvider provider, bool isResolvedText = false)
		{
			return FormatBindingValue(bindingText, bindingSource, (value, format) => FormatValue(value, format, provider), isResolvedText);
		}

		public static string FormatBindingValue(string bindingText, object bindingSource, BindingFormatter format, bool isResolvedText = false)
		{
			if(string.IsNullOrWhiteSpace(bindingText))
				return bindingText;
				//return bindingSource != null ? bindingSource.ToString() : string.Empty;

			var matched = BindingExpressionRegex.IsMatch(bindingText);

			if(matched)
			{
				return BindingExpressionRegex.Replace(bindingText, match =>
				{
					if(match.Success)
					{
						object value = GetMemberValue(bindingSource, match.Groups["path"].Value, true);

						if(format == null)
							return (value == null ? string.Empty : value.ToString());
						else
							return format(value, match.Groups["format"].Value);
					}
					else
						return match.Value;
				});
			}
			else
			{
				if(isResolvedText)
				{
					object value = GetMemberValue(bindingSource, bindingText, false);

					if(value == null)
						return bindingText;

					if(format == null)
						return value.ToString();
					else
						return format(value, string.Empty);
				}

				if(format == null)
					return bindingText;
				else
					return format(bindingText, string.Empty);
			}
		}
		#endregion

		#region 内部方法
		internal static object GetBindingSource(System.Web.UI.Control control)
		{
			if(control == null)
				return null;

			if(control.DataItemContainer != null)
				return control.DataItemContainer;

			//注意：以下判断是专为MVC中的局部视图(即用户控件)发现的问题而特别处理。
			if(control.TemplateControl is System.Web.Mvc.ViewUserControl)
				return control.TemplateControl;

			return control.Page;
		}

		internal static string FormatValue(object value, string format, IFormatProvider provider)
		{
			if(value == null)
				return string.Empty;

			var enumType = Zongsoft.Common.EnumUtility.GetEnumType(value.GetType());

			if(enumType != null)
				return Zongsoft.Common.EnumUtility.Format(value, format);

			if(string.IsNullOrWhiteSpace(format))
				return string.Format(provider, "{0}", value);
			else
				return string.Format(provider, "{0:" + format + "}", value);
		}
		#endregion

		#region 私有方法
		private static object GetMemberValue(object bindingSource, string memberPath, bool throwExceptionOnMemberNotFound)
		{
			if(bindingSource == null || string.IsNullOrWhiteSpace(memberPath))
				return bindingSource;

			var matches = BindingPartsRegex.Matches(memberPath);

			if(matches == null || matches.Count < 1)
				return null;

			foreach(Match match in matches)
			{
				if(!match.Success)
					return null;

				var name = match.Groups["name"].Value;
				var key = match.Groups["index"].Value;

				PropertyDescriptor property = TypeDescriptor.GetProperties(bindingSource).Find(name, true);
				if(property == null)
				{
					if(throwExceptionOnMemberNotFound)
						throw new ArgumentException(string.Format("This '{0}@{1}' property in type name is '{2}' of component is not exists.", name, memberPath, bindingSource.GetType().FullName));
					else
						return null;
				}

				if(string.IsNullOrWhiteSpace(key))
				{
					bindingSource = property.GetValue(bindingSource);
				}
				else
				{
					Type baseType;
					Type[] baseTypes = new Type[] { typeof(IDictionary<,>), typeof(IList<>), typeof(IDictionary), typeof(IList), typeof(System.Collections.Specialized.NameValueCollection) };
					Type foundType = FindType(property.PropertyType, baseTypes, out baseType);

					if(foundType == null)
						return null;

					object propertyValue = property.GetValue(bindingSource);

					if(baseType == baseTypes[0] || baseType == baseTypes[1])
					{
						if(foundType.GetGenericArguments()[0] == typeof(string))
							bindingSource = propertyValue.GetType().GetProperty("Item", (BindingFlags.Instance | BindingFlags.Public)).GetValue(propertyValue, new object[] { key });
						else if(foundType.GetGenericArguments()[0] == typeof(int))
						{
							int index;
							if(int.TryParse(key, out index))
								bindingSource = propertyValue.GetType().GetProperty("Item", (BindingFlags.Instance | BindingFlags.Public)).GetValue(propertyValue, new object[] { index });
						}
					}
					else if(baseType == baseTypes[2])
					{
						bindingSource = ((IDictionary)propertyValue)[key];
					}
					else if(baseType == baseTypes[3])
					{
						int index;
						if(!int.TryParse(key, out index))
							throw new ArgumentException(string.Format("The value of member is '{0}'.", match.Value));

						bindingSource = ((IList)propertyValue)[index];
					}
					else if(baseType == baseTypes[4])
					{
						bindingSource = ((System.Collections.Specialized.NameValueCollection)propertyValue)[key];
					}
					else
					{
						return null;
					}
				}
			}

			return bindingSource;
		}

		private static Type FindType(Type type, IEnumerable<Type> baseTypes, out Type matchedType)
		{
			if(type == null)
				throw new ArgumentNullException("type");
			if(baseTypes == null)
				throw new ArgumentNullException("baseTypes");

			matchedType = null;

			foreach(var baseType in baseTypes)
			{
				if(baseType.IsGenericTypeDefinition)
				{
					if(type.IsGenericType && !type.IsGenericTypeDefinition)
					{
						if(type.GetGenericTypeDefinition() == baseType)
						{
							matchedType = baseType;
							return type;
						}
					}
				}
				else
				{
					if(baseType.IsAssignableFrom(type))
					{
						matchedType = baseType;
						return type;
					}
				}
			}

			foreach(var baseType in baseTypes)
			{
				if(baseType.IsInterface && baseType.IsGenericTypeDefinition)
				{
					var interfaces = type.GetInterfaces();
					foreach(var it in interfaces)
					{
						if(it.IsGenericType && it.GetGenericTypeDefinition() == baseType)
						{
							matchedType = baseType;
							return it;
						}
					}
				}
			}

			return null;
		}
		#endregion

		#region 嵌套子类
		public struct BindingDescription
		{
			private string _bindingPath;
			private string _bindingFormat;
			private int _index;
			private int _length;

			public BindingDescription(string bindingPath, string bindingFormat, int index, int length)
			{
				_index = index;
				_length = length;
				_bindingPath = bindingPath;
				_bindingFormat = bindingFormat;
			}

			public int Index
			{
				get
				{
					return _index;
				}
			}

			public int Length
			{
				get
				{
					return _length;
				}
			}

			public string BindingPath
			{
				get
				{
					return _bindingPath;
				}
			}

			public string BindingFormat
			{
				get
				{
					return _bindingFormat;
				}
			}
		}
		#endregion
	}
}
