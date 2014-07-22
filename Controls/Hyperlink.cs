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
using System.Web;
using System.Web.UI;
using System.Web.Mvc;

namespace Zongsoft.Web.Controls
{
	public class Hyperlink : DataBoundControl
	{
		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
		public string Url
		{
			get
			{
				return this.GetAttributeValue<string>("Url", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Url, value);
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
			//生成其他Attributes，并忽略URL属性
			this.RenderAttributes(writer, "Url");
			//生成特定的URL属性
			writer.AddAttribute(HtmlTextWriterAttribute.Href, this.ResolveUrl(this.Url));

			writer.RenderBeginTag(HtmlTextWriterTag.A);
			writer.WriteEncodedText(this.Text);
			writer.RenderEndTag();
		}
		#endregion
	}
}
