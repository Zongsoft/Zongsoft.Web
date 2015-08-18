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
	/// <summary>
	/// 关于Web部件的控件。
	/// </summary>
	/// <remarks>
	///	<![CDATA[
	///	<div class="widget">
	///		<div class="widget-header ui top attached message">
	///			<h3 class="ui header">
	///				<i class="widget-header-icon icon"></i>
	///				<div class="content">
	///					{title}
	///					<p class="widget-header-description sub header">{description}</p>
	///				</div>
	///			</h3>
	///		</div>
	///		<div class="widget-body ui attached segment">
	///		{ContentTemplate}
	///		</div>
	///		<div class=widget-footer ui bottom attached segment">
	///		{FooterTemplate}
	///		</div>
	///	</div>
	///	]]>
	/// </remarks>
	[DefaultProperty("Title")]
	[ParseChildren(true)]
	[PersistChildren(true)]
	public class Widget : DataBoundControl, INamingContainer
	{
		#region 成员字段
		private ITemplate _headerTemplate;
		private ITemplate _footerTemplate;
		private ITemplate _bodyTemplate;
		private Control _headerContainer;
		private Control _footerContainer;
		private Control _bodyContainer;
		private Image _image;
		private WidgetSettings _settings;
		#endregion

		#region 构造函数
		public Widget()
		{
			this.CssClass = "widget";
			_settings = new WidgetSettings("div", "widget-header ui top attached message", "div", "widget-body ui attached segment", "", "ui bottom attached segment");
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
		[Localizable(true)]
		[PropertyMetadata(false)]
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

		[Bindable(true)]
		[DefaultValue("")]
		[Localizable(true)]
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
		[Localizable(true)]
		[PropertyMetadata(false)]
		public string Description
		{
			get
			{
				return this.GetPropertyValue(() => this.Description);
			}
			set
			{
				this.SetPropertyValue(() => this.Description, value);
			}
		}

		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string Icon
		{
			get
			{
				return _image == null ? null : _image.Icon;
			}
			set
			{
				this.Image.Icon = value;
			}
		}

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[PropertyMetadata(false)]
		public Image Image
		{
			get
			{
				if(_image == null)
					System.Threading.Interlocked.CompareExchange(ref _image, new Image(this), null);

				return _image;
			}
			set
			{
				_image = value;
			}
		}

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public WidgetSettings Settings
		{
			get
			{
				return _settings;
			}
			set
			{
				_settings = value;
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(Widget))]
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
		[TemplateContainerAttribute(typeof(Widget))]
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

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(Widget))]
		public ITemplate BodyTemplate
		{
			get
			{
				return _bodyTemplate;
			}
			set
			{
				_bodyTemplate = value;
			}
		}
		#endregion

		#region 保护属性
		protected virtual Control HeaderContainer
		{
			get
			{
				if(_headerContainer == null && this.HeaderContainerRequired)
					_headerContainer = _settings.CreateHeaderContainer();

				return _headerContainer;
			}
		}

		protected virtual Control BodyContainer
		{
			get
			{
				if(_bodyContainer == null && this.BodyContainerRequired)
					_bodyContainer = _settings.CreateBodyContainer();

				return _bodyContainer;
			}
		}

		protected virtual Control FooterContainer
		{
			get
			{
				if(_footerContainer == null && this.FooterContainerRequired)
					_footerContainer = _settings.CreateFooterContainer();

				return _footerContainer;
			}
		}
		#endregion

		#region 重写方法
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			this.AddAttributes(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			this.CreateHeader(this);
			this.CreateBody(this);
			this.CreateFooter(this);

			//调用基类同名方法
			base.RenderContent(writer);
		}
		#endregion

		#region 虚拟方法
		protected virtual void CreateHeader(Control container)
		{
			var header = this.HeaderContainer;

			if(header != null)
			{
				container.Controls.Add(header);
				container = header;
			}

			if(_headerTemplate != null)
				_headerTemplate.InstantiateIn(container);
			else
				this.CreateHeaderContent(container);
		}

		protected virtual void CreateHeaderContent(Control container)
		{
			var header = new Literal("h3", "ui header");
			container.Controls.Add(header);

			if(this.Image != null)
			{
				var control = this.Image.ToHtmlControl();
				header.Controls.Add(control);
			}

			var content = new Literal("div", "content");
			header.Controls.Add(content);

			if(!string.IsNullOrWhiteSpace(this.Title))
			{
				var control = new Literal("a", "widget-header-title")
				{
					Text = this.Title,
					TextEncoded = this.TextEncoded,
				};

				if(!string.IsNullOrWhiteSpace(this.NavigateUrl))
					control.SetAttributeValue("href", this.ResolveUrl(this.NavigateUrl));

				content.Controls.Add(control);
			}

			if(!string.IsNullOrWhiteSpace(this.Description))
			{
				var control = new Literal("p", "widget-header-description sub header")
				{
					Text = this.Description,
					TextEncoded = this.TextEncoded,
				};

				content.Controls.Add(control);
			}
		}

		protected virtual void CreateBody(Control container)
		{
			var body = this.BodyContainer;

			if(body != null)
			{
				container.Controls.Add(body);
				container = body;
			}

			if(_bodyTemplate != null)
				_bodyTemplate.InstantiateIn(container);
			else
				this.CreateBodyContent(container);
		}

		protected virtual void CreateBodyContent(Control container)
		{
		}

		protected virtual void CreateFooter(Control container)
		{
			var footer = this.FooterContainer;

			if(footer != null)
			{
				container.Controls.Add(footer);
				container = footer;
			}

			if(_footerTemplate != null)
				_footerTemplate.InstantiateIn(container);
			else
				this.CreateFooterContent(container);
		}

		protected virtual void CreateFooterContent(Control container)
		{
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual bool HeaderContainerRequired
		{
			get
			{
				return _settings != null && !string.IsNullOrWhiteSpace(_settings.HeaderContainerTagName);
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual bool BodyContainerRequired
		{
			get
			{
				return _settings != null && !string.IsNullOrWhiteSpace(_settings.BodyContainerTagName);
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual bool FooterContainerRequired
		{
			get
			{
				return _settings != null && !string.IsNullOrWhiteSpace(_settings.FooterContainerTagName);
			}
		}
		#endregion

		#region 嵌套子类
		[Serializable]
		public class WidgetSettings
		{
			#region 成员字段
			private string _headerContainerTagName;
			private string _headerContainerCssClass;
			private string _bodyContainerTagName;
			private string _bodyContainerCssClass;
			private string _footerContainerTagName;
			private string _footerContainerCssClass;
			#endregion

			#region 构造函数
			public WidgetSettings()
			{
			}

			public WidgetSettings(string headerContainerTagName, string headerContainerCssClass,
			                      string bodyContainerTagName, string bodyContainerCssClass,
			                      string footerContainerTagName, string footerContainerCssClass)
			{
				_headerContainerTagName = headerContainerTagName;
				_headerContainerCssClass = headerContainerCssClass;
				_bodyContainerTagName = bodyContainerTagName;
				_bodyContainerCssClass = bodyContainerCssClass;
				_footerContainerTagName = footerContainerTagName;
				_footerContainerCssClass = footerContainerCssClass;
			}
			#endregion

			#region 读写属性
			public string HeaderContainerTagName
			{
				get
				{
					return _headerContainerTagName;
				}
				set
				{
					_headerContainerTagName = value;
				}
			}

			public string HeaderContainerCssClass
			{
				get
				{
					return _headerContainerCssClass;
				}
				set
				{
					_headerContainerCssClass = value;
				}
			}

			public string BodyContainerTagName
			{
				get
				{
					return _bodyContainerTagName;
				}
				set
				{
					_bodyContainerTagName = value;
				}
			}

			public string BodyContainerCssClass
			{
				get
				{
					return _bodyContainerCssClass;
				}
				set
				{
					_bodyContainerCssClass = value;
				}
			}

			public string FooterContainerTagName
			{
				get
				{
					return _footerContainerTagName;
				}
				set
				{
					_footerContainerTagName = value;
				}
			}

			public string FooterContainerCssClass
			{
				get
				{
					return _footerContainerCssClass;
				}
				set
				{
					_footerContainerCssClass = value;
				}
			}
			#endregion

			#region 内部方法
			internal Literal CreateHeaderContainer()
			{
				if(string.IsNullOrWhiteSpace(this.HeaderContainerTagName))
					return null;

				return new Literal(this.HeaderContainerTagName, this.HeaderContainerCssClass);
			}

			internal Literal CreateBodyContainer()
			{
				if(string.IsNullOrWhiteSpace(this.BodyContainerTagName))
					return null;

				return new Literal(this.BodyContainerTagName, this.BodyContainerCssClass);
			}

			internal Literal CreateFooterContainer()
			{
				if(string.IsNullOrWhiteSpace(this.FooterContainerTagName))
					return null;

				return new Literal(this.FooterContainerTagName, this.FooterContainerCssClass);
			}
			#endregion
		}
		#endregion
	}
}
