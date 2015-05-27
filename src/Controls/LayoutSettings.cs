using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Zongsoft.Web.Controls
{
	public class TableLayoutSettings
	{
		#region 成员字段
		private bool _mergeLastCells;
		#endregion

		#region 构造函数
		public TableLayoutSettings()
		{
			_mergeLastCells = true;
		}
		#endregion

		#region 公共属性
		[DefaultValue(true)]
		public bool MergeLastCells
		{
			get
			{
				return _mergeLastCells;
			}
			set
			{
				_mergeLastCells = value;
			}
		}
		#endregion
	}

	public class TableLayoutCellSettings
	{
		#region 成员变量
		private int _colSpan;
		private int _rowSpan;
		#endregion

		#region 构造函数
		public TableLayoutCellSettings()
		{
			_colSpan = 1;
			_rowSpan = 1;
		}
		#endregion

		#region 公共属性
		public int ColSpan
		{
			get
			{
				return _colSpan;
			}
			set
			{
				_colSpan = Math.Max(value, 1);
			}
		}

		public int RowSpan
		{
			get
			{
				return _rowSpan;
			}
			set
			{
				_rowSpan = Math.Max(value, 1);
			}
		}
		#endregion
	}

	public class FluidLayoutCellSettings
	{
		#region 成员变量
		private FluidLayoutFlex _flex;
		#endregion

		#region 公共属性
		public FluidLayoutFlex Flex
		{
			get
			{
				if(_flex == null)
					_flex = new FluidLayoutFlex();

				return _flex;
			}
			set
			{
				_flex = value;
			}
		}
		#endregion

		#region 嵌套结构
		[Serializable]
		[TypeConverter(typeof(FluidLayoutFlexConverter))]
		public class FluidLayoutFlex
		{
			public int Tiny;
			public int Small;
			public int Medium;
			public int Large;

			#region 构造函数
			public FluidLayoutFlex()
			{
			}

			public FluidLayoutFlex(int tiny, int small, int medium, int large)
			{
				this.Tiny = tiny;
				this.Small = small;
				this.Medium = medium;
				this.Large = large;
			}
			#endregion

			#region 重写方法
			public override string ToString()
			{
				return string.Format("{0}, {1}, {2}, {3}", Tiny, Small, Medium, Large);
			}
			#endregion

			#region 静态解析
			public static FluidLayoutFlex Parse(string text)
			{
				if(string.IsNullOrWhiteSpace(text))
					return new FluidLayoutFlex();

				var parts = text.Split(',');
				var flex = new FluidLayoutFlex();

				if(parts.Length > 0)
					flex.Tiny = Zongsoft.Common.Convert.ConvertValue<int>(parts[0].Trim());
				if(parts.Length > 1)
					flex.Small = Zongsoft.Common.Convert.ConvertValue<int>(parts[1].Trim());
				if(parts.Length > 2)
					flex.Medium = Zongsoft.Common.Convert.ConvertValue<int>(parts[2].Trim());
				if(parts.Length > 3)
					flex.Large = Zongsoft.Common.Convert.ConvertValue<int>(parts[3].Trim());

				return flex;
			}
			#endregion
		}

		public class FluidLayoutFlexConverter : TypeConverter
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
					return FluidLayoutFlex.Parse(text);

				return base.ConvertFrom(context, culture, value);
			}

			public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
			{
				if(destinationType == typeof(string))
				{
					var flex = value as FluidLayoutFlex;

					if(flex == null)
						return null;

					return flex.ToString();
				}

				if(destinationType == typeof(System.ComponentModel.Design.Serialization.InstanceDescriptor))
				{
					var ctor = typeof(FluidLayoutFlex).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) });
					var flex = (FluidLayoutFlex)value;
					return new System.ComponentModel.Design.Serialization.InstanceDescriptor(ctor, new int[] { flex.Tiny, flex.Small, flex.Medium, flex.Large });
				}

				return base.ConvertTo(context, culture, value, destinationType);
			}
		}
		#endregion
	}
}
