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
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class Widget : DataBoundControl, INamingContainer
	{
		#region 成员字段
		private ITemplate _headerTemplate;
		private ITemplate _footerTemplate;
		private ITemplate _bodyTemplate;
		private Image _image;
		private WidgetSettings _settings;
		#endregion

		#region 构造函数
		public Widget()
		{
			this.CssClass = "widget";
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

		[Bindable(true)]
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
					System.Threading.Interlocked.CompareExchange(ref _image, new Image(), null);

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

		#region 虚拟方法
		protected virtual void CreateHeader()
		{
			Control container = this;

			if(_settings != null && _settings.HeaderContainerRequired)
			{
				container = _settings.CreateHeaderContainer();
				this.Controls.Add(container);
			}

			if(_headerTemplate != null)
			{
				_headerTemplate.InstantiateIn(container);
				return;
			}

			var header = new Literal("h3", "ui header");
			container.Controls.Add(header);

			if(this.Image != null)
			{
				//var control = new Literal("i", "widget-header-icon icon icon-" + this.Icon.Trim().ToLowerInvariant());
				var control = this.Image.ToHtmlControl();
				header.Controls.Add(control);
			}

			var content = new Literal("div", "content");
			header.Controls.Add(content);

			if(!string.IsNullOrWhiteSpace(this.Title))
			{
				var control = new Literal("span", "widget-header-title")
				{
					Text = this.Title,
				};

				content.Controls.Add(control);
			}

			if(!string.IsNullOrWhiteSpace(this.Description))
			{
				var control = new Literal("p", "widget-header-description sub header")
				{
					Text = this.Description,
				};

				content.Controls.Add(control);
			}
		}

		protected virtual void CreateFooter()
		{
			//如果没有指定脚模板则返回（即什么也不用生成）
			if(_footerTemplate == null)
				return;

			Control container = this;

			if(_settings != null && _settings.FooterContainerRequired)
			{
				container = _settings.CreateFooterContainer();
				this.Controls.Add(container);
			}

			_footerTemplate.InstantiateIn(container);
		}

		protected virtual void CreateBody()
		{
			//如果没有指定内容模板则返回（即什么也不用生成）
			if(_bodyTemplate == null)
				return;

			Control container = this;

			if(_settings != null && _settings.BodyContainerRequired)
			{
				container = _settings.CreateBodyContainer();
				this.Controls.Add(container);
			}

			_bodyTemplate.InstantiateIn(container);
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
			this.CreateHeader();
			this.CreateBody();
			this.CreateFooter();

			//调用基类同名方法
			base.RenderContent(writer);
		}
		#endregion

		#region 嵌套子类
		[Serializable]
		public class WidgetSettings
		{
			#region 构造函数
			public WidgetSettings()
			{
				HeaderContainerTagName = "div";
				HeaderContainerCssClass = "widget-header ui top attached message";

				BodyContainerTagName = null;
				BodyContainerCssClass = "widget-body ui attached segment";

				FooterContainerTagName = "div";
				FooterContainerCssClass = "ui bottom attached segment";
			}
			#endregion

			#region 只读属性
			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public bool HeaderContainerRequired
			{
				get
				{
					return !string.IsNullOrWhiteSpace(this.HeaderContainerTagName);
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public bool BodyContainerRequired
			{
				get
				{
					return !string.IsNullOrWhiteSpace(this.BodyContainerTagName);
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public bool FooterContainerRequired
			{
				get
				{
					return !string.IsNullOrWhiteSpace(this.FooterContainerTagName);
				}
			}
			#endregion

			#region 读写属性
			public string HeaderContainerTagName
			{
				get;
				set;
			}

			public string HeaderContainerCssClass
			{
				get;
				set;
			}

			public string BodyContainerTagName
			{
				get;
				set;
			}

			public string BodyContainerCssClass
			{
				get;
				set;
			}

			public string FooterContainerTagName
			{
				get;
				set;
			}

			public string FooterContainerCssClass
			{
				get;
				set;
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
