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
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	[ControlBuilder(typeof(DataBoundControlBuilder))]
	public class DataBoundControl : Control, IAttributeAccessor
	{
		#region 私有变量
		private NameValueCollection _bindingAttributes;
		#endregion

		#region 成员变量
		private IDictionary<string, object> _attributes;
		#endregion

		#region 构造函数
		protected DataBoundControl()
		{
			//重置客户端Id的生成方式为静态
			this.ClientIDMode = System.Web.UI.ClientIDMode.Static;

			_attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			_bindingAttributes = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue("")]
		public string CssClass
		{
			get
			{
				return this.GetAttributeValue<string>("CssClass", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.CssClass, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(true)]
		public override bool Visible
		{
			get
			{
				return this.GetAttributeValue<bool>("Visible", true);
			}
			set
			{
				this.SetAttributeValue(() => this.Visible, value);
			}
		}
		#endregion

		#region 保护属性
		protected IDictionary<string, object> Attributes
		{
			get
			{
				return _attributes;
			}
		}
		#endregion

		#region 公共方法
		public T GetAttributeValue<T>(string attributeName, T defaultValue = default(T))
		{
			if(string.IsNullOrWhiteSpace(attributeName))
				throw new ArgumentNullException("attributeName");

			attributeName = attributeName.Trim();

			if(_attributes != null && _attributes.ContainsKey(attributeName))
				return (T)_attributes[attributeName];

			return defaultValue;
		}

		public T GetAttributeValue<T>(Expression<Func<T>> expression, T defaultValue = default(T))
		{
			System.Linq.Expressions.MemberExpression exp = null;

			switch(expression.Body.NodeType)
			{
				case ExpressionType.Convert:
					exp = ((UnaryExpression)expression.Body).Operand as MemberExpression;
					break;
				case ExpressionType.MemberAccess:
					exp = (System.Linq.Expressions.MemberExpression)expression.Body;
					break;
			}

			if(exp == null)
				throw new ArgumentException("expression");

			return this.GetAttributeValue<T>(exp.Member.Name, defaultValue);
		}

		public void SetAttributeValue<T>(Expression<Func<T>> expression, T value)
		{
			System.Linq.Expressions.MemberExpression exp = null;

			switch(expression.Body.NodeType)
			{
				case ExpressionType.Convert:
					exp = ((UnaryExpression)expression.Body).Operand as MemberExpression;
					break;
				case ExpressionType.MemberAccess:
					exp = (System.Linq.Expressions.MemberExpression)expression.Body;
					break;
			}

			if(exp == null)
				return;

			var attribute = (System.ComponentModel.BindableAttribute)Attribute.GetCustomAttribute(exp.Member, typeof(System.ComponentModel.BindableAttribute), true);

			this.SetAttributeValue<T>(exp.Member.Name, value, (attribute == null ? false : attribute.Bindable));
		}

		public void SetAttributeValue<T>(string attributeName, T value, bool bindable)
		{
			if(string.IsNullOrWhiteSpace(attributeName))
				throw new ArgumentNullException("attributeName");

			attributeName = attributeName.Trim();

			//如果当前控件所在的页面为空，则表明页面还处在刚解析完成阶段。
			//因此控件的属性值有待于初始化过程中再进行绑定表达式计算，需要将当前属性值保存到_bindingAttributes集中。
			if(this.Page == null)
			{
				if(bindable && (typeof(T) == typeof(string) || (value != null && value.GetType() == typeof(string))))
					_bindingAttributes[attributeName] = (string)(object)value;
				else
					_attributes[attributeName] = value;
			}
			else
			{
				if(bindable && typeof(T) == typeof(string))
				{
					_attributes[attributeName] = BindingUtility.FormatBindingValue((string)(object)value, this.GetBindingSource());
				}
				else
				{
					_attributes[attributeName] = value;
				}
			}
		}
		#endregion

		#region 虚拟方法
		protected virtual void RenderAttributes(HtmlTextWriter writer, params string[] excludedAttributes)
		{
			string attributeName;

			if(_attributes == null || _attributes.Count < 1)
				return;

			foreach(KeyValuePair<string, object> attribute in _attributes)
			{
				if(attribute.Value != null)
				{
					if(string.Equals(attribute.Key, "DataSource", StringComparison.OrdinalIgnoreCase) ||
					   string.Equals(attribute.Key, "BindingSource", StringComparison.OrdinalIgnoreCase))
						continue;

					if(excludedAttributes != null && excludedAttributes.Contains(attribute.Key, StringComparer.OrdinalIgnoreCase))
						continue;

					if(string.Equals(attribute.Key, "CssClass", StringComparison.OrdinalIgnoreCase))
						attributeName = "class";
					else
						attributeName = attribute.Key.ToLowerInvariant();

					string attributeValue;
					if(this.OnRenderAttribute(attributeName, attribute.Value, out attributeValue))
						writer.AddAttribute(attributeName, attributeValue ?? string.Empty);
				}
			}
		}

		protected virtual bool OnRenderAttribute(string name, object value, out string renderValue)
		{
			if(value == null)
				renderValue = string.Empty;
			else
			{
				if(string.Equals(name, "value", StringComparison.OrdinalIgnoreCase))
					renderValue = value.ToString();
				else
					renderValue = value.ToString().ToLowerInvariant();
			}

			return true;
		}

		protected virtual string FormatAttributeValue(string name, object value, string format)
		{
			return BindingUtility.FormatValue(value, format, null);
		}
		#endregion

		#region 重写方法
		protected override void OnInit(EventArgs e)
		{
			if(_bindingAttributes != null && _bindingAttributes.Count > 0)
			{
				foreach(string key in _bindingAttributes)
				{
					var property = TypeDescriptor.GetProperties(this).Find(key, true);

					if(property == null)
					{
						_attributes[key] = BindingUtility.FormatBindingValue(_bindingAttributes[key], this.GetBindingSource(), (value, format) => this.FormatAttributeValue(key, value, format));
					}
					else
					{
						if(property.PropertyType == typeof(string))
							_attributes[key] = BindingUtility.FormatBindingValue(_bindingAttributes[key], this.GetBindingSource(), (value, format) => this.FormatAttributeValue(key, value, format));
						else
							_attributes[key] = BindingUtility.GetBindingValue(_bindingAttributes[key], this.GetBindingSource(), property.PropertyType);
					}
				}
			}

			//调用基类同名方法
			base.OnInit(e);
		}
		#endregion

		#region 显式实现
		string IAttributeAccessor.GetAttribute(string key)
		{
			return _bindingAttributes[key.Trim('$')];
		}

		void IAttributeAccessor.SetAttribute(string key, string value)
		{
			if(string.Equals(key, "$ID", StringComparison.OrdinalIgnoreCase))
				this.ID = value;
			else
				_bindingAttributes[key.Trim('$')] = value;
		}
		#endregion

		#region 私有方法
		private object GetBindingSource()
		{
			if(this.DataItemContainer != null)
				return this.DataItemContainer;

			//注意：以下判断是专为MVC中的局部视图(即用户控件)发现的问题而特别处理。
			if(this.TemplateControl is System.Web.Mvc.ViewUserControl)
				return this.TemplateControl;

			return this.Page;
		}

		private object GetPropertyDefaultValue(PropertyDescriptor property)
		{
			if(property == null)
				throw new ArgumentNullException("property");

			DefaultValueAttribute attribute = (DefaultValueAttribute)property.Attributes[typeof(DefaultValueAttribute)];

			if(attribute != null)
				return attribute.Value;

			return Zongsoft.Common.Convert.GetDefaultValue(property.PropertyType);
		}
		#endregion
	}
}
