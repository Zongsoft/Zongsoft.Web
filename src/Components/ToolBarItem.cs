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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	[Serializable]
	public class ToolBarItem
	{
		#region 成员字段
		private string _name;
		private string _text;
		private string _url;
		private string _icon;
		private string _toolTip;
		private string _description;
		private bool _visible;
		private ITemplate _context;
		#endregion

		#region 构造函数
		public ToolBarItem()
		{
			_visible = true;
		}
		#endregion

		#region 公共属性
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_name = value.Trim();
			}
		}

		[DefaultValue("")]
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
			}
		}

		[DefaultValue("")]
		public string Url
		{
			get
			{
				return _url;
			}
			set
			{
				_url = value;
			}
		}

		[DefaultValue("")]
		public string Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				_icon = value;
			}
		}

		[DefaultValue("")]
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

		[DefaultValue("")]
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		[DefaultValue(true)]
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

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(ToolBar))]
		public ITemplate Content
		{
			get
			{
				return _context;
			}
			set
			{
				_context = value;
			}
		}
		#endregion

		#region 公共方法
		public virtual bool IsEmpty()
		{
			return _context == null && (string.IsNullOrWhiteSpace(_text) || _text == "-");
		}
		#endregion
	}
}
