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
	[DefaultProperty("Title")]
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class HeaderView : Literal
	{
		#region 成员字段
		private Image _image;
		#endregion

		#region 构造函数
		public HeaderView()
		{
			this.Level = 3;
			this.CssClass = "ui header";
		}
		#endregion

		#region 公共属性
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

		[DefaultValue(3)]
		[PropertyMetadata(false)]
		public byte Level
		{
			get
			{
				return this.GetPropertyValue(() => this.Level);
			}
			set
			{
				//确保赋值的数值在1~6之间
				value = (value > 0 && value < 7 ? value : (byte)3);

				//将新值赋值到属性存储区中
				this.SetPropertyValue(() => this.Level, value);

				//更改当前控件要生成的标签名
				this.TagName = "h" + value.ToString();
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string Title
		{
			get
			{
				return this.GetPropertyValue(() => this.Title);
			}
			set
			{
				this.SetPropertyValue(() => this.Title, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata("href", PropertyRender = "UrlPropertyRender.Default", Renderable = false)]
		public string NavigateUrl
		{
			get
			{
				return this.GetPropertyValue(() => this.NavigateUrl);
			}
			set
			{
				this.SetPropertyValue(() => this.NavigateUrl, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string Description
		{
			get
			{
				return this.GetPropertyValue(() => this.Description);
			}
			set
			{
				this.SetPropertyValue(() => this.Description, value);
			}
		}
		#endregion

		#region 重写方法
		protected override void RenderContent(HtmlTextWriter writer)
		{
			if(this.Image != null)
				this.Image.ToHtmlString(writer);

			if(!string.IsNullOrWhiteSpace(this.Description))
			{
				//生成外层的内容标签(<div class="content">)
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "content");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
			}

			if(!string.IsNullOrWhiteSpace(this.Title))
			{
				if(!string.IsNullOrWhiteSpace(this.NavigateUrl))
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Href, this.NavigateUrl);
					writer.RenderBeginTag(HtmlTextWriterTag.A);
				}

				writer.Write(this.Title);

				if(!string.IsNullOrWhiteSpace(this.NavigateUrl))
					writer.RenderEndTag();
			}

			if(!string.IsNullOrWhiteSpace(this.Description))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "sub header");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.Write(this.Description);
				writer.RenderEndTag();

				//关闭外层的内容标签(</div>)
				writer.RenderEndTag();
			}
		}
		#endregion
	}
}
