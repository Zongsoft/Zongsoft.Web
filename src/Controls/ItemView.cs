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
	public class ItemView : Literal, INamingContainer
	{
		#region 成员字段
		private Image _image;
		private ITemplate _headerTemplate;
		private ITemplate _footerTemplate;
		private ITemplate _contentTemplate;
		#endregion

		#region 构造函数
		public ItemView()
		{
			this.TagName = "div";
			this.CssClass = "item";
		}
		#endregion

		#region 公共属性
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[PropertyMetadata(false)]
		public Image Image
		{
			get
			{
				if(_image == null)
					System.Threading.Interlocked.CompareExchange(ref _image, new Image(), null);

				return _image;
			}
			set
			{
				_image = value;
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
		[PropertyMetadata(false)]
		public string ContentText
		{
			get
			{
				return this.GetPropertyValue(() => this.ContentText);
			}
			set
			{
				this.SetPropertyValue(() => this.ContentText, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string HeaderTitle
		{
			get
			{
				return this.GetPropertyValue(() => this.HeaderTitle);
			}
			set
			{
				this.SetPropertyValue(() => this.HeaderTitle, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string HeaderDescription
		{
			get
			{
				return this.GetPropertyValue(() => this.HeaderDescription);
			}
			set
			{
				this.SetPropertyValue(() => this.HeaderDescription, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string FooterText
		{
			get
			{
				return this.GetPropertyValue(() => this.FooterText);
			}
			set
			{
				this.SetPropertyValue(() => this.FooterText, value);
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(ItemView))]
		public ITemplate ContentTemplate
		{
			get
			{
				return _contentTemplate;
			}
			set
			{
				_contentTemplate = value;
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(ItemView))]
		public ITemplate HeaderTemplate
		{
			get
			{
				return _headerTemplate;
			}
			set
			{
				_headerTemplate = value;
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(ItemView))]
		public ITemplate FooterTemplate
		{
			get
			{
				return _footerTemplate;
			}
			set
			{
				_footerTemplate = value;
			}
		}
		#endregion

		#region 重写方法
		protected override void RenderContent(HtmlTextWriter writer)
		{
			if(_image != null)
			{
				var cssClass = "image";

				if(this.Image.Dimension != Dimension.None)
					cssClass = this.Image.Dimension.ToString().ToLowerInvariant() + " ui image";

				var imageContainer = new Literal("div", cssClass);
				imageContainer.Controls.Add(this.Image.ToHtmlControl());
				this.Controls.Add(imageContainer);
			}

			var container = new Literal("div", "content");
			this.Controls.Add(container);

			this.GenerateContentHeader(container);
			this.GenerateContentBody(container);
			this.GenerateContentFooter(container);

			//调用基类同名方法(以确保生成所有子控件)
			base.RenderContent(writer);
		}
		#endregion

		#region 虚拟方法
		protected virtual void GenerateContentHeader(DataBoundControl container)
		{
			if(_headerTemplate != null)
			{
				_headerTemplate.InstantiateIn(container);
				return;
			}

			if(!string.IsNullOrWhiteSpace(this.HeaderTitle))
			{
				Literal headerTitle;

				if(string.IsNullOrWhiteSpace(this.NavigateUrl))
					headerTitle = new Literal("div", "header");
				else
				{
					headerTitle = new Literal("a", "header");
					headerTitle.SetAttributeValue("href", this.NavigateUrl);
				}

				headerTitle.Text = this.HeaderTitle;
				container.Controls.Add(headerTitle);
			}

			if(!string.IsNullOrWhiteSpace(this.HeaderDescription))
			{
				var control = new Literal("div", "meta")
				{
					Text = this.HeaderDescription,
				};

				container.Controls.Add(control);
			}
		}

		protected virtual void GenerateContentBody(DataBoundControl container)
		{
			if(_contentTemplate != null)
			{
				_contentTemplate.InstantiateIn(container);
				return;
			}

			if(!string.IsNullOrWhiteSpace(this.ContentText))
			{
				var control = new Literal("div", "description")
				{
					Text = this.ContentText,
				};

				container.Controls.Add(control);
			}
		}

		protected virtual void GenerateContentFooter(DataBoundControl container)
		{
			if(_footerTemplate != null)
			{
				_footerTemplate.InstantiateIn(container);
				return;
			}

			if(!string.IsNullOrWhiteSpace(this.FooterText))
			{
				var control = new Literal("div", "extra")
				{
					Text = this.FooterText,
				};

				container.Controls.Add(control);
			}
		}
		#endregion
	}
}
