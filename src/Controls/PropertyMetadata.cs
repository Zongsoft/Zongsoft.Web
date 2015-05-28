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

namespace Zongsoft.Web.Controls
{
	public class PropertyMetadata
	{
		#region 常量定义
		private const int EVALUATE_VALUE = 1;
		private const int EVALUATE_ATTRIBUTEVALUE = 2;
		#endregion

		#region 私有变量
		private Zongsoft.Common.BitVector32 _flags;
		#endregion

		#region 成员字段
		private System.Web.UI.Control _control;
		private object _value;
		private string _valueString;
		private string _attributeValue;
		#endregion

		#region 构造函数
		public PropertyMetadata(System.Web.UI.Control control, string name, Type propertyType, object value = null)
		{
			if(control == null)
				throw new ArgumentNullException("control");

			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(propertyType == null)
				throw new ArgumentNullException("propertyType");

			_control = control;

			this.Name = name.Trim();
			this.AttributeName = this.Name.ToLowerInvariant();
			this.PropertyType = propertyType;
			this.Value = value;
			this.DefaultValue = Zongsoft.Common.Convert.ConvertValue(null, propertyType);
			this.Bindable = true;
			this.Renderable = true;
		}

		public PropertyMetadata(System.Web.UI.Control control, string name, string valueString)
		{
			if(control == null)
				throw new ArgumentNullException("control");

			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			_control = control;

			this.Name = name.Trim();
			this.AttributeName = this.Name.ToLowerInvariant();
			this.ValueString = valueString;
			this.Bindable = true;
			this.Renderable = true;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取当前属性元数据所属的控件。
		/// </summary>
		public System.Web.UI.Control Control
		{
			get
			{
				return _control;
			}
		}

		/// <summary>
		/// 获取属性的名称。
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// 获取属性的类型。
		/// </summary>
		public Type PropertyType
		{
			get;
			private set;
		}

		/// <summary>
		/// 获取或设置属性的值。
		/// </summary>
		public object Value
		{
			get
			{
				if(_flags[EVALUATE_VALUE])
				{
					_value = BindingUtility.GetBindingValue(_valueString, BindingUtility.GetBindingSource(_control), this.PropertyType);
					_flags[EVALUATE_VALUE] = false;
				}

				return _value ?? this.DefaultValue;
			}
			set
			{
				if(object.ReferenceEquals(_value, value))
					return;

				_value = value;
				_attributeValue = value.ToString();
			}
		}

		/// <summary>
		/// 获取或设置通过后台页面显式设置的值的标量文本。
		/// </summary>
		public string ValueString
		{
			get
			{
				return _valueString;
			}
			set
			{
				_valueString = value;
				_flags[EVALUATE_VALUE + EVALUATE_ATTRIBUTEVALUE] = true;
			}
		}

		/// <summary>
		/// 获取或设置声明的默认值。
		/// </summary>
		public object DefaultValue
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置属性生成的HTML特性(attribute)名。
		/// </summary>
		public string AttributeName
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置属性生成的HTML特性(attribute)值。
		/// </summary>
		public string AttributeValue
		{
			get
			{
				if(_flags[EVALUATE_ATTRIBUTEVALUE])
				{
					_attributeValue = BindingUtility.FormatBindingValue(_valueString, BindingUtility.GetBindingSource(_control), (v, format) => BindingUtility.FormatValue(v, format, null));
					_flags[EVALUATE_ATTRIBUTEVALUE] = false;
				}

				return _attributeValue;
			}
			set
			{
				_attributeValue = value;
			}
		}

		/// <summary>
		/// 获取或设置属性是否为绑定属性。
		/// </summary>
		public bool Bindable
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置属性是否要生成对应的HTML特性(attribute)。
		/// </summary>
		public bool Renderable
		{
			get;
			set;
		}

		public IPropertyRender PropertyRender
		{
			get;
			set;
		}
		#endregion

		#region 重写方法
		public override string ToString()
		{
			const string FORMAT = @"{0} ({1})
{{
	Value: {2}
	ValueString: {3}

	AttributeName: {4}
	AttributeValue: {5}

	Bindable: {6}
	Renderable: {7}

	PropertyRender: {8}
}}";

			return string.Format(FORMAT, this.Name, this.PropertyType, this.Value, this.ValueString, this.AttributeName, this.AttributeValue, this.Bindable, this.Renderable, this.PropertyRender);
		}
		#endregion
	}
}
