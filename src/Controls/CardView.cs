/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Zongsoft.Web.Controls
{
	[DefaultProperty("Title")]
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class CardView : Widget
	{
		#region 构造函数
		public CardView()
		{
			this.CssClass = "ui card";
			this.Settings = new WidgetSettings("", "image", "", "", "", "ui bottom attached");
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
		[Localizable(true)]
		[PropertyMetadata(false)]
		public string Content
		{
			get
			{
				return this.GetPropertyValue(() => this.Content);
			}
			set
			{
				this.SetPropertyValue(() => this.Content, value);
			}
		}
		#endregion

		#region 重写方法
		protected override void CreateHeaderContent(Control container)
		{
			if(this.Image != null && !string.IsNullOrWhiteSpace(this.Image.ImageUrl))
			{
				if(string.IsNullOrWhiteSpace(this.Image.NavigateUrl))
				{
					var literal = new Literal("div", "image");
					container.Controls.Add(literal);
					container = literal;
				}

				container.Controls.Add(this.Image.ToHtmlControl());
			}
		}

		protected override void CreateBodyContent(Control container)
		{
			var content = new Literal("div", "content");
			content.Controls.Add(new Literal("h3", "header", this.Title));
			content.Controls.Add(new Literal("div", "meta", this.Description));
			content.Controls.Add(new Literal("div", "description", this.Content));

			container.Controls.Add(content);
		}
		#endregion
	}
}
