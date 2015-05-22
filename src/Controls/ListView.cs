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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Zongsoft.Web.Controls
{
	[ParseChildren(true)]
	public class ListView : CompositeDataBoundControl, INamingContainer
	{
		#region 成员字段
		private ITemplate _emptyTemplate;
		private ITemplate _itemTemplate;
		private ITemplate _headerTemplate;
		private ITemplate _footerTemplate;
		#endregion

		#region 公共属性
		[DefaultValue(false)]
		[PropertyMetadata(false)]
		public bool IsOrdered
		{
			get
			{
				return this.GetPropertyValue(() => this.IsOrdered);
			}
			set
			{
				this.SetPropertyValue(() => this.IsOrdered, value);
			}
		}

		[DefaultValue(EmptyTemplateScope.Item)]
		[PropertyMetadata(false)]
		public EmptyTemplateScope EmptyTemplateScope
		{
			get
			{
				return this.GetPropertyValue(() => this.EmptyTemplateScope);
			}
			set
			{
				this.SetPropertyValue(() => this.EmptyTemplateScope, value);
			}
		}

		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string ItemTemplateText
		{
			get
			{
				return this.GetPropertyValue(() => this.ItemTemplateText);
			}
			set
			{
				this.SetPropertyValue(() => this.ItemTemplateText, value);
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(ListView))]
		public ITemplate EmptyTemplate
		{
			get
			{
				return _emptyTemplate;
			}
			set
			{
				_emptyTemplate = value;
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(ListView))]
		public ITemplate ItemTemplate
		{
			get
			{
				return _itemTemplate;
			}
			set
			{
				_itemTemplate = value;
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(ListView))]
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
		[TemplateContainerAttribute(typeof(ListView))]
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
		protected override void Render(HtmlTextWriter writer)
		{
			if(this.DataSource == null && this.EmptyTemplateScope == EmptyTemplateScope.Control)
			{
				if(_emptyTemplate != null)
				{
					_emptyTemplate.InstantiateIn(this);
					this.RenderChildren(writer);
				}

				return;
			}

			//生成头部内容
			this.CreateHeader();

			if(this.DataSource == null)
			{
				if(_emptyTemplate != null)
					_emptyTemplate.InstantiateIn(this);
			}
			else
			{
				IEnumerable dataSource = this.DataSource as IEnumerable;

				if(dataSource == null)
				{
					var item = new ListViewItem(this, this.DataSource, 0);
					this.CreateItem(item);
					this.Controls.Add(item);
				}
				else
				{
					int index = 1;

					foreach(var dataItem in dataSource)
					{
						var item = new ListViewItem(this, dataItem, index++);
						this.CreateItem(item);
						this.Controls.Add(item);
					}
				}
			}

			//生成脚部内容
			this.CreateFooter();

			//生成所有子控件
			this.RenderChildren(writer);

			//调用基类同名方法
			base.Render(writer);
		}
		#endregion

		#region 虚拟方法
		protected virtual void CreateHeader()
		{
			if(_headerTemplate == null && _footerTemplate == null)
			{
				var text = new StringBuilder(this.IsOrdered ? "<ol" : "<ul", 64);

				if(!string.IsNullOrWhiteSpace(this.ID))
					text.AppendFormat(" id=\"{0}\"", this.ID);

				if(!string.IsNullOrWhiteSpace(this.CssClass))
					text.AppendFormat(" class=\"{0}\"", this.CssClass);

				text.Append(">");

				this.Controls.Add(new LiteralControl(text.ToString()));
			}
			else
			{
				if(_headerTemplate != null)
					_headerTemplate.InstantiateIn(this);
			}
		}

		protected virtual void CreateFooter()
		{
			if(_headerTemplate == null && _footerTemplate == null)
			{
				this.Controls.Add(new LiteralControl(string.Format("</{0}>", this.IsOrdered ? "ol" : "ul")));
			}
			else
			{
				if(_footerTemplate != null)
					_footerTemplate.InstantiateIn(this);
			}
		}

		protected virtual void CreateItem(ListViewItem item)
		{
			if(_itemTemplate == null)
			{
				if(item.DataItem == null)
					return;

				var child = new System.Web.UI.LiteralControl();

				if(string.IsNullOrWhiteSpace(this.ItemTemplateText))
					child.Text = item.DataItem.ToString();
				else
					child.Text = BindingUtility.FormatBindingValue(this.ItemTemplateText, item);

				item.Controls.Add(child);
			}
			else
			{
				_itemTemplate.InstantiateIn(item);
			}
		}
		#endregion
	}
}
