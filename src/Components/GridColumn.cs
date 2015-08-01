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
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class GridColumn : GridColumnBase
	{
		#region 成员变量
		private string _text;
		private string _navigateUrl;
		private string _navigateTitle;
		private string _navigateCssClass;
		private string _imageUrl;
		private string _imageTitle;
		#endregion

		#region 公共属性
		public string NavigateUrl
		{
			get
			{
				return _navigateUrl;
			}
			set
			{
				_navigateUrl = value;
			}
		}

		public string NavigateTitle
		{
			get
			{
				return _navigateTitle;
			}
			set
			{
				_navigateTitle = value;
			}
		}

		public string NavigateCssClass
		{
			get
			{
				return _navigateCssClass;
			}
			set
			{
				_navigateCssClass = value;
			}
		}

		public string ImageUrl
		{
			get
			{
				return _imageUrl;
			}
			set
			{
				_imageUrl = value;
			}
		}

		public string ImageTitle
		{
			get
			{
				return _imageTitle;
			}
			set
			{
				_imageTitle = value;
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
		#endregion

		#region 生成内容
		protected override void OnRender(HtmlTextWriter writer, object dataItem, int index)
		{
			if(!string.IsNullOrWhiteSpace(_navigateUrl))
			{
				string navigateUrl = BindingUtility.FormatBindingValue(_navigateUrl, dataItem, true);
				string navigateTitle = BindingUtility.FormatBindingValue(_navigateTitle, dataItem, true);
				string navigateCssClass = BindingUtility.FormatBindingValue(_navigateCssClass, dataItem, true);

				if(navigateUrl == null)
					writer.AddAttribute(HtmlTextWriterAttribute.Href, Utility.EmptyLink);
				else
					writer.AddAttribute(HtmlTextWriterAttribute.Href, navigateUrl);

				if(!string.IsNullOrWhiteSpace(navigateTitle))
					writer.AddAttribute(HtmlTextWriterAttribute.Title, navigateTitle);

				if(!string.IsNullOrWhiteSpace(navigateCssClass))
					writer.AddAttribute(HtmlTextWriterAttribute.Class, navigateCssClass);

				writer.RenderBeginTag(HtmlTextWriterTag.A);
			}

			if(!string.IsNullOrWhiteSpace(_imageUrl))
			{
				string imageUrl = BindingUtility.FormatBindingValue(_imageUrl, dataItem, true);
				if(!string.IsNullOrWhiteSpace(imageUrl))
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Src, imageUrl);

					string imageTitle = BindingUtility.FormatBindingValue(_imageTitle, dataItem, true);
					if(!string.IsNullOrWhiteSpace(imageTitle))
						writer.AddAttribute(HtmlTextWriterAttribute.Alt, imageTitle);

					writer.RenderBeginTag(HtmlTextWriterTag.Img);
					writer.RenderEndTag();
				}
			}

			string text = BindingUtility.FormatBindingValue(_text ?? this.Name, dataItem, true);
			if(string.IsNullOrEmpty(text))
				text = this.NullText;

			if(string.Equals(text, "true", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "false", StringComparison.OrdinalIgnoreCase))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
				writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

				if(string.Equals(text, "true", StringComparison.OrdinalIgnoreCase))
					writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");

				writer.RenderBeginTag(HtmlTextWriterTag.Input);
				writer.RenderEndTag();
			}
			else
			{
				if(string.IsNullOrEmpty(text))
					writer.Write("&nbsp;");
				else
					writer.WriteEncodedText(text);
			}

			if(!string.IsNullOrWhiteSpace(_navigateUrl))
				writer.RenderEndTag();

			//调用基类同名方法
			base.OnRender(writer, dataItem, index);
		}
		#endregion
	}
}
