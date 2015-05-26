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
	public class ImageBox : DataBoundControl
	{
		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata("src", PropertyRender = "UrlPropertyRender.Default")]
		public string ImageUrl
		{
			get
			{
				return this.GetPropertyValue(() => this.ImageUrl);
			}
			set
			{
				this.SetPropertyValue(() => this.ImageUrl, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata("href", PropertyRender = "UrlPropertyRender.Default", Renderable = false)]
		public string NavigateUrl
		{
			get
			{
				return this.GetPropertyValue(() => this.NavigateUrl);
			}
			set
			{
				this.SetPropertyValue(() => this.NavigateUrl, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata("alt")]
		public string Title
		{
			get
			{
				return this.GetPropertyValue(() => this.Title);
			}
			set
			{
				this.SetPropertyValue(() => this.Title, value);
			}
		}

		public Unit Width
		{
			get
			{
				return this.GetPropertyValue(() => this.Width);
			}
			set
			{
				this.SetPropertyValue(() => this.Width, value);
			}
		}

		public Unit Height
		{
			get
			{
				return this.GetPropertyValue(() => this.Height);
			}
			set
			{
				this.SetPropertyValue(() => this.Height, value);
			}
		}
		#endregion

		#region 生成控件
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			this.AddAttributes(writer);

			if(!Unit.IsEmpty(this.Width))
				writer.AddAttribute(HtmlTextWriterAttribute.Width, this.Width.ToString());

			if(!Unit.IsEmpty(this.Height))
				writer.AddAttribute(HtmlTextWriterAttribute.Height, this.Height.ToString());

			writer.RenderBeginTag(HtmlTextWriterTag.Img);
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			base.RenderContent(writer);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.NavigateUrl))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "image");
				writer.AddAttribute(HtmlTextWriterAttribute.Href, this.NavigateUrl);
				writer.RenderBeginTag(HtmlTextWriterTag.A);
			}

			//调用基类同名方法
			base.Render(writer);

			if(!string.IsNullOrWhiteSpace(this.NavigateUrl))
				writer.RenderEndTag();
		}
		#endregion
	}
}
