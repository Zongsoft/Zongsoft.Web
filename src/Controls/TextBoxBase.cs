/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2015 Zongsoft Corporation <http://www.zongsoft.com>
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
		public TextBoxBase()
		{
			this.InputType = InputBoxType.Text;
		}

		protected TextBoxBase(InputBoxType inputType)
		{
			this.InputType = inputType;
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue(false)]
		[PropertyMetadata(PropertyRender = "BooleanPropertyRender.True")]
		public bool ReadOnly
		{
			get
			{
				return this.GetPropertyValue(() => this.ReadOnly);
			}
			set
			{
				this.SetPropertyValue(() => this.ReadOnly, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(-1)]
		public int MaxLength
		{
			get
			{
				return this.GetPropertyValue(() => this.MaxLength);
			}
			set
			{
				this.SetPropertyValue(() => this.MaxLength, value);
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

				//必须手动更新对应的真实属性元数据的原始文本值
				this.GetProperty("Value").ValueString = value;
			}
		}
		#endregion

		#region 重写属性
		[DefaultValue(InputBoxType.Text)]
		public override InputBoxType InputType
		{
			get
			{
				return base.InputType;
			}
			set
			{
				if(value != InputBoxType.Text && value != InputBoxType.Password)
				{
					var field = typeof(InputBoxType).GetField(value.ToString());

					if(field != null)
					{
						var attribute = Attribute.GetCustomAttribute(field, typeof(CategoryAttribute));

						if(attribute == null || ((CategoryAttribute)attribute).Category != "TextBox")
							throw new ArgumentOutOfRangeException();
					}
				}

				base.InputType = value;
			}
		}
		#endregion
	}
}
