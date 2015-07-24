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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Zongsoft.Web.Controls
{
	/// <summary>
	/// 关于选项卡的控件。
	/// </summary>
	/// <remarks>
	///		<para>该控件生成的HTML文档结构如下：</para>
	///		<code>
	///		<![CDATA[
	///		<div class="tabStrip">
	///			<dl>
	///				<dt class="active"><a href="#tab-1" title="ToolTip">第一页</a></dt>
	///				<dt><a href="#tab-2">第二页</a></dt>
	///			</dl>
	///			<div id="tab-1" class="active">
	///				<p>我是第一页的内容</p>
	///			</div>
	///			<div id="tab-2">
	///				<p>我是第二页的内容</p>
	///			</div>
	///		</div>
	///		]]>
	///		</code>
	/// </remarks>
	[DefaultProperty("Panels")]
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class TabStrip : DataBoundControl, INamingContainer
	{
		#region 成员变量
		private TabStripPanelCollection _panels;
		#endregion

		#region 构造函数
		public TabStrip()
		{
			_panels = new TabStripPanelCollection(this);
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
		public string SelectedPanel
		{
			get
			{
				return this.GetPropertyValue(() => this.SelectedPanel);
			}
			set
			{
				this.SetPropertyValue(() => this.SelectedPanel, value);
			}
		}

		public TabStripPanelCollection Panels
		{
			get
			{
				return _panels;
			}
		}
		#endregion

		#region 重写方法
		protected override void Render(HtmlTextWriter writer)
		{
			if(_panels.Count < 1)
				return;

			if(!string.IsNullOrWhiteSpace(this.ID))
				writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "tabStrip");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			//生成头部元素(开始)
			writer.RenderBeginTag(HtmlTextWriterTag.Dl);

			//获取所有可见的面板
			var visiblePanels = _panels.GetVisiblePanels();

			foreach(var panel in visiblePanels)
			{
				if(string.Equals(panel.Name, this.SelectedPanel, StringComparison.OrdinalIgnoreCase))
					writer.AddAttribute(HtmlTextWriterAttribute.Class, "active");

				writer.RenderBeginTag(HtmlTextWriterTag.Dt);

				writer.AddAttribute(HtmlTextWriterAttribute.Href, "#" + panel.Name);
				if(!string.IsNullOrEmpty(panel.ToolTip))
					writer.AddAttribute(HtmlTextWriterAttribute.Title, BindingUtility.FormatBindingValue(panel.ToolTip, this.GetBindingSource()));

				//生成选项面板的头部标签的开始部分
				writer.RenderBeginTag(HtmlTextWriterTag.A);

				if(!string.IsNullOrEmpty(panel.Title))
					writer.WriteEncodedText(BindingUtility.FormatBindingValue(panel.Title, this.GetBindingSource()));

				//生成选项面板的头部标签的结束部分
				writer.RenderEndTag();

				writer.RenderEndTag();

				//生成选项面板的注释内容
				if(!string.IsNullOrWhiteSpace(panel.Description))
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Dd);
					writer.WriteEncodedText(BindingUtility.FormatBindingValue(panel.Description, this.GetBindingSource()));
					writer.RenderEndTag();
				}
			}

			//生成头部元素(结束)
			writer.RenderEndTag();

			foreach(var panel in visiblePanels)
			{
				if(string.Equals(panel.Name, this.SelectedPanel, StringComparison.OrdinalIgnoreCase))
					writer.AddAttribute(HtmlTextWriterAttribute.Class, "active");

				writer.AddAttribute(HtmlTextWriterAttribute.Id, panel.Name);
				writer.RenderBeginTag(HtmlTextWriterTag.Div);

				if(panel.Content != null)
				{
					panel.Content.InstantiateIn(this);
					this.RenderChildren(writer);
					this.Controls.Clear();
				}

				writer.RenderEndTag();
			}

			writer.RenderEndTag();

			//调用基类同名方法
			base.Render(writer);
		}
		#endregion
	}
}
