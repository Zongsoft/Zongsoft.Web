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

namespace Zongsoft.Web.Controls
{
	public class TreeViewNodeRenderEventArgs : EventArgs
	{
		#region 成员变量
		private object _dataItem;
		private TreeViewNode _node;
		#endregion

		#region 构造函数
		public TreeViewNodeRenderEventArgs(TreeViewNode node) : this(node, null)
		{
		}

		public TreeViewNodeRenderEventArgs(TreeViewNode node, object dataItem)
		{
			if(node == null)
				throw new ArgumentNullException("node");

			_node = node;
			_dataItem = dataItem;
		}
		#endregion

		#region 公共属性
		public object DataItem
		{
			get
			{
				return _dataItem;
			}
		}

		public TreeViewNode Node
		{
			get
			{
				return _node;
			}
		}
		#endregion
	}
}
