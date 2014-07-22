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
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class TextBoxBase : InputBox
	{
		#region 构造函数
		protected TextBoxBase()
		{
		}
		#endregion

		#region 公共属性
		[DefaultValue("")]
		public string FormatString
		{
			get
			{
				return this.GetAttributeValue<string>("format", string.Empty);
			}
			set
			{
				this.SetAttributeValue<string>("format", value, false);
			}
		}

		[Bindable(true)]
		[DefaultValue(false)]
		public bool ReadOnly
		{
			get
			{
				return this.GetAttributeValue<bool>("ReadOnly", false);
			}
			set
			{
				this.SetAttributeValue(() => this.ReadOnly, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(-1)]
		public int MaxLength
		{
			get
			{
				return this.GetAttributeValue<int>("MaxLength", -1);
			}
			set
			{
				this.SetAttributeValue(() => this.MaxLength, value);
			}
		}

		public string Text
		{
			get
			{
				return base.Value;
			}
			set
			{
				base.Value = value;
			}
		}
		#endregion

		#region 重写方法
		protected override string FormatAttributeValue(string name, object value, string format)
		{
			if(string.Equals(name, "value", StringComparison.OrdinalIgnoreCase))
			{
				if(!string.IsNullOrWhiteSpace(this.FormatString))
					return base.FormatAttributeValue(name, value, this.FormatString);
			}

			return base.FormatAttributeValue(name, value, format);
		}

		protected override bool OnRenderAttribute(string name, object value, out string renderValue)
		{
			switch(name.ToLowerInvariant())
			{
				case "format":		//保持格式字符串生成的属性值不要改变大小写
					renderValue = (value == null ? string.Empty : value.ToString());
					return true;
				case "readonly":	//如果只读属性为真则返回真表示要生成该属性，否则返回假
					renderValue = "readonly";
					return Convert.ToBoolean(value);
			}

			return base.OnRenderAttribute(name, value, out renderValue);
		}
		#endregion
	}
}
