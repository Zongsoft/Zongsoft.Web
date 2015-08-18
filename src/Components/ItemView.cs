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
using System.ComponentModel;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	[DefaultProperty("Title")]
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class ItemView : Widget
	{
		#region 成员字段
		private ViewPartCollection _headerParts;
		private ViewPartCollection _footerParts;
		#endregion

		#region 构造函数
		public ItemView()
		{
			this.TagName = "div";
			this.CssClass = "ui item";
			this.Settings = new WidgetSettings("", "image", "div", "content", "div", "extra");
		}
		#endregion

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

		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ViewPartCollection HeaderParts
		{
			get
			{
				if(_headerParts == null)
					_headerParts = new ViewPartCollection(this);

				return _headerParts;
			}
		}

		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ViewPartCollection FooterParts
		{
			get
			{
				if(_footerParts == null)
					_footerParts = new ViewPartCollection(this);

				return _footerParts;
			}
		}
		#endregion

		#region 重写方法
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.TagName))
			{
				this.AddAttributes(writer);
				writer.RenderBeginTag(this.TagName);
			}
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.TagName))
				writer.RenderEndTag();
		}

		protected override void CreateHeaderContent(Control container)
		{
			if(this.Image != null)
			{
				if(string.IsNullOrWhiteSpace(this.Image.NavigateUrl))
				{
					var imageCss = "ui top aligned image";

					if(this.Image.Dimension != Dimension.None && !string.IsNullOrWhiteSpace(this.Image.ImageUrl))
						imageCss = "ui top aligned " + this.Image.Dimension.ToString() + " image";

					var literal = new Literal("div", imageCss);
					container.Controls.Add(literal);
					container = literal;
				}

				container.Controls.Add(this.Image.ToHtmlControl());
			}
		}

		protected override void CreateBodyContent(Control container)
		{
			if(string.IsNullOrWhiteSpace(this.NavigateUrl))
				container.Controls.Add(new Literal("h3", "header", this.Title));
			else
				container.Controls.Add(new Literal("a", "header", this.Title, new KeyValuePair<string, string>("href", this.ResolveUrl(this.NavigateUrl))));

			if(_headerParts != null && _headerParts.Count > 0)
			{
				//var meta = new Literal("div", "meta");
				//container.Controls.Add(meta);
				//Utility.GenerateParts(meta, _headerParts);

				var metas = Utility.GenerateParts(_headerParts, part => new Literal("div", "meta"));
				Utility.AddRange(container, metas);
			}

			container.Controls.Add(new Literal("div", "description", this.Description)
			{
				TextEncoded = this.TextEncoded,
			});
		}

		protected override void CreateFooterContent(Control container)
		{
			if(_footerParts != null && _footerParts.Count > 0)
			{
				//Utility.GenerateParts(container, _footerParts);

				var extras = Utility.GenerateParts(_footerParts, part => new Literal("div", "extra"));
				Utility.AddRange(container, extras);
			}
		}

		protected override void CreateFooter(Control container)
		{
			container = this.BodyContainer;

			if(this.FooterTemplate != null)
				this.CreateFooterContent(this.FooterContainer ?? container);

			base.CreateFooter(container);
		}

		protected override bool FooterContainerRequired
		{
			get
			{
				return base.FooterContainerRequired && (this.FooterTemplate != null || (_footerParts != null && _footerParts.Count > 0));
			}
		}
		#endregion
	}
}
