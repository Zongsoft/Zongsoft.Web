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
	public class CheckBox : InputBox
	{
		#region 构造函数
		public CheckBox()
		{
			this.Type = InputBoxType.CheckBox;
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue(false)]
		public bool Checked
		{
			get
			{
				return this.GetAttributeValue<bool>("Checked");
			}
			set
			{
				this.SetAttributeValue(() => this.Checked, value);
			}
		}
		#endregion

		#region 重写属性
		[DefaultValue(InputBoxType.CheckBox)]
		public override InputBoxType Type
		{
			get
			{
				return InputBoxType.CheckBox;
			}
			set
			{
				if(value != InputBoxType.CheckBox)
					throw new NotSupportedException();
			}
		}
		#endregion

		#region 重写方法
		protected override bool OnRenderAttribute(string name, object value, out string renderValue)
		{
			//如果选中属性为真则返回真表示生成该属性，否则返回假。
			if(string.Equals(name, "checked", StringComparison.OrdinalIgnoreCase))
			{
				renderValue = "checked";
				return Convert.ToBoolean(value);
			}

			return base.OnRenderAttribute(name, value, out renderValue);
		}
		#endregion
	}
}
