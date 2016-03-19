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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Zongsoft.Web.Controls
{
	public abstract class GridColumnBase
	{
		#region 成员变量
		private Grid _grid;
		private string _name;
		private string _cssClass;
		private Unit _width;
		private string _nullText;
		private bool _visible;
		private string _title;
		private string _value;
		private string _toolTip;
		private HorizontalAlignment _alignment;
		private HorizontalAlignment _titleAlignment;
		private NameValueCollection _attributes;
		#endregion

		#region 构造函数
		protected GridColumnBase()
		{
			_visible = true;
			_nullText = string.Empty;
			_attributes = null;
			_alignment = HorizontalAlignment.Left;
			_titleAlignment = HorizontalAlignment.Left;
		}
		#endregion

		#region 公共属性
		public Grid Grid
		{
			get
			{
				return _grid;
			}
			internal set
			{
				_grid = value;
			}
		}

		public NameValueCollection Attributes
		{
			get
			{
				if(_attributes == null)
					System.Threading.Interlocked.CompareExchange(ref _attributes, new NameValueCollection(StringComparer.OrdinalIgnoreCase), null);

				return _attributes;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public string CssClass
		{
			get
			{
				return _cssClass;
			}
			set
			{
				_cssClass = value;
			}
		}

		public HorizontalAlignment Alignment
		{
			get
			{
				return _alignment;
			}
			set
			{
				_alignment = value;
			}
		}

		public Unit Width
		{
			get
			{
				return _width;
			}
			set
			{
				_width = value;
			}
		}

		public string NullText
		{
			get
			{
				return string.IsNullOrWhiteSpace(_nullText) ? "<无>" : _nullText;
			}
			set
			{
				_nullText = value ?? string.Empty;
			}
		}

		public bool Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				_visible = value;
			}
		}

		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				_title = value;
			}
		}

		public HorizontalAlignment TitleAlignment
		{
			get
			{
				return _titleAlignment;
			}
			set
			{
				_titleAlignment = value;
			}
		}

		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public string ToolTip
		{
			get
			{
				return _toolTip;
			}
			set
			{
				_toolTip = value;
			}
		}
		#endregion

		#region 虚拟方法
		protected virtual void OnRender(HtmlTextWriter writer, object dataItem, int index)
		{
			if(!string.IsNullOrWhiteSpace(this.Value))
			{
				if(!string.IsNullOrWhiteSpace(this.Name))
					writer.AddAttribute(HtmlTextWriterAttribute.Name, this.Name);

				writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
				writer.AddAttribute(HtmlTextWriterAttribute.Value, BindingUtility.FormatBindingValue(this.Value, dataItem, true));

				if(!string.IsNullOrWhiteSpace(this.ToolTip))
					writer.AddAttribute(HtmlTextWriterAttribute.Title, BindingUtility.FormatBindingValue(this.ToolTip, dataItem, true));

				writer.RenderBeginTag(HtmlTextWriterTag.Input);
				writer.RenderEndTag();
			}
		}
		#endregion

		#region 内部方法
		internal void Render(HtmlTextWriter writer, object dataItem, int index)
		{
			this.OnRender(writer, dataItem, index);
		}
		#endregion
	}
}
