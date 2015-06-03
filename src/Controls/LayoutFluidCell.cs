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
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	[PersistChildren(true)]
	[ParseChildren(false)]
	public class LayoutFluidCell : Literal
	{
		#region 成员字段
		private bool _isNewRow;
		private int _columnCount;
		private LayoutFluidVisiblity _visiblity;
		private LayoutFluidFlex _flex;
		#endregion

		#region 构造函数
		public LayoutFluidCell() : base("div")
		{
			_visiblity = LayoutFluidVisiblity.None;
		}
		#endregion

		#region 公共属性
		public bool IsNewRow
		{
			get
			{
				return _isNewRow;
			}
			set
			{
				_isNewRow = value;
			}
		}

		public int ColumnCount
		{
			get
			{
				return _columnCount;
			}
			set
			{
				_columnCount = Math.Abs(value);
			}
		}

		public LayoutFluidVisiblity Visiblity
		{
			get
			{
				return _visiblity;
			}
			set
			{
				_visiblity = value;
			}
		}

		public LayoutFluidFlex Flex
		{
			get
			{
				if(_flex == null)
					_flex = new LayoutFluidFlex();

				return _flex;
			}
			set
			{
				_flex = value;
			}
		}
		#endregion

		#region 生成界面
		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			if(_isNewRow)
			{
				if(_columnCount > 0)
					this.CssClass = ":" + Utility.GetNumberString(_columnCount) + " column row" + this.GetVisiblityString();
				else
					this.CssClass = ":row" + this.GetVisiblityString();
			}
			else
			{
				if(_columnCount > 0)
					this.CssClass = ":" + (_flex == null ? Utility.GetNumberString(_columnCount) + " wide column" : _flex.ToString()) + this.GetVisiblityString();
				else
					this.CssClass = ":column " + (_flex == null ? "" : _flex.ToString()) + this.GetVisiblityString();
			}

			this.AddAttributes(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			foreach(Control control in Utility.GetVisibleChildren(this))
			{
				if(control is LayoutFluidCell)
				{
					control.RenderControl(writer);
				}
				else
				{
					if(_isNewRow)
					{
						writer.AddAttribute(HtmlTextWriterAttribute.Class, "column");
						writer.RenderBeginTag(HtmlTextWriterTag.Div);
					}

					control.RenderControl(writer);

					if(_isNewRow)
						writer.RenderEndTag();
				}
			}
		}
		#endregion

		#region 私有方法
		private string GetVisiblityString()
		{
			if(_visiblity == LayoutFluidVisiblity.None)
				return string.Empty;

			if(_visiblity == (LayoutFluidVisiblity.Phone | LayoutFluidVisiblity.Tablet | LayoutFluidVisiblity.Computer))
				return string.Empty;

			var text = string.Empty;

			if((_visiblity & LayoutFluidVisiblity.Phone) == LayoutFluidVisiblity.Phone)
				text += " mobile";

			if((_visiblity & LayoutFluidVisiblity.Tablet) == LayoutFluidVisiblity.Tablet)
				text += " tablet";

			if((_visiblity & LayoutFluidVisiblity.Computer) == LayoutFluidVisiblity.Computer)
				text += " computer";

			if(!string.IsNullOrWhiteSpace(text))
				text += " only ";

			return text;
		}
		#endregion

		#region 枚举定义
		/// <summary>
		/// 表示流式布局的可见性。
		/// </summary>
		[Flags]
		public enum LayoutFluidVisiblity
		{
			/// <summary>不指定可见性</summary>
			None = 0,

			/// <summary>只有手机可见</summary>
			Phone = 1,
			/// <summary>只有平板可见</summary>
			Tablet = 2,
			/// <summary>只有电脑可见</summary>
			Computer = 4,
		}
		#endregion

		#region 嵌套子类
		[Serializable]
		[TypeConverter(typeof(LayoutFluidFlexConverter))]
		public class LayoutFluidFlex
		{
			public int Tiny;
			public int Small;
			public int Medium;
			public int Large;
			public int LargeWide;

			#region 构造函数
			public LayoutFluidFlex()
			{
			}

			public LayoutFluidFlex(int tiny, int small, int medium, int large, int largeWide)
			{
				this.Tiny = tiny;
				this.Small = small;
				this.Medium = medium;
				this.Large = large;
				this.LargeWide = largeWide;
			}
			#endregion

			#region 重写方法
			public override string ToString()
			{
				var text = new System.Text.StringBuilder();

				if(this.Tiny > 0)
					text.AppendFormat(" {0} wide mobile", Utility.GetNumberString(this.Tiny));

				if(this.Small > 0)
					text.AppendFormat(" {0} wide tablet", Utility.GetNumberString(this.Small));

				if(this.Medium > 0)
					text.AppendFormat(" {0} wide computer", Utility.GetNumberString(this.Medium));

				if(this.Large > 0)
					text.AppendFormat(" {0} wide large screen", Utility.GetNumberString(this.Large));

				if(this.LargeWide > 0)
					text.AppendFormat(" {0} wide widescreen", Utility.GetNumberString(this.LargeWide));

				return text.ToString();
			}
			#endregion

			#region 静态解析
			public static LayoutFluidFlex Parse(string text)
			{
				if(string.IsNullOrWhiteSpace(text))
					return new LayoutFluidFlex();

				var parts = text.Split(',');
				var flex = new LayoutFluidFlex();

				if(parts.Length == 1)
					flex.Tiny = flex.Small = flex.Medium = flex.Large = flex.LargeWide = Zongsoft.Common.Convert.ConvertValue<int>(parts[0].Trim());
				else
				{
					if(parts.Length > 0)
						flex.Tiny = Zongsoft.Common.Convert.ConvertValue<int>(parts[0].Trim());
					if(parts.Length > 1)
						flex.Small = Zongsoft.Common.Convert.ConvertValue<int>(parts[1].Trim());
					if(parts.Length > 2)
						flex.Medium = Zongsoft.Common.Convert.ConvertValue<int>(parts[2].Trim());
					if(parts.Length > 3)
						flex.Large = Zongsoft.Common.Convert.ConvertValue<int>(parts[3].Trim());
					if(parts.Length > 4)
						flex.LargeWide = Zongsoft.Common.Convert.ConvertValue<int>(parts[4].Trim());
				}

				return flex;
			}
			#endregion
		}

		public class LayoutFluidFlexConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				if(sourceType == typeof(string))
					return true;

				return base.CanConvertFrom(context, sourceType);
			}

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				if(destinationType == typeof(string) || destinationType == typeof(System.ComponentModel.Design.Serialization.InstanceDescriptor))
					return true;

				return base.CanConvertTo(context, destinationType);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			{
				if(value == null)
					return null;

				var text = value as string;

				if(text != null)
					return LayoutFluidFlex.Parse(text);

				return base.ConvertFrom(context, culture, value);
			}

			public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
			{
				if(destinationType == typeof(string))
				{
					var flex = value as LayoutFluidFlex;

					if(flex == null)
						return null;

					return flex.ToString();
				}

				if(destinationType == typeof(System.ComponentModel.Design.Serialization.InstanceDescriptor))
				{
					var ctor = typeof(LayoutFluidFlex).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });
					var flex = (LayoutFluidFlex)value;
					return new System.ComponentModel.Design.Serialization.InstanceDescriptor(ctor, new int[] { flex.Tiny, flex.Small, flex.Medium, flex.Large, flex.LargeWide });
				}

				return base.ConvertTo(context, culture, value, destinationType);
			}
		}
		#endregion
	}
}
