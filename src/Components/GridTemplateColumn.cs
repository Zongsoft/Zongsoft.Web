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
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	[DefaultProperty("Content")]
	[ParseChildren(true, "Content")]
	public class GridTemplateColumn : GridColumnBase
	{
		#region 成员字段
		private ITemplate _content;
		#endregion

		#region 公共属性
		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(ListView))]
		public ITemplate Content
		{
			get
			{
				return _content;
			}
			set
			{
				_content = value;
			}
		}
		#endregion

		#region 重写方法
		protected override void OnRender(HtmlTextWriter writer, object dataItem, int index)
		{
			if(_content != null)
			{
				var container = new GridTemplateColumnControl(this, dataItem, index);
				_content.InstantiateIn(container);
				container.RenderControl(writer);
			}

			//调用基类同名方法
			base.OnRender(writer, dataItem, index);
		}
		#endregion

		#region 嵌套子类
		internal class GridTemplateColumnControl : DataItemContainer<Grid>
		{
			#region 成员字段
			private GridTemplateColumn _column;
			#endregion

			#region 构造函数
			public GridTemplateColumnControl(GridTemplateColumn column, object dataItem, int index) : base(column.Grid, dataItem, index)
			{
				_column = column;
			}
			#endregion

			#region 公共属性
			public GridTemplateColumn Column
			{
				get
				{
					return _column;
				}
			}
			#endregion
		}
		#endregion
	}
}
