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
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	[DefaultProperty("Items")]
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class ComboBox : CompositeDataBoundControl
	{
		#region 成员变量
		private ComboBoxBinding _binding;
		private ComboBoxItemCollection _items;
		#endregion

		#region 构造函数
		public ComboBox()
		{
			_items = new ComboBoxItemCollection();
		}
		#endregion

		#region 公共属性
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ComboBoxBinding Binding
		{
			get
			{
				if(_binding == null)
					System.Threading.Interlocked.CompareExchange(ref _binding, new ComboBoxBinding(), null);

				return _binding;
			}
		}

		[Bindable(true)]
		[DefaultValue(false)]
		[PropertyMetadata(false)]
		public bool AutoSubmit
		{
			get
			{
				return this.GetPropertyValue(() => this.AutoSubmit);
			}
			set
			{
				this.SetPropertyValue(() => this.AutoSubmit, value);
			}
		}

		[DefaultValue(ComboBoxRenderMode.Classic)]
		[PropertyMetadata(false)]
		public ComboBoxRenderMode RenderMode
		{
			get
			{
				return this.GetPropertyValue(() => this.RenderMode);
			}
			set
			{
				this.SetPropertyValue(() => this.RenderMode, value);
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
		[DefaultValue(-1)]
		[PropertyMetadata(false)]
		public int SelectedIndex
		{
			get
			{
				return this.GetPropertyValue(() => this.SelectedIndex);
			}
			set
			{
				this.SetPropertyValue(() => this.SelectedIndex, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
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

		public ComboBoxItemCollection Items
		{
			get
			{
				return _items;
			}
		}
		#endregion

		#region 生成控件
		protected override void Render(HtmlTextWriter writer)
		{
			switch(this.RenderMode)
			{
				case ComboBoxRenderMode.Classic:
					this.RenderClassic(writer);
					break;
				case ComboBoxRenderMode.Custom:
					this.RenderCustom(writer);
					break;
			}

			//调用基类同名方法
			base.Render(writer);
		}
		#endregion

		#region 私有方法
		private void RenderCustom(HtmlTextWriter writer)
		{
			if(string.IsNullOrWhiteSpace(this.ID))
				throw new InvalidOperationException("The ID property is null or empty.");

			int index = 0;
			string displayName = this.ID + "-combobox";
			string selectedText = null;
			string selectedValue = null;

			//生成下拉框视图的开始标签<div>
			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID + "-view");
			writer.AddAttribute(HtmlTextWriterAttribute.Style, "display:none");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			//生成下拉框视图内的列表开始标签<ul>
			writer.RenderBeginTag(HtmlTextWriterTag.Ul);

			foreach(var item in _items)
			{
				writer.AddAttribute("zs:value", item.Value);

				if(item.Disabled)
					writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

				writer.RenderBeginTag(HtmlTextWriterTag.Li);
				writer.WriteEncodedText(item.Text);
				writer.RenderEndTag();

				if(this.SelectedIndex < 0)
				{
					if(string.Equals(this.SelectedValue, item.Value, StringComparison.OrdinalIgnoreCase))
					{
						selectedText = item.Text;
						selectedValue = item.Value;
					}
				}
				else
				{
					if(this.SelectedIndex == index)
					{
						selectedText = item.Text;
						selectedValue = item.Value;
					}
				}

				index++;
			}

			if(this.DataSource != null && _binding != null)
			{
				IEnumerable dataSource = this.DataSource as IEnumerable;

				if(dataSource != null)
				{
					foreach(object dataItem in dataSource)
					{
						string value = BindingUtility.FormatBindingValue(_binding.ValueMember, dataItem);

						if(string.IsNullOrEmpty(value))
							continue;

						if(this.SelectedIndex < 0)
						{
							if(string.Equals(this.SelectedValue, value, StringComparison.OrdinalIgnoreCase))
							{
								selectedText = BindingUtility.FormatBindingValue(_binding.TextMember, dataItem);
								selectedValue = value;
							}
						}
						else
						{
							if(this.SelectedIndex == index)
							{
								selectedText = BindingUtility.FormatBindingValue(_binding.TextMember, dataItem);
								selectedValue = value;
							}
						}

						writer.AddAttribute("zs:value", value);

						writer.RenderBeginTag(HtmlTextWriterTag.Li);
						writer.WriteEncodedText(BindingUtility.FormatBindingValue(_binding.TextMember, dataItem));
						writer.RenderEndTag();

						index++;
					}
				}
			}

			//生成下拉框视图内的列表结束标签</ul>
			writer.RenderEndTag();

			//生成下拉框视图内的隐藏域(开始)
			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
			writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID);
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "combobox");
			writer.AddAttribute(HtmlTextWriterAttribute.Value, selectedValue);
			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();
			//生成下拉框视图内的隐藏域(完成)

			//生成下拉框视图的结束标签</div>
			writer.RenderEndTag();

			if(!string.IsNullOrWhiteSpace(this.Label))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.For, displayName);
				writer.RenderBeginTag(HtmlTextWriterTag.Label);
				writer.WriteEncodedText(this.Label);
				writer.RenderEndTag();
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Id, displayName);
			writer.AddAttribute(HtmlTextWriterAttribute.Name, displayName);
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
			writer.AddAttribute(HtmlTextWriterAttribute.ReadOnly, "readonly");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "combobox-input");
			writer.AddAttribute(HtmlTextWriterAttribute.Value, selectedText);
			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();
		}

		private void RenderClassic(HtmlTextWriter writer)
		{
			int index = 0;

			if(!string.IsNullOrWhiteSpace(this.Label))
			{
				if(!string.IsNullOrWhiteSpace(this.ID))
					writer.AddAttribute(HtmlTextWriterAttribute.For, this.ID);

				writer.RenderBeginTag(HtmlTextWriterTag.Label);
				writer.WriteEncodedText(this.Label);
				writer.RenderEndTag();
			}

			if(!string.IsNullOrWhiteSpace(this.ID))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
				writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID);
			}

			if(!string.IsNullOrWhiteSpace(this.CssClass))
				writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);

			//生成自动提交的脚本事件代码
			if(this.AutoSubmit)
				writer.AddAttribute(HtmlTextWriterAttribute.Onchange, @"javascript:var current = this.parentNode;while(current != null && current.tagName.toLowerCase() != 'form'){current = current.parentNode;}if(current != null && current.tagName.toLowerCase() == 'form')current.submit();", false);

			//生成控件标记(开始)
			writer.RenderBeginTag(HtmlTextWriterTag.Select);

			foreach(var item in _items)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Value, item.Value);

				if(item.Disabled)
					writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

				if(this.SelectedIndex < 0)
				{
					if(string.Equals(this.SelectedValue, item.Value, StringComparison.OrdinalIgnoreCase))
						writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
				}
				else
				{
					if(this.SelectedIndex == index)
						writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
				}

				writer.RenderBeginTag(HtmlTextWriterTag.Option);
				writer.WriteEncodedText(item.Text);
				writer.RenderEndTag();

				index++;
			}

			if(this.DataSource != null)
			{
				IEnumerable dataSource = this.DataSource as IEnumerable;

				if(dataSource != null)
				{
					foreach(object dataItem in dataSource)
					{
						string value = BindingUtility.FormatBindingValue(_binding != null ? _binding.ValueMember : string.Empty, dataItem);

						if(string.IsNullOrEmpty(value))
							continue;

						writer.AddAttribute(HtmlTextWriterAttribute.Value, value);

						if(this.SelectedIndex < 0)
						{
							if(string.Equals(this.SelectedValue, value, StringComparison.OrdinalIgnoreCase))
								writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
						}
						else
						{
							if(this.SelectedIndex == index)
								writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
						}

						writer.RenderBeginTag(HtmlTextWriterTag.Option);
						writer.WriteEncodedText(BindingUtility.FormatBindingValue(_binding != null ? _binding.TextMember : string.Empty, dataItem));
						writer.RenderEndTag();

						index++;
					}
				}
			}

			//生成控件标记(结束)
			writer.RenderEndTag();
		}
		#endregion

		#region 嵌套子类
		[Serializable]
		public class ComboBoxBinding
		{
			public string TextMember
			{
				get;
				set;
			}

			public string ValueMember
			{
				get;
				set;
			}
		}
		#endregion
	}
}
