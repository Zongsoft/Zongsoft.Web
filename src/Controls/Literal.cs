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
	[DefaultProperty("Text")]
	[ParseChildren(true, "Text")]
	[ControlBuilder(typeof(DataBoundControlBuilder))]
	public class Literal : DataBoundControl
	{
		#region 公共属性
		[DefaultValue("")]
		public string TagName
		{
			get
			{
				return this.GetAttributeValue<string>("TagName", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.TagName, value);
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

		[DefaultValue(true)]
		public bool TextEncoded
		{
			get
			{
				return this.GetAttributeValue<bool>("TextEncoded", true);
			}
			set
			{
				this.SetAttributeValue(() => this.TextEncoded, value);
			}
		}
		#endregion

		#region 生成控件
		public override void RenderControl(HtmlTextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.TagName))
			{
				this.RenderAttributes(writer, new string[] { "Tag", "Text", "TextEncoded" });
				writer.RenderBeginTag(this.TagName.Trim().ToLowerInvariant());
			}

			this.EnsureChildControls();

			if(this.TextEncoded)
				writer.WriteEncodedText(this.Text);
			else
				writer.Write(this.Text);

			if(!string.IsNullOrWhiteSpace(this.TagName))
				writer.RenderEndTag();
		}
		#endregion
	}
}
