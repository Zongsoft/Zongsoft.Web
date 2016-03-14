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
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class TextBox : TextBoxBase
	{
		#region 成员字段
		private TextBoxButton _button;
		#endregion

		#region 公共属性
		[Bindable(true)]
		[PropertyMetadata("data-icon")]
		public string Icon
		{
			get
			{
				return this.GetPropertyValue(() => this.Icon);
			}
			set
			{
				this.SetPropertyValue(() => this.Icon, value);
			}
		}

		[DefaultValue(HorizontalAlignment.Right)]
		[PropertyMetadata("data-icon-align")]
		public HorizontalAlignment IconAlignment
		{
			get
			{
				return this.GetPropertyValue(() => this.IconAlignment);
			}
			set
			{
				this.SetPropertyValue(() => this.IconAlignment, value);
			}
		}

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[PropertyMetadata(false)]
		public TextBoxButton Button
		{
			get
			{
				if(_button == null)
					System.Threading.Interlocked.CompareExchange(ref _button, new TextBoxButton(), null);

				return _button;
			}
			set
			{
				_button = value;
			}
		}
		#endregion

		#region 重写方法
		protected override string GetCssClass()
		{
			var css = base.GetCssClass();

			if(!string.IsNullOrWhiteSpace(this.Icon))
			{
				if(this.IconAlignment == HorizontalAlignment.Left)
					css = Utility.ResolveCssClass(":left icon", () => css);
				else
					css = Utility.ResolveCssClass(":icon", () => css);
			}

			if(_button != null)
				css = Utility.ResolveCssClass(":action", () => css);

			return css;
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			//调用基类同名方法
			base.RenderEndTag(writer);

			if(!string.IsNullOrWhiteSpace(this.Icon))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "icon " + this.Icon.Trim().ToLowerInvariant());
				writer.RenderBeginTag(HtmlTextWriterTag.I);
				writer.RenderEndTag();
			}

			if(_button != null)
			{
				if(!string.IsNullOrWhiteSpace(_button.Id))
					writer.AddAttribute(HtmlTextWriterAttribute.Id, _button.Id);
				if(!string.IsNullOrWhiteSpace(_button.Url))
					writer.AddAttribute(HtmlTextWriterAttribute.Href, BindingUtility.FormatBindingValue(_button.Url, this.GetBindingSource()));

				var cssClass = "ui icon basic button";
				if(!string.IsNullOrWhiteSpace(_button.CssClass ))
					cssClass = Utility.ResolveCssClass(_button.CssClass, () => cssClass);

				writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
				writer.RenderBeginTag(HtmlTextWriterTag.A);

				if(!string.IsNullOrWhiteSpace(_button.Icon))
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Class, "icon " + _button.Icon.ToString().ToLowerInvariant());
					writer.RenderBeginTag(HtmlTextWriterTag.I);
					writer.RenderEndTag();
				}

				if(!string.IsNullOrWhiteSpace(_button.Text))
					writer.WriteEncodedText(_button.Text);

				writer.RenderEndTag();
			}
		}
		#endregion

		#region 嵌套子类
		[Serializable]
		public class TextBoxButton
		{
			public string Id
			{
				get;
				set;
			}

			public string Icon
			{
				get;
				set;
			}

			public string Text
			{
				get;
				set;
			}

			public string Url
			{
				get;
				set;
			}

			public string CssClass
			{
				get;
				set;
			}
		}
		#endregion
	}
}
