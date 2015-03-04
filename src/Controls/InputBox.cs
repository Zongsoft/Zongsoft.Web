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
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class InputBox : DataBoundControl
	{
		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
		public string Name
		{
			get
			{
				var name = this.GetAttributeValue<string>("Name", string.Empty);

				if(string.IsNullOrWhiteSpace(name))
					return this.ID ?? string.Empty;

				return name;
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

		[DefaultValue(InputBoxType.Text)]
		public virtual InputBoxType Type
		{
			get
			{
				return this.GetAttributeValue<InputBoxType>("Type", InputBoxType.Text);
			}
			set
			{
				this.SetAttributeValue(() => this.Type, value);
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
		[DefaultValue("")]
		public string Value
		{
			get
			{
				return this.GetAttributeValue<string>("Value", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Value, value);
			}
		}
		#endregion

		#region 重写方法
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

			writer.AddAttribute(HtmlTextWriterAttribute.Type, this.Type.ToString().ToLowerInvariant());

			if(!this.Enabled)
				writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

			//生成其他属性
			this.RenderAttributes(writer, new string[] { "Name", "Enabled", "Label", "Type" });

			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();
		}
		#endregion
	}
}
