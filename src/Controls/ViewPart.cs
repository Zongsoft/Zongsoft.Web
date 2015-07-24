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
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class ViewPart : Zongsoft.ComponentModel.NotifyObject, IDataBoundControlPart
	{
		#region 成员字段
		private DataBoundControl _owner;
		private string _text;
		private string _icon;
		private string _navigateUrl;
		private string _cssClass;
		private HorizontalAlignment _iconAlignment;
		private HorizontalAlignment _alignment;
		private string _style;
		private PropertyCollection _properties;
		#endregion

		#region 公共属性
		internal DataBoundControl Owner
		{
			get
			{
				return _owner;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_owner = value;
			}
		}

		protected object BindingSource
		{
			get
			{
				if(_owner == null)
					return null;

				return _owner.GetBindingSource();
			}
		}

		public string Text
		{
			get
			{
				return BindingUtility.FormatBindingValue(_text, this.BindingSource);
			}
			set
			{
				this.SetPropertyValue(() => this.Text, ref _text, value);
			}
		}

		public string Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				this.SetPropertyValue(() => this.Icon, ref _icon, value);
			}
		}

		public string NavigateUrl
		{
			get
			{
				var result = BindingUtility.FormatBindingValue(_navigateUrl, this.BindingSource);

				if(!string.IsNullOrWhiteSpace(result))
					return _owner.ResolveUrl(result);

				return result;
			}
			set
			{
				this.SetPropertyValue(() => this.NavigateUrl, ref _navigateUrl, value);
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
				this.SetPropertyValue(() => this.CssClass, ref _cssClass, value);
			}
		}

		public string Style
		{
			get
			{
				return _style;
			}
			set
			{
				_style = value;
			}
		}

		public HorizontalAlignment IconAlignment
		{
			get
			{
				return _iconAlignment;
			}
			set
			{
				this.SetPropertyValue(() => this.IconAlignment, ref _iconAlignment, value);
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
				this.SetPropertyValue(() => this.Alignment, ref _alignment, value);
			}
		}

		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public new PropertyCollection Properties
		{
			get
			{
				if(_properties == null)
					System.Threading.Interlocked.CompareExchange(ref _properties, new PropertyCollection(this), null);

				return _properties;
			}
		}
		#endregion

		#region 显式实现
		DataBoundControl IDataBoundControlPart.Control
		{
			get
			{
				return _owner;
			}
		}

		object IDataBoundControlPart.BindingSource
		{
			get
			{
				return this.BindingSource;
			}
		}
		#endregion
	}
}
