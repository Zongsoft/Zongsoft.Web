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
using System.Web.UI.HtmlControls;

namespace Zongsoft.Web.Controls
{
	[ParseChildren(true, "Columns")]
	public class Grid : CompositeDataBoundControl, INamingContainer
	{
		#region 成员变量
		private ITemplate _emptyTemplate;
		private ITemplate _headerTemplate;
		private ITemplate _footerTemplate;
		private GridColumnCollection _columns;
		#endregion

		#region 构造函数
		public Grid()
		{
			this.CssClass = "ui table striped";
			_columns = new GridColumnCollection(this);
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue(SelectionMode.None)]
		[PropertyMetadata("data-selectionMode")]
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
		[DefaultValue("select")]
		[PropertyMetadata("data-selectionName")]
		public string SelectionName
		{
			get
			{
				return this.GetPropertyValue(() => this.SelectionName);
			}
			set
			{
				this.SetPropertyValue(() => this.SelectionName, value);
			}
		}

		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridColumnCollection Columns
		{
			get
			{
				return _columns;
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(Grid))]
		public ITemplate EmptyTemplate
		{
			get
			{
				return _emptyTemplate;
			}
			set
			{
				_emptyTemplate = value;
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(Grid))]
		public ITemplate HeaderTemplate
		{
			get
			{
				return _headerTemplate;
			}
			set
			{
				_headerTemplate = value;
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(Grid))]
		public ITemplate FooterTemplate
		{
			get
			{
				return _footerTemplate;
			}
			set
			{
				_footerTemplate = value;
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

		#region 生成控件
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			//生成标准的属性集
			this.AddAttributes(writer);

			//生成内置的表格特性
			writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");

			writer.RenderBeginTag(HtmlTextWriterTag.Table);
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			this.RenderTableHeader(writer);
			this.RenderTableBody(writer);
			this.RenderTableFooter(writer);
		}
		#endregion

		#region 私有方法
		private void RenderTableHeader(HtmlTextWriter writer)
		{
			if(_headerTemplate != null)
			{
				var container = new HtmlGenericControl("thead");
				_headerTemplate.InstantiateIn(container);
				container.RenderControl(writer);
				return;
			}

			writer.RenderBeginTag(HtmlTextWriterTag.Thead);

			//开始生成表头行(<tr>
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			//生成选择列
			this.GenerateSelection(writer, null);

			//表头列(开始)
			foreach(GridColumnBase column in _columns)
			{
				string style = string.Empty;

				if(!string.IsNullOrWhiteSpace(column.Name))
					writer.AddAttribute(HtmlTextWriterAttribute.Name, column.Name);
				if(!string.IsNullOrWhiteSpace(column.CssClass))
					writer.AddAttribute(HtmlTextWriterAttribute.Class, column.CssClass);

				writer.AddAttribute(HtmlTextWriterAttribute.Align, column.TitleAlignment.ToString().ToLowerInvariant());

				if(column.Visible)
				{
					if(column.Width.Value != 0)
						style = "width:" + Utility.GetWidth(column.Width, _columns.GetTotalWeight());
				}
				else
				{
					style = "display:none;";
				}

				if(!string.IsNullOrEmpty(style))
					writer.AddAttribute(HtmlTextWriterAttribute.Style, style);

				writer.RenderBeginTag(HtmlTextWriterTag.Th);

				if(string.IsNullOrWhiteSpace(column.Title))
					writer.Write("&nbsp;");
				else
					writer.Write(column.Title);

				writer.RenderEndTag();
			}//表头列(结束)

			//结束生成表头行(</tr>
			writer.RenderEndTag();

			//结束生成表头部分
			writer.RenderEndTag();
		}

		private void RenderTableFooter(HtmlTextWriter writer)
		{
			if(_footerTemplate == null)
				return;

			var container = new HtmlGenericControl("tfoot");
			_footerTemplate.InstantiateIn(container);
			container.RenderControl(writer);
		}

		private void RenderTableBody(HtmlTextWriter writer)
		{
			//生成表体标记(开始)
			writer.RenderBeginTag(HtmlTextWriterTag.Tbody);

			if(this.DataSource == null || (this.DataSource.GetType() == typeof(string) && ((string)this.DataSource).Length == 0))
			{
				if(_emptyTemplate != null)
				{
					var tr = new System.Web.UI.HtmlControls.HtmlTableRow();
					var td = new System.Web.UI.HtmlControls.HtmlTableCell()
					{
						ColSpan = this.Columns.Count,
					};

					tr.Controls.Add(td);
					_emptyTemplate.InstantiateIn(td);
					tr.RenderControl(writer);
				}
			}
			else
			{
				IEnumerable dataSource = this.DataSource as IEnumerable;

				if(dataSource == null)
				{
					this.GenerateRow(writer, this.DataSource, 1);
				}
				else
				{
					int index = 0;
					foreach(var dataItem in dataSource)
					{
						FormExtension.PushDataItem(this.Page, dataItem, index);
						this.GenerateRow(writer, dataItem, index++);
						FormExtension.PopDataItem(this.Page);
					}
				}
			}

			//生成表体标记(结束)
			writer.RenderEndTag();
		}

		private void GenerateSelection(HtmlTextWriter writer, object dataItem)
		{
			if(this.SelectionMode == Web.Controls.SelectionMode.None)
				return;

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "selection");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);

			switch(this.SelectionMode)
			{
				case Web.Controls.SelectionMode.Single:
					if(dataItem != null)
					{
						writer.AddAttribute(HtmlTextWriterAttribute.Name, this.SelectionName);
						writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
						writer.AddAttribute(HtmlTextWriterAttribute.Value, Utility.GetDataValue(dataItem, this.DataKeys));
						writer.RenderBeginTag(HtmlTextWriterTag.Input);
						writer.RenderEndTag();
					}
					break;
				case Web.Controls.SelectionMode.Multiple:
					if(dataItem != null)
					{
						writer.AddAttribute(HtmlTextWriterAttribute.Name, this.SelectionName);
						writer.AddAttribute(HtmlTextWriterAttribute.Value, Utility.GetDataValue(dataItem, this.DataKeys));
					}
					writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
					writer.RenderBeginTag(HtmlTextWriterTag.Input);
					writer.RenderEndTag();
					break;
			}

			writer.RenderEndTag();
		}

		private void GenerateRow(HtmlTextWriter writer, object dataItem, int rowIndex)
		{
			if(dataItem == null)
				return;

			//生成表体的行标记(<tr>)
			writer.AddAttribute(HtmlTextWriterAttribute.Class, (rowIndex % 2 == 1 ? "odd" : "even"));
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			//生成选择列
			this.GenerateSelection(writer, dataItem);

			foreach(var column in _columns)
			{
				//循环生成列的外围属性
				for(int i = 0; i < column.Attributes.Count; i++)
					writer.AddAttribute(column.Attributes.Keys[i], column.Attributes[i]);

				if(column.Visible)
					writer.AddAttribute(HtmlTextWriterAttribute.Align, column.Alignment.ToString().ToLowerInvariant());
				else
					writer.AddAttribute(HtmlTextWriterAttribute.Style, "display:none;");

				//生成表体的数据列标记(<td>)
				writer.RenderBeginTag(HtmlTextWriterTag.Td);

				//调用当前表格列的生成方法
				column.Render(writer, dataItem, rowIndex);

				//生成表体的数据列标记(</td>)
				writer.RenderEndTag();
			}

			//生成表体的行标记(</tr>)
			writer.RenderEndTag();
		}
		#endregion
	}
}
