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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	[DefaultProperty("Title")]
	[ParseChildren(true)]
	public class InputGroup : CompositeDataBoundControl, INamingContainer
	{
		#region 成员字段
		private ITemplate _emptyTemplate;
		private ITemplate _itemTemplate;
		#endregion

		#region 构造函数
		public InputGroup()
		{
			this.CssClass = "fields";
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
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

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(InputGroup))]
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
		[TemplateContainerAttribute(typeof(InputGroup))]
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
		#endregion

		#region 重写方法
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			this.AddAttributes(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			if(!string.IsNullOrWhiteSpace(this.Title))
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Label);
				writer.WriteEncodedText(this.Title);
				writer.RenderEndTag();
			}
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			if(_emptyTemplate == null && _itemTemplate == null)
				return;

			if(this.DataSource == null)
			{
				if(_emptyTemplate == null)
					return;

				_emptyTemplate.InstantiateIn(this);
			}
			else
			{
				if(_itemTemplate == null)
					return;

				var dataItems = this.DataSource as IEnumerable;

				if(dataItems == null || this.DataSource.GetType() == typeof(string))
					_itemTemplate.InstantiateIn(this);
				else
				{
					int index = 0;

					foreach(var dataItem in dataItems)
					{
						var container = new DataItemContainer<InputGroup>(this, dataItem, index++);
						_itemTemplate.InstantiateIn(container);
						this.Controls.Add(container);
					}
				}
			}

			base.RenderContent(writer);
		}
		#endregion
	}
}
