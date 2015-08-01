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
	/// <summary>
	/// 表示数据提取框的控件。
	/// </summary>
	/// <remarks>
	/// 该控件生成的HTML元素如下：
	/// <![CDATA[
	/// <label for="ID-pickbox">LabelText</label>
	/// <input id="ID-pickbox" name="ID-pickbox" class="pickbox" value="formatted value" zs:format="{Property}-{Property}" />
	/// <div id="ID-view" title="" style="display:none">
	/// 	<table width="" height="">
	/// 		<thead>
	/// 			<tr>
	/// 				<td name="PropertyName" width="x%">Property-Title</td>
	/// 				<td name="PropertyName" width="x%">Property-Title</td>
	/// 			</tr>
	/// 		</thead>
	/// 	</table>
	/// 
	/// 	<input id="ID" name="ID" type="hidden" class="picker" value="value of the key" zs:selectionMode="" zs:keys="KeyProperty, KeyProperty" zs:url="/Tollgates/Picker/Get=type=Road&values=..." />
	/// </div>
	/// ]]>
	/// </remarks>
	[ParseChildren(true, "Columns")]
	public class Picker : DataBoundControl
	{
		#region 成员变量
		private PickerColumnCollection _columns;
		#endregion

		#region 构造函数
		public Picker()
		{
			_columns = new PickerColumnCollection();
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
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

		[Bindable(true)]
		[DefaultValue("")]
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

		public string DataKeys
		{
			get
			{
				return this.GetPropertyValue(() => this.DataKeys);
			}
			set
			{
				this.SetPropertyValue(() => this.DataKeys, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		public string SelectedValue
		{
			get
			{
				return this.GetPropertyValue(() => this.SelectedValue);
			}
			set
			{
				this.SetPropertyValue(() => this.SelectedValue, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(SelectionMode.Single)]
		public SelectionMode SelectionMode
		{
			get
			{
				return this.GetPropertyValue(() => this.SelectionMode);
			}
			set
			{
				this.SetPropertyValue(() => this.SelectionMode, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
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
		public string FormatString
		{
			get
			{
				return this.GetPropertyValue(() => this.FormatString);
			}
			set
			{
				this.SetPropertyValue(() => this.FormatString, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata("href", PropertyRender = "UrlPropertyRender.Default")]
		public string Url
		{
			get
			{
				return this.GetPropertyValue(() => this.Url);
			}
			set
			{
				this.SetPropertyValue(() => this.Url, value);
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

		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public PickerColumnCollection Columns
		{
			get
			{
				return _columns;
			}
		}
		#endregion

		#region 生成控件
		protected override void Render(HtmlTextWriter writer)
		{
			if(string.IsNullOrWhiteSpace(this.ID))
				throw new InvalidOperationException("The ID property is null or empty.");

			string diplayName = this.ID + "-pickbox";

			if(!string.IsNullOrWhiteSpace(this.Label))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.For, diplayName);
				writer.RenderBeginTag(HtmlTextWriterTag.Label);
				writer.WriteEncodedText(this.Label);
				writer.RenderEndTag();
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Id, diplayName);
			writer.AddAttribute(HtmlTextWriterAttribute.Name, diplayName);
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "pickbox");
			writer.AddAttribute(HtmlTextWriterAttribute.ReadOnly, "readonly");
			writer.AddAttribute(HtmlTextWriterAttribute.Value, this.Text);
			writer.AddAttribute("zs:format", this.FormatString, false);

			if(!this.Enabled)
				writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();

			//开始生成提取框的视图部分
			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID + "-view");
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "display:none");
			if(!string.IsNullOrWhiteSpace(this.Title))
				writer.AddAttribute(HtmlTextWriterAttribute.Title, this.Title);

			//生成视图部分的DIV起始元素
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			if(this.Width.Value != 0)
				writer.AddAttribute(HtmlTextWriterAttribute.Width, this.Width.ToString());
			if(this.Height.Value != 0)
				writer.AddAttribute(HtmlTextWriterAttribute.Height, this.Height.ToString());

			writer.RenderBeginTag(HtmlTextWriterTag.Table); //生成视图表格元素<table>
			writer.RenderBeginTag(HtmlTextWriterTag.Thead);	//生成视图表格内的<thead>
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);	//生成视图表格内的<tr>

			foreach(var column in _columns)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Name, column.Name);
				writer.AddAttribute(HtmlTextWriterAttribute.Width, Utility.GetWidth(column.Width, _columns.GetTotalWeight()).ToString());
				writer.RenderBeginTag(HtmlTextWriterTag.Td);

				if(string.IsNullOrWhiteSpace(column.Title))
					writer.WriteEncodedText(column.Name);
				else
					writer.WriteEncodedText(column.Title);

				writer.RenderEndTag();
			}

			writer.RenderEndTag();	//结束视图表格内的<tr>
			writer.RenderEndTag();	//结束视图表格内的<thead>
			writer.RenderEndTag();	//结束视图表格内的<table>

			//生成视图内的隐藏域
			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
			if(string.IsNullOrWhiteSpace(this.Name))
				writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID);
			else
				writer.AddAttribute(HtmlTextWriterAttribute.Name, this.Name);

			writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "picker");
			writer.AddAttribute(HtmlTextWriterAttribute.Value, this.SelectedValue);
			writer.AddAttribute("zs:selectionMode", this.SelectionMode.ToString());
			writer.AddAttribute("zs:keys", this.DataKeys);
			writer.AddAttribute("zs:url", this.Url, false);
			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();

			//生成视图部分的DIV结束元素
			writer.RenderEndTag();

			//调用基类同名方法
			base.Render(writer);
		}
		#endregion
	}
}
