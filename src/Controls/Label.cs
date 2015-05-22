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
	public class Label : DataBoundControl
	{
		#region 构造函数
		public Label()
		{
			this.CssClass = "ui-label";
		}
		#endregion

		#region 公共属性
		[DefaultValue("")]
		public string AssociatedControlId
		{
			get
			{
				return this.GetPropertyValue(() => this.AssociatedControlId);
			}
			set
			{
				this.SetPropertyValue(() => this.AssociatedControlId, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
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

		[DefaultValue(LabelRenderMode.Label)]
		[PropertyMetadata(false)]
		public LabelRenderMode RenderMode
		{
			get
			{
				return this.GetPropertyValue(() => this.RenderMode);
			}
			set
			{
				this.SetPropertyValue(() => this.RenderMode, value);
			}
		}
		#endregion

		#region 生成控件
		protected override void Render(HtmlTextWriter writer)
		{
			HtmlTextWriterTag? tag = null;

			switch(this.RenderMode)
			{
				case LabelRenderMode.Label:
					if(!string.IsNullOrWhiteSpace(this.AssociatedControlId))
						writer.AddAttribute(HtmlTextWriterAttribute.For, this.AssociatedControlId);
					tag = HtmlTextWriterTag.Label;
					break;
				case LabelRenderMode.Span:
					tag = HtmlTextWriterTag.Span;
					break;
				case LabelRenderMode.Div:
					tag = HtmlTextWriterTag.Div;
					break;
			}

			if(tag.HasValue)
			{
				this.RenderAttributes(writer);
				writer.RenderBeginTag(tag.Value);
			}

			writer.WriteEncodedText(this.Text);

			if(tag.HasValue)
				writer.RenderEndTag();

			//调用基类同名方法
			base.Render(writer);
		}
		#endregion
	}
}
