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
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class Button : DataBoundControl
	{
		#region 成员字段
		private Image _image;
		private ButtonType _buttonType;
		#endregion

		#region 构造函数
		public Button()
		{
			_buttonType = Web.Controls.ButtonType.Normal;
			this.CssClass = "ui basic button btn";
		}
		#endregion

		#region 公共属性
		[DefaultValue(ButtonType.Normal)]
		[PropertyMetadata(false)]
		public ButtonType ButtonType
		{
			get
			{
				return _buttonType;
			}
			set
			{
				_buttonType = value;
			}
		}

		[Bindable(true)]
		public string Name
		{
			get
			{
				return this.GetPropertyValue(() => this.Name);
			}
			set
			{
				this.SetPropertyValue(() => this.Name, value);
			}
		}

		[Bindable(true)]
		[PropertyMetadata(false)]
		public string Value
		{
			get
			{
				return this.GetPropertyValue(() => this.Value);
			}
			set
			{
				this.SetPropertyValue(() => this.Value, value);
			}
		}

		[Bindable(true)]
		public string Text
		{
			get
			{
				return this.GetPropertyValue(() => this.Text);
			}
			set
			{
				this.SetPropertyValue(() => this.Text, value);
			}
		}

		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string Icon
		{
			get
			{
				return _image == null ? null : _image.Icon;
			}
			set
			{
				this.Image.Icon = value;
			}
		}

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[PropertyMetadata(false)]
		public Image Image
		{
			get
			{
				if(_image == null)
					System.Threading.Interlocked.CompareExchange(ref _image, new Image(), null);

				return _image;
			}
			set
			{
				_image = value;
			}
		}

		[DefaultValue(HorizontalAlignment.Right)]
		[PropertyMetadata(false)]
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
		#endregion

		#region 重写方法
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			if(string.IsNullOrWhiteSpace(this.Name) && (!string.IsNullOrWhiteSpace(this.ID)))
				writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID);

			this.AddAttributes(writer);

			if(this.ButtonType == ButtonType.Link)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Href, string.IsNullOrWhiteSpace(this.Value) ? Utility.EmptyLink : this.Value);
				writer.RenderBeginTag(HtmlTextWriterTag.A);
			}
			else
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Value, this.Value);
				writer.RenderBeginTag(HtmlTextWriterTag.Button);
			}
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			if(_image != null)
				_image.ToHtmlString(writer);

			if(this.Text != null)
				writer.Write(this.Text);
		}
		#endregion
	}
}
