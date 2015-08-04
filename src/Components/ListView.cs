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
	[PersistChildren(false)]
	public class ListView : CompositeDataBoundControl, INamingContainer
	{
		#region 成员字段
		private ITemplate _emptyTemplate;
		private ITemplate _itemTemplate;
		private ITemplate _headerTemplate;
		private ITemplate _footerTemplate;
		#endregion

		#region 构造函数
		public ListView()
		{
			this.CssClass = "ui list";
		}
		#endregion

		#region 公共属性
		[DefaultValue(ListRenderMode.List)]
		[PropertyMetadata(false)]
		public ListRenderMode RenderMode
		{
			get
			{
				return this.GetPropertyValue(() => this.RenderMode);
			}
			set
			{
				this.SetPropertyValue(() => this.RenderMode, value);
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

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string ItemText
		{
			get
			{
				return this.GetPropertyValue(() => this.ItemText);
			}
			set
			{
				this.SetPropertyValue(() => this.ItemText, value);
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
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			var tagName = string.Empty;

			switch(this.RenderMode)
			{
				case ListRenderMode.List:
					tagName = "dl";
					break;
				case ListRenderMode.BulletList:
					tagName = "ul";
					break;
				case ListRenderMode.OrderedList:
					tagName = "ol";
					break;
			}

			if(!string.IsNullOrWhiteSpace(tagName))
			{
				if(!string.IsNullOrWhiteSpace(this.ID))
					writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);

				if(!string.IsNullOrWhiteSpace(this.CssClass))
					writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);

				writer.RenderBeginTag(tagName);
			}

			if(_headerTemplate != null)
				_headerTemplate.InstantiateIn(this);
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			if(_footerTemplate != null)
				_footerTemplate.InstantiateIn(this);

			if(this.RenderMode != ListRenderMode.None)
				writer.RenderEndTag();
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			if(this.DataSource == null)
			{
				if(_emptyTemplate != null)
					_emptyTemplate.InstantiateIn(this);

				return;
			}

			IEnumerable dataItems = this.DataSource as IEnumerable;

			if(dataItems == null || dataItems.GetType() == typeof(string))
			{
				var item = new DataItemContainer<ListView>(this, this.DataSource, 0);
				this.RenderItem(writer, item);
			}
			else
			{
				int index = 0;

				foreach(var dataItem in dataItems)
				{
					FormExtension.PushDataItem(this.Page, dataItem);

					var item = new DataItemContainer<ListView>(this, dataItem, index++);
					this.RenderItem(writer, item);

					FormExtension.PopDataItem(this.Page);
				}
			}
		}

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

			//调用基类同名方法
			base.Render(writer);
		}
		#endregion

		#region 虚拟方法
		protected virtual void RenderItem(HtmlTextWriter writer, DataItemContainer<ListView> item)
		{
			switch(this.RenderMode)
			{
				case ListRenderMode.List:
					item.TagName = "dt";
					break;
				case ListRenderMode.BulletList:
				case ListRenderMode.OrderedList:
					item.TagName = "li";
					break;
			}

			if(_itemTemplate != null)
				_itemTemplate.InstantiateIn(item);

			item.RenderControl(writer);
		}
		#endregion
	}
}
