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
	///		<div class="widget-header"><h3>{title}</h3></div>
	///		<div class="widget-content">
	///		{ContentTemplate}
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
		private ITemplate _contentTemplate;
		#endregion

		#region 公共属性
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(true)]
		public string Title
		{
			get
			{
				return this.GetAttributeValue<string>("Title", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Title, value);
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
		#endregion

		#region 虚拟方法
		protected virtual void CreateHeader()
		{
			HtmlGenericControl container = new HtmlGenericControl("div");
			container.Attributes.Add("class", "widget-header");

			if(_headerTemplate == null)
			{
				container.Controls.Add(new HtmlGenericControl("h3")
				{
					InnerText = this.Title,
				});
			}
			else
			{
				_headerTemplate.InstantiateIn(container);
			}

			this.Controls.Add(container);
		}

		protected virtual void CreateContent()
		{
			HtmlGenericControl container = new HtmlGenericControl("div");
			container.Attributes.Add("class", "widget-content");

			if(_contentTemplate != null)
				_contentTemplate.InstantiateIn(container);

			this.Controls.Add(container);
		}
		#endregion

		#region 重写方法
		protected override void CreateChildControls()
		{
			this.CreateHeader();
			this.CreateContent();
		}

		public override void RenderControl(HtmlTextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.ID))
				writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "widget");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			this.EnsureChildControls();
			this.RenderChildren(writer);

			writer.RenderEndTag();
		}
		#endregion
	}
}
