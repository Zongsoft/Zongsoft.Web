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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Zongsoft.Web.Controls
{
	[TypeConverter(typeof(UnitConverter))]
	public struct Unit
	{
		#region 私有变量
		private static readonly Regex Regex = new Regex(@"\s*(?<value>-?\d+(\.?\d+)?)\s*(?<type>(%|\w+))?", (RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline));
		#endregion

		#region 静态常量
		public static readonly Unit Empty = new Unit(null);
		#endregion

		#region 成员变量
		private double _value;
		private UnitType _type;
		private string _unitName;
		#endregion

		#region 构造函数
		public Unit(string text)
		{
			_value = 0;
			_type = UnitType.None;
			_unitName = string.Empty;

			if(!string.IsNullOrWhiteSpace(text))
			{
				var match = Unit.Regex.Match(text);

				if(match.Success)
				{
					double.TryParse(match.Groups["value"].Value, out _value);
					_unitName = match.Groups["type"].Value.Trim();
				}
			}

			_type = GetUnitType(_unitName);
		}
		#endregion

		#region 公共属性
		public double Value
		{
			get
			{
				return _value;
			}
		}

		public UnitType Type
		{
			get
			{
				return _type;
			}
		}
		#endregion

		#region 重写方法
		public override string ToString()
		{
			return string.Format("{0}{1}", _value, _unitName);
		}
		#endregion

		#region 静态方法
		public static bool IsEmpty(Unit value)
		{
			return (value.Value == 0d);
		}

		public static Unit Parse(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
				return Unit.Empty;
			else
				return new Unit(value);
		}
		#endregion

		#region 私有方法
		private static UnitType GetUnitType(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return UnitType.None;

			text = text.Trim().ToLowerInvariant();

			switch(text)
			{
				case "em":
					return UnitType.Em;
				case "%":
					return UnitType.Percentage;
				case "px":
					return UnitType.Pixel;
				default:
					return UnitType.Unknown;
			}
		}
		#endregion
	}
}
