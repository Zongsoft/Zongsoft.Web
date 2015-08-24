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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	[DefaultProperty("Value")]
	public class InputBox : DataBoundControl
	{
		#region 构造函数
		public InputBox()
		{
			this.CssClass = "ui input";
		}

		public InputBox(InputBoxType inputType)
		{
			this.InputType = inputType;
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string Name
		{
			get
			{
				var name = this.GetPropertyValue(() => this.Name);

				if(string.IsNullOrWhiteSpace(name))
					return this.ID;

				return name;
			}
			set
			{
				this.SetPropertyValue(() => this.Name, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(true)]
		[PropertyMetadata("disabled", PropertyRender = "BooleanPropertyRender.False")]
		public bool Enabled
		{
			get
			{
				return this.GetPropertyValue(() => this.Enabled);
			}
			set
			{
				this.SetPropertyValue(() => this.Enabled, value);
			}
		}

		[PropertyMetadata(false)]
		protected bool IsRenderFieldTag
		{
			get
			{
				return !string.IsNullOrWhiteSpace(this.FieldTagName);
			}
		}

		[DefaultValue("div")]
		[PropertyMetadata(false)]
		public string FieldTagName
		{
			get
			{
				return this.GetPropertyValue(() => this.FieldTagName);
			}
			set
			{
				this.SetPropertyValue(() => this.FieldTagName, value);
			}
		}

		[DefaultValue("field")]
		[PropertyMetadata(false)]
		public string FieldCssClass
		{
			get
			{
				return this.GetPropertyValue(() => this.FieldCssClass);
			}
			set
			{
				if(value != null && value.Length > 0)
					value = Utility.ResolveCssClass(value, () => this.FieldCssClass);

				this.SetPropertyValue(() => this.FieldCssClass, value);
			}
		}

		[PropertyMetadata(false)]
		public string FieldStyle
		{
			get
			{
				return this.GetPropertyValue(() => this.FieldStyle);
			}
			set
			{
				this.SetPropertyValue(() => this.FieldStyle, value);
			}
		}

		[DefaultValue(InputBoxType.Text)]
		[PropertyMetadata("type")]
		public virtual InputBoxType InputType
		{
			get
			{
				return this.GetPropertyValue(() => this.InputType);
			}
			set
			{
				this.SetPropertyValue(() => this.InputType, value);

				switch(value)
				{
					case InputBoxType.Button:
						this.CssClass = ":ui button btn";
						break;
					case InputBoxType.Reset:
						this.CssClass = ":ui button btn btn-reset";
						break;
					case InputBoxType.Submit:
						this.CssClass = ":ui button primary btn btn-primary";
						break;
					case InputBoxType.File:
						this.CssClass = ":file";
						break;
					case InputBoxType.Image:
						this.CssClass = ":image";
						break;
					case InputBoxType.CheckBox:
						this.CssClass = ":ui checkbox";
						break;
					case InputBoxType.Radio:
						this.CssClass = ":ui radio checkbox";
						break;
					case InputBoxType.Text:
						this.CssClass = ":ui input";
						break;
					case InputBoxType.Password:
						this.CssClass = ":ui input input-password";
						break;
					default:
						this.CssClass = ":ui input " + value.ToString().ToLowerInvariant();
						break;
				}
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string Label
		{
			get
			{
				return this.GetPropertyValue(() => this.Label);
			}
			set
			{
				this.SetPropertyValue(() => this.Label, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
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
		#endregion

		#region 重写方法
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.Name))
				writer.AddAttribute(HtmlTextWriterAttribute.Name, this.Name);

			this.AddAttributes(writer, "Name", "CssClass");

			writer.RenderBeginTag(HtmlTextWriterTag.Input);
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected override void Render(HtmlTextWriter writer)
		{
			if(this.InputType == InputBoxType.Hidden)
			{
				if(string.IsNullOrWhiteSpace(this.Name) && (!string.IsNullOrWhiteSpace(this.ID)))
					writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID);

				this.AddAttributes(writer, "CssClass");

				writer.RenderBeginTag(HtmlTextWriterTag.Input);
				writer.RenderEndTag();
				return;
			}

			//生成最外层的Div布局元素，即<div class="field">
			if(this.IsRenderFieldTag)
			{
				if(!string.IsNullOrWhiteSpace(this.FieldCssClass))
					writer.AddAttribute(HtmlTextWriterAttribute.Class, this.FieldCssClass);

				if(!string.IsNullOrWhiteSpace(this.FieldStyle))
					writer.AddAttribute(HtmlTextWriterAttribute.Style, this.FieldStyle);

				writer.RenderBeginTag(this.FieldTagName);
			}

			if(this.InputType == InputBoxType.CheckBox || this.InputType == InputBoxType.Radio)
			{
				//生成输入框的外层元素，即<div class="ui input">
				if(!string.IsNullOrWhiteSpace(this.CssClass))
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Class, this.GetCssClass());
					writer.RenderBeginTag(HtmlTextWriterTag.Div);
				}

				//调用基类同名方法(生成input元素及其内容)
				base.Render(writer);

				//在input元素后面再生成Label标签
				this.RenderLabel(writer);
			}
			else
			{
				//在input元素前面先生成Label标签
				this.RenderLabel(writer);

				//生成输入框的外层元素，即<div class="ui input">
				if(!string.IsNullOrWhiteSpace(this.CssClass))
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Class, this.GetCssClass());
					writer.RenderBeginTag(HtmlTextWriterTag.Div);
				}

				//调用基类同名方法(生成input元素及其内容)
				base.Render(writer);
			}

			//关闭输入框的外层元素，即</div>
			if(!string.IsNullOrWhiteSpace(this.CssClass))
				writer.RenderEndTag();

			if(this.IsRenderFieldTag)
			{
				//关闭最外层的Div布局元素，即生成</div>
				writer.RenderEndTag();
			}
		}

		protected virtual string GetCssClass()
		{
			return this.CssClass;
		}

		protected virtual void RenderLabel(HtmlTextWriter writer)
		{
			if(string.IsNullOrWhiteSpace(this.Label))
				return;

			if(!string.IsNullOrWhiteSpace(this.ID))
				writer.AddAttribute(HtmlTextWriterAttribute.For, this.ID);

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "label");
			writer.RenderBeginTag(HtmlTextWriterTag.Label);
			writer.WriteEncodedText(this.Label);
			writer.RenderEndTag();
		}
		#endregion
	}
}
