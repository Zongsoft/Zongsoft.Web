﻿/*
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
	/// <summary>
	/// 提供布局的控件。
	/// </summary>
	/// <remarks>
	///		<para>使用范例：</para>
	///		<example>
	///		<code>
	///		<InputPanel id="" runat="server">
	///			<InputBox id="" type="" runat="server" />
	///			<InputBox id="" type="" runat="server" />
	///			<TextBox id="" runat="server" />
	///			<input name="submit" type="submit" value="" />
	///			<input name="clear" type="reset" value="" />
	///		</InputPanel>
	///		</code>
	///		</example>
	/// </remarks>
	[PersistChildren(true)]
	[ParseChildren(false)]
	public class LayoutPanel : DataBoundControl
	{
		#region 成员字段
		private LayoutTableSettings _tableLayoutSettings;
		#endregion

		#region 公共属性
		[DefaultValue(LayoutMode.Fluid)]
		[PropertyMetadata(false)]
		public LayoutMode LayoutMode
		{
			get
			{
				return this.GetPropertyValue(() => this.LayoutMode);
			}
			set
			{
				this.SetPropertyValue(() => this.LayoutMode, value);
			}
		}

		[DefaultValue(2)]
		[PropertyMetadata(false)]
		public int LayoutColumnCount
		{
			get
			{
				return this.GetPropertyValue(() => this.LayoutColumnCount);
			}
			set
			{
				this.SetPropertyValue(() => this.LayoutColumnCount, Math.Max(value, 1));
			}
		}

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public LayoutTableSettings LayoutTableSettings
		{
			get
			{
				if(_tableLayoutSettings == null)
					_tableLayoutSettings = new LayoutTableSettings();

				return _tableLayoutSettings;
			}
			set
			{
				_tableLayoutSettings = value;
			}
		}
		#endregion

		#region 重写方法
		protected override string GetDefaultCssClass()
		{
			switch(this.LayoutMode)
			{
				case Web.Controls.LayoutMode.Fluid:
					if(this.LayoutColumnCount > 0)
						return "ui " + Utility.GetNumberString(this.LayoutColumnCount) + " column grid layout layout-fluid";
					else
						return "ui grid layout layout-fluid";
				case Web.Controls.LayoutMode.Table:
					return "layout layout-table";
				case Web.Controls.LayoutMode.Responsive:
					return "layout layout-responsive";
			}

			return base.GetDefaultCssClass();
		}

		protected override void Render(HtmlTextWriter writer)
		{
			IList<Control> controls = Utility.GetVisibleChildren(this);

			if(controls.Count < 1)
				return;

			switch(this.LayoutMode)
			{
				case Web.Controls.LayoutMode.Fluid:
					this.RenderFluidLayout(writer, controls);
					break;
				case Web.Controls.LayoutMode.Table:
					this.RenderTableLayout(writer, controls);
					break;
				case Web.Controls.LayoutMode.Responsive:
					this.RenderResponsiveLayout(writer, controls);
					break;
			}

			//调用基类同名方法
			//base.Render(writer);
		}
		#endregion

		#region 私有方法
		private void RenderFluidLayout(HtmlTextWriter writer, IList<Control> controls)
		{
			//生成其他自定义属性
			this.AddAttributes(writer);

			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			foreach(Control control in controls)
			{
				if(control is LayoutFluidCell)
				{
					control.RenderControl(writer);
				}
				else
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Class, "column");
					writer.RenderBeginTag(HtmlTextWriterTag.Div);
					control.RenderControl(writer);
					writer.RenderEndTag();
				}
			}

			writer.RenderEndTag();
		}

		private void RenderResponsiveLayout(HtmlTextWriter writer, IList<Control> controls)
		{
			//生成其他自定义属性
			this.AddAttributes(writer);

			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			for(int i = 0; i < controls.Count; i++)
			{
				if(i % this.LayoutColumnCount == 0)
				{
					if(i > 0)
						writer.RenderEndTag();

					writer.AddAttribute(HtmlTextWriterAttribute.Class, "layout-responsive-row");
					writer.RenderBeginTag(HtmlTextWriterTag.Div);
				}

				controls[i].RenderControl(writer);
			}

			if(controls.Count > 0)
				writer.RenderEndTag();

			writer.RenderEndTag();
		}

		private void RenderTableLayout(HtmlTextWriter writer, IList<Control> controls)
		{
			//生成其他自定义属性
			this.AddAttributes(writer);

			writer.RenderBeginTag(HtmlTextWriterTag.Table);

			int columnIndex = 0;
			int columnCount = this.LayoutColumnCount;

			//定义行合并的计数器数组：数组元素按下标依次对应到相应列；
			//元素内容值表示对应列剩下待合并的行数，默认为零表示当前行对应的列无需合并即需要生成对应的<td/>元素；
			//元素内容值为正数，表示其还需要合并的行数，即当前行无需对其生成对应的<td/>元素；
			//元素内容值为负数，表示其需要一直合并，即当前行无需对其生成对应的<td/>元素；
			var rowSpans = new int[this.LayoutColumnCount];

			for(int i = 0; i < controls.Count; i++)
			{
				int colSpan = 1;
				int spanIndex = -1;

				if(columnIndex == 0)
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Tr);
					columnCount = rowSpans.Count(span => span == 0);
				}

				if(i == controls.Count - 1 && this.LayoutTableSettings.MergeLastCells)
				{
					colSpan = columnCount - columnIndex;
				}
				else
				{
					var cell = controls[i] as LayoutTableCell;

					if(cell != null)
					{
						colSpan = Math.Min(cell.ColSpan, columnCount - columnIndex);

						if(cell.RowSpan > 1 || cell.RowSpan == 0)
						{
							spanIndex = this.GetRowSpanIndex(rowSpans, columnIndex);

							if(spanIndex >= 0)
							{
								for(int j = 0; j < colSpan; j++)
								{
									rowSpans[spanIndex + j] = cell.RowSpan == 0 ? -1 : cell.RowSpan;
								}
							}
						}
					}
				}

				if(columnCount > 0)
				{
					if(colSpan > 1 || colSpan == 0)
						writer.AddAttribute(HtmlTextWriterAttribute.Colspan, colSpan.ToString());

					if(spanIndex >= 0)
						writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, rowSpans[spanIndex] < 0 ? "0" : rowSpans[spanIndex].ToString());

					writer.RenderBeginTag(HtmlTextWriterTag.Td);
					controls[i].RenderControl(writer);
					writer.RenderEndTag();
				}

				columnIndex += colSpan;

				if(columnIndex >= columnCount)
				{
					for(int j = 0; j < rowSpans.Length; j++)
					{
						if(rowSpans[j] > 0)
							rowSpans[j]--;
					}

					//生成表格行元素的关闭标签</tr>
					writer.RenderEndTag();
					columnIndex = 0;
				}
			}

			if(columnIndex > 0 && columnIndex < this.LayoutColumnCount)
			{
				if(this.LayoutColumnCount - columnIndex > 1)
					writer.AddAttribute(HtmlTextWriterAttribute.Colspan, (this.LayoutColumnCount - columnIndex).ToString());

				writer.RenderBeginTag(HtmlTextWriterTag.Td);
				writer.RenderEndTag();

				//生成表格行元素的关闭标签</tr>
				writer.RenderEndTag();
			}

			//生成表格元素的关闭标签</table>
			writer.RenderEndTag();
		}

		private int GetRowSpanIndex(int[] rowSpans, int columnIndex)
		{
			int count = 0;

			for(int j = 0; j < rowSpans.Length; j++)
			{
				if(rowSpans[j] == 0)
				{
					if(columnIndex == count)
						return j;
					else
						count++;
				}
			}

			return -1;
		}
		#endregion
	}
}
