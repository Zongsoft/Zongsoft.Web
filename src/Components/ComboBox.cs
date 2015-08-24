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
	public class ComboBox : CompositeDataBoundControl, INamingContainer
	{
		#region 成员变量
		private ComboBoxItem _selectedItem;
		private ITemplate _itemTemplate;
		private ComboBoxBinding _binding;
		private ComboBoxItemCollection _items;
		#endregion

		#region 构造函数
		public ComboBox()
		{
			this.CssClass = "ui selection dropdown";
			_items = new ComboBoxItemCollection(this);
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
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
				if(!string.IsNullOrWhiteSpace(value))
					this.SetPropertyValue(() => this.Name, value);
			}
		}

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

		[DefaultValue(true)]
		[PropertyMetadata(false)]
		public bool IsRenderFieldTag
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

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(ComboBox))]
		public ITemplate ItemTemplate
		{
			get
			{
				return _itemTemplate;
			}
			set
			{
				_itemTemplate = value;
			}
		}

		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
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
			//生成最外层的Div布局元素，即<div class="field">
			if(this.IsRenderFieldTag)
			{
				if(!string.IsNullOrWhiteSpace(this.FieldCssClass))
					writer.AddAttribute(HtmlTextWriterAttribute.Class, this.FieldCssClass);

				if(!string.IsNullOrWhiteSpace(this.FieldStyle))
					writer.AddAttribute(HtmlTextWriterAttribute.Style, this.FieldStyle);

				writer.RenderBeginTag(this.FieldTagName);
			}

			//生成Label标签
			if(!string.IsNullOrWhiteSpace(this.Label))
			{
				if(!string.IsNullOrWhiteSpace(this.ID))
					writer.AddAttribute(HtmlTextWriterAttribute.For, this.ID);

				writer.AddAttribute(HtmlTextWriterAttribute.Class, "label");
				writer.RenderBeginTag(HtmlTextWriterTag.Label);
				writer.WriteEncodedText(this.Label);
				writer.RenderEndTag();
			}

			//调用基类同名方法(生成下拉框元素及其内容)
			base.Render(writer);

			if(this.IsRenderFieldTag)
			{
				//关闭最外层的Div布局元素，即生成</div>
				writer.RenderEndTag();
			}
		}

		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			if(this.RenderMode == ComboBoxRenderMode.Classic)
			{
				if(!string.IsNullOrWhiteSpace(this.Name))
					writer.AddAttribute(HtmlTextWriterAttribute.Name, this.Name);

				this.AddAttributes(writer);

				//生成自动提交的脚本事件代码
				if(this.AutoSubmit)
					writer.AddAttribute(HtmlTextWriterAttribute.Onchange, @"javascript:var current = this.parentNode;while(current != null && current.tagName.toLowerCase() != 'form'){current = current.parentNode;}if(current != null && current.tagName.toLowerCase() == 'form')current.submit();", false);

				//生成控件标记(开始)
				writer.RenderBeginTag(HtmlTextWriterTag.Select);
			}
			else
			{
				this.AddAttributes(writer);
				writer.RenderBeginTag(HtmlTextWriterTag.Div);

				writer.AddAttribute(HtmlTextWriterAttribute.Class, "dropdown icon");
				writer.RenderBeginTag(HtmlTextWriterTag.I);
				writer.RenderEndTag();

				writer.AddAttribute(HtmlTextWriterAttribute.Class, "menu");
				writer.RenderBeginTag(HtmlTextWriterTag.Dl);
			}
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();

			if(this.RenderMode == ComboBoxRenderMode.Custom)
			{
				if(!string.IsNullOrWhiteSpace(this.Name))
					writer.AddAttribute(HtmlTextWriterAttribute.Name, this.Name);
				if(_selectedItem != null || this.SelectedValue != null)
					writer.AddAttribute(HtmlTextWriterAttribute.Value, _selectedItem == null ? this.SelectedValue : _selectedItem.Value);
				writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
				this.AddAttributes(writer, "ID", "CssClass");
				if(this.AutoSubmit)
					writer.AddAttribute(HtmlTextWriterAttribute.Onchange, @"javascript:var current = this.parentNode;while(current != null && current.tagName.toLowerCase() != 'form'){current = current.parentNode;}if(current != null && current.tagName.toLowerCase() == 'form')current.submit();", false);
				writer.RenderBeginTag(HtmlTextWriterTag.Input);
				writer.RenderEndTag();

				writer.AddAttribute(HtmlTextWriterAttribute.Class, "default text");
				writer.RenderBeginTag(HtmlTextWriterTag.Span);
				if(_selectedItem != null)
					writer.WriteEncodedText(_selectedItem.Text);
				writer.RenderEndTag();

				writer.RenderEndTag();
			}
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			int index = 0;

			index = this.RenderItems(writer, index);
			this.RenderDataItems(writer, index);
		}
		#endregion

		#region 私有方法
		private int RenderItems(HtmlTextWriter writer, int index)
		{
			foreach(var item in _items)
			{
				var isSelected = this.SelectedIndex < 0 ? string.Equals(this.SelectedValue, item.Value, StringComparison.OrdinalIgnoreCase) : this.SelectedIndex == index;

				if(isSelected)
					_selectedItem = item;

				//if(_itemTemplate == null)
					item.ToHtmlString(writer, this.RenderMode, isSelected);
				//else
				//	this.RenderItemTemplate(writer, item.Value, item, index);

				index++;
			}

			return index;
		}

		private int RenderDataItems(HtmlTextWriter writer, int index)
		{
			if(this.DataSource == null)
				return index;

			var dataSource = this.DataSource as IEnumerable;

			if(dataSource == null)
			{
				var item = new ComboBoxItem()
				{
					Value = BindingUtility.FormatBindingValue(_binding != null ? _binding.ValueMember : string.Empty, this.DataSource, true),
					Text = BindingUtility.FormatBindingValue(_binding != null ? _binding.TextMember : string.Empty, this.DataSource, true),
					Description = BindingUtility.FormatBindingValue(_binding != null ? _binding.DescriptionMember : string.Empty, this.DataSource, true),
					Icon = BindingUtility.FormatBindingValue(_binding != null ? _binding.IconMember : string.Empty, this.DataSource, true),
				};

				var isSelected = this.SelectedIndex < 0 ? string.Equals(this.SelectedValue, item.Value, StringComparison.OrdinalIgnoreCase) : this.SelectedIndex == index;

				if(isSelected)
					_selectedItem = item;

				if(_itemTemplate == null)
					item.ToHtmlString(writer, this.RenderMode, isSelected);
				else
					this.RenderItemTemplate(writer, item.Value, this.DataSource, index, isSelected);
			}
			else
			{
				foreach(object dataItem in dataSource)
				{
					var item = new ComboBoxItem()
					{
						Value = BindingUtility.FormatBindingValue(_binding != null ? _binding.ValueMember : string.Empty, dataItem, true),
						Text = BindingUtility.FormatBindingValue(_binding != null ? _binding.TextMember : string.Empty, dataItem, true),
						Description = BindingUtility.FormatBindingValue(_binding != null ? _binding.DescriptionMember : string.Empty, dataItem, true),
						Icon = BindingUtility.FormatBindingValue(_binding != null ? _binding.IconMember : string.Empty, dataItem, true),
					};

					var isSelected = this.SelectedIndex < 0 ? string.Equals(this.SelectedValue, item.Value, StringComparison.OrdinalIgnoreCase) : this.SelectedIndex == index;

					if(isSelected)
						_selectedItem = item;

					if(_itemTemplate == null)
						item.ToHtmlString(writer, this.RenderMode, isSelected);
					else
						this.RenderItemTemplate(writer, item.Value, dataItem, index, isSelected);
				}
			}

			return ++index;
		}

		private void RenderItemTemplate(HtmlTextWriter writer, string value, object dataItem, int index, bool isSelected)
		{
			DataItemContainer<ComboBox> container;

			if(this.RenderMode == ComboBoxRenderMode.Classic)
			{
				container = new DataItemContainer<ComboBox>(this, dataItem, index, "option");
				container.SetAttributeValue("value", value);

				if(isSelected)
					container.SetAttributeValue("selected", "selected");
			}
			else
			{
				container = new DataItemContainer<ComboBox>(this, dataItem, index, "dt", "item");
				container.SetAttributeValue("data-value", value);

				if(isSelected)
				{
					container.SetAttributeValue("data-selected", "selected");
					container.CssClass = "active selected item";
				}
			}

			_itemTemplate.InstantiateIn(container);
			container.RenderControl(writer);
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

			public string DescriptionMember
			{
				get;
				set;
			}

			public string IconMember
			{
				get;
				set;
			}
		}
		#endregion
	}
}
