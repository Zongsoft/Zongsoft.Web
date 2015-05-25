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
	public enum GridCommandType
	{
		Delete,
		Edit,
		Details
	}

	public class GridCommandColumn : GridColumnBase
	{
		#region 成员变量
		private bool _enabled;
		private string _text;
		private string _arguments;
		private GridCommandType _commandType;
		#endregion

		#region 构造函数
		public GridCommandColumn()
		{
			_enabled = true;
			_commandType = Controls.GridCommandType.Details;

			this.Attributes.Add("class", "grid-command");
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue(true)]
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
			}
		}

		public string Arguments
		{
			get
			{
				return _arguments;
			}
			set
			{
				_arguments = value;
			}
		}

		public GridCommandType CommandType
		{
			get
			{
				return _commandType;
			}
			set
			{
				_commandType = value;
			}
		}
		#endregion

		#region 重写方法
		protected override void OnRender(HtmlTextWriter writer, object dataItem)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Name, _commandType.ToString());
			writer.AddAttribute(HtmlTextWriterAttribute.Href, BindingUtility.FormatBindingValue(_arguments, dataItem, true));
			writer.RenderBeginTag(HtmlTextWriterTag.A);
			writer.WriteEncodedText(BindingUtility.FormatBindingValue(_text, dataItem, true));
			writer.RenderEndTag();

			//调用基类同名方法
			base.OnRender(writer, dataItem);
		}
		#endregion
	}
}
