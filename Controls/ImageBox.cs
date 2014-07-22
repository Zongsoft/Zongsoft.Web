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
	public class ImageBox : DataBoundControl
	{
		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
		public string ImageUrl
		{
			get
			{
				return this.GetAttributeValue<string>("ImageUrl", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.ImageUrl, value ?? string.Empty);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		public string NavigateUrl
		{
			get
			{
				return this.GetAttributeValue<string>("NavigateUrl", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.NavigateUrl, value ?? string.Empty);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		public string Title
		{
			get
			{
				return this.GetAttributeValue<string>("Title", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Title, value ?? string.Empty);
			}
		}

		public Unit Width
		{
			get
			{
				return this.GetAttributeValue<Unit>("Width", Unit.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Width, value);
			}
		}

		public Unit Height
		{
			get
			{
				return this.GetAttributeValue<Unit>("Height", Unit.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Height, value);
			}
		}
		#endregion

		#region 生成控件
		public override void RenderControl(HtmlTextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.NavigateUrl))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Href, this.NavigateUrl);
				writer.RenderBeginTag(HtmlTextWriterTag.A);
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Src, this.ImageUrl);
			writer.AddAttribute(HtmlTextWriterAttribute.Alt, this.Title);

			if(!Unit.IsEmpty(this.Width))
				writer.AddAttribute(HtmlTextWriterAttribute.Width, this.Width.ToString());

			if(!Unit.IsEmpty(this.Height))
				writer.AddAttribute(HtmlTextWriterAttribute.Height, this.Height.ToString());

			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();

			if(!string.IsNullOrWhiteSpace(this.NavigateUrl))
				writer.RenderEndTag();
		}
		#endregion
	}
}
