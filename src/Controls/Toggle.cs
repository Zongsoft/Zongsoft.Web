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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	[DefaultProperty("Items")]
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class Toggle : CompositeDataBoundControl
	{
		#region 成员字段
		private ToggleBinding _binding;
		private ToggleItemCollection _items;
		#endregion

		#region 构造函数
		public Toggle()
		{
			this.CssClass = "field-items";
			_items = new ToggleItemCollection(this);
		}
		#endregion

		#region 公共属性
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string Name
		{
			get
			{
				var name = this.GetPropertyValue(() => this.Name);

				if(string.IsNullOrWhiteSpace(name))
					return this.ID;

				return name.Trim();
			}
			set
			{
				this.SetPropertyValue(() => this.Name, value);
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

		[DefaultValue(ToggleType.Single)]
		[PropertyMetadata(false)]
		public ToggleType Type
		{
			get
			{
				return this.GetPropertyValue(() => this.Type);
			}
			set
			{
				this.SetPropertyValue(() => this.Type, value);
			}
		}

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ToggleBinding Binding
		{
			get
			{
				if(_binding == null)
					System.Threading.Interlocked.CompareExchange(ref _binding, new ToggleBinding(), null);

				return _binding;
			}
		}

		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ToggleItemCollection Items
		{
			get
			{
				return _items;
			}
		}
		#endregion

		#region 重写方法
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			this.AddAttributes(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			foreach(var item in _items)
			{
				this.RenderItem(writer, item);
			}

			if(this.DataSource != null && _binding != null)
			{
				var dataSource = this.DataSource as IEnumerable;

				if(dataSource != null && dataSource.GetType() != typeof(string))
				{
					foreach(object dataItem in dataSource)
					{
						var item = new ToggleItem();

						item.Text = BindingUtility.FormatBindingValue(_binding != null ? _binding.TextMember : string.Empty, dataItem, true);
						item.Value = BindingUtility.FormatBindingValue(_binding != null ? _binding.ValueMember : string.Empty, dataItem, true);
						item.Checked = Zongsoft.Common.Convert.ConvertValue<bool>(BindingUtility.GetBindingValue(_binding.CheckedMember, dataItem, true), false);
						item.Disabled = Zongsoft.Common.Convert.ConvertValue<bool>(BindingUtility.GetBindingValue(_binding.DisabledMember, dataItem, true), false);

						this.RenderItem(writer, item);
					}
				}
				else
				{
					var item = new ToggleItem();

					item.Text = BindingUtility.FormatBindingValue(_binding != null ? _binding.TextMember : string.Empty, this.DataSource, true);
					item.Value = BindingUtility.FormatBindingValue(_binding != null ? _binding.ValueMember : string.Empty, this.DataSource, true);
					item.Checked = Zongsoft.Common.Convert.ConvertValue<bool>(BindingUtility.GetBindingValue(_binding.CheckedMember, this.DataSource, true), false);
					item.Disabled = Zongsoft.Common.Convert.ConvertValue<bool>(BindingUtility.GetBindingValue(_binding.DisabledMember, this.DataSource, true), false);

					this.RenderItem(writer, item);
				}
			}
		}

		private void RenderItem(HtmlTextWriter writer, ToggleItem item)
		{
			if(item == null)
				return;

			if(!string.IsNullOrWhiteSpace(item.Text))
				writer.RenderBeginTag(HtmlTextWriterTag.Label);

			if(!string.IsNullOrWhiteSpace(this.Name))
				writer.AddAttribute(HtmlTextWriterAttribute.Name, this.Name);

			if(!string.IsNullOrWhiteSpace(item.Value))
				writer.AddAttribute(HtmlTextWriterAttribute.Value, item.Value);

			if(item.Checked)
				writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");

			if(item.Disabled)
				writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

			if(this.Type == ToggleType.Single)
				writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
			else
				writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");

			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();

			if(!string.IsNullOrWhiteSpace(item.Text))
			{
				writer.WriteEncodedText(item.Text);
				writer.RenderEndTag();
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			//生成最外层的Div布局元素，即<div class="field">
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "field");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			if(!string.IsNullOrWhiteSpace(this.Title))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "label field-title");
				writer.RenderBeginTag(HtmlTextWriterTag.Label);
				writer.WriteEncodedText(this.Title);
				writer.RenderEndTag();
			}

			//调用基类同名方法
			base.Render(writer);

			//关闭最外层的Div布局元素，即生成</div>
			writer.RenderEndTag();
		}
		#endregion

		#region 嵌套子类
		public class ToggleBinding
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

			public string CheckedMember
			{
				get;
				set;
			}

			public string DisabledMember
			{
				get;
				set;
			}
		}
		#endregion
	}
}
