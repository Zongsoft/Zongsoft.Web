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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class ListViewItem : Control, IDataItemContainer
	{
		#region 成员字段
		private ListView _owner;
		private object _dataItem;
		private int _index;
		private int _displayIndex;
		#endregion

		#region 构造函数
		internal ListViewItem(ListView owner, object dataItem, int index) : this(owner, dataItem, index, index)
		{
		}

		internal ListViewItem(ListView owner, object dataItem, int index, int displayIndex)
		{
			if(owner == null)
				throw new ArgumentNullException("owner");

			_owner = owner;
			_dataItem = dataItem;
			_index = index;
			_displayIndex = displayIndex;
		}
		#endregion

		#region 公共属性
		public object DataSource
		{
			get
			{
				return _owner.DataSource;
			}
		}

		public object DataItem
		{
			get
			{
				return _dataItem;
			}
		}

		public int Index
		{
			get
			{
				return _index;
			}
		}

		public int DisplayIndex
		{
			get
			{
				return _displayIndex;
			}
		}
		#endregion

		#region 显式实现
		int IDataItemContainer.DataItemIndex
		{
			get
			{
				return _index;
			}
		}
		#endregion

		#region 重写方法
		public override void RenderControl(HtmlTextWriter writer)
		{
			bool isGenerateDefaultTag = _owner.HeaderTemplate == null && _owner.FooterTemplate == null;

			if(isGenerateDefaultTag)
				writer.RenderBeginTag(HtmlTextWriterTag.Li);

			this.RenderChildren(writer);

			if(isGenerateDefaultTag)
				writer.RenderEndTag();
		}
		#endregion
	}
}
