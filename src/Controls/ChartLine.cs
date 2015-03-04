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
using System.Linq;
using System.Text;
using System.Globalization;

namespace Zongsoft.Web.Controls
{
	public class ChartLine
	{
		public string Name
		{
			get;
			set;
		}

		public string Title
		{
			get;
			set;
		}

		[TypeConverter(typeof(ValuesConvert))]
		public decimal[] Values
		{
			get;
			set;
		}
	}

	internal class ValuesConvert : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return (sourceType == typeof(string) || base.CanConvertFrom(context, sourceType));
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			string text = ((string)value).Trim();
			string[] parts = text.Split(',', ';', '|');
			List<decimal> values = new List<decimal>();

			foreach(string part in parts)
			{
				if(string.IsNullOrWhiteSpace(part))
					continue;

				decimal result;
				if(decimal.TryParse(part, (NumberStyles.Number), culture ?? CultureInfo.CurrentCulture, out result))
					values.Add(result);
			}

			return values.ToArray();
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			decimal[] values = value as decimal[];

			if(values == null || values.Length < 1)
				return string.Empty;

			string result = string.Empty;

			foreach(decimal entry in values)
			{
				result += entry.ToString() + ",";
			}

			return result.Trim(',');
		}
	}
}
