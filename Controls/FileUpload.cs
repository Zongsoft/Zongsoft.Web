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
	public class FileUpload : DataBoundControl
	{
		#region 公共属性
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
			if(string.IsNullOrWhiteSpace(this.ID))
				throw new InvalidOperationException("The ID property is null or empty.");

			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
			writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID);
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "file");
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "display:none");
			writer.AddAttribute(HtmlTextWriterAttribute.Value, this.Value);

			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();

			if(!string.IsNullOrWhiteSpace(this.Label))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.For, this.ID + "_input");
				writer.RenderBeginTag(HtmlTextWriterTag.Label);
				writer.WriteEncodedText(this.Label);
				writer.RenderEndTag();
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID + "_input");
			writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID + "_input");
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "file");
			writer.AddAttribute(HtmlTextWriterAttribute.Value, this.Value);

			if(!this.Enabled)
				writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();
		}
		#endregion
	}
}
