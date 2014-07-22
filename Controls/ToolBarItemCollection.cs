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

namespace Zongsoft.Web.Controls
{
	public class ToolBarItemCollection : Zongsoft.Collections.NamedCollectionBase<ToolBarItem>
	{
		#region 私有变量
		private int _index;
		#endregion

		#region 成员变量
		private ToolBar _owner;
		#endregion

		#region 构造函数
		public ToolBarItemCollection(ToolBar owner) : base(StringComparer.OrdinalIgnoreCase)
		{
			if(owner == null)
				throw new ArgumentNullException("owner");

			_owner = owner;
		}
		#endregion

		#region 公共属性
		public ToolBar Owner
		{
			get
			{
				return _owner;
			}
		}
		#endregion

		#region 重写方法
		protected override string GetKeyForItem(ToolBarItem item)
		{
			return item.Name;
		}

		protected override void InsertItem(int index, ToolBarItem item)
		{
			if(item == null)
				return;

			if(string.IsNullOrWhiteSpace(item.Name))
				item.Name = "ToolbarItem_" + System.Threading.Interlocked.Increment(ref _index).ToString();

			base.InsertItem(index, item);
		}
		#endregion
	}
}
