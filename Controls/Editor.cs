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
	public class Editor : DataBoundControl
	{
		#region 构造函数
		public Editor()
		{
			this.CssClass = "editor";
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
		public string Name
		{
			get
			{
				return this.GetAttributeValue<string>("Name", this.ID);
			}
			set
			{
				this.SetAttributeValue(() => this.Name, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(true)]
		public bool Enabled
		{
			get
			{
				return this.GetAttributeValue<bool>("Enabled", true);
			}
			set
			{
				this.SetAttributeValue(() => this.Enabled, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		public string Label
		{
			get
			{
				return this.GetAttributeValue<string>("Label", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Label, value);
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

		[Bindable(true)]
		[DefaultValue("")]
		public string Text
		{
			get
			{
				return this.GetAttributeValue<string>("Text", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Text, value);
			}
		}
		#endregion

		#region 生成控件
		public override void RenderControl(HtmlTextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.Label))
			{
				if(!string.IsNullOrWhiteSpace(this.ID))
					writer.AddAttribute(HtmlTextWriterAttribute.For, this.ID);

				writer.RenderBeginTag(HtmlTextWriterTag.Label);
				writer.WriteEncodedText(this.Label);
				writer.RenderEndTag();
			}

			if(!string.IsNullOrWhiteSpace(this.ID))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);

				if(string.IsNullOrWhiteSpace(this.Name))
					writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID);
			}

			if(!string.IsNullOrWhiteSpace(this.Name))
				writer.AddAttribute(HtmlTextWriterAttribute.Name, this.Name);

			if(!this.Enabled)
				writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

			//生成其他属性
			this.RenderAttributes(writer, new string[] { "Name", "Enabled", "Label", "Text" });

			writer.RenderBeginTag(HtmlTextWriterTag.Textarea);
			writer.WriteEncodedText(this.Text);
			writer.RenderEndTag();
		}
		#endregion
	}
}
