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
	///		<div class="widget-header">
	///			<i class="widget-header-icon" />
	///			<h3 class="widget-header-text">{title}</h3>
	///			<p class="widget-header-description">{description}</p>
	///		</div>
	///		<div class="widget-body">
	///		{ContentTemplate}
	///		</div>
	///		<div class=widget-footer">
	///		{FooterTemplate}
	///		</div>
	///	</div>
	///	]]>
	/// </remarks>
	[DefaultProperty("Title")]
	[ParseChildren(true)]
	public class Widget : DataBoundControl, INamingContainer
	{
		#region 成员变量
		private ITemplate _headerTemplate;
		private ITemplate _footerTemplate;
		private ITemplate _bodyTemplate;
		#endregion

		#region 构造函数
		public Widget()
		{
			this.CssClass = "widget";
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(false)]
		[PropertyMetadata(false)]
		public string Icon
		{
			get
			{
				return this.GetPropertyValue(() => this.Icon);
			}
			set
			{
				this.SetPropertyValue(() => this.Icon, value);
			}
		}

		[Bindable(true)]
		[Category("Appearance")]
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
		[Category("Appearance")]
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
			HtmlGenericControl container = new HtmlGenericControl("div");
			container.Attributes.Add("class", "widget-header");

			if(_headerTemplate == null)
			{
				HtmlControl control;

				if(!string.IsNullOrWhiteSpace(this.Icon))
				{
					control = new HtmlGenericControl("i");
					control.Attributes.Add("class", "widget-header-icon icon icon-" + this.Icon.Trim().ToLowerInvariant());
					container.Controls.Add(control);
				}

				if(!string.IsNullOrWhiteSpace(this.Title))
				{
					control = new HtmlGenericControl("h3")
					{
						InnerText = this.Title,
					};

					control.Attributes.Add("class", "widget-header-title");
					container.Controls.Add(control);
				}

				if(!string.IsNullOrWhiteSpace(this.Description))
				{
					control = new HtmlGenericControl("p")
					{
						InnerText = this.Description,
					};

					control.Attributes.Add("class", "widget-header-description");
					container.Controls.Add(control);
				}
			}
			else
			{
				_headerTemplate.InstantiateIn(container);
			}

			this.Controls.Add(container);
		}

		protected virtual void CreateFooter()
		{
			//如果没有指定脚模板则返回（即什么也不用生成）
			if(_footerTemplate == null)
				return;

			HtmlGenericControl container = new HtmlGenericControl("div");
			container.Attributes.Add("class", "widget-footer");

			_footerTemplate.InstantiateIn(container);

			this.Controls.Add(container);
		}

		protected virtual void CreateBody()
		{
			HtmlGenericControl container = new HtmlGenericControl("div");
			container.Attributes.Add("class", "widget-body");

			if(_bodyTemplate != null)
				_bodyTemplate.InstantiateIn(container);

			this.Controls.Add(container);
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
	}
}
