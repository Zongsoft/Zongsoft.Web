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
using System.IO;
using System.ComponentModel;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class Image : Zongsoft.ComponentModel.NotifyObject
	{
		#region 公共属性
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public object BindingSource
		{
			get
			{
				return this.GetPropertyValue(() => this.BindingSource);
			}
			set
			{
				this.SetPropertyValue(() => this.BindingSource, value);
			}
		}

		[DefaultValue("")]
		public string Name
		{
			get
			{
				var value = this.GetPropertyValue(() => this.Name);
				return BindingUtility.FormatBindingValue(value, this.BindingSource);
			}
			set
			{
				this.SetPropertyValue(() => this.Name, value);
			}
		}

		[DefaultValue("")]
		public string ImageUrl
		{
			get
			{
				var value = this.GetPropertyValue(() => this.ImageUrl);
				return BindingUtility.FormatBindingValue(value, this.BindingSource);
			}
			set
			{
				this.SetPropertyValue(() => this.ImageUrl, value);
			}
		}

		[DefaultValue("")]
		public string ImageCssClass
		{
			get
			{
				return this.GetPropertyValue(() => this.ImageCssClass);
			}
			set
			{
				this.SetPropertyValue(() => this.ImageCssClass, Utility.ResolveCssClass(value, () => this.ImageCssClass));
			}
		}

		[DefaultValue("")]
		public string NavigateUrl
		{
			get
			{
				var value = this.GetPropertyValue(() => this.NavigateUrl);
				return BindingUtility.FormatBindingValue(value, this.BindingSource);
			}
			set
			{
				this.SetPropertyValue(() => this.NavigateUrl, value);
			}
		}

		[DefaultValue("")]
		public string Placeholder
		{
			get
			{
				var value = this.GetPropertyValue(() => this.Placeholder);
				return BindingUtility.FormatBindingValue(value, this.BindingSource);
			}
			set
			{
				this.SetPropertyValue(() => this.Placeholder, value);
			}
		}

		[DefaultValue(Dimension.None)]
		public Dimension Dimension
		{
			get
			{
				return this.GetPropertyValue(() => this.Dimension);
			}
			set
			{
				this.SetPropertyValue(() => this.Dimension, value);
			}
		}

		public Unit Width
		{
			get
			{
				return this.GetPropertyValue(() => this.Width);
			}
			set
			{
				this.SetPropertyValue(() => this.Width, value);
			}
		}

		public Unit Height
		{
			get
			{
				return this.GetPropertyValue(() => this.Height);
			}
			set
			{
				this.SetPropertyValue(() => this.Height, value);
			}
		}
		#endregion

		#region 公共方法
		public override string ToString()
		{
			using(var writer = new StringWriter())
			{
				this.ToHtmlString(writer);
				return writer.ToString();
			}
		}

		public virtual void ToHtmlString(TextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.NavigateUrl))
				writer.Write("<a class=\"image\" href=\"{0}\">", this.NavigateUrl);

			if(this.Dimension != Dimension.None)
				this.ImageCssClass = ":" + this.Dimension.ToString();

			if(string.IsNullOrWhiteSpace(ImageUrl))
			{
				if(!string.IsNullOrWhiteSpace(this.Name))
					this.ImageCssClass = ":icon " + this.Name.Trim();

				writer.Write("<i");

				this.WriteAttribute(writer, "class", this.ImageCssClass);

				writer.WriteLine("></i>");
			}
			else
			{
				writer.Write("<img");

				this.WriteAttribute(writer, "class", this.ImageCssClass);
				this.WriteAttribute(writer, "alt", this.Placeholder);
				this.WriteAttribute(writer, "src", this.ImageUrl);

				if(!Unit.IsEmpty(this.Width))
					this.WriteAttribute(writer, "width", this.Width.ToString());

				if(!Unit.IsEmpty(this.Height))
					this.WriteAttribute(writer, "height", this.Height.ToString());

				writer.WriteLine(" />");
			}

			if(!string.IsNullOrWhiteSpace(this.NavigateUrl))
				writer.WriteLine("</a>");
		}

		public virtual Control ToHtmlControl()
		{
			Literal control;

			if(this.Dimension != Dimension.None)
				this.ImageCssClass = ":" + this.Dimension.ToString();

			if(string.IsNullOrWhiteSpace(this.ImageUrl))
			{
				var css = Utility.ResolveCssClass(":" + this.Name, () => this.ImageCssClass);
				control = new Literal("i", "icon" + (string.IsNullOrWhiteSpace(css) ? "" : " " + css));
			}
			else
			{
				control = new Literal("img", "image");
				control.SetAttributeValue("src", this.ImageUrl);

				if(!string.IsNullOrWhiteSpace(this.Placeholder))
					control.SetAttributeValue("alt", this.Placeholder);

				if(!Unit.IsEmpty(this.Width))
					control.SetAttributeValue("width", this.Width.ToString());

				if(!Unit.IsEmpty(this.Height))
					control.SetAttributeValue("height", this.Height.ToString());
			}

			if(!string.IsNullOrWhiteSpace(this.ImageCssClass))
				control.SetAttributeValue("class", this.ImageCssClass);

			if(string.IsNullOrWhiteSpace(this.NavigateUrl))
				return control;

			var container = new Literal("a", "image");
			container.SetAttributeValue("href", this.NavigateUrl);
			container.Controls.Add(control);
			return container;
		}
		#endregion

		#region 私有方法
		private bool WriteAttribute(TextWriter writer, string name, string value)
		{
			if(writer == null)
				throw new ArgumentNullException("writer");

			if(string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
				return false;

			writer.Write(" {0}=\"{1}\"", name, value);

			return true;
		}
		#endregion
	}
}
