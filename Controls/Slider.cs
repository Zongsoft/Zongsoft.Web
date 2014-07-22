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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	///		<example>
	///		<![CDATA[
	///	<dl class="slider">
	///		<dt>
	///			<a href=""><img src="" /></a>
	///	
	///			<div>
	///				<h4></h4>
	///				<p></p>
	///			</div>
	///		</dt>
	///	</dl>
	///		]]>
	///		</example>
	/// </remarks>
	public class Slider : CompositeDataBoundControl
	{
		#region 成员字段
		private IList<SliderItem> _items;
		private SliderItem _binding;
		#endregion

		#region 公共属性
		public Slider()
		{
			_items = new List<SliderItem>();
		}
		#endregion

		#region 公共属性
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public SliderItem Binding
		{
			get
			{
				if(_binding == null)
					_binding = new SliderItem();

				return _binding;
			}
		}

		public ICollection<SliderItem> Items
		{
			get
			{
				return _items;
			}
		}
		#endregion

		#region 重写方法
		public override void RenderControl(HtmlTextWriter writer)
		{
			if(!string.IsNullOrWhiteSpace(this.ID))
				writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);

			if(string.IsNullOrWhiteSpace(this.CssClass))
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "slider");
			else
				writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);

			writer.RenderBeginTag(HtmlTextWriterTag.Dl);

			for(int i = 0; i < _items.Count; i++)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Dt);

				this.RenderItem(writer, _items[i], i == _items.Count - 1 ? -1 : i);

				writer.RenderEndTag();
			}

			//根据绑定数据源生成对应的列表项
			this.RenderItems(writer);

			writer.RenderEndTag();
		}
		#endregion

		#region 虚拟方法
		protected virtual void RenderItem(HtmlTextWriter writer, SliderItem item, int index)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Href, item.Url);
			writer.RenderBeginTag(HtmlTextWriterTag.A);

			writer.AddAttribute(HtmlTextWriterAttribute.Src, item.ImageUrl);
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();

			writer.RenderEndTag();

			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			writer.RenderBeginTag(HtmlTextWriterTag.H4);
			writer.WriteEncodedText(item.Title);
			writer.RenderEndTag();

			var lines = item.Description.Split('\r');
			foreach(var line in lines)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.P);
				writer.WriteEncodedText(line);
				writer.RenderEndTag();
			}

			writer.RenderEndTag();
		}
		#endregion

		#region 私有方法
		private void RenderItems(HtmlTextWriter writer)
		{
			if(this.DataSource == null)
				return;

			IEnumerable dataSource = this.DataSource as IEnumerable;

			if(dataSource == null)
			{
				RenderItem(writer, this.DataSource, _items.Count);
			}
			else
			{
				int offset = 0;

				foreach(var dataItem in dataSource)
				{
					this.RenderItem(writer, dataItem, _items.Count + offset++);
				}
			}
		}

		private void RenderItem(HtmlTextWriter writer, object dataItem, int index)
		{
			if(_binding == null)
				return;

			var item = new SliderItem()
			{
				Url = BindingUtility.FormatBindingValue(_binding.Url, dataItem),
				ImageUrl = BindingUtility.FormatBindingValue(_binding.ImageUrl, dataItem),
				Title = BindingUtility.FormatBindingValue(_binding.Title, dataItem),
				Description = BindingUtility.FormatBindingValue(_binding.Description, dataItem),
			};

			writer.RenderBeginTag(HtmlTextWriterTag.Dt);
			this.RenderItem(writer, item, index);
			writer.RenderEndTag();
		}
		#endregion

		#region 嵌套子类
		public class SliderItem
		{
			#region 成员字段
			private string _url;
			private string _imageUrl;
			private string _title;
			private string _description;
			#endregion

			#region 公共属性
			public string Url
			{
				get
				{
					return _url ?? string.Empty;
				}
				set
				{
					_url = value;
				}
			}

			public string ImageUrl
			{
				get
				{
					return _imageUrl ?? string.Empty;
				}
				set
				{
					_imageUrl = value;
				}
			}

			public string Title
			{
				get
				{
					return _title ?? string.Empty;
				}
				set
				{
					_title = value;
				}
			}

			public string Description
			{
				get
				{
					return _description ?? string.Empty;
				}
				set
				{
					_description = value;
				}
			}
			#endregion
		}
		#endregion
	}
}
