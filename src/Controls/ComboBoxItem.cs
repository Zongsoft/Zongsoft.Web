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
using System.IO;

namespace Zongsoft.Web.Controls
{
	[Serializable]
	public class ComboBoxItem
	{
		#region 成员变量
		private bool _disabled;
		private string _text;
		private string _value;
		private Image _image;
		private string _description;
		#endregion

		#region 公共属性
		public bool Disabled
		{
			get
			{
				return _disabled;
			}
			set
			{
				_disabled = value;
			}
		}

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

		public string Icon
		{
			get
			{
				return _image == null ? null : _image.Icon;
			}
			set
			{
				if(_image == null)
					System.Threading.Interlocked.CompareExchange(ref _image, new Image(), null);

				_image.Icon = value;
			}
		}

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
		#endregion

		#region 公共方法
		public void ToHtmlString(TextWriter writer, ComboBoxRenderMode renderMode, bool isSelected)
		{
			if(renderMode == ComboBoxRenderMode.Classic)
			{
				writer.Write("<option");
				this.WriteAttribute(writer, "value", this.Value);
			}
			else
			{
				writer.Write("<dt");

				this.WriteAttribute(writer, "data-value", this.Value);
				this.WriteAttribute(writer, "class", "item" + (isSelected ? " active selected" : ""));
			}

			if(this.Disabled)
				this.WriteAttribute(writer, "disabled", "disabled");

			if(isSelected)
			{
				if(renderMode == ComboBoxRenderMode.Classic)
					this.WriteAttribute(writer, "selected", "selected");
				else
					this.WriteAttribute(writer, "data-selected", "selected");
			}

			writer.Write(">");

			if(this.Image != null)
				this.Image.ToHtmlString(writer);

			writer.Write(this.Text);

			if(!string.IsNullOrWhiteSpace(this.Description))
			{
				writer.Write("<span");
				this.WriteAttribute(writer, "class", "right floated description");
				writer.Write(">");
				writer.Write(this.Description);
				writer.Write("</span>");
			}

			if(renderMode == ComboBoxRenderMode.Classic)
				writer.WriteLine("</option>");
			else
				writer.WriteLine("</dt>");
		}
		#endregion

		#region 私有方法
		private bool WriteAttribute(TextWriter writer, string name, string value)
		{
			if(writer == null)
				throw new ArgumentNullException("writer");

			if(string.IsNullOrWhiteSpace(name))
				return false;

			writer.Write(" {0}=\"{1}\"", name, value);

			return true;
		}
		#endregion
	}
}
