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
	[DefaultProperty("Text")]
	[ParseChildren(true, "Text")]
	[ControlBuilder(typeof(DataBoundControlBuilder))]
	public class Literal : DataBoundControl
	{
		#region 公共属性
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string TagName
		{
			get
			{
				return this.GetPropertyValue(() => this.TagName);
			}
			set
			{
				this.SetPropertyValue(() => this.TagName, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string Text
		{
			get
			{
				return this.GetPropertyValue(() => this.Text);
			}
			set
			{
				this.SetPropertyValue(() => this.Text, value);
			}
		}

		[DefaultValue(true)]
		[PropertyMetadata(false)]
		public bool TextEncoded
		{
			get
			{
				return this.GetPropertyValue(() => this.TextEncoded);
			}
			set
			{
				this.SetPropertyValue(() => this.TextEncoded, value);
			}
		}
		#endregion

		#region 生成控件
		protected override void Render(HtmlTextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.TagName))
			{
				this.RenderAttributes(writer);
				writer.RenderBeginTag(this.TagName.Trim().ToLowerInvariant());
			}

			if(this.TextEncoded)
				writer.WriteEncodedText(this.Text);
			else
				writer.Write(this.Text);

			//确认生成了所有的子控件
			this.EnsureChildControls();

			//绘制所有子控件
			this.RenderChildren(writer);

			if(!string.IsNullOrWhiteSpace(this.TagName))
				writer.RenderEndTag();
		}
		#endregion
	}
}
