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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Zongsoft.Web.Controls
{
	/// <summary>
	/// 关于工具条的控件。
	/// </summary>
	/// <remarks>
	///		<para>该控件生成的HTML文档结构如下：</para>
	///		<code><![CDATA[
	///		<dl class="toolbar">
	///			<dt><a name="" href="url" title="tooltip">text</a></dt>
	///			<dt><a name="" href="url" title="tooltip">text</a></dt>
	///			<dt></dt>
	///			<dt><a name="" href="url" title="tooltip">text</a></dt>
	///			<dt>模板项内容</dt>
	///		</dl>
	///		]]>
	///		</code>
	/// </remarks>
	[DefaultProperty("Items")]
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class ToolBar : DataBoundControl, INamingContainer
	{
		#region 成员变量
		private ToolBarItemCollection _items;
		#endregion

		#region 构造函数
		public ToolBar()
		{
			this.CssClass = "toolbar";
			_items = new ToolBarItemCollection(this);
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
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

		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ToolBarItemCollection Items
		{
			get
			{
				return _items;
			}
		}
		#endregion

		#region 重写方法
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			this.AddAttributes(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Dl);
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			foreach(ToolBarItem item in _items)
			{
				if(!item.Visible)
					continue;

				writer.RenderBeginTag(HtmlTextWriterTag.Dt);

				if(!item.IsEmpty())
					this.RenderItem(writer, item);

				writer.RenderEndTag();

				if(!string.IsNullOrWhiteSpace(item.Description))
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Dd);
					writer.WriteEncodedText(BindingUtility.FormatBindingValue(item.Description, this.GetBindingSource()));
					writer.RenderEndTag();
				}
			}
		}
		#endregion

		#region 私有方法
		private void RenderItem(HtmlTextWriter writer, ToolBarItem item)
		{
			if(item.Content != null)
			{
				item.Content.InstantiateIn(this);
				this.RenderChildren(writer);
				this.Controls.Clear();
				return;
			}

			if(!string.IsNullOrWhiteSpace(item.Icon))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "icon icon-" + item.Icon.Trim());
				writer.RenderBeginTag(HtmlTextWriterTag.I);
				writer.RenderEndTag();
			}

			if(!string.IsNullOrWhiteSpace(item.Name))
				writer.AddAttribute(HtmlTextWriterAttribute.Name, item.Name);

			if(!string.IsNullOrWhiteSpace(item.ToolTip))
				writer.AddAttribute(HtmlTextWriterAttribute.Title, BindingUtility.FormatBindingValue(item.ToolTip, this.GetBindingSource()));

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "toolbar-button");
			writer.AddAttribute(HtmlTextWriterAttribute.Href, string.IsNullOrWhiteSpace(item.Url) ? Utility.EmptyLink : BindingUtility.FormatBindingValue(item.Url, this.GetBindingSource()));
			writer.RenderBeginTag(HtmlTextWriterTag.A);

			if(!string.IsNullOrEmpty(item.Text))
				writer.WriteEncodedText(item.Text);

			writer.RenderEndTag();
		}
		#endregion
	}
}
